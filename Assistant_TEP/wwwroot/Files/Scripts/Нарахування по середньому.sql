/***-- ����������� �� ���������� --***/

drop table IF EXISTS ##sered$cok$

select	acc.AccountNumber
		,SUM(br.ConsumptionQuantity) AS ConsumptionQuantity
		,SUM(br.TotalSumm) AS TotalSumm
		,br.CalculatePeriod
		,(ROW_NUMBER() over (PARTITION BY AccountNumber Order by br.CalculatePeriod asc)) as periodNumber
INTO ##sered$cok$
FROM FinanceCommon.BillRegular br 
LEFT JOIN AccountingCommon.Account acc ON acc.AccountId = br.AccountId
WHERE br.CalcMethod=3 AND br.IsDeleted = 0 AND br.ConsumptionFrom BETWEEN @DateFrom AND @DateTo AND acc.DateTo=convert(DATETIME,'06/06/2079',103)
GROUP BY acc.AccountNumber,
         --br.ConsumptionQuantity,
         --br.TotalSumm,
         br.CalculatePeriod

SELECT	AccountNumber AS [��. ���.]
		,SUM(ConsumptionQuantity) AS [���]
		,SUM(TotalSumm) AS [����]
		,MAX(CalculatePeriod) AS [������� �����]
		,MAX(periodNumber) AS [�-�� ������]
FROM ##sered$cok$
--WHERE periodNumber>=3
GROUP BY AccountNumber--, ConsumptionQuantity, TotalSumm
ORDER BY AccountNumber


