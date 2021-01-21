DECLARE @tbl$cok$ TABLE(
				AccountId INT,
				AccountNumber VARCHAR(10),
				AccountNumberNew VARCHAR(10),
				zipcode VARCHAR(5),
				name_city VARCHAR(100),
				adresa VARCHAR(100),
				pip VARCHAR(70),
				suma_pay DECIMAL(9,2),
				pokaz VARCHAR(20),
				data DATE
			)
/*Вибираємо всіх в кого є борг*/
INSERT INTO @tbl$cok$(AccountId,suma_pay)
SELECT o.AccountId,
		SUM(o.RestSumm) RestSumm
	FROM FinanceMain.Operation o
	WHERE PeriodTo=207906
	AND IsIncome=0
	AND DocumentTypeId IN (15)---1,9/15
	AND o.RestSumm>0
	GROUP BY o.AccountId

UPDATE @tbl$cok$		-- добавляємо ПІП і ос.рах та адресу
SET AccountNumber = s.AccountNumber
	,AccountNumberNew = s.AccountNumberNew
	,pip = s.FullName
	,zipcode =s.ZipCode
	,name_city = s.name
	,adresa = s.adresa
FROM (SELECT a.AccountId
			,a.AccountNumber 
			,a.AccountNumberNew
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
WHERE s.AccountId = [@tbl$cok$].AccountId

UPDATE @tbl$cok$		--добавляємо показник
SET pokaz = s.CachedIndexes,
	DATA = s.Date
FROM (
		SELECT a.AccountId
				,gi.Date
				,gi.CachedIndexes 
		FROM AccountingCommon.Account a
		JOIN AccountingCommon.UsageObject uo ON uo.AccountId = a.AccountId
		JOIN AccountingCommon.Point p ON p.UsageObjectId = uo.UsageObjectId
		JOIN AccountingCommon.UsageCalculationMethod ucm ON ucm.PointId = p.PointId
				AND ucm.DateTo = '20790606' AND ucm.Generation = 0 	
		LEFT JOIN (SELECT UsageCalculationMethodId, Date, CachedIndexes, IsForCalculate, GroupIndexSourceId
				,ROW_NUMBER() OVER (PARTITION BY UsageCalculationMethodId ORDER BY Date DESC) AS rw
				FROM AccountingMeasuring.GroupIndex) gi ON gi.UsageCalculationMethodId = ucm.UsageCalculationMethodId
				AND gi.IsForCalculate=1
				--AND ISNULL(gi.GroupIndexSourceId,0) NOT IN (18)
				AND gi.rw = 1
		WHERE a.DateTo = '2079-06-06' -- тільки незакриті ОР
	)s 
WHERE s.AccountId = [@tbl$cok$].AccountId

SELECT zipcode AS [Індекс]
		,name_city AS [Нас.пункт]
		,adresa AS [Адреса абонента]
		--,AccountNumber AS [особовий рахунок]
		,AccountNumberNew AS [особовий рахунок]
		,pip AS [ПІП абонента]
		,suma_pay AS [сума до оплати]
		,pokaz AS [Показ]
FROM @tbl$cok$
WHERE (@zip_code = '' OR ZipCode = @zip_code)
		AND suma_pay >= @sum_pay 
ORDER BY zipcode, name_city, adresa