SELECT	a.OrganizationUnitId AS [Код ЦОК]
		, a.AccountNumber AS [особовий]
		, pp.FullName AS [ПІП]
		, pp.LastName AS [Прізвище]
		, pp.FirstName AS [Ім'я]
		, pp.SecondName AS [По батькові]
		, addr.FullAddress AS [Повна адреса]
		, z.ZipCode AS [Індекс]
		, ad.Name AS [область],
		r.Name AS [район],
		ct.Name AS [тип пункту],
		ct.ShortName AS [тип н.п.],
		c.Name AS [Нас.пункт],
		st.ShortName AS [тип в],
		st.Name AS [тип вул],
		s.Name AS [вулиця],
		addr.Building AS [будинок],
		addr.BuildingPart AS [корпус],
		addr.Apartment AS [квартира]
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
