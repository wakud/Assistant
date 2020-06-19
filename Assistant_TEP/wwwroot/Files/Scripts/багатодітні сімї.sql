SELECT	DISTINCT (acc.AccountNumber) as 'ОС рах'
		,pers.FullName AS 'ПІБ'
		,bc.Name as 'вид пільги'
		,bcc.BCSeries+'/'+bcc.BCNumber as '№ посвідчення'
		,bcc.DateTo AS 'Дата закінчення'
FROM AccountingCommon.Account as acc --'ОС рах'
    JOIN AccountingCommon.PhysicalPerson as pers ON pers.PhysicalPersonId =acc.PhysicalPersonId --'ПІБ'
    JOIN AccountingCommon.Address as ad ON ad.AddressId = acc.AddressId --'Адреса'
    JOIN AccountingCommon.UsageObject as uo ON uo.AccountId = acc.AccountId
    JOIN AccountingCommon.Point as p ON p.UsageObjectId = uo.UsageObjectId
    JOIN (SELECT tm.PointId
				,tm.BenefitsCertificateId
				,tm.TariffGroupId
				,tmi.BenefitsCategoryId
				,ROW_NUMBER() OVER (PARTITION BY PointId ORDER BY PointId,tm.DateFrom desc,tm.DateTo desc) AS id
				,tm.HasCentralizedWaterSupply
				,tm.HasGasWaterHeater
				,tm.HasHotWater
			FROM AccountingCommon.TarifficationMethod as tm 
			JOIN AccountingCommon.TarifficationMethodItem tmi ON tm.TarifficationMethodId = tmi.TarifficationMethodId) tm 
				ON tm.PointId = p.PointId /*AND tm.id = 2*/
	JOIN AccountingDictionary.BenefitsCategory bc ON tm.BenefitsCategoryId =bc.BenefitsCategoryId --'вид пільги', 'Знижка'
	JOIN AccountingCommon.BenefitsCertificate bcc ON bcc.BenefitsCertificateId =tm.BenefitsCertificateId --'№ посвідчення'
    JOIN AccountingCommon.PhysicalPerson ppp ON bcc.PhysicalPersonId =ppp.PhysicalPersonId --'ПІБ пільговика', 'ідентифік_код'
	WHERE bc.MinistryCode = 35 AND bcc.DateTo <= @stanom_na
	ORDER BY 1,5