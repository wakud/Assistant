DROP TABLE IF EXISTS ##qw
DECLARE @ExBill INT 
SET @ExBill =       --Термін погашення боргу по рахунках
	(SELECT TOP 1 cast([Value] as int)  FROM [Services].[Setting] WHERE [Guid] = '826C4666-F79C-4558-A0BB-2D5A428FCE1B')  

SELECT	 AccountId [qwer]
		,SUM(o.RestSumm) [сума]
		,MAX(DATEDIFF(MONTH, CAST(o.Date AS DATE), GETDATE())) AS [місяць]
INTO ##qw
FROM FinanceMain.Operation o
WHERE PeriodTo=207906
	AND IsIncome=0
	AND DocumentTypeId IN (15)
	AND o.RestSumm>0
	AND o.Date<=DATEADD(dd,-@ExBill,GETDATE())
GROUP BY o.AccountId

SELECT	d.[Початковий борг], d.Нараховано, d.Оплачено, d.[Кінцевий борг], a.[Всього], b.[мають борг], c.[3 і більше]
FROM (	SELECT	1 ID
		,COUNT(DISTINCT AccountId) [Всього]
		FROM AccountingCommon.Account
		WHERE DateTo = convert(DATETIME,'6/6/2079',103)) AS [a]
LEFT JOIN (
			-- Всі хто мають заборгованість
			SELECT	1 ID
					, COUNT(qwer) [мають борг]
					,SUM(сума) [сума]
			FROM ##qw
			) AS [b] ON b.ID = a.ID
LEFT JOIN (
			--Заборгованість 3 місяці і більше
			SELECT	1 ID
					, COUNT(qwer) [3 і більше]
					,SUM(сума) [сума]
			FROM ##qw
			WHERE місяць >= 3
			) [c] ON c.ID = a.ID
LEFT JOIN(
			--Рядки 1-4
		SELECT	1 ID
				,SUM(fs.DebetBegin) [Початковий борг]
				,SUM(fs.ChargedSumm) [Нараховано]
				,SUM(fs.PaidCashSumm) [Оплачено]
				,SUM(fs.DebetEnd) [Кінцевий борг]
		FROM AccountingCommon.Account acc
		LEFT JOIN FinanceCommon.SupplierSaldo fs ON fs.AccountId = acc.AccountId
		WHERE	fs.Period = CONVERT(VARCHAR(6),DATEADD(mm, -1, GETDATE()),112) AND acc.DateTo = convert(DATETIME,'6/6/2079',103)
		) [d] ON d.ID = a.ID