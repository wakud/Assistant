--DECLARE @Period INT;set @Period = CONVERT(VARCHAR(6),DATEADD(mm, -1, GETDATE()),112)
DECLARE @mis1 INT; SET @mis1 = CONVERT(VARCHAR(6),DATEADD(mm, -1, GETDATE()),112) -- до 1 міс
DECLARE @mis2 INT; SET @mis2 = CONVERT(VARCHAR(6),DATEADD(mm, -2, GETDATE()),112)	-- до 2 міс
DECLARE @mis3 INT; SET @mis3 = CONVERT(VARCHAR(6),DATEADD(mm, -3, GETDATE()),112)	-- до 3 міс
DECLARE @mis4 INT; SET @mis4 = CONVERT(VARCHAR(6),DATEADD(mm, -4, GETDATE()),112)	-- до 4 міс
--DECLARE @mis5 INT; SET @mis5 = CONVERT(VARCHAR(6),DATEADD(mm, -5, GETDATE()),112)	-- до 5
DECLARE @mis6 INT; SET @mis6 = CONVERT(VARCHAR(6),DATEADD(mm, -6, GETDATE()),112)	-- до 6
DECLARE @mis7 INT; SET @mis7 = CONVERT(VARCHAR(6),DATEADD(mm, -7, GETDATE()),112)	-- до 7
--DECLARE @mis8 INT; SET @mis8 = CONVERT(VARCHAR(6),DATEADD(mm, -8, GETDATE()),112)	-- до 8
--DECLARE @mis9 INT; SET @mis9 = CONVERT(VARCHAR(6),DATEADD(mm, -9, GETDATE()),112)	-- до 9
--DECLARE @mis10 INT; SET @mis10 = CONVERT(VARCHAR(6),DATEADD(mm, -10, GETDATE()),112)	-- до 10
--DECLARE @mis11 INT; SET @mis11 = CONVERT(VARCHAR(6),DATEADD(mm, -11, GETDATE()),112)	-- до 11
DECLARE @mis12 INT; SET @mis12 = CONVERT(VARCHAR(6),DATEADD(mm, -12, GETDATE()),112)	-- до 12


--DECLARE @OnPayDate BIT; SET @OnPayDate=1

DROP TABLE IF EXISTS ##oper$cok$

SELECT	  fs.DebetEnd
		, fs.Period
		, fs.ChargedSumm
		, fs.DebetBegin
		, fs.ChargedSummBudget
into ##oper$cok$
FROM FinanceCommon.SupplierSaldo AS fs 

--SELECT * FROM ##oper

SELECT 
    --SUM (o.RestSumm) AS [Всього]
	SUM(CASE WHEN o.Period = @mis1 THEN o.DebetEnd ELSE 0.00 END) AS [Всього]
    ,sum (CASE WHEN o.Period=@mis1 THEN o.ChargedSumm + o.ChargedSummBudget ELSE 0.00 END) AS [до 1 міс]
    ,sum (CASE WHEN o.Period BETWEEN @mis3 AND @mis2 THEN o.DebetEnd ELSE 0.00 END) AS [від 1 до 3]
    ,sum (CASE WHEN o.Period BETWEEN @mis6 AND @mis4 THEN o.DebetEnd ELSE 0.00 END) AS [від 3 до 6]
    ,sum (CASE WHEN o.Period BETWEEN @mis12 AND @mis7 THEN o.DebetEnd ELSE 0.00 END) AS [від 6 до 12]
    ,sum (CASE WHEN o.Period < @mis12 THEN o.DebetEnd ELSE 0.00 END) AS [від 1 року]
FROM ##oper$cok$ o