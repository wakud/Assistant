DECLARE @table TABLE (AccountNumberNew BIGINT, RegisteredPerson TINYINT)
$params$

DECLARE @debt TABLE (debet DECIMAL(11,2), accId int)
INSERT @debt
(
    debet,
    accId
)
SELECT 
	debet = ISNULL((
		SELECT SUM(b.RestSumm)
		FROM FinanceCommon.BillRegular b 
		WHERE 1 = 1 
			AND b.AccountId = a.AccountId
			AND b.IsDeleted = 0
			AND b.RestSumm > 0	
			AND b.Date < DATEADD(mm,-3,GETDATE())
		), 0),
	accId = a.AccountId
From @table t 
JOIN AccountingCommon.Account a ON t.AccountNumberNew = a.AccountNumberNew
;
WITH Tariff AS (
SELECT tg.TariffGroupId,t.DateFrom,t.DateTo,t.price,tbl.shortname,tg.Name AS TariffGroupName,
		CASE WHEN tbl.shortname LIKE '%до 100%' THEN 100
		ELSE 99999
		END AS MaxTariffLimit,
		ROW_NUMBER() OVER (PARTITION BY tg.TariffGroupId ORDER BY tbl.TarifficationBlockLineId ASC) id 
	FROM Dictionary.Tariff t 
	JOIN Dictionary.TariffGroup tg ON t.TariffGroupId = tg.TariffGroupId
	JOIN Dictionary.TarifficationBlockLine tbl ON t.TarifficationBlockLineId = tbl.TarifficationBlockLineId
	JOIN Dictionary.TarifficationBlock tb ON tb.TarifficationBlockId = tbl.TarifficationBlockId
	JOIN Dictionary.TariffGroupTarifficationBlock tbb ON tbb.TariffGroupId = tg.TariffGroupId
	    AND tbb.TarifficationBlockId = tb.TarifficationBlockId
        AND tbb.DateFrom < t.DateTo AND tbb.DateTo > t.DateFrom
    WHERE t.Is30km=0
		AND t.IsAPK=0
		AND t.IsHighlander=0
		AND t.IsHeating=0
		AND t.TimeZoneId=1
        AND t.DateFrom='20210101'
)
,a AS (
        SELECT a.AccountId,
                a.AccountNumberNew,
        --opp ознака наявності послуг
        '00100000' AS opp,
        --opl ознака наявності лічильника
        '00100000' AS opl,
        --базовий тариф
        at.BasePrice*1.2 AS taryf_6,
        tm.BeneficiaryQuantity,
        tm.TariffGroupId,
        tbl.RegisteredPerson AS CntPhysicalPerson,
        -- Виправлено значення пільгових лімітів, що вступили в силу з 01.04.2020 згідно карантину
        CASE WHEN tm.TariffGroupId IN (2,4,8,10) 
             THEN CASE WHEN tm.HasHotWater=1
                       THEN 165
                       ELSE 195
                  END
             ELSE CASE WHEN tm.HasCentralizedWaterSupply=1
                            AND tm.HasHotWater=0
                            AND tm.HasGasWaterHeater=0
                       THEN 150
                       ELSE 105
                  END
        END AS MinValue,

        CASE WHEN tm.TariffGroupId IN (2,4,8,10) 
             THEN CASE WHEN tm.HasHotWater=1
                       THEN 345
                       ELSE 375
                  END
             ELSE CASE WHEN tm.HasCentralizedWaterSupply=1
                            AND tm.HasHotWater=0
                            AND tm.HasGasWaterHeater=0
                       THEN 330
                       ELSE 285
                  END
        END AS MaxValue,
        30 AS IncrementValue,
        tt.Price*1.2 AS taryf_61,
        t.MaxTariffLimit,
        at.QuantityTo,
        at.Discount/100.00 AS DiscountKoeff
        , d.debet AS debt -- 2017051 Лемешко
        FROM AccountingCommon.Account a 
        JOIN @debt d ON d.accId = a.AccountId
        JOIN @table tbl ON tbl.AccountNumberNew = a.AccountNumberNew
        JOIN SupportDefined.[_AccountCategoryByOpenTM] at ON at.AccountId = a.AccountId
        JOIN AccountingCommon.TarifficationMethod tm ON at.TarifficationMethodId = tm.TarifficationMethodId
        JOIN Tariff t ON t.TariffGroupId = tm.TariffGroupId AND t.id =1
        JOIN AccountingCommon.UsageObject uo ON uo.AccountId = a.AccountId
        LEFT JOIN Tariff tt ON tt.TariffGroupId = tm.TariffGroupId AND tt.id = 2
) ,
b AS (
        SELECT *,
        CASE 
	        WHEN MinValue+(IncrementValue*(CntPhysicalPerson-1))<MinValue THEN MinValue
	        WHEN MinValue+(IncrementValue*(CntPhysicalPerson-1))>MaxValue THEN MaxValue
	        ELSE MinValue+(IncrementValue*(CntPhysicalPerson-1))
        END norm_f6
        FROM a 
),
c AS (
        SELECT *,
        CASE WHEN norm_f6>MaxTariffLimit THEN convert(DECIMAL(10,2),MaxTariffLimit*taryf_6)
            +convert(DECIMAL(10,2),(norm_f6-MaxTariffLimit)*ISNULL(taryf_61,taryf_6))
        ELSE convert(DECIMAL(10,2),norm_f6*taryf_6)
        END SanNormaSubsGrn,
        -- 2017-03-18 Лемешко: тарифи taryf_6 і taryf_61 вже включають ПДВ, забрано множник 1.2
        CASE
        WHEN DiscountKoeff<>0 THEN -- 2017-03-18 Лемешко: було DiscountKoeff<>1 !!
            CASE 
            WHEN QuantityTo>norm_f6 THEN 
                CASE WHEN norm_f6>MaxTariffLimit THEN convert(DECIMAL(10,2),MaxTariffLimit*taryf_6*DiscountKoeff)
                    +convert(DECIMAL(10,2),(norm_f6-MaxTariffLimit)*ISNULL(taryf_61,taryf_6)*DiscountKoeff)
                ELSE convert(DECIMAL(10,2),norm_f6*taryf_6*DiscountKoeff)
                END
            ELSE 
                CASE WHEN QuantityTo>MaxTariffLimit THEN convert(DECIMAL(10,2),MaxTariffLimit*taryf_6/**1.2*/*DiscountKoeff)
                    +convert(DECIMAL(10,2),(QuantityTo-MaxTariffLimit)*ISNULL(taryf_61,taryf_6)/**1.2*/*DiscountKoeff)
                ELSE convert(DECIMAL(10,2),QuantityTo*taryf_6/**1.2*/*DiscountKoeff)
                END 
            END
        ELSE 0 
        END AS QuantityToGrn
        FROM b
)

SELECT	*
		, SanNormaSubsGrn-QuantityToGrn AS nm_pay 
FROM c