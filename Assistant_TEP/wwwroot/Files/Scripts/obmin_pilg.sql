SELECT a.AccountNumber
		,SUM(o.RestSumm) RestSumm
    FROM FinanceMain.Operation o
	JOIN AccountingCommon.Account a ON o.AccountId = a.AccountId

	WHERE PeriodTo=207906
    AND IsIncome=0
    AND DocumentTypeId IN (15)
    AND o.RestSumm>0
    AND o.Date<=DATEADD(mm,-3,GETDATE())
    GROUP BY a.AccountNumber