--- ****Вибір боржників по напрямках місто Тернопіль**** ---

	--вибираємо базу даних
	--use [TR27_Utility]
	--use [TR28_Utility]
	--use [TR29_Utility]
	--use [TR30_Utility]
	--use [TR31_Utility]
	--use [TR32_Utility]
	--use [TR33_Utility]
	--use [TR34_Utility]
	--use [TR35_Utility]
	--use [TR36_Utility]
	--use [TR37_Utility]
	--use [TR38_Utility]
	--use [TR39_Utility]
	--use [TR40_Utility]
	--use [TR41_Utility]
	--use [TR42_Utility]
	--use [TR43_Utility]
	--use [TR44_Utility]

/*знищуємо тимчасову таблицю*/
drop table IF EXISTS ##temp

DECLARE @date DATETIME; SET @date = dateadd(DAY, 1 - DAY(GETDATE()), GETDATE());
DECLARE @date_from DATETIME; SET @date_from = dateadd( month, datediff( MONTH, 0, DATEADD(month,-1,GETDATE())), 0);
DECLARE @date_to DATE; SET @date_to = dateadd(month,1,dateadd(day,1-day(@date_from),@date_from))-1

DECLARE @zaliz TABLE (
						AccountNumber BIGINT,
						AccountId INT,
						FullName CHAR(100),
						OSR CHAR(20),
						PoperDatePokaz DATE,
						PoperPokaz CHAR(15),
						NextDatePokaz DATE,
						NextPokaz CHAR(15),
						RiznPokazDen INT,
						RiznPokazNich INT,
						RiznPokazPik INT
)

INSERT INTO @zaliz
(
    AccountNumber,
    AccountId,
    FullName,
    OSR
)
SELECT	a.AccountNumber as [особовий]
		,a.AccountId
		, pp.FullName [ПІП]
		, cg.Name as [ОСР]
FROM AccountingCommon.Account a --ос рах
    JOIN AccountingCommon.PhysicalPerson pp ON pp.PhysicalPersonId =a.PhysicalPersonId --фіо AccountingCommon.PhysicalPerson
    JOIN AccountingCommon.UsageObject uo ON uo.AccountId = a.AccountId
	JOIN AccountingCommon.Point p ON p.UsageObjectId = uo.UsageObjectId
	JOIN AccountingCommon.UsageCalculationMethod ucm ON ucm.PointId = p.PointId
	join [AccountingCommon].[ClassifierGroupAccount] cl on cl.[AccountId]=a.AccountId  -- для зв'язки
	join [Dictionary].[ClassifierGroup] cg on cg.ClassifierGroupId = cl.ClassifierGroupId 
	WHERE cg.ClassifierGroupId IN ('14', '15', '16')-- виводим напрямок

UPDATE @zaliz
SET PoperDatePokaz = s.[дата показу], PoperPokaz = s.[останій показ]
FROM ( SELECT a.AccountId
			  , CONVERT(DATE, gi.Date) AS [дата показу]
			  , gi.CachedIndexes [останій показ]
		FROM @zaliz a
		JOIN AccountingCommon.UsageObject uo ON uo.AccountId = a.AccountId
		JOIN AccountingCommon.Point p ON p.UsageObjectId = uo.UsageObjectId
		JOIN AccountingCommon.UsageCalculationMethod ucm ON ucm.PointId = p.PointId
		JOIN AccountingMeasuring.GroupIndex gi ON gi.UsageCalculationMethodId = ucm.UsageCalculationMethodId
			AND ucm.DateTo = '20790606' 
			AND gi.IsForCalculate=1
		WHERE CONVERT(DATE, gi.Date) BETWEEN @date_from AND @date_to
) AS s
WHERE [@zaliz].AccountId = s.AccountId

UPDATE @zaliz
SET NextDatePokaz = s.[дата показу], NextPokaz = s.[останій показ]
FROM ( SELECT a.AccountId
			  , CONVERT(DATE, gi.Date) AS [дата показу]
			  , gi.CachedIndexes [останій показ]
		FROM @zaliz a
		JOIN AccountingCommon.UsageObject uo ON uo.AccountId = a.AccountId
		JOIN AccountingCommon.Point p ON p.UsageObjectId = uo.UsageObjectId
		JOIN AccountingCommon.UsageCalculationMethod ucm ON ucm.PointId = p.PointId
		JOIN AccountingMeasuring.GroupIndex gi ON gi.UsageCalculationMethodId = ucm.UsageCalculationMethodId
			AND ucm.DateTo = '20790606' 
			AND gi.IsForCalculate=1
		WHERE CONVERT(DATE, gi.Date) BETWEEN @date_to AND GETDATE()
) AS s
WHERE [@zaliz].AccountId = s.AccountId

UPDATE @zaliz
SET RiznPokazDen = CONVERT(INT, SUBSTRING(NextPokaz, 1, CASE CHARINDEX('/', NextPokaz)
					WHEN 0
					THEN LEN(NextPokaz)
					ELSE CHARINDEX('/', NextPokaz) - 1
					END))	-- останній показник день
					- 
				 CONVERT(INT, (SUBSTRING(PoperPokaz, 1, CASE CHARINDEX('/', PoperPokaz)
					WHEN 0
					THEN LEN(PoperPokaz)
					ELSE CHARINDEX('/', PoperPokaz) - 1
					END)))		-- попередній показник день
	, RiznPokazNich = CONVERT(INT, SUBSTRING(NextPokaz, CASE CHARINDEX('/', NextPokaz)
						WHEN 0
						THEN LEN(NextPokaz) + 1
						ELSE CHARINDEX('/', NextPokaz) + 1
						END, 1000))		-- останній показник ніч
					- 
					CONVERT(INT, SUBSTRING(PoperPokaz, CASE CHARINDEX('/', PoperPokaz)
						WHEN 0
							THEN LEN(PoperPokaz) + 1
						ELSE CHARINDEX('/', PoperPokaz) + 1
						END, 1000))		-- поередній показник ніч
FROM @zaliz

--SELECT DISTINCT * FROM @zaliz

SELECT DISTINCT 'Львівобленерго' [ОСР], SUM(RiznPokazDen) + SUM(RiznPokazNich) [сума]
FROM @zaliz
WHERE OSR LIKE 'Львівобленерго%'
UNION
SELECT DISTINCT 'Укрзалізниця' [ОСР], SUM(RiznPokazDen) + SUM(RiznPokazNich) [сума]
FROM @zaliz
WHERE OSR LIKE 'Укрзалізниця%'

