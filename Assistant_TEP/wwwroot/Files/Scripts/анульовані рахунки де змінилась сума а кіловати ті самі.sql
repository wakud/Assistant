IF EXISTS (SELECT * FROM tempdb.sys.objects WHERE object_id = OBJECT_ID(N'tempdb..#x') AND type in (N'U'))

DROP TABLE IF EXISTS #x$cok$

SELECT  br.accountid
		, a.AccountNumber AS [�������� �������]
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

SELECT [�������� �������]
		,kvt_minus AS [���������� ����������]
		,sum_minus AS [���������� ����]
		,kvt_plus AS [�������� ����������]
		,sum_plus AS [�������� ����]
		,kvt_plus-kvt_minus AS [������ ���]
		,sum_plus-sum_minus AS [������ ���.]
FROM #x$cok$ 
WHERE kvt_plus=kvt_minus AND sum_plus<sum_minus
ORDER BY [�������� �������]
