IF EXISTS (SELECT * FROM tempdb.sys.objects WHERE object_id = OBJECT_ID(N'tempdb..#x') AND type in (N'U'))

DROP TABLE IF EXISTS #x$cok$

SELECT  br.accountid
		, a.AccountNumber AS [Особовий рахунок]
		, SUM(br.ConsumptionQuantity) kvt_plus
		,SUM(br.TotalSumm) sum_plus
		, (SELECT SUM( br1.ConsumptionQuantity)
           FROM FinanceCommon.BillRegular br1
           WHERE isdeleted=1
                 AND br1.CalculatePeriod>=@period
                 AND br1.AccountId=br.AccountId) kvt_minus
        , (SELECT SUM( br1.totalsumm)
           FROM FinanceCommon.BillRegular br1
           WHERE isdeleted=1
                AND br1.CalculatePeriod>=@period
                AND br1.AccountId=br.AccountId) sum_minus
INTO #x$cok$

FROM FinanceCommon.BillRegular br
LEFT JOIN AccountingCommon.Account a ON a.AccountId = br.AccountId
WHERE isdeleted=0
      AND br.CalculatePeriod=@period+1
GROUP by br.accountid, a.AccountNumber

SELECT [Особовий рахунок]
		,kvt_minus AS [анульоване споживання]
		,sum_minus AS [анульована сума]
		,kvt_plus AS [фактичне споживання]
		,sum_plus AS [фактична сума]
		,kvt_plus-kvt_minus AS [різниця кВт]
		,sum_plus-sum_minus AS [різниця грн.]
FROM #x$cok$ 
WHERE kvt_plus=kvt_minus AND sum_plus<sum_minus
ORDER BY [Особовий рахунок]
