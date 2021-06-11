/* Інформація по побутових споживачах, 
	у яких більше ніж пів року не знімали показник електролічильника
	та по яких рахунки сформовано по розрахунковому споживанню е/е
	*/

	use [TR27_Utility]
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
	--USE [TR40_Utility]
	--use [TR41_Utility]
	--use [TR42_Utility]
	--use [TR43_Utility]
	--use [TR44_Utility]

DECLARE @d$cok$ DATETIME; SET @d$cok$=convert(char(8),getdate(),112)
DECLARE @date_from$cok$ DATE; SET @date_from$cok$ = '2019-01-01 00:00:00'		--дата з
DECLARE @date_to$cok$ DATE; SET @date_to$cok$ = dateadd(day,1-day(@d$cok$),@d$cok$)		--дата по

declare @rozrah$cok$ TABLE (
		AccountId INT,
		AccountNumber VARCHAR(10),
		Pip VARCHAR(300),
		MonthRozrzh INT,
		FirstDate DATE,
		LastDate DATE
		)

--Вибираємо коли знімалися показники контролерами
INSERT INTO @rozrah$cok$ (AccountId, AccountNumber,pip, FirstDate, MonthRozrzh)
SELECT acc.AccountId
		,acc.AccountNumber
		,pp.FullName
		,MAX(gi.Date) max_Date
		, DATEDIFF(mm,MAX(gi.Date),GETDATE()) countmonth
FROM AccountingCommon.Account acc
LEFT JOIN AccountingCommon.PhysicalPerson pp ON pp.PhysicalPersonId = acc.PhysicalPersonId
JOIN AccountingCommon.UsageObject uo ON uo.AccountId = acc.AccountId 
JOIN AccountingCommon.Point p ON uo.UsageObjectId = p.UsageObjectId
		AND p.DateTo = CONVERT(DATETIME,'06/06/2079',103)
		AND acc.DateTo = CONVERT(DATETIME,'06/06/2079',103)
JOIN AccountingCommon.UsageCalculationMethod ucm ON ucm.PointId = p.PointId
JOIN AccountingMeasuring.GroupIndex gi ON gi.UsageCalculationMethodId = ucm.UsageCalculationMethodId
		AND ucm.DateTo = '20790606' AND ucm.Generation = 0 	
		AND gi.IsForCalculate=1
		AND ISNULL(gi.GroupIndexSourceId,0) NOT IN (18)  --Не враховувати Розрахункові покази
GROUP BY acc.AccountId
		, acc.AccountNumber
		, pp.FullName
HAVING DATEDIFF(mm,MAX(gi.Date),GETDATE()) >= 6

SELECT AccountNumber AS [Особовий]
		, Pip AS [ПІП]
		, MonthRozrzh AS [К-ть місяців]
FROM @rozrah$cok$
