DECLARE @ExBill INT 
SET @ExBill =       --Термін погашення боргу по рахунках
	(SELECT TOP 1 cast([Value] as int)  FROM [Services].[Setting] WHERE [Guid] = '826C4666-F79C-4558-A0BB-2D5A428FCE1B')

DECLARE @pretenzia$cok$ TABLE(
				AccountId INT NOT NULL,
				AccountNumber VARCHAR(10) NULL,
				AccountNumberNew VARCHAR(10) NULL,
				zipcode VARCHAR(5) NULL,
				name_city VARCHAR(100) NULL,
				adresa VARCHAR(100) NULL,
				pip VARCHAR(170) NULL,
				suma_pay DECIMAL(9,2)NULL,
				DateFrom DATE NULL,
				DateTo DATE NULL
			)
        
/*Вибираємо всіх в кого є борг*/
INSERT INTO @pretenzia$cok$(AccountId,suma_pay)
SELECT o.AccountId
	   , SUM(o.RestSumm) RestSumm
FROM FinanceMain.Operation o
JOIN AccountingCommon.Account a ON a.AccountId = o.AccountId
WHERE o.PeriodTo=207906
		AND o.IsIncome=0
		AND o.DocumentTypeId IN (15)
		AND o.RestSumm > 0
		AND o.Date <= DATEADD(dd,-@ExBill,GETDATE())
		AND a.AccountNumber = @OsRahList
GROUP BY o.AccountId

UPDATE @pretenzia$cok$		-- добавляємо ПІП і ос.рах та адресу
SET AccountNumber = s.AccountNumber
	, AccountNumberNew = s.AccountNumberNew
	,pip = s.FullName
	,zipcode =s.ZipCode
	,name_city = s.name
	,adresa = s.adresa
FROM (SELECT a.AccountId
			,a.AccountNumber 
			, a.AccountNumberNew
			,pp.FullName
			,z.ZipCode
			,ct.ShortName +' '+ c.Name AS name
			,st.ShortName +' '+ s.Name
			+ (CASE WHEN addr.Building IS NOT NULL AND addr.Building<>'' THEN ', буд. '+ addr.Building +' ' +addr.BuildingPart ELSE ''END)
			+ (CASE WHEN addr.Apartment IS NOT NULL AND addr.Apartment<>'' THEN ', кв.'+addr.Apartment ELSE '' END) AS adresa
		FROM AccountingCommon.Account a
		JOIN AccountingCommon.PhysicalPerson pp ON pp.PhysicalPersonId = a.PhysicalPersonId
		JOIN AccountingCommon.Address addr ON addr.AddressId = a.AddressId
		JOIN [TR_Organization].[AddressDictionary].[Zip] AS z ON z.ZipId = addr.ZipId
		JOIN [TR_Organization].[AddressDictionary].[City] AS c ON c.CityId = addr.CityId
		JOIN [TR_Organization].[AddressDictionary].[CityType] AS ct ON ct.CityTypeId = c.CityTypeId
		JOIN [TR_Organization].[AddressDictionary].Street AS s ON s.StreetId = addr.StreetId
		JOIN [TR_Organization].[AddressDictionary].[StreetType]AS st ON st.StreetTypeId = s.StreetTypeId
		WHERE a.DateTo = '2079-06-06' -- тільки незакриті ОР
	) AS s
WHERE s.AccountId = [@pretenzia$cok$].AccountId

UPDATE @pretenzia$cok$		--добавляємо дату боргу
SET DateFrom = s.ConsumptionFrom, DateTo = s.ConsumptionTo
FROM (
		SELECT br.AccountId
				, MIN(br.ConsumptionFrom) AS ConsumptionFrom
			   , MAX(br.ConsumptionTo) AS ConsumptionTo
		FROM FinanceCommon.BillRegular br
		WHERE br.RestSumm > 0 AND br.IsDeleted = 0
				AND br.ConsumptionTo <= DATEADD(dd,-@ExBill,GETDATE())
		GROUP BY br.AccountId
) AS s
WHERE s.AccountId = [@pretenzia$cok$].AccountId

SELECT AccountNumber AS [Особовий]
		, AccountNumberNew AS [Новий]
		, pip AS [ПІП]
		, zipcode + ', ' + name_city + ', ' + adresa AS [Адреса]
		, suma_pay AS [сума до оплати]
		, DateFrom
		, DateTo
FROM @pretenzia$cok$
--WHERE AccountNumber = @OsRahList
--ORDER BY AccountNumber