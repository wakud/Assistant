SELECT	a.OrganizationUnitId AS [��� ���]
		, a.AccountNumber AS [��������]
		, pp.FullName AS [ϲ�]
		, pp.LastName AS [�������]
		, pp.FirstName AS [��'�]
		, pp.SecondName AS [�� �������]
		, addr.FullAddress AS [����� ������]
		, z.ZipCode AS [������]
		, ad.Name AS [�������],
		r.Name AS [�����],
		ct.Name AS [��� ������],
		ct.ShortName AS [��� �.�.],
		c.Name AS [���.�����],
		st.ShortName AS [��� �],
		st.Name AS [��� ���],
		s.Name AS [������],
		addr.Building AS [�������],
		addr.BuildingPart AS [������],
		addr.Apartment AS [��������]
FROM AccountingCommon.Account a
JOIN AccountingCommon.PhysicalPerson pp ON pp.PhysicalPersonId = a.PhysicalPersonId
LEFT JOIN AccountingCommon.Address addr ON addr.AddressId = a.AddressId
LEFT JOIN [TR_Organization].[AddressDictionary].[Zip] AS z ON z.ZipId = addr.ZipId
LEFT JOIN [TR_Organization].[AddressDictionary].[City] AS c ON c.CityId = addr.CityId
LEFT JOIN [TR_Organization].[AddressDictionary].[CityType] AS ct ON ct.CityTypeId = c.CityTypeId
LEFT JOIN [TR_Organization].[AddressDictionary].Region AS r ON r.RegionId = c.RegionId
LEFT JOIN [TR_Organization].[AddressDictionary].[District] AS ad ON ad.DistrictId = c.DistrictId
LEFT JOIN [TR_Organization].[AddressDictionary].[Street] AS s ON s.StreetId = addr.StreetId
LEFT JOIN [TR_Organization].[AddressDictionary].[StreetType]AS st ON st.StreetTypeId = s.StreetTypeId
