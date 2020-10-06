
SELECT a.AccountNumber AS [Ос. рахунок]
		, pp.FullName AS [ПІП]
		, adr.FullAddress AS [Адреса абонента]
		, r.TotalSumm AS [сума проплати]
		, r.PayDate AS [Дата проплати]
		, @date_from AS [Дата з]
		, @date_to AS [Дата по]
FROM FinanceCommon.Receipt r
JOIN AccountingCommon.Account a ON a.AccountId = r.AccountId
JOIN AccountingCommon.PhysicalPerson pp ON pp.PhysicalPersonId = a.PhysicalPersonId
JOIN AccountingCommon.Address adr ON adr.AddressId = a.AddressId
WHERE a.AccountNumber = @accN
		AND r.IsDeleted=0
		AND r.BillDocumentTypeId IN (15)
		AND (r.PayDate) BETWEEN @date_from AND @date_to
ORDER BY r.PayDate DESC