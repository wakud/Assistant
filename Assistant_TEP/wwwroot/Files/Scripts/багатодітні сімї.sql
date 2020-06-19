SELECT	DISTINCT (acc.AccountNumber) as '�� ���'
		,pers.FullName AS 'ϲ�'
		,bc.Name as '��� �����'
		,bcc.BCSeries+'/'+bcc.BCNumber as '� ����������'
		,bcc.DateTo AS '���� ���������'
FROM AccountingCommon.Account as acc --'�� ���'
    JOIN AccountingCommon.PhysicalPerson as pers ON pers.PhysicalPersonId =acc.PhysicalPersonId --'ϲ�'
    JOIN AccountingCommon.Address as ad ON ad.AddressId = acc.AddressId --'������'
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
	JOIN AccountingDictionary.BenefitsCategory bc ON tm.BenefitsCategoryId =bc.BenefitsCategoryId --'��� �����', '������'
	JOIN AccountingCommon.BenefitsCertificate bcc ON bcc.BenefitsCertificateId =tm.BenefitsCertificateId --'� ����������'
    JOIN AccountingCommon.PhysicalPerson ppp ON bcc.PhysicalPersonId =ppp.PhysicalPersonId --'ϲ� ���������', '���������_���'
	WHERE bc.MinistryCode = 35 AND bcc.DateTo <= @stanom_na
	ORDER BY 1,5