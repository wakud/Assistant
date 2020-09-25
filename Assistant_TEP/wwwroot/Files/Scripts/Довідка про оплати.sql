
SELECT a.AccountNumber
		, pp.FullName
		, adr.FullAddress
		,r.TotalSumm AS PaySum
		,(r.PayDate) AS PayDate
		, @date_from
		, @date_to
FROM FinanceCommon.Receipt r
JOIN AccountingCommon.Account a ON a.AccountId = r.AccountId
JOIN AccountingCommon.PhysicalPerson pp ON pp.PhysicalPersonId = a.PhysicalPersonId
JOIN AccountingCommon.Address adr ON adr.AddressId = a.AddressId
WHERE a.AccountNumber = @accN
		AND r.IsDeleted=0
		AND r.BillDocumentTypeId IN (15)
		AND (r.PayDate) BETWEEN @date_from AND @date_to
ORDER BY r.PayDate DESC