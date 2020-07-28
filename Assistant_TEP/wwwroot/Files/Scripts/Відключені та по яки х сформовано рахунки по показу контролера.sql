/* Інформація щодо заборгованості за спожиту е/е по побутових споживачах, 
	які відключені та по яких сформовано рахунки за показами, знятими контролером РЕМ (від 100грн)
	*/

DECLARE @ExBill INT 
SET @ExBill =       --Термін погашення боргу по рахунках
            	(SELECT TOP 1 cast([Value] as int)  FROM [Services].[Setting] WHERE [Guid] = '826C4666-F79C-4558-A0BB-2D5A428FCE1B')
				
DECLARE @SummBorh INT; SET @SummBorh=100	--сума боргу від 100
DECLARE @date_from DATE; SET @date_from = '2019-01-01 00:00:00'		--дата з
DECLARE @date_to DATE; SET @date_to = GETDATE()		--дата по

declare @vykl TABLE (
		AccountId INT,
		AccountNumber VARCHAR(10),
		pip VARCHAR(300),
		monthBorgu INT,
		dateVykl DATE,
		kvtVykl INT,
		sumaVykl DECIMAL(10,2),
		kvtZvit INT,
		sumaZvit DECIMAL(10,2),
		kvtRizn INT,
		sumaRizn DECIMAL(10,2),
		PayDate DATE,
		PaySum DECIMAL(10,2),
		pokaz DATE
		)

--Вибираємо коли абонента відключили
INSERT INTO @vykl (AccountId, AccountNumber,pip, dateVykl)
SELECT	
		a.AccountId
		,a.AccountNumber
		,pp.FullName
		,d.DateFrom
	FROM AccountingCommon.Account a
	JOIN AccountingCommon.PhysicalPerson pp ON pp.PhysicalPersonId = a.PhysicalPersonId
	JOIN AccountingCommon.UsageObject uo ON uo.AccountId = a.AccountId
	JOIN AccountingCommon.Point p ON p.UsageObjectId = uo.UsageObjectId
		AND p.DateTo=convert(DATETIME,'06/06/2079',103)
	JOIN AccountingCommon.Disconnection d ON p.PointId = d.PointId AND d.DisconnectionStatus=1
	JOIN (
			SELECT
				 d2.[DisconnectionId],
				ROW_NUMBER() OVER (PARTITION BY d2.PointId ORDER BY d2.DateFrom DESC) AS RowNumber
			FROM [AccountingCommon].[Disconnection] as d2
		)
			AS dtemp ON dtemp.[DisconnectionId] = d.[DisconnectionId] AND dtemp.RowNumber = 1
	WHERE a.DateTo = '2079-06-06' -- тільки незакриті ОР

UPDATE @vykl		--витягуємо борг на дату відключення
SET kvtVykl = s.Quantity, sumaVykl = s.RestSumm
FROM (		
		SELECT o.AccountId
				,SUM(o.RestSumm) RestSumm
				,SUM(ro.Quantity) Quantity
		FROM FinanceMain.Operation o
		LEFT OUTER JOIN (
							SELECT r.OperationId,
									SUM(r.Quantity) AS Quantity
							FROM FinanceMain.OperationRow r 
							GROUP BY r.OperationId
						)ro ON ro.OperationId = o.OperationId
		LEFT JOIN @vykl n ON n.AccountId = o.AccountId
				WHERE o.PeriodTo = '207906'
						AND o.DocumentTypeId = 15
						AND o.RestSumm > 0
						AND o.Date BETWEEN @date_from AND n.dateVykl
				GROUP BY o.AccountId
	 ) AS s
WHERE s.AccountId = [@vykl].AccountId

UPDATE @vykl		--заповнюємо останню оплату
SET PayDate = s.PayDate
	,PaySum = s.PaySum
FROM ( SELECT r.AccountId
				,MAX(r.PayDate) AS PayDate
				,rc.TotalSumm AS PaySum
		FROM FinanceCommon.Receipt r
		INNER JOIN FinanceCommon.Receipt rc ON rc.AccountId = r.AccountId
		WHERE r.IsDeleted=0
			AND r.BillDocumentTypeId IN (15,8,16,14)
		GROUP BY r.AccountId, rc.TotalSumm
		HAVING DATEDIFF(mm,MAX(r.PayDate),GETDATE())>=0 AND MAX(rc.PayDate) = MAX(r.PayDate)
	  ) AS s
WHERE s.AccountId = [@vykl].AccountId

UPDATE @vykl		--витягуємо к-ть місяців боргу
SET monthBorgu = s.місяць
FROM (		
		SELECT o.AccountId
				, MAX(DATEDIFF(MONTH, 
					SUBSTRING(CONVERT(CHAR(10),o.PeriodFrom), 1, 4)+ '-' + 
					SUBSTRING(CONVERT(CHAR(10),o.PeriodFrom), 5, 2)+ '-01'
						, @date_to
				) ) AS [місяць]
		FROM FinanceMain.Operation o
		WHERE o.PeriodTo = '207906'
				AND o.DocumentTypeId = 15
				AND o.RestSumm > 0
				AND o.Date<=DATEADD(dd,-@ExBill,GETDATE())
		GROUP BY o.AccountId
	) AS s
WHERE s.AccountId = [@vykl].AccountId

UPDATE @vykl		--остання дата показів знятих контролером
SET pokaz = s.date
FROM (
		SELECT a.AccountId
				, gi.Date
				, ROW_NUMBER() OVER (PARTITION BY a.AccountId ORDER BY gi.Date DESC) AS rn
		FROM AccountingCommon.Account a
		JOIN AccountingCommon.UsageObject uo ON a.AccountId = uo.AccountId
		JOIN AccountingCommon.Point p ON p.UsageObjectId = uo.UsageObjectId
		JOIN AccountingCommon.UsageCalculationMethod cm ON cm.PointId = p.PointId
		JOIN AccountingMeasuring.GroupIndex gi ON gi.UsageCalculationMethodId = cm.UsageCalculationMethodId
		JOIN Organization.Staff s ON s.StaffId = gi.StaffId
		WHERE gi.IsForCalculate = 1
			  AND gi.Date BETWEEN @date_from AND @date_to
			AND ISNULL(gi.GroupIndexSourceId, 0) <> 18
			AND NOT (
						gi.StaffId = 599
						OR (s.LastName = 'Сайт ТОЕ' OR ISNULL(gi.GroupIndexSourceId, 0) = 20)
						OR ISNULL(gi.GroupIndexSourceId, 0) = 104
						OR ISNULL(gi.GroupIndexSourceId, 0) = 21
						OR ISNULL(gi.GroupIndexSourceId, 0) = 22
						OR (s.LastName LIKE '%абонент%' OR s.FirstName LIKE '%абонент%')
			)
		GROUP BY a.AccountId, gi.Date
		) AS s
WHERE s.AccountId = [@vykl].AccountId AND s.rn = 1

UPDATE @vykl
SET kvtZvit = s.Quantity
	,sumaZvit = s.RestSumm
FROM (		--витягуємо борг від останньої дати зняття показів
				SELECT o.AccountId
						,SUM(o.RestSumm) RestSumm
						,SUM(ro.Quantity) Quantity
				FROM FinanceMain.Operation o
				LEFT OUTER JOIN (
								SELECT r.OperationId,
										SUM(r.Quantity) AS Quantity
								FROM FinanceMain.OperationRow r 
								GROUP BY r.OperationId
								)ro ON ro.OperationId = o.OperationId
				JOIN @vykl AS v ON v.AccountId = o.AccountId
				WHERE o.PeriodTo = '207906'
						AND o.DocumentTypeId = 15
						AND o.RestSumm > 0
						AND o.Date BETWEEN v.pokaz AND  DATEADD(dd,-@ExBill,GETDATE())
				GROUP BY o.AccountId
				) AS s
WHERE s.AccountId = [@vykl].AccountId

UPDATE @vykl		--Шукаємо різницю боргу
SET kvtRizn = ISNULL(kvtZvit-kvtVykl, kvtZvit)
	,sumaRizn = ISNULL(sumaZvit - sumaVykl, sumaZvit)

SELECT n.AccountNumber AS [ос.рах]
		,n.pip AS [ПІП]
		,n.monthBorgu AS [К-ть місяців боргу]
		,n.dateVykl AS [Дата відключення]
		,n.kvtVykl AS [кВт на дату викл.]
		,n.sumaVykl AS [борг на дату викл.]
		,n.kvtZvit AS [кВт на дату показів]
		,n.sumaZvit AS [борг на дату показів]
		,n.kvtRizn AS [різниця кВт]
		,n.sumaRizn AS [борг різниця]
		,n.PayDate AS [остання дата оплати]
		,n.PaySum AS [сума оплати]
		,n.pokaz AS [Примітка]
FROM @vykl n
JOIN AccountingCommon.UsageObject uo ON n.AccountId = uo.AccountId
JOIN AccountingCommon.Point p ON p.UsageObjectId = uo.UsageObjectId
JOIN AccountingCommon.UsageCalculationMethod cm ON cm.PointId = p.PointId
JOIN AccountingMeasuring.GroupIndex gi ON gi.UsageCalculationMethodId = cm.UsageCalculationMethodId
JOIN Organization.Staff s ON s.StaffId = gi.StaffId
WHERE sumaZvit > @SummBorh 
	AND gi.IsForCalculate = 1
	AND gi.Date BETWEEN n.dateVykl AND @date_to
	AND ISNULL(gi.GroupIndexSourceId, 0) <> 18
	AND NOT (
				gi.StaffId = 599
				OR (s.LastName = 'Сайт ТОЕ' OR ISNULL(gi.GroupIndexSourceId, 0) = 20)
				OR ISNULL(gi.GroupIndexSourceId, 0) = 104
				OR ISNULL(gi.GroupIndexSourceId, 0) = 21
				OR ISNULL(gi.GroupIndexSourceId, 0) = 22
				OR (s.LastName LIKE '%абонент%' OR s.FirstName LIKE '%абонент%')
			)
GROUP BY n.AccountId
		,n.AccountNumber
		,n.pip
		,n.monthBorgu
		,n.dateVykl
		,n.kvtVykl
		,n.sumaVykl
		,n.kvtZvit
		,n.sumaZvit
		,n.kvtRizn
		,n.sumaRizn
		,n.PayDate
		,n.PaySum
		,n.pokaz
ORDER BY n.AccountNumber