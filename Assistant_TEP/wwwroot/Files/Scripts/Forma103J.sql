SELECT    sett.Value AS [��� ���]
		, c.ContractNumber AS [����� ��������]
		, ct.ShortName AS [������� �����]
		, ct.FullName AS [����� �����]
		, (
			--(CASE WHEN d.Name IS NULL THEN  '' ELSE d.Name END) +
			(CASE WHEN ISNULL(r.Name, r2.Name) IS NULL THEN  '' ELSE r.Name + ' �-�' END) +
			(CASE WHEN ci.Name IS NULL THEN  '' ELSE ', ' + ctp.ShortName + ' ' + ci.Name END) +
			(CASE WHEN s.Name IS NULL THEN  '' ELSE ', ' + st.ShortName + ' ' + s.Name END) +
			(CASE WHEN addr.Building IS NULL THEN  '' ELSE ', ���. ' + addr.Building END) +
			(CASE WHEN addr.Apartment IS NULL THEN  '' ELSE ', ��. ' + addr.Apartment END)
		) AS [����� ������]
		, z.ZipCode AS [������]
		, d.Name AS [�������],
		ISNULL(r.Name, r2.Name) AS [�����],
		ctp.Name AS [��� ������],
		ctp.ShortName AS [��� �.�.],
		ci.Name AS [���.�����],
		st.ShortName AS [��� �],
		st.Name AS [��� ���],
		s.Name AS [������],
		addr.Building AS [�������],
		addr.BuildingPart AS [������],
		addr.Apartment AS [��������]
FROM Contract c
JOIN Contractor ct on ct.ContractorId = c.ContractorId
LEFT JOIN Address addr on addr.AddressId = ct.JuridicalAddressId
LEFT JOIN TR_Organization.AddressDictionary.Zip z on z.ZipId = addr.ZipId
LEFT JOIN TR_Organization.AddressDictionary.City ci on ci.CityId = addr.CityId
LEFT JOIN TR_Organization.AddressDictionary.CityType ctp on ctp.CityTypeId = ci.CityTypeId
LEFT JOIN TR_Organization.AddressDictionary.Street s on s.StreetId = addr.StreetId
LEFT JOIN TR_Organization.AddressDictionary.StreetType st on s.StreetTypeId = st.StreetTypeId
LEFT JOIN TR_Organization.AddressDictionary.District d on ci.DistrictId = d.DistrictId
LEFT JOIN TR_Organization.AddressDictionary.Region r on r.RegionId = ci.RegionId
LEFT JOIN TR_Organization.AddressDictionary.Region r2 on r2.RegionId = s.CityRegionId
JOIN Setting sett on sett.SettingId = 1801
where 1 = 1
	and c.ContractType = 0
	and c.ContractState = 1
