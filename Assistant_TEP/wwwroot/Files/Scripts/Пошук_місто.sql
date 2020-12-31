DECLARE @ExBill INT 
SET @ExBill =       --Термін погашення боргу по рахунках
            	(SELECT TOP 1 cast([Value] as int)  FROM [Services].[Setting] WHERE [Guid] = '826C4666-F79C-4558-A0BB-2D5A428FCE1B')
				
DECLARE @vykl TABLE (
		[Код ЦОК] SMALLINT,
		[особовий] VARCHAR(10),
		[accId] INT,
		[ПІП] VARCHAR(292),
		[Прізвище] VARCHAR(150),
		[Ім'я] VARCHAR(70),
		[По батькові] VARCHAR(70),
		[Повна адреса] VARCHAR(300),
		[Ідент.код] VARCHAR(10),
		[Паспорт] VARCHAR(12),
		[Моб.тел] VARCHAR(100),
		[UtilityAddressId] INT,
		[Індекс] CHAR(5),
		[область] VARCHAR(40),
		[район] VARCHAR(40),
		[тип пункту] VARCHAR(400),
		[тип н.п.] VARCHAR(40),
		[Напрямок] VARCHAR(50),
		[Нас.пункт] VARCHAR(50),
		--[Код нас.пункту] SMALLINT,
		[тип в] VARCHAR(10),
		[тип вул] VARCHAR(50),
		[вулиця] VARCHAR(50),
		[будинок] VARCHAR(9),
		[корпус] VARCHAR(10),
		[квартира] VARCHAR(5),
		[сума боргу] DECIMAL(10,2),
		[ост. дата опл.] DATE,
		[дата викл.] DATE,
		[дата попер.] DATE,
		[№ лічильника] VARCHAR(40),
		[ЕІС] VARCHAR(16)
		)

--Вибираємо коли абонента відключили
INSERT INTO @vykl ([accId], [Код ЦОК], [особовий],[ПІП], [Прізвище], [Ім'я], [По батькові], [Ідент.код], [Паспорт], [Моб.тел], [дата викл.])
SELECT	
		  a.AccountId
		, a.OrganizationUnitId
		, a.AccountNumber
		, pp.FullName
		, pp.LastName
		, pp.FirstName
		, pp.SecondName
		, pp.IdentificationCode
		, pp.PassportSeries +' '+ pp.PassportNumber
		, pp.MobilePhoneNumber
		, d.DateFrom
	FROM AccountingCommon.Account a
	JOIN AccountingCommon.PhysicalPerson pp ON pp.PhysicalPersonId = a.PhysicalPersonId
	JOIN AccountingCommon.UsageObject uo ON uo.AccountId = a.AccountId
	JOIN AccountingCommon.Point p ON p.UsageObjectId = uo.UsageObjectId
		AND p.DateTo=convert(DATETIME,'06/06/2079',103)
	JOIN AccountingCommon.Disconnection d ON p.PointId = d.PointId AND d.DisconnectionStatus=1
	JOIN (
			SELECT
				 d2.[DisconnectionId],
				ROW_NUMBER() OVER (PARTITION BY d2.PointId ORDER BY d2.DateFrom DESC) AS RowNumber
			FROM [AccountingCommon].[Disconnection] as d2
		)
			AS dtemp ON dtemp.[DisconnectionId] = d.[DisconnectionId] AND dtemp.RowNumber = 1
	WHERE a.DateTo = '2079-06-06' -- тільки незакриті ОР
-- заповнюємо адресу
UPDATE @vykl
SET [Повна адреса] = s.[Повна адреса], [UtilityAddressId] = s.AddressId, [Індекс] = s.Індекс, [область] = s.[область], район = s.район, [тип пункту] = s.[тип пункту]
	, [тип н.п.] = s.[тип н.п.], [Напрямок] = s.Напрямок, [Нас.пункт] = s.[Нас.пункт], /*[Код нас.пункту] = s.[Код нас.пункту],*/ [тип в] = s.[тип в], [тип вул] = s.[тип вул], вулиця = s.вулиця
	, будинок = s.будинок, корпус = s.корпус, квартира = s.квартира
FROM (
		SELECT a.AccountId
				, addr.FullAddress AS [Повна адреса]
				, addr.AddressId
				, z.ZipCode AS [Індекс]
				,'Тернопільська' AS [область]
				,'Тернопільський' AS [район]
				, ct.Name AS [тип пункту],
				ct.ShortName AS [тип н.п.],
				napr.ShortName AS [Напрямок],
				c.Name AS [Нас.пункт],
				--c.CityId AS [Код нас.пункту],
				st.ShortName AS [тип в],
				st.Name AS [тип вул],
				s.Name AS [вулиця],
				addr.Building AS [будинок],
				addr.BuildingPart AS [корпус],
				addr.Apartment AS [квартира]
		FROM AccountingCommon.Account a
		LEFT JOIN AccountingCommon.Address addr ON addr.AddressId = a.AddressId
		LEFT JOIN [TR_Organization].[AddressDictionary].[Zip] AS z ON z.ZipId = addr.ZipId
		LEFT JOIN [TR_Organization].[AddressDictionary].[City] AS c ON c.CityId = addr.CityId
		LEFT JOIN [TR_Organization].[AddressDictionary].[CityType] AS ct ON ct.CityTypeId = c.CityTypeId
		LEFT JOIN [TR_Organization].[AddressDictionary].Region AS r ON r.RegionId = c.RegionId
		LEFT JOIN [TR_Organization].[AddressDictionary].[District] AS ad ON ad.DistrictId = c.DistrictId
		LEFT JOIN [TR_Organization].[AddressDictionary].[Street] AS s ON s.StreetId = addr.StreetId
		LEFT JOIN [TR_Organization].[AddressDictionary].[StreetType]AS st ON st.StreetTypeId = s.StreetTypeId
		JOIN [AccountingCommon].[ClassifierGroupAccount] cl on cl.[AccountId]=a.AccountId  -- для зв'язки, щоб вивести напрямок місто
	JOIN [Dictionary].[ClassifierGroup] napr ON  napr.ClassifierGroupId = cl.ClassifierGroupId and napr.[ClassifierId] = 5 -- виводим напрямок міста Тернопіль
		) AS s
WHERE accId = s.AccountId
-- заповнюємо борг
UPDATE @vykl
SET [сума боргу] = s.RestSumm
FROM (
		SELECT a.accId
				, o.RestSumm
		FROM @vykl a
		JOIN (SELECT o.AccountId,SUM(o.RestSumm) RestSumm
				FROM FinanceMain.Operation o
				WHERE PeriodTo=207906
						AND IsIncome=0
						AND DocumentTypeId IN (15)
						AND o.Date<=DATEADD(dd,-@ExBill,GETDATE())
				GROUP BY o.AccountId
			) o ON a.accId = o.AccountId
	) AS s
WHERE [@vykl].accId = s.accId
--заповнюємо № лічильника і ЕІС
UPDATE @vykl
SET [№ лічильника] = s.CounterNumber, ЕІС = s.ЕІС
FROM (
		SELECT v.accId
				, cch.CounterNumber CounterNumber
				, p.EIC AS [ЕІС]
		FROM @vykl v
		JOIN AccountingCommon.UsageObject uo ON uo.AccountId = v.accId
		JOIN AccountingCommon.Point p ON p.UsageObjectId = uo.UsageObjectId
		JOIN AccountingCommon.UsageCalculationMethod AS ucm ON ucm.PointId = p.PointId 
		JOIN Counter.CounterMeasuring cm ON ucm.CounterMeasuringId=cm.CounterMeasuringId AND ucm.DateTo = '2079-06-06'
		JOIN Counter.CounterHistory cch ON cch.CounterHistoryId=cm.CounterHistoryId AND cch.DateTo='2079-06-06'
	) AS s
WHERE [@vykl].accId = s.accId
--заповнюємо останню оплату
UPDATE @vykl
SET [ост. дата опл.] = s.PayDate
FROM (
		SELECT v.accId
				,MAX(r.PayDate) AS PayDate
	FROM @vykl v
	LEFT JOIN FinanceCommon.Receipt r ON r.AccountId = v.accId
	WHERE r.IsDeleted=0
	AND r.BillDocumentTypeId IN (15,8,16,14)--Solenko20190515
	GROUP BY v.accId
	HAVING DATEDIFF(mm,MAX(r.PayDate),GETDATE())>=0
	) AS s
WHERE [@vykl].accId = s.accId
-- заповнюємо коли видане останнє попередження
UPDATE @vykl
SET [дата попер.] = s.DeliverDate
FROM (
		SELECT v.accId
				, dw.DeliverDate
		FROM @vykl v
		LEFT JOIN LawCommon.DisconnectWarning dw ON dw.AccountId = v.accId
		WHERE dw.Date = (SELECT MAX(date) 
						 FROM LawCommon.DisconnectWarning
						 WHERE AccountId = dw.AccountId
						 AND IsActive=1
						 AND IsDelivered=1
						 )
			AND dw.Summ-dw.UsedSumm>0
	) AS s
WHERE [@vykl].accId = s.accId

SELECT * FROM @vykl
