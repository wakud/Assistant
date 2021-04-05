--DECLARE @date_from DATE; SET @date_from = convert(DATETIME,'1/02/2021', 103)
--DECLARE @date_to DATE; SET @date_to = convert(DATETIME,'1/03/2021',103)
DECLARE @period INT = YEAR(@date_from) * 100 + MONTH(@date_from)
--DECLARE @cntZones INT; SET @cntZones = 2
DECLARE @dtPeriod DATETIME; SET @dtPeriod = convert(DATETIME,'01/'+LTRIM(RTRIM(@Period%100))+'/'+LTRIM(RTRIM(@Period/100)),103)

DECLARE @zones TABLE (
	id INT IDENTITY(1,1),
	AccountNumber VARCHAR(20),
	PIP VARCHAR(200),
    TarifficationBlockId INT,
	BlockLabel VARCHAR(5),
    BlockLabelName varchar(50),
	TariffGroupId INT,
	TimeZonalId INT,
    isHeating int,
	BasePrice DECIMAL(16,5),
	Quantity_Nich INT,
	Quantity_PivPick INT,
	Quantity_Pick INT,
	Tariff_Nich DECIMAL(16,5),
	Tariff_PivPick DECIMAL(16,5),
	Tariff_Pick DECIMAL(16,5)
)
;
WITH oper AS(
	SELECT o.AccountId,bc.TarifficationBlockId,t.TariffId,
    t.TariffGroupId,
    --CASE WHEN t.TarifficationBlockLineId = 1 THEN 7 ELSE t.TariffGroupId END AS TariffGroupId, -- 20180515 Лемешко: збут хоче, щоб всі споживачі з безблочним тарифом були відокремлені в одну групу,
           --так буде працювати лише поки безблочні тарифи однакові для різних тарифних груп, потім потрібно буде прийняти рішення, яка поведінка має бути реалізована, і переробити
    t.Price,t.IsHeating,tz.TimeZonalId,t.TimeZoneId,t.TarifficationBlockLineId,
    REPLACE(tbl.shortname,'кВт*год','') as ShortName,
	t1.Price AS BasePrice,
	SUM(
	CASE WHEN o.PeriodTo=@Period THEN -1 ELSE 1 END *op.Summ) TotalSumm,
	SUM(op.Quantity*CASE WHEN o.PeriodTo=@Period THEN -1 ELSE 1 END) Quantity
	FROM FinanceMain.Operation o
	JOIN FinanceMain.OperationRow op ON o.OperationId = op.OperationId
	AND @Period IN (o.PeriodFrom,o.PeriodTo)
	AND MONTH(op.ConsumptionFrom) = MONTH(@dtPeriod)
	AND YEAR(op.ConsumptionFrom) = YEAR(@dtPeriod)
--    and op.Discount=0
	AND o.IsIncome=0
	JOIN AccountingDictionary.BenefitsCategory bc ON  bc.BenefitsCategoryId = op.BenefitsCategoryId
	JOIN Dictionary.Tariff t ON t.TariffId = op.TariffId
	JOIN Dictionary.TimeZone tz ON tz.TimeZoneId = t.TimeZoneId
	JOIN Dictionary.TimeZonal ttz ON ttz.TimeZonalId = tz.TimeZonalId
    AND ttz.TimeZoneCount=isnull(@cntZones,ttz.TimeZoneCount)
    and ttz.TimeZoneCount in (2,3)
	JOIN Dictionary.TarifficationBlockLine tbl ON t.TarifficationBlockLineId = tbl.TarifficationBlockLineId
	left join Dictionary.Tariff t1  
		on	t1.DateFrom=t.DateFrom
		and t1.DateTo=t.DateTo
		and t1.TariffGroupId=t.TariffGroupId
		and t1.VoltageId=t.VoltageId
		and t1.Is30km=t.Is30km
		and t1.IsAPK=t.IsAPK
		and t1.IsHighlander=t.IsHighlander
		and t1.IsHeating=t.IsHeating
		and t1.TarifficationBlockLineId=t.TarifficationBlockLineId
		and t1.TimeZoneId=1
	GROUP BY o.AccountId,bc.TarifficationBlockId,t.TariffId,t.TariffGroupId,t.Price,t.IsHeating,tz.TimeZonalId,t.TimeZoneId,t.TarifficationBlockLineId,t1.Price,tbl.ShortName
),zvit AS 
(
	SELECT a.AccountNumber,
	pp.LastName+' '+pp.FirstName+' '+pp.SecondName AS pip,
    isnull(o.TarifficationBlockId,0) as TarifficationBlockId,
	CASE 
		-- WHEN o.TarifficationBlockLineId IN (3,5,7,10,21,24,27,29) THEN 'Б1' -- 20170420: ,27,29 added
		-- WHEN o.TarifficationBlockLineId IN (8,11,13,15,17,19,22,25) THEN 'Б2'
		-- 20171221 Lemeshko
		WHEN vtb.BlockNumber = 2 THEN 'Б1'
		WHEN vtb.BlockNumber = 3 THEN 'Б2'
		ELSE '' 
	END AS BlockLabel,
    o.shortname as BlockLabelName,
	o.TariffGroupId,o.TimeZonalId,o.isHeating,o.BasePrice,
	SUM(
	CASE 
		WHEN o.TimeZoneId IN (3,5) THEN o.Quantity ELSE 0 
	END) AS kwt_nich,
	SUM(
	CASE 
		WHEN o.TimeZoneId IN (2,4) THEN o.Quantity ELSE 0 
	END ) AS kwt_pivpick,
	SUM(
	CASE WHEN o.TimeZoneId IN (6) THEN o.Quantity ELSE 0 
	END) AS kwt_pick,
		MAX(CASE 
		WHEN o.TimeZoneId IN (3,5) THEN o.Price ELSE 0 
	END) tariff_nich,
	MAX(
	CASE 
		WHEN o.TimeZoneId IN (2,4) THEN o.Price ELSE 0 
	END) tariff_pivpick,
	MAX(CASE 
		WHEN o.TimeZoneId IN (6) THEN o.Price ELSE 0 
	END) tariff_pick
	FROM AccountingCommon.Account a 
	JOIN oper o ON o.AccountId = a.AccountId
	JOIN AccountingCommon.PhysicalPerson pp ON a.PhysicalPersonId = pp.PhysicalPersonId
    JOIN SupportDefined.V_TarifficationBlock vtb ON vtb.TarifficationBlockLineId = o.TarifficationBlockLineId -- 20171221 Lemeshko
	GROUP BY 
	a.AccountId,a.AccountNumber,
	LastName,FirstName,SecondName,
	CASE 
		-- WHEN o.TarifficationBlockLineId IN (3,5,7,10,21,24,27,29) THEN 'Б1' -- 20170420: ,27,29 added
		-- WHEN o.TarifficationBlockLineId IN (8,11,13,15,17,19,22,25) THEN 'Б2'
		-- 20171221 Lemeshko
		WHEN vtb.BlockNumber = 2 THEN 'Б1'
		WHEN vtb.BlockNumber = 3 THEN 'Б2'
		ELSE '' 
	END ,
o.TariffGroupId,o.TimeZonalId,o.ShortName,o.BasePrice,o.isHeating,
	o.TarifficationBlockId
)
INSERT @Zones
SELECT * FROM zvit

SELECT 	id,AccountNumber,PIP,BlockLabel,BlockLabelName,TariffGroupId,
	TimeZonalId,isHeating,BasePrice,Quantity_Nich,Quantity_PivPick,Quantity_Pick,
	Tariff_Nich,Tariff_PivPick,Tariff_Pick,TarifficationBlockId
FROM @Zones
order by id
