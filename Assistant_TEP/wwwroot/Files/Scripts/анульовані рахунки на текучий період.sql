DECLARE @period$cok$ INT; SET @period$cok$ = (SELECT value FROM Services.Setting s WHERE settingid=1)
DECLARE @anul$cok$ TABLE(
					AccountId INT,
					AccountNumber VARCHAR(10),
					pip VARCHAR(300),
					kvt_minus INT,
					sum_minus DECIMAL(10,2),
					per_minus INT,
					dateFrom_minus DATE,
					dateTo_minus DATE,
					kvt_plus INT,
					sum_plus DECIMAL(10,2),
					per_plus INT,
					per_kor INT,
					kvtRizn INT,
					sumaRizn DECIMAL(10,2)
					)

INSERT INTO @anul$cok$ (AccountId, AccountNumber, pip, per_minus, kvt_minus, sum_minus, dateFrom_minus, dateTo_minus)
SELECT a.AccountId
		,a.AccountNumber
		,pp.FullName
		,fo.PeriodTo
		,ro.Quantity
		,fo.TotalSumm
		,ro.ConsumptionFrom
		,ro.ConsumptionTo
FROM AccountingCommon.Account a
JOIN AccountingCommon.PhysicalPerson pp ON pp.PhysicalPersonId = a.PhysicalPersonId
JOIN FinanceMain.Operation fo ON fo.AccountId = a.AccountId
LEFT OUTER JOIN (
					SELECT	r.OperationId
							,SUM(r.Quantity) AS Quantity
							,r.ConsumptionFrom
							,r.ConsumptionTo
					FROM FinanceMain.OperationRow r 
					GROUP BY r.OperationId, r.ConsumptionTo, r.ConsumptionFrom
				)ro ON ro.OperationId = fo.OperationId
WHERE a.DateTo = '2079-06-06' -- т≥льки незакрит≥ ќ–
		AND fo.DocumentTypeId = 15
		AND fo.PeriodTo < 207906

UPDATE @anul$cok$
SET kvt_plus = s.kvt_plus
	, sum_plus = s.sum_plus
	,per_plus = s.per_plus
FROM (
		SELECT a.AccountId
		,b.CalculatePeriod AS per_plus
		,b.ConsumptionFrom
		,b.ConsumptionTo
		,b.ConsumptionQuantity AS kvt_plus
		,b.TotalSumm AS sum_plus
FROM AccountingCommon.Account a
JOIN FinanceCommon.BillRegular b ON b.AccountId = a.AccountId
JOIN AccountingCommon.PhysicalPerson pp ON pp.PhysicalPersonId = a.PhysicalPersonId
WHERE a.DateTo = '2079-06-06' -- т≥льки незакрит≥ ќ–
		AND b.ConsumptionFrom BETWEEN '2019-01-01' AND GETDATE()
		AND b.IsDeleted = 0
	) AS s
WHERE s.AccountId = [@anul$cok$].AccountId AND s.per_plus = per_minus AND dateFrom_minus = s.ConsumptionFrom

UPDATE @anul$cok$
SET  kvtRizn = an.kvt_plus - an.kvt_minus, sumaRizn = an.sum_plus - an.sum_minus
FROM @anul$cok$ an
WHERE an.AccountId = AccountId

SELECT AccountNumber AS [ос.рах]
		,pip AS [ѕ≤ѕ]
		,SUM(kvt_minus) AS [анульован≥ к¬т]
		,SUM(sum_minus) AS [анульована сума]
		,SUM(kvt_plus) AS [фактичн≥ к¬т]
		,SUM(sum_plus) AS [фактична сума]
		,SUM(kvtRizn) AS [р≥зниц€ к¬т]
		,SUM(sumaRizn) AS [р≥зниц€ грн.]
FROM @anul$cok$
WHERE per_plus = @period$cok$
		AND kvt_plus<kvt_minus
GROUP BY AccountNumber, pip
ORDER BY AccountNumber
