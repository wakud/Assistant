SELECT	d.[Початковий борг], d.Нараховано, d.Оплачено, d.[Кінцевий борг], a.[Всього], b.[к-ть 1], c.[к-ть 3]
FROM (	SELECT	1 ID
		,COUNT(AccountNumber) [Всього]
		FROM AccountingCommon.Account
		WHERE DateTo = convert(DATETIME,'6/6/2079',103)) AS [a]
LEFT JOIN (
			--Заборгованість 1 місяць
		SELECT	1 ID
				,COUNT(a.AccountNumber) [к-ть 1]
				,SUM(o.RestSumm) [сума]
		FROM FinanceMain.Operation o
		LEFT JOIN AccountingCommon.Account a ON a.AccountId = o.AccountId
		LEFT JOIN AccountingCommon.PhysicalPerson p ON p.PhysicalPersonId = a.PhysicalPersonId
		WHERE	PeriodTo=207906
				AND IsIncome=0
				AND DocumentTypeId IN (15)
				AND o.RestSumm>0
				AND o.Date<=DATEADD(mm,-1,GETDATE())
				) AS [b] ON b.ID = a.ID
LEFT JOIN (
			--Заборгованість 3 місяці і більше
			SELECT	1 ID
					,COUNT(a.AccountNumber) [к-ть 3]
					,SUM(o.RestSumm) [сума]
			FROM FinanceMain.Operation o
			LEFT JOIN AccountingCommon.Account a ON a.AccountId = o.AccountId
			LEFT JOIN AccountingCommon.PhysicalPerson p ON p.PhysicalPersonId = a.PhysicalPersonId
			WHERE	PeriodTo=207906
					AND IsIncome=0
					AND DocumentTypeId IN (15)
					AND o.RestSumm>0
					AND o.Date<=DATEADD(mm,-3,GETDATE())) [c] ON c.ID = a.ID
LEFT JOIN(
			--Рядки 1-4
		SELECT	1 ID
				,SUM(fs.DebetBegin - fs.CreditBegin - fs.CreditBeginSubsidy) [Початковий борг]
				,SUM(fs.ChargedSumm) [Нараховано]
				,SUM(fs.PaidCashSumm + fs.PaidSubsidySumm + fs.PaidWriteOffSumm) [Оплачено]
				,SUM(fs.DebetEnd - fs.CreditEnd - fs.CreditEndSubsidy) [Кінцевий борг]
		FROM AccountingCommon.Account acc
		LEFT JOIN FinanceCommon.SupplierSaldo fs ON fs.AccountId = acc.AccountId
		WHERE	fs.Period = CONVERT(VARCHAR(6),DATEADD(mm,-1,GETDATE()),112) AND acc.DateTo = convert(DATETIME,'6/6/2079',103)
		) [d] ON d.ID = a.ID