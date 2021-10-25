DECLARE @raj INT; SET @raj = 27
--DECLARE @date_from DATETIME, @date_to DATETIME
--SET @date_from = CONVERT(datetime,'01.11.2020', 103)
--SET @date_to = CONVERT(datetime,'01.12.2020', 103)
DECLARE @period INT = YEAR(@date_from) * 100 + MONTH(@date_from)

DECLARE @table TABLE (
		AccountId INT,
		zona INT,
		All_kWt DECIMAL(10, 5),
		kwt1_den DECIMAL(10, 5),
		kwt2_nich DECIMAL(10, 5),
		kwt3_pik DECIMAL(10, 5)
)

WHILE @raj <=44
	BEGIN
/*вибираЇмо базу даних*/
		IF @raj = 27
			use [TR27_Utility] 
		IF @raj = 28
			USE [TR28_Utility] 
		IF @raj = 29
			USE [TR29_Utility] 
		IF @raj = 30
			USE [TR30_Utility] 
		IF @raj = 31
			USE [TR31_Utility] 
		IF @raj = 32
			USE [TR32_Utility] 
		IF @raj = 33
			USE [TR33_Utility] 
		IF @raj = 34
			USE [TR34_Utility] 
		IF @raj = 35
			USE [TR35_Utility] 
		IF @raj = 36
			USE [TR36_Utility] 
		IF @raj = 37
			USE [TR37_Utility] 
		IF @raj = 38
			USE [TR38_Utility] 
		IF @raj = 39
			USE [TR39_Utility] 
		IF @raj = 40
			USE [TR40_Utility] 
		IF @raj = 41
			USE [TR41_Utility] 
		IF @raj = 42
			USE [TR42_Utility] 
		IF @raj = 43
			USE [TR43_Utility] 
		IF @raj = 44
			USE [TR44_Utility] 
        
INSERT @table
(
    AccountId
	, zona
	, All_kWt
)
SELECT  br.AccountId
		, MAX(ucm.TimeZonalId) AS zona
		, br.ConsumptionQuantity
FROM FinanceCommon.BillRegular br (NOLOCK)
LEFT JOIN AccountingCommon.UsageObject as uo ON uo.AccountId = br.AccountId
LEFT JOIN AccountingCommon.Point as p ON p.UsageObjectId = uo.UsageObjectId
LEFT JOIN AccountingCommon.TarifficationMethod tm ON tm.PointId = p.PointId
LEFT JOIN AccountingCommon.UsageCalculationMethod ucm ON ucm.PointId = p.PointId
			AND ucm.DateTo > @date_to
WHERE br.IsDeleted = 0 AND br.ConsumptionFrom = @date_from
GROUP BY br.AccountId, br.ConsumptionQuantity

UPDATE @table
SET kwt1_den = s.к¬т1
	, kwt2_nich = s.к¬т2
	, kwt3_pik = s.к¬т3
FROM(
	SELECT o.AccountId
		, SUM(CASE WHEN t.TimeZoneId in (1, 2, 4) THEN r.Quantity ELSE 0 end) к¬т1
		, SUM(CASE WHEN t.TimeZoneId in (5, 3) THEN r.Quantity ELSE 0 end) к¬т2
		, SUM(CASE WHEN t.TimeZoneId = 6 THEN r.Quantity ELSE 0 end) к¬т3
	FROM FinanceMain.Operation o 
	JOIN FinanceMain.OperationRow r ON r.OperationId = o.OperationId
	JOIN Dictionary.Tariff t ON t.TariffId = r.TariffId
	WHERE o.DocumentTypeId = 15 AND o.PeriodFrom = @period AND o.PeriodTo = '207906'
	GROUP BY o.AccountId
)s
WHERE [@table].AccountId = s.AccountId

	SET @raj = @raj +1 
END

SELECT 'без диф. < 250' AS [градац≥€]
		, SUM(All_kWt) AS [к¬т*год]
FROM @table
WHERE zona = 1 AND All_kWt <= 250
UNION
SELECT 'без диф. > 250'
		, SUM(All_kWt) AS [к¬т*год]
FROM @table
WHERE zona = 1 AND All_kWt >= 251
UNION
SELECT '2 зонн≥ день < 250'
		, SUM(kwt1_den) AS [к¬т*год]
FROM @table
WHERE zona = 2 AND kwt1_den <= 250
UNION
SELECT '2 зонн≥ день > 250'
		, SUM(kwt1_den) AS [к¬т*год]
FROM @table
WHERE zona = 2 AND kwt1_den >= 251
UNION
SELECT '2 зонн≥ н≥ч < 250'
		, SUM(kwt2_nich) AS [к¬т*год]
FROM @table
WHERE zona = 2 AND kwt2_nich <= 250
UNION
SELECT '2 зонн≥ н≥ч > 250'
		, SUM(kwt2_nich) AS [к¬т*год]
FROM @table
WHERE zona = 2 AND kwt2_nich >= 251
UNION
SELECT '3 зонн≥ день < 250'
		, SUM(kwt1_den) AS [к¬т*год]
FROM @table
WHERE zona = 3 AND kwt1_den <= 250
UNION
SELECT '3 зонн≥ день > 250'
		, SUM(kwt1_den) AS [к¬т*год]
FROM @table
WHERE zona = 3 AND kwt1_den >= 251
UNION
SELECT '3 зонн≥ н≥ч < 250'
		, SUM(kwt2_nich) AS [к¬т*год]
FROM @table
WHERE zona = 3 AND kwt2_nich <= 250
UNION
SELECT '3 зонн≥ н≥ч > 250'
		, SUM(kwt2_nich) AS [к¬т*год]
FROM @table
WHERE zona = 3 AND kwt2_nich >= 251
UNION
SELECT '3 зонн≥ п≥к < 250'
		, SUM(kwt3_pik) AS [к¬т*год]
FROM @table
WHERE zona = 3 AND kwt3_pik <= 250
UNION
SELECT '3 зонн≥ п≥к > 250'
		, SUM(kwt3_pik) AS [к¬т*год]
FROM @table
WHERE zona = 3 AND kwt3_pik >= 251
