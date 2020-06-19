SELECT	z.ZipCode AS [Індекс]
		, c.Name AS [Нас.пункт]
		, addr.FullAddress AS [Адреса абонента]
 		, a.AccountNumber AS [особовий рахунок]
		, pp.FullName AS [ПІП абонента]
		--,addr.CityId
		,o.RestSumm AS [сума до оплати]
		--,o.RestSummRound AS [сума до оплати заокруглена]
FROM AccountingCommon.Account a 
JOIN (SELECT o.AccountId,SUM(o.RestSumm) RestSumm,CEILING(SUM(o.RestSumm)) as RestSummRound
	FROM FinanceMain.Operation o
	WHERE PeriodTo=207906
	AND IsIncome=0
	AND DocumentTypeId IN (15)---1,9/15
	AND o.RestSumm>0
	GROUP BY o.AccountId
	HAVING SUM(o.RestSumm)>=@sum_pay) o ON a.AccountId = o.AccountId
JOIN AccountingCommon.PhysicalPerson pp ON pp.PhysicalPersonId = a.PhysicalPersonId
JOIN AccountingCommon.Address addr ON addr.AddressId = a.AddressId
LEFT JOIN [TR_Organization].[AddressDictionary].[Zip] AS z ON z.ZipId = addr.ZipId
LEFT JOIN [TR_Organization].[AddressDictionary].[City] AS c ON c.CityId = addr.CityId
JOIN AccountingCommon.UsageObject uo ON uo.AccountId = a.AccountId
LEFT JOIN (SELECT o.AccountId,r.PayDate,r.TotalSumm,
ROW_NUMBER() OVER (PARTITION BY o.AccountId ORDER BY r.PayDate DESC) id
	FROM FinanceMain.Operation o 
	JOIN FinanceCommon.Receipt r ON r.ReceiptId =o.DocumentId
	and IsIncome=1
	AND r.PaymentFormId IN (1,2)
	AND o.PeriodTo=207906) op ON op.AccountId = a.AccountId
	AND op.id = 1
WHERE z.ZipCode = @zip_code
ORDER BY c.Name