/* Інформація щодо заборгованості за спожиту е/е по побутових споживачах, 
	які відключені та по яких сформовано рахунки за показами, знятими контролером РЕМ (від 100грн)
	*/

DECLARE @ExBill$cok$ INT 
SET @ExBill$cok$ =       --Термін погашення боргу по рахунках
            	(SELECT TOP 1 cast([Value] as int)  FROM [Services].[Setting] WHERE [Guid] = '826C4666-F79C-4558-A0BB-2D5A428FCE1B')
				
DECLARE @SummBorh$cok$ INT; SET @SummBorh$cok$=100	--сума боргу від 100
DECLARE @date_from$cok$ DATE; SET @date_from$cok$ = '2019-01-01 00:00:00'		--дата з
DECLARE @date_to$cok$ DATE; SET @date_to$cok$ = GETDATE()		--дата по

declare @vykl$cok$ TABLE (
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
		dataPokazu DATE
		)

--Вибираємо коли абонента відключили
INSERT INTO @vykl$cok$ (AccountId, AccountNumber,pip, dateVykl)
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

UPDATE @vykl$cok$		--витягуємо борг на дату відключення
SET kvtVykl = s.Quantity, sumaVykl = s.RestSumm
FROM (		
		SELECT o.AccountId
				,SUM(r.TotalSumm) - SUM(r.UsedSumm) - SUM(r.DiscountSumm) AS RestSumm
				,SUM(r.Quantity) Quantity
		FROM FinanceMain.Operation o
		JOIN FinanceMain.OperationRow r ON r.OperationId = o.OperationId
		LEFT JOIN @vykl$cok$ n ON n.AccountId = o.AccountId
		WHERE o.PeriodTo = '207906'
				AND o.DocumentTypeId = 15
				AND o.RestSumm > 0
				AND r.ConsumptionFrom >= @date_from$cok$ 
				--AND r.ConsumptionFrom <= n.dateVykl		--то як каже Кондратьєва 
				AND r.ConsumptionTo <= n.dateVykl		-- то як каже Кордас
		GROUP BY o.AccountId
	 ) AS s
WHERE s.AccountId = [@vykl$cok$].AccountId

UPDATE @vykl$cok$		--заповнюємо останню оплату
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
WHERE s.AccountId = [@vykl$cok$].AccountId

UPDATE @vykl$cok$		--витягуємо к-ть місяців боргу
SET monthBorgu = s.місяць
FROM (		
		SELECT o.AccountId
				, MAX(DATEDIFF(MONTH, 
					SUBSTRING(CONVERT(CHAR(10),o.PeriodFrom), 1, 4)+ '-' + 
					SUBSTRING(CONVERT(CHAR(10),o.PeriodFrom), 5, 2)+ '-01'
						, @date_to$cok$
				) ) AS [місяць]
		FROM FinanceMain.Operation o
		WHERE o.PeriodTo = '207906'
				AND o.DocumentTypeId = 15
				AND o.RestSumm > 0
				AND o.Date<=DATEADD(dd,-@ExBill$cok$,GETDATE())
		GROUP BY o.AccountId
	) AS s
WHERE s.AccountId = [@vykl$cok$].AccountId

UPDATE @vykl$cok$		--остання дата показів знятих контролером
SET dataPokazu = s.date
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
			  AND gi.Date BETWEEN @date_from$cok$ AND @date_to$cok$
			AND ISNULL(gi.GroupIndexSourceId, 0) <> 18
			AND NOT (
						gi.StaffId = 599
						OR (s.LastName = 'Сайт ТОЕ' OR ISNULL(gi.GroupIndexSourceId, 0) = 20)
						OR ISNULL(gi.GroupIndexSourceId, 0) = 104
						OR ISNULL(gi.GroupIndexSourceId, 0) = 21
						OR ISNULL(gi.GroupIndexSourceId, 0) = 22
						OR (s.LastName LIKE '%абонент%' 
							OR s.FirstName LIKE '%абонент%'
							OR s.LastName LIKE '%-Call-%'
						   )
					)
		GROUP BY a.AccountId, gi.Date
		) AS s
WHERE s.AccountId = [@vykl$cok$].AccountId AND s.rn = 1

UPDATE @vykl$cok$
SET kvtZvit = s.Quantity
	,sumaZvit = s.RestSumm
FROM (		--витягуємо борг до останньої дати зняття показів
				SELECT o.AccountId
				,SUM(r.TotalSumm) - SUM(r.UsedSumm) - SUM(r.DiscountSumm) AS RestSumm
				,SUM(r.Quantity) Quantity
		FROM FinanceMain.Operation o
		JOIN FinanceMain.OperationRow r ON r.OperationId = o.OperationId
		LEFT JOIN @vykl$cok$ n ON n.AccountId = o.AccountId
		WHERE o.PeriodTo = '207906'
				AND o.DocumentTypeId = 15
				AND o.RestSumm > 0
				AND r.ConsumptionFrom >= @date_from$cok$ 
				--AND r.ConsumptionFrom <= n.dataPokazu		--то як каже Кондратьєва 
				AND r.ConsumptionTo <= n.dataPokazu		-- то як каже Кордас
		GROUP BY o.AccountId
				) AS s
WHERE s.AccountId = [@vykl$cok$].AccountId

UPDATE @vykl$cok$		--Шукаємо різницю боргу
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
		,n.dataPokazu AS [Примітка]
FROM @vykl$cok$ n
WHERE n.sumaRizn > @SummBorh$cok$ 
ORDER BY n.AccountNumber