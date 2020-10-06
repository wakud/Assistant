
SELECT a.AccountNumber AS [��. �������]
		, pp.FullName AS [ϲ�]
		, adr.FullAddress AS [������ ��������]
		, r.TotalSumm AS [���� ��������]
		, r.PayDate AS [���� ��������]
		, @date_from AS [���� �]
		, @date_to AS [���� ��]
FROM FinanceCommon.Receipt r
JOIN AccountingCommon.Account a ON a.AccountId = r.AccountId
JOIN AccountingCommon.PhysicalPerson pp ON pp.PhysicalPersonId = a.PhysicalPersonId
JOIN AccountingCommon.Address adr ON adr.AddressId = a.AddressId
WHERE a.AccountNumber = @accN
		AND r.IsDeleted=0
		AND r.BillDocumentTypeId IN (15)
		AND (r.PayDate) BETWEEN @date_from AND @date_to
ORDER BY r.PayDate DESC