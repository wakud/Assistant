SELECT DISTINCT	adr.CityId AS KodNasPunktu
				, adc.Name AS NasPunkt
FROM AccountingCommon.Account as acc
    JOIN AccountingCommon.Address Adr WITH ( NOLOCK ) ON Adr.AddressId = acc.AddressId
	LEFT JOIN AddressDictionary.City adc ON adc.CityId = Adr.CityId
	ORDER BY 2