/* ≤нформац≥€ щодо заборгованост≥ за спожиту е/е по побутових споживачах, 
	у €ких б≥льше н≥ж п≥в року не зн≥мали показник електрол≥чильника
	та по €ких рахунки сформовано по середньому споживанню е/е (в≥д 100грн)
	*/
DECLARE @SummBorh$cok$ INT; SET @SummBorh$cok$=100	--сума боргу в≥д 100
DECLARE @CntMonth$cok$ INT; SET @CntMonth$cok$ = 6 -- ≥льк≥сть м≥с€ц≥в необходу
DECLARE @ExBill$cok$ INT 
SET @ExBill$cok$ =       --“ерм≥н погашенн€ боргу по рахунках
            	(SELECT TOP 1 cast([Value] as int)  FROM [Services].[Setting] WHERE [Guid] = '826C4666-F79C-4558-A0BB-2D5A428FCE1B') 
DECLARE @d$cok$ DATETIME; SET @d$cok$=convert(char(8),getdate(),112)
DECLARE @date_from$cok$ DATE; SET @date_from$cok$ = '2019-01-01 00:00:00'		--дата з
DECLARE @date_to$cok$ DATE; SET @date_to$cok$ = dateadd(day,1-day(@d$cok$),@d$cok$)		--дата по

declare @neo$cok$ TABLE (
		AccountId INT,
		AccountNumber VARCHAR(10),
		pip VARCHAR(300),
		monthBorgu INT,
		resultdate DATE,
		--pokaz VARCHAR(50),
		kvtZn INT,
		sumaZn DECIMAL(10,2),
		kvtZvit INT,
		sumaZvit DECIMAL(10,2),
		kvtRizn INT,
		sumaRizn DECIMAL(10,2),
		PayDate DATE,
		PaySum DECIMAL(10,2),
		CalcMethod TINYINT
		)

--¬ибираЇмо коли зн≥малис€ показники контролерами
INSERT INTO @neo$cok$ (AccountId, AccountNumber,pip, resultdate/*, pokaz*/)
SELECT acc.AccountId
		,acc.AccountNumber
		,pp.FullName
		,MAX(gi.Date) max_Date
		--,gi.CachedIndexes
FROM AccountingCommon.Account acc
LEFT JOIN AccountingCommon.PhysicalPerson pp ON pp.PhysicalPersonId = acc.PhysicalPersonId
JOIN AccountingCommon.UsageObject uo ON uo.AccountId = acc.AccountId 
JOIN AccountingCommon.Point p ON uo.UsageObjectId = p.UsageObjectId
		AND p.DateTo=convert(DATETIME,'06/06/2079',103)
		AND acc.DateTo =convert(DATETIME,'06/06/2079',103)
JOIN AccountingCommon.UsageCalculationMethod ucm ON ucm.PointId = p.PointId
JOIN AccountingMeasuring.GroupIndex gi ON gi.UsageCalculationMethodId = ucm.UsageCalculationMethodId
		AND ucm.DateTo = '20790606' AND ucm.Generation = 0 	
		AND gi.IsForCalculate=1
		AND ISNULL(gi.GroupIndexSourceId,0) NOT IN (18)  --Ќе враховувати –озрахунков≥ покази
GROUP BY acc.AccountId
		, acc.AccountNumber
		, pp.FullName
		--,gi.CachedIndexes
HAVING DATEDIFF(mm,MAX(gi.Date),GETDATE())>=@CntMonth$cok$

UPDATE @neo$cok$		-- вит€гуЇмо останню оплату
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
WHERE s.AccountId = [@neo$cok$].AccountId

UPDATE @neo$cok$
SET kvtZvit = s.Quantity
	,sumaZvit = s.RestSumm
	,monthBorgu = s.м≥с€ць
FROM (		--вит€гуЇмо борг на зв≥т дату
				SELECT o.AccountId
						, SUM(o.RestSumm) RestSumm
						,SUM(ro.Quantity) Quantity
						, MAX(DATEDIFF(MONTH, 
							SUBSTRING(CONVERT(CHAR(10),o.PeriodFrom), 1, 4)+ '-' + 
							SUBSTRING(CONVERT(CHAR(10),o.PeriodFrom), 5, 2)+ '-01'
								, @date_to$cok$
						) ) AS [м≥с€ць]
				FROM FinanceMain.Operation o
				LEFT OUTER JOIN (
								SELECT r.OperationId,
										SUM(r.Quantity) AS Quantity
								FROM FinanceMain.OperationRow r 
								GROUP BY r.OperationId
								)ro ON ro.OperationId = o.OperationId
				WHERE o.PeriodTo = '207906'
						AND o.DocumentTypeId = 15
						AND o.RestSumm > 0
						AND o.Date<=DATEADD(dd,-@ExBill$cok$,GETDATE())
				GROUP BY o.AccountId
				) AS s
WHERE s.AccountId = [@neo$cok$].AccountId

UPDATE @neo$cok$
SET kvtZn = s.Quantity, sumaZn = s.RestSumm
FROM (		--вит€гуЇмо борг на дату зн€тт€ показ≥в
		SELECT o.AccountId
				,SUM(r.TotalSumm) - SUM(r.UsedSumm) - SUM(r.DiscountSumm) AS RestSumm
				,SUM(r.Quantity) Quantity
		FROM FinanceMain.Operation o
		JOIN FinanceMain.OperationRow r ON r.OperationId = o.OperationId
		LEFT JOIN @neo$cok$ n ON n.AccountId = o.AccountId
		WHERE o.PeriodTo = '207906'
				AND o.DocumentTypeId = 15
				AND o.RestSumm > 0
				AND r.ConsumptionFrom >= @date_from$cok$
				--AND r.ConsumptionFrom <= n.resultdate		--то €к каже  ондратьЇва 
				AND r.ConsumptionTo <= n.resultdate		-- то €к каже  ордас
		GROUP BY o.AccountId
	 ) AS s
WHERE s.AccountId = [@neo$cok$].AccountId


UPDATE @neo$cok$		--ЎукаЇмо р≥зницю боргу
SET kvtRizn = ISNULL(kvtZvit-kvtZn, kvtZvit)
	,sumaRizn = ISNULL(sumaZvit - sumaZn, sumaZvit)

UPDATE @neo$cok$		--шукаЇмо в кого було нарахуванн€ по середньому
	SET CalcMethod = s.CalcMethod
    FROM (SELECT o.AccountId
				,COUNT(o.CalcMethod) AS CalcMethod
			FROM FinanceMain.Operation o
			WHERE o.PeriodTo = '207906'
					AND o.DocumentTypeId = 15
					AND o.CalcMethod = 3
			GROUP BY o.AccountId
		 ) AS s
	WHERE s.AccountId = [@neo$cok$].AccountId

SELECT AccountNumber AS [ос.рах]
		,pip AS [ѕ≤ѕ]
		,monthBorgu AS [ -ть м≥с€ц≥в боргу]
		,resultdate AS [ƒата показника]
		,kvtZn AS [к¬т на дату зн€тт€]
		,sumaZn AS [борг на дату зн€тт€]
		,kvtZvit AS [к¬т на зв≥т. дату]
		,sumaZvit AS [борг на зв≥т. дату]
		,kvtRizn AS [р≥зниц€ к¬т]
		,sumaRizn AS [борг р≥зниц€]
		,PayDate AS [останн€ дата оплати]
		,PaySum AS [сума оплати]
		,CalcMethod AS [к-ть нарах. по сер.]
FROM @neo$cok$
WHERE sumaRizn > @SummBorh$cok$ AND CalcMethod > 0