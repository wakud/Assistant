DROP TABLE IF EXISTS #anul$cok$

DECLARE @per_start INT, @per_end INT
SET @per_start = 201901
SET @per_end = (SELECT TOP 1 cast([Value] as int)  FROM [Services].[Setting] WHERE [Guid] = 'E6AC6284-6983-46E1-9A9D-D110BE68E954')

DECLARE @date_from DATE; SET @date_from = '2019-01-01 00:00:00'
DECLARE @d DATETIME; SET @d=convert(char(8),getdate(),112)
DECLARE @date_to DATE; SET @date_to = dateadd(day,1-day(@d),@d)		--���� ��

SELECT  br.accountid
		, a.AccountNumber AS [�������� �������]
		, SUM(br.ConsumptionQuantity) kvt_plus
		, SUM(br.TotalSumm) sum_plus
		, (SELECT SUM( br1.ConsumptionQuantity)
            FROM FinanceCommon.BillRegular br1
            WHERE isdeleted=1
			AND br1.ConsumptionFrom BETWEEN @date_from AND @date_to
            AND br1.AccountId=br.AccountId) kvt_minus
		, (SELECT SUM( br1.totalsumm)
            FROM FinanceCommon.BillRegular br1
            WHERE isdeleted=1
			AND br1.ConsumptionFrom BETWEEN @date_from AND @date_to
            AND br1.AccountId=br.AccountId) sum_minus
INTO #anul$cok$
FROM FinanceCommon.BillRegular br
LEFT JOIN AccountingCommon.Account a ON a.AccountId = br.AccountId
WHERE isdeleted=0
		AND br.ConsumptionFrom BETWEEN @date_from AND @date_to

GROUP by br.accountid, a.AccountNumber

SELECT [�������� �������]
		,kvt_minus AS [���������� ����������]
		,sum_minus AS [���������� ����]
		,kvt_plus AS [�������� ����������]
		,sum_plus AS [�������� ����]
		,kvt_plus-kvt_minus AS [������ ���]
		,sum_plus-sum_minus AS [������ ���.]
FROM #anul$cok$
WHERE kvt_plus<kvt_minus
ORDER BY [�������� �������]
