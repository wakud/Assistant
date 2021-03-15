DROP TABLE IF EXISTS #OsRahList
/**********************************************************************************************
*  Друк довідок на субсидію
*  Вхідний параметр - список ОР через кому
**********************************************************************************************/

CREATE TABLE #OsRahList (
	  AccountId INT
	, AccountNumber BIGINT
	, AccountNumberNew BIGINT
	, PIP VARCHAR(MAX)
	, FullAddress VARCHAR(max)
	, Pip_Pilg VARCHAR(MAX)
	, Pilg_category VARCHAR(MAX)
	, BeneficiaryQuantity INT
	, TariffGroupId SMALLINT
	, DateFrom SMALLDATETIME
	, DateTo SMALLDATETIME
	, Price DECIMAL(10, 2)
	, ShortName VARCHAR(40) COLLATE Ukrainian_CI_AS
	, TariffGroupName VARCHAR(400) COLLATE Ukrainian_CI_AS
	, MaxTariffLimit INT
	, Id TINYINT
	, TimeZone INT
	, IsHeating INT
	, Discount INT
	, DiscountKoeff DECIMAL(18, 6)
	, PricePDV DECIMAL(10, 2)
	, GVP VARCHAR(MAX)
	, CPGV VARCHAR(MAX)
	, MinValue INT
	, MaxValue INT
	, IncrementValue INT
	, Borg DECIMAL (10,2)
	, RegisteredQuantity INT
	, QuantityTo DECIMAL(18, 5)
	, SanNormaSubsKwt INT
	, SanNormaSubsGrn DECIMAL (10,2)
	, QuantityToGrn DECIMAL (10,2)
	, nm_pay DECIMAL (10, 2)
)

INSERT #OsRahList (
			AccountId 
			, AccountNumber
			, AccountNumberNew
			, PIP
			, FullAddress
			, Pip_Pilg
			, Pilg_category
			, BeneficiaryQuantity
			, TimeZone
			, IsHeating
			, Discount
			, DiscountKoeff
			, GVP
			, CPGV
			, MinValue
			, MaxValue
			, IncrementValue
			, RegisteredQuantity
			)
SELECT a.AccountId
		, a.AccountNumber
		, a.AccountNumberNew
		, CASE 
			WHEN so.AccountId IS NOT NULL
			THEN ISNULL(so.LastName,'')+' '+ISNULL(so.FirstName,'')+' '+ISNULL(so.SecondName,'')
			ELSE pp.LastName+' '+pp.FirstName+' '+pp.SecondName
		  END AS pip
		, ad.FullAddress
		, ppp.LastName+' '+ppp.FirstName+' '+ppp.SecondName AS pip_pilg
		, bc.Name AS pilg_category_name
		, stm.BeneficiaryQuantity
		, ucm.TimeZonalId
		, stm.IsHeating
		, stm.Discount
		, stm.Discount/100.00 AS DiscountKoeff
		, CASE 
			WHEN EXISTS (SELECT * FROM Dictionary.ClassifierGroup cf
						JOIN AccountingCommon.ClassifierGroupAccount cfa ON cf.ClassifierGroupId = cfa.ClassifierGroupId
						AND cf.Guid = 'FD5547AA-3F35-4A97-94F8-541770E26FC4'
						AND cfa.AccountId = a.AccountId) 
						OR stm.HasGasWaterHeater>0 THEN 'Наявний газовий водонагрівальний прилад' 
			ELSE NULL 
			END AS GVP
		, CASE 
			WHEN EXISTS (SELECT * FROM Dictionary.ClassifierGroup cf
						JOIN AccountingCommon.ClassifierGroupAccount cfa ON cf.ClassifierGroupId = cfa.ClassifierGroupId
						AND cf.Guid = 'B11D8441-6FC5-46A2-99E0-90DA1A6AA591'
						AND cfa.AccountId = a.AccountId) 
				OR stm.HasHotWater>0 THEN 'Наявне централізоване постачання гарячої води' 
			ELSE NULL 
			END AS CPGV
		, CASE
            WHEN stm.TariffGroupId IN (2,4,8,10)
            THEN CASE
                    WHEN stm.HasHotWater = 1
                    THEN 165
                    ELSE 195
                END
            ELSE CASE
                    WHEN stm.HasCentralizedWaterSupply = 1 AND stm.HasHotWater = 0 AND stm.HasGasWaterHeater = 0
                    THEN 150
                    ELSE 105
                END
        END AS MinValue
		, CASE
            WHEN stm.TariffGroupId IN (2,4,8,10)
            THEN CASE
                    WHEN stm.HasHotWater = 1
                    THEN 345
                    ELSE 375
                END
            ELSE CASE
                    WHEN stm.HasCentralizedWaterSupply = 1 AND stm.HasHotWater = 0 AND stm.HasGasWaterHeater = 0
                    THEN 330
                    ELSE 285
                END
        END AS MaxValue
		, 30 AS IncrementValue
		, uo.RegisteredQuantity
FROM AccountingCommon.Account a
JOIN AccountingCommon.Address ad ON ad.AddressId = a.AddressId
JOIN AccountingCommon.PhysicalPerson pp ON pp.PhysicalPersonId = a.PhysicalPersonId
JOIN AccountingCommon.UsageObject uo ON uo.AccountId = a.AccountId
JOIN AccountingCommon.Point p ON uo.UsageObjectId = p.UsageObjectId
JOIN AccountingCommon.UsageCalculationMethod ucm ON ucm.PointId = p.PointId
     AND ucm.DateTo = '2079-06-06' AND ucm.Generation = 0
JOIN (
        SELECT tm.PointId,
        (SELECT TOP 1 BenefitsLimitId
        FROM AccountingDictionary.BenefitsLimit bl
        WHERE bl.TariffGroupId = tm.TariffGroupId
        AND bl.BenefitsCategoryId  = tmi.BenefitsCategoryId
        AND DateFrom<=tm.DateFrom AND DateTo>tm.DateFrom
        AND (HasHotWater IS NULL OR HasHotWater=tm.HasHotWater OR HasHotWater = tm.HasGasWaterHeater)
        ORDER BY MinValue) BenefitsLimitId,
        tm.BeneficiaryQuantity,tm.HasGasWaterHeater,tm.HasHotWater,tmi.BenefitsCategoryId,tmi.Discount,tmi.QuantityTo,
        tm.TariffGroupId,tg.Name AS TariffGroupName,tm.BenefitsCertificateId,tm.HasCentralizedWaterSupply,
        tm.isHeating
        FROM AccountingCommon.TarifficationMethod tm 
        JOIN Dictionary.TariffGroup tg ON tg.TariffGroupId = tm.TariffGroupId
        JOIN AccountingCommon.TarifficationMethodItem tmi ON tm.TarifficationMethodId = tmi.TarifficationMethodId
        AND tmi.QuantityFrom=0
        AND tmi.IsForHeatingSeason=0
        AND tm.DateTo=convert(DATETIME,'6/6/2079',103)
) stm ON stm.PointId   = p.PointId
JOIN AccountingDictionary.BenefitsCategory bc ON bc.BenefitsCategoryId = stm.BenefitsCategoryId
LEFT JOIN AccountingDictionary.BenefitsLimit bl ON bl.BenefitsLimitId= stm.BenefitsLimitId
LEFT JOIN AccountingCommon.BenefitsCertificate bfc ON bfc.BenefitsCertificateId = stm.BenefitsCertificateId
LEFT JOIN AccountingCommon.PhysicalPerson ppp ON ppp.PhysicalPersonId = bfc.PhysicalPersonId
LEFT JOIN (
            SELECT *,ROW_NUMBER() OVER (PARTITION BY AccountId ORDER BY AccountId,DateFrom DESC) AS id
            FROM SupportDefined.SubsOrendar
        ) so ON so.AccountId = a.AccountId AND so.id =1
WHERE AccountNumber in (@OsRahList)

-- заборгованість 3-місячної давності
UPDATE g SET g.Borg = ISNULL(b.RestSumm,0)
FROM #OsRahList g JOIN (
    SELECT o.AccountId, SUM(o.RestSumm) RestSumm
    FROM FinanceMain.Operation o JOIN #OsRahList g1 ON g1.AccountId = o.AccountId
    WHERE o.PeriodTo=207906
        AND o.IsIncome=0
        AND o.RestSumm>0
        AND o.Date<=DATEADD(mm,-3,GETDATE())
    GROUP BY o.AccountId
) b ON b.AccountId = g.AccountId;

UPDATE #OsRahList
SET TariffGroupId = s.TariffGroupId
	, DateFrom = s.DateFrom
	, DateTo = s.DateTo
	, Price = s.Price
	, ShortName = s.ShortName
	, TariffGroupName = s.TariffGroupName
	, MaxTariffLimit = s.MaxTariffLimit
	, Id = s.id
	, PricePDV = s.PricePDV
	, QuantityTo = s.QuantityTo
FROM (
		SELECT tg.TariffGroupId,t.DateFrom,t.DateTo,t.price,tbl.shortname,tg.Name AS TariffGroupName,
			CASE WHEN tbl.shortname LIKE '%до 100%' THEN 100
			ELSE 99999
			END AS MaxTariffLimit,
			ROW_NUMBER() OVER (PARTITION BY tg.TariffGroupId ORDER BY tbl.TarifficationBlockLineId ASC) id,
			a.AccountId
			, t.Price*1.2 AS PricePDV
			, at.QuantityTo
		FROM AccountingCommon.Account a
		JOIN AccountingCommon.UsageObject uo ON uo.AccountId = a.AccountId
		JOIN AccountingCommon.Point p ON p.UsageObjectId = uo.UsageObjectId
		JOIN AccountingCommon.TarifficationMethod tm ON tm.PointId = p.PointId
		JOIN Dictionary.Tariff t ON t.TariffGroupId = tm.TariffGroupId
		JOIN Dictionary.TariffGroup tg ON t.TariffGroupId = tg.TariffGroupId
		JOIN Dictionary.TarifficationBlockLine tbl ON t.TarifficationBlockLineId = tbl.TarifficationBlockLineId
		JOIN Dictionary.TarifficationBlock tb ON tb.TarifficationBlockId = tbl.TarifficationBlockId
		JOIN Dictionary.TariffGroupTarifficationBlock tbb ON tbb.TariffGroupId = tg.TariffGroupId
			AND tbb.TarifficationBlockId = tb.TarifficationBlockId
			AND tbb.DateFrom < t.DateTo AND tbb.DateTo > t.DateFrom
		JOIN SupportDefined.[_AccountCategoryByOpenTM] at ON at.AccountId = a.AccountId
		WHERE t.Is30km=0
			AND t.IsAPK=0
			AND t.IsHighlander=0
			AND t.IsHeating=0
			AND t.TimeZoneId=1
			AND t.DateFrom='20210101'
			AND a.AccountNumber IN (@OsRahList)
) AS s
WHERE #OsRahList.AccountId = s.AccountId

UPDATE #OsRahList
SET SanNormaSubsKwt = 
    CASE 
        WHEN MinValue+(IncrementValue*(RegisteredQuantity-1))<MinValue THEN MinValue
        WHEN MinValue+(IncrementValue*(RegisteredQuantity-1))>MaxValue THEN MaxValue
        ELSE MinValue+(IncrementValue*(RegisteredQuantity-1))
    END

UPDATE #OsRahList
SET SanNormaSubsGrn = 
	CASE WHEN SanNormaSubsKwt > MaxTariffLimit THEN convert(DECIMAL(10,2),MaxTariffLimit*PricePDV)
            +convert(DECIMAL(10,2),(SanNormaSubsKwt - MaxTariffLimit) * ISNULL(Price*1.2,PricePDV))
		 ELSE convert(DECIMAL(10,2),SanNormaSubsKwt*PricePDV)
    END

UPDATE #OsRahList
SET QuantityToGrn = 
        CASE
        WHEN DiscountKoeff <> 0 THEN -- було DiscountKoeff<>1 !!
            CASE 
            WHEN QuantityTo > SanNormaSubsKwt THEN 
                CASE WHEN SanNormaSubsKwt > MaxTariffLimit THEN convert(DECIMAL(10,2),MaxTariffLimit * PricePDV * DiscountKoeff)
                    +convert(DECIMAL(10,2),(SanNormaSubsKwt-MaxTariffLimit)*ISNULL(Price*1.2,PricePDV)*DiscountKoeff)
                ELSE convert(DECIMAL(10,2),SanNormaSubsKwt*PricePDV*DiscountKoeff)
                END
            ELSE 
                CASE WHEN QuantityTo>MaxTariffLimit THEN convert(DECIMAL(10,2),MaxTariffLimit*PricePDV*DiscountKoeff)
                    +convert(DECIMAL(10,2),(QuantityTo-MaxTariffLimit)*ISNULL(Price*1.2,PricePDV)*DiscountKoeff)
                ELSE convert(DECIMAL(10,2),QuantityTo*PricePDV*DiscountKoeff)
                END 
            END
        ELSE 0 
        END

UPDATE #OsRahList
SET nm_pay = SanNormaSubsGrn-QuantityToGrn

SELECT * FROM #OsRahList