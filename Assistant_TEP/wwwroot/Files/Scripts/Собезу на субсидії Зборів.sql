DECLARE @ExBill INT 
SET @ExBill =       --Термін погашення боргу по рахунках
	(SELECT TOP 1 cast([Value] as int)  FROM [Services].[Setting] WHERE [Guid] = '826C4666-F79C-4558-A0BB-2D5A428FCE1B')  

DECLARE @ElectroOpalennya BIT; SET @ElectroOpalennya = 1; -- ознака наявності електроопалення
DECLARE @ElectroPlyty BIT; SET @ElectroPlyty = 1; -- ознака тарифу "електроплити"

DECLARE @sobes TABLE (
						AccountId INT,
						Rash INT,
						Numb CHAR(40),
						Fio CHAR(100),
						Name_v CHAR(50),
						Bld CHAR(9),
						Corp CHAR(10),
						Flat CHAR(5),
						Nazva CHAR(70),
						Tariff CHAR(40),
						Discount DECIMAL(20,5),
						Pilgovuk CHAR(100),
						Gar_voda DECIMAL(20,5),
						Gaz_vn DECIMAL(20,5),
						El_opal DECIMAL(20,5),
						Kilk_pilg CHAR(2),
						T11_cod_na CHAR(40),
						Orendar CHAR(40),
						Borg DECIMAL(20,5)
					)

--вибираємо всіх абонентів з адресою
INSERT @sobes (
				AccountId,
				Rash,
				Fio,
				Name_v,
				Bld,
				Corp,
				Flat
			  )

SELECT  acc.AccountId
		, acc.AccountNumber
		, pp.FullName
		--, c.Name
		, s.Name
		, ad.Building
		, ad.BuildingPart
		, ad.Apartment
FROM AccountingCommon.Account acc
LEFT JOIN AccountingCommon.PhysicalPerson pp ON pp.PhysicalPersonId = acc.PhysicalPersonId
LEFT JOIN AccountingCommon.Address ad ON ad.AddressId = acc.AddressId
LEFT JOIN [TR_Organization].AddressDictionary.City c ON c.CityId = ad.CityId
LEFT JOIN [TR_Organization].[AddressDictionary].[Street] AS s ON s.StreetId = ad.StreetId
WHERE acc.DateTo = convert(DATETIME,'6/6/2079',103) 
	AND (c.Name LIKE '%Кобзарівка%' OR c.Name LIKE '%Вертелка%' OR c.Name LIKE '%Курівці%'
		OR c.Name LIKE '%Малашівці%' OR c.Name LIKE '%Іванківці%' OR c.Name LIKE '%Чернихів%'
		OR c.Name LIKE '%Глядки%' OR c.Name LIKE '%Плесківці%' OR c.Name LIKE '%Городище%'
		OR c.Name LIKE '%Носівці%'
	)

--проставляємо ЖКГ блага
UPDATE @sobes
SET El_opal = s.IsHeating, Gar_voda = s.HasHotWater, Gaz_vn = s.HasGasWaterHeater, Nazva = s.[typ nas]
FROM(
		SELECT acc.AccountId
				, tm.IsHeating	-- наявність електроопалення
				, tm.HasGasWaterHeater	-- наявність газового водонагрівача
				, tm.HasHotWater	--наявність гарячої центральної води
				, tg.Name AS [typ nas]	--тип населення(місто чи село)
		FROM @sobes acc
		LEFT JOIN AccountingCommon.UsageObject as uo ON uo.AccountId = acc.AccountId
		LEFT JOIN AccountingCommon.Point as p ON p.UsageObjectId = uo.UsageObjectId
		LEFT JOIN AccountingCommon.TarifficationMethod tm ON tm.PointId = p.PointId
		LEFT JOIN AccountingCommon.TarifficationMethodItem tmi ON tmi.TarifficationMethodId = tm.TarifficationMethodId
		LEFT JOIN Dictionary.TariffGroup tg ON tg.TariffGroupId = tm.TariffGroupId
		--WHERE @ElectroPlyty = 1 AND tg.TariffGroupId IN (2, 4, 8, 10)
) AS s
WHERE [@sobes].AccountId = s.AccountId

--проставляємо основний тариф
UPDATE @sobes
SET Tariff = s.[вид пільги]
FROM(
		SELECT acc.AccountId
				, bc.Name AS [вид пільги]	--категорія пільги
		FROM @sobes acc
		LEFT JOIN AccountingCommon.UsageObject as uo ON uo.AccountId = acc.AccountId
		LEFT JOIN AccountingCommon.Point as p ON p.UsageObjectId = uo.UsageObjectId
		LEFT JOIN AccountingCommon.TarifficationMethod tm ON tm.PointId = p.PointId
		LEFT JOIN AccountingCommon.TarifficationMethodItem tmi ON tmi.TarifficationMethodId = tm.TarifficationMethodId
		LEFT JOIN AccountingDictionary.BenefitsCategory bc ON bc.BenefitsCategoryId = tmi.BenefitsCategoryId			--'вид пільги', 'Знижка'
		WHERE bc.Code =1
) AS s
WHERE [@sobes].AccountId = s.AccountId

--проставляємо пільговий тариф
UPDATE @sobes
SET Pilgovuk = s.[ПІБ пільговика], Tariff = s.[вид пільги], Discount = s.dis, Kilk_pilg = s.kt
FROM(
		SELECT acc.AccountId
				, bc.Name AS [вид пільги]	--категорія пільги
				, bc.Discount AS dis	--знижка по категоріїї пільги
				, ppp.LastName+' '+ppp.FirstName+' '+ppp.SecondName AS [ПІБ пільговика]
				, COUNT(DISTINCT bcc.BenefitsCertificateId) kt	--к-ть пільговиків
		FROM @sobes acc
		LEFT JOIN AccountingCommon.UsageObject as uo ON uo.AccountId = acc.AccountId
		LEFT JOIN AccountingCommon.Point as p ON p.UsageObjectId = uo.UsageObjectId
		LEFT JOIN AccountingCommon.TarifficationMethod tm ON tm.PointId = p.PointId
		LEFT JOIN AccountingCommon.TarifficationMethodItem tmi ON tmi.TarifficationMethodId = tm.TarifficationMethodId
		LEFT JOIN AccountingDictionary.BenefitsCategory bc ON bc.BenefitsCategoryId = tmi.BenefitsCategoryId			--'вид пільги', 'Знижка'
		LEFT JOIN AccountingCommon.BenefitsCertificate bcc ON bcc.BenefitsCertificateId =tm.BenefitsCertificateId		--'№ посвідчення'
		LEFT JOIN AccountingCommon.PhysicalPerson ppp ON bcc.PhysicalPersonId =ppp.PhysicalPersonId						--'ПІБ пільговика', 'ідентифік_код'
		WHERE bc.Code in (71, 129, 93, 35, 36, 131, 28) AND bcc.DateTo = convert(DATETIME,'6/6/2079',103)
		GROUP BY acc.AccountId, bc.Name, bc.Discount, ppp.LastName+' '+ppp.FirstName+' '+ppp.SecondName
) AS s
WHERE [@sobes].AccountId = s.AccountId

UPDATE @sobes
SET T11_cod_na = s.t11
FROM(
		SELECT acc.AccountId
				, tm.HasHotWater
				, 'електроенергія без стац. ел\пл, село' AS t11
		FROM @sobes acc
		LEFT JOIN AccountingCommon.UsageObject as uo ON uo.AccountId = acc.AccountId
		LEFT JOIN AccountingCommon.Point as p ON p.UsageObjectId = uo.UsageObjectId
		LEFT JOIN AccountingCommon.TarifficationMethod tm ON tm.PointId = p.PointId
		LEFT JOIN AccountingCommon.TarifficationMethodItem tmi ON tmi.TarifficationMethodId = tm.TarifficationMethodId
		LEFT JOIN Dictionary.TariffGroup tg ON tg.TariffGroupId = tm.TariffGroupId
		WHERE tg.TariffGroupId NOT IN (2, 4, 8, 10)
) AS s
WHERE [@sobes].AccountId = s.AccountId

UPDATE @sobes
SET T11_cod_na = s.t11
FROM(
		SELECT acc.AccountId
				, tm.HasHotWater
				, (CASE WHEN tm.HasHotWater = 1 THEN 'електроенергія з ст.ел\пл, з цент.г.вп.' ELSE 'електроенергія з ст.ел\пл, без г.вп.' END ) AS t11
		FROM @sobes acc
		LEFT JOIN AccountingCommon.UsageObject as uo ON uo.AccountId = acc.AccountId
		LEFT JOIN AccountingCommon.Point as p ON p.UsageObjectId = uo.UsageObjectId
		LEFT JOIN AccountingCommon.TarifficationMethod tm ON tm.PointId = p.PointId
		LEFT JOIN AccountingCommon.TarifficationMethodItem tmi ON tmi.TarifficationMethodId = tm.TarifficationMethodId
		LEFT JOIN Dictionary.TariffGroup tg ON tg.TariffGroupId = tm.TariffGroupId
		WHERE @ElectroPlyty = 1 AND tg.TariffGroupId IN (2, 4, 8, 10)
) AS s
WHERE [@sobes].AccountId = s.AccountId

UPDATE @sobes
SET Borg = s.RestSumm
FROM(
		SELECT a.AccountId
				,SUM(o.RestSumm) RestSumm 
		FROM FinanceMain.Operation o
		JOIN @sobes a ON a.AccountId = o.AccountId
		WHERE PeriodTo=207906
			AND IsIncome=0
			AND DocumentTypeId IN (15)
			AND o.RestSumm > 0
			AND o.Date<=DATEADD(mm,-2,GETDATE())
			GROUP BY a.AccountId
		HAVING SUM(o.RestSumm) > 340
)AS s
WHERE [@sobes].AccountId = s.AccountId 

UPDATE @sobes
SET Discount = ISNULL(Discount, 0), Pilgovuk = ISNULL(Pilgovuk, ''), Kilk_pilg = ISNULL(Kilk_pilg, 0), Orendar = ISNULL(Orendar, ''), Borg = ISNULL(Borg, 0)

SELECT * FROM @sobes
