--DECLARE @d DATETIME; SET @d=convert(char(8),getdate(),112)
--DECLARE @date_to DATETIME;SET @date_to = dateadd(day,1-day(@d),@d)

DECLARE @pokaz TABLE(
	AccountId INT,
	AccountNumber BIGINT,
	FullName VARCHAR(100),
	FullAddress VARCHAR(150),
	DatePokazu DATE,
	HtoPodavPokaz VARCHAR(100),
	EIC VARCHAR(20),
	RowNumber INT
)

INSERT @pokaz
(
    AccountId,
    AccountNumber,
    FullName,
    FullAddress,
    DatePokazu,
    HtoPodavPokaz,
    EIC,
    RowNumber
)
SELECT acc.AccountId
		, acc.AccountNumber
		, pp.FullName
		, ad.FullAddress
		, CONVERT(DATE, gi.Date, 105) max_Date
		, s.LastName
		, p.EIC
		, ROW_NUMBER() OVER (PARTITION BY acc.AccountId ORDER BY gi.Date DESC) AS rn
FROM AccountingCommon.Account acc
LEFT JOIN AccountingCommon.PhysicalPerson pp ON pp.PhysicalPersonId = acc.PhysicalPersonId
LEFT JOIN AccountingCommon.Address ad ON ad.AddressId = acc.AddressId
JOIN AccountingCommon.UsageObject uo ON uo.AccountId = acc.AccountId 
JOIN AccountingCommon.Point p ON uo.UsageObjectId = p.UsageObjectId
		AND p.DateTo = CONVERT(DATETIME,'06/06/2079',103)
		AND acc.DateTo = CONVERT(DATETIME,'06/06/2079',103)
JOIN AccountingCommon.UsageCalculationMethod ucm ON ucm.PointId = p.PointId
JOIN AccountingMeasuring.GroupIndex gi ON gi.UsageCalculationMethodId = ucm.UsageCalculationMethodId
		AND ucm.DateTo = '20790606' AND ucm.Generation = 0 	
		AND gi.IsForCalculate=1
		--AND ISNULL(gi.GroupIndexSourceId,0) NOT IN (18)  --Не враховувати Розрахункові покази
JOIN Organization.Staff s ON s.StaffId = gi.StaffId
GROUP BY acc.AccountId
		, acc.AccountNumber
		, pp.FullName
		, ad.FullAddress
		, gi.Date
		, s.LastName
		, p.EIC
ORDER BY acc.AccountId

SELECT AccountNumber AS [Особовий]
		, FullName AS [ПІП]
		, FullAddress AS [Адреса]
		, DatePokazu AS [Дата показу]
		, HtoPodavPokaz AS [Хто подав]
		, EIC AS [ЕІС]
FROM @pokaz
WHERE RowNumber = 1 AND DatePokazu < @date_to
ORDER BY Особовий