--DECLARE @date_from DATE; SET @date_from = convert(DATETIME,'1/02/2021', 103)
--DECLARE @date_to DATE; SET @date_to = convert(DATETIME,'1/03/2021',103)
DECLARE @period INT = YEAR(@date_from) * 100 + MONTH(@date_from)
--DECLARE @cntZones INT; SET @cntZones = 2
DECLARE @dtPeriod DATETIME; SET @dtPeriod = convert(DATETIME,'01/'+LTRIM(RTRIM(@Period%100))+'/'+LTRIM(RTRIM(@Period/100)),103)

DECLARE @tableZone TABLE (
		id INT IDENTITY(1,1),
		AccountNumber VARCHAR(20),
		AccountId INT,
		PIP VARCHAR(200),
		TarifficationBlockId INT,
		BlockLabel VARCHAR(5),
		BlockLabelName varchar(50),
		TimeZonalId INT,
		All_kWt DECIMAL(10, 2),
		kategory CHAR(50),
		TariffGroupId INT,
		isHeating int,
		BasePrice DECIMAL(16,5),
		Quantity_PivPick INT,
		Tariff_PivPick DECIMAL(16,5),
		Quantity_Nich INT,
		Tariff_Nich DECIMAL(16,5),
		Quantity_Pick INT,
		Tariff_Pick DECIMAL(16,5)
)
--завантажуЇмо вс≥х в кого Ї диф. обл≥к
INSERT @tableZone
(
    AccountId,
	AccountNumber,
    PIP,
	TimeZonalId
)
SELECT  acc.AccountId
		, acc.AccountNumber
		, pp.LastName+' '+pp.FirstName+' '+pp.SecondName AS pip
		, MAX(ucm.TimeZonalId) AS zona
FROM AccountingCommon.Account acc
LEFT JOIN AccountingCommon.PhysicalPerson pp ON pp.PhysicalPersonId = acc.PhysicalPersonId
LEFT JOIN AccountingCommon.UsageObject as uo ON uo.AccountId = acc.AccountId
LEFT JOIN AccountingCommon.Point as p ON p.UsageObjectId = uo.UsageObjectId
LEFT JOIN AccountingCommon.TarifficationMethod tm ON tm.PointId = p.PointId
LEFT JOIN AccountingCommon.UsageCalculationMethod ucm ON ucm.PointId = p.PointId
		AND ucm.DateFrom <= @date_from
        AND ucm.Dateto >= @date_from
WHERE acc.DateTo = convert(DATETIME,'6/6/2079',103)
GROUP BY acc.AccountId, pp.LastName+' '+pp.FirstName+' '+pp.SecondName, acc.AccountNumber
HAVING MAX(ucm.TimeZonalId) <> 1

----проставл€Їмо електроопаленн€
UPDATE @tableZone
SET isHeating = s.IsHeating
	, TariffGroupId = s.TariffGroupId
FROM (
		SELECT acc.AccountId
				, tm.IsHeating	-- на€вн≥сть електроопаленн€
				, tm.TariffGroupId AS TariffGroupId
		FROM @tableZone acc
		LEFT JOIN AccountingCommon.UsageObject as uo ON uo.AccountId = acc.AccountId
		LEFT JOIN AccountingCommon.Point as p ON p.UsageObjectId = uo.UsageObjectId
		LEFT JOIN AccountingCommon.TarifficationMethod tm ON tm.PointId = p.PointId
) AS s
WHERE [@tableZone].AccountId = s.AccountId

---- заповнюЇмо к≥ловати ≥ тарифи
UPDATE @tableZone
SET All_kWt = s.Quantity
	, Quantity_PivPick = s.к¬т1
	, Quantity_Nich = s.к¬т2
	, Quantity_Pick = s.к¬т3
	, Tariff_PivPick = s.tarif1
	, Tariff_nich = s.tarif2
	, Tariff_Pick = s.tarif3
	, BlockLabelName = s.ShortName
	,BasePrice = s.BasePrice
	,TarifficationBlockId = s.TarifficationBlockId
	,BlockLabel = s.BlockLabel
FROM (
		SELECT o.AccountId
				, SUM(op.Quantity*CASE WHEN o.PeriodTo=@Period THEN -1 ELSE 1 END) Quantity
				, SUM(CASE WHEN t.TimeZoneId in (2, 4) THEN op.Quantity ELSE 0 end) к¬т1
				, SUM(CASE WHEN t.TimeZoneId in (5, 3) THEN op.Quantity ELSE 0 end) к¬т2
				, SUM(CASE WHEN t.TimeZoneId = 6 THEN op.Quantity ELSE 0 end) к¬т3
				, SUM(CASE WHEN t.TimeZoneId = 2 THEN t.Price 
						   WHEN t.TimeZoneId = 4 THEN t.Price
					  ELSE 0 END) AS tarif1
				, SUM(CASE WHEN t.TimeZoneId = 3 THEN t.Price 
						   WHEN t.TimeZoneId = 5 THEN t.Price
					  ELSE 0 END) AS tarif2
				, SUM(CASE WHEN t.TimeZoneId = 6 THEN t.Price ELSE 0 END) AS tarif3
				, REPLACE(tbl.shortname,'к¬т*год','') as ShortName
				, t1.Price AS BasePrice
				, isnull(tbl.TarifficationBlockId,0) as TarifficationBlockId
				, CASE 
					WHEN t1.Price = 1.4 THEN 'Ѕ1'
					WHEN t1.Price = 1.2 THEN 'Ѕ2'
					ELSE '' 
				  END AS BlockLabel
		FROM FinanceMain.Operation o
		JOIN FinanceMain.OperationRow op ON o.OperationId = op.OperationId
			AND @Period IN (o.PeriodFrom,o.PeriodTo)
			AND MONTH(op.ConsumptionFrom) = MONTH(@dtPeriod)
			AND YEAR(op.ConsumptionFrom) = YEAR(@dtPeriod)
			AND o.IsIncome=0
		JOIN Dictionary.Tariff t ON t.TariffId = op.TariffId
		JOIN Dictionary.TarifficationBlockLine tbl ON tbl.TarifficationBlockLineId = t.TarifficationBlockLineId
		left join Dictionary.Tariff t1 ON t1.DateFrom=t.DateFrom
		and t1.DateTo=t.DateTo
		and t1.TariffGroupId=t.TariffGroupId
		and t1.VoltageId=t.VoltageId
		and t1.Is30km=t.Is30km
		and t1.IsAPK=t.IsAPK
		and t1.IsHighlander=t.IsHighlander
		and t1.IsHeating=t.IsHeating
		and t1.TarifficationBlockLineId=t.TarifficationBlockLineId
		and t1.TimeZoneId=1
		WHERE 1=1 
		GROUP BY o.AccountId, REPLACE(tbl.shortname,'к¬т*год',''), t1.Price, TarifficationBlockId
) AS s
WHERE [@tableZone].AccountId = s.AccountId

SELECT 	id,AccountNumber,PIP,BlockLabel,BlockLabelName,TariffGroupId,
	TimeZonalId,isHeating,BasePrice,Quantity_Nich,Quantity_PivPick,Quantity_Pick,
	Tariff_Nich,Tariff_PivPick,Tariff_Pick,TarifficationBlockId
FROM @tableZone
WHERE 1=1
		AND BasePrice IS NOT NULL
		AND TimeZonalId = @cntZones
ORDER BY id