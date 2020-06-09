/*		
		Звіт Інформація щодо заборгованості за спожиту електроенергію населенням 
		(від 1000 грн та/або до 1000грн з терміном винекнення 3 місяці і більше)
		станом на __________________
*/

DROP TABLE IF EXISTS ##qwerty

SELECT	acc.AccountNumber AS [ос.рах]
		, pers.FullName AS [ПІП]
		, ad.FullAddress AS [Адреса]
		, SUM(CASE WHEN br.CalculatePeriod BETWEEN @per_start AND @per_end THEN br.RestSumm ELSE 0 END) AS [борг]
		, MAX(DATEDIFF(MONTH, 
						CAST(
							CAST(br.CalculatePeriod AS CHAR(6)) + '01' AS DATE
						),
						CAST(@stanom_na AS DATE) 
					) )AS [місяць]
INTO ##qwerty
FROM FinanceCommon.BillRegular br
LEFT JOIN AccountingCommon.Account acc ON acc.AccountId = br.AccountId
JOIN AccountingCommon.PhysicalPerson as pers ON pers.PhysicalPersonId = acc.PhysicalPersonId
JOIN AccountingCommon.Address as ad ON ad.AddressId = acc.AddressId 
WHERE	br.IsDeleted = 0 
		AND br.CalculatePeriod BETWEEN @per_start AND @per_end
		AND br.RestSumm > 0.00
GROUP BY acc.AccountNumber
		, pers.FullName
		, ad.FullAddress

SELECT [ос.рах]
		,[ПІП]
		,[Адреса]
		,[борг]
		,[місяць] - 1 AS [місяць]
FROM ##qwerty
WHERE борг>=1000.00
		OR (борг>=100.00 AND місяць BETWEEN 3 AND 30)
ORDER BY [ос.рах], Адреса