DECLARE 
--	@SetPeriod INT,
	@PaymentKindId int,
	@IsIncome bit,
--	@ClassifierGroupId int,
	@ClsId int
--	@N int

SET @IsIncome=0
SET @PaymentKindId=15
--SET @SetPeriod=@SelectedPeriod
--SET @ClassifierGroupId=@SelectedClassifierGroup --  10924 - ПУП	-- 10923 - ВЦ вільна ціна 
SET @ClsId = 2 -- Класифікатор
--SET @N = @SelectedMinSum

BEGIN

IF @PaymentKindId=71
BEGIN
	SET @PaymentKindId=715
END
ELSE
BEGIN
	IF @PaymentKindId=81
	BEGIN
		SET @PaymentKindId=815
	END
	ELSE
	BEGIN
		IF @PaymentKindId=91
		BEGIN
			SET @PaymentKindId=915
		END
		ELSE
		BEGIN
			SET @PaymentKindId=@PaymentKindId
		END
	END
END


DECLARE @CurrentPeriod INT , @Period int, @LastCachePeriod int
SET @CurrentPeriod = dbo.sfGetCurrentPeriod()
print @CurrentPeriod

SELECT @SetPeriod=isnull (@SetPeriod,@CurrentPeriod),
		@PaymentKindId=isnull (@PaymentKindId,1),
        @IsIncome=isnull (@IsIncome,0)

SELECT @Period=dbo.sfGetPrevPeriod(@SetPeriod),@LastCachePeriod = dbo.sfGetPrevPeriod(@CurrentPeriod)

IF OBJECT_ID(N'tempdb.dbo.#Operation', N'U') is not null drop table #Operation

CREATE TABLE #Operation (contractId int, operationId int,period int, EndRest money)

IF @Period<@CurrentPeriod
BEGIN
	IF @ClassifierGroupId=0 
	BEGIN
		INSERT INTO #Operation (contractId , operationId ,period , EndRest)
		SELECT 
			SC.ContractId, SC.OperationId,o.CurrentPeriod
			,SC.EndRest 
		FROM BalanceCache SC
			INNER JOIN operation o ON o.operationId=SC.OperationId
		WHERE SC.CurrentPeriod = @Period
			AND SC.IsAdmitted = 1 AND SC.IsIncome = @IsIncome AND SC.EndRest <>0.00
			AND o.PaymentKindId = @PaymentKindId AND SC.ContractId IS NOT NULL 
	END
	ELSE
	BEGIN
		INSERT INTO #Operation (contractId , operationId ,period , EndRest)
		SELECT 
			SC.ContractId, SC.OperationId,o.CurrentPeriod
			,SC.EndRest 
		FROM BalanceCache SC
			INNER JOIN operation o ON o.operationId=SC.OperationId
		WHERE SC.CurrentPeriod = @Period
			AND SC.IsAdmitted = 1 AND SC.IsIncome = @IsIncome AND SC.EndRest <>0.00
			AND o.PaymentKindId = @PaymentKindId AND SC.ContractId IS NOT NULL 
			AND SC.ContractId IN (
			SELECT cgc.ContractId FROM ClassifierGroup cg, ClassifierGroupContract cgc
				WHERE cg.ClassifierGroupId = cgc.ClassifierGroupId AND
				 cg.ClassifierGroupId=@ClassifierGroupId
			)
	END
END
ELSE 
BEGIN
	IF @ClassifierGroupId=0 
	BEGIN
		INSERT INTO #Operation (contractId , operationId ,period , EndRest)
		SELECT 
			o.ContractId, o.OperationId,o.CurrentPeriod
			,sum(R.TotalWithVAT * RT.Sign)
		FROM Operation O 
			JOIN OperationRow R ON R.OperationId = O.OperationId 
			JOIN OperationRowType RT on R.OperationRowTypeId = RT.OperationRowTypeId
			JOIN OperationType OT on OT.OperationTypeId = O.OperationTypeId	
		WHERE 		r.IsAdmitted = 1 AND o.IsIncome = @IsIncome AND o.Rest <>0.00
			AND o.PaymentKindId = @PaymentKindId AND o.ContractId IS NOT NULL 
		GROUP BY o.ContractId, o.OperationId,o.CurrentPeriod
	END
	ELSE
	BEGIN
		INSERT INTO #Operation (contractId , operationId ,period , EndRest)
		SELECT 
			o.ContractId, o.OperationId,o.CurrentPeriod
			,sum(R.TotalWithVAT * RT.Sign)
		FROM Operation O 
			JOIN OperationRow R ON R.OperationId = O.OperationId 
			JOIN OperationRowType RT on R.OperationRowTypeId = RT.OperationRowTypeId
			JOIN OperationType OT on OT.OperationTypeId = O.OperationTypeId	
		WHERE 		r.IsAdmitted = 1 AND o.IsIncome = @IsIncome AND o.Rest <>0.00
			AND o.PaymentKindId = @PaymentKindId AND o.ContractId IS NOT NULL 
			AND O.ContractId IN (
			SELECT cgc.ContractId FROM ClassifierGroup cg, ClassifierGroupContract cgc
				WHERE cg.ClassifierGroupId = cgc.ClassifierGroupId AND
				 cg.ClassifierGroupId=@ClassifierGroupId
			)
		GROUP BY o.ContractId, o.OperationId,o.CurrentPeriod
	END
END

DECLARE @2MonthBefore money, @3MonthBefore money,@4MonthBefore money,@5MonthBefore money,@6MonthBefore money
DECLARE @1YearBefore int, @3YearBefore int
SELECT @2MonthBefore=dbo.sfGetPrevPeriod(@Period)
SELECT @3MonthBefore=dbo.sfGetPrevPeriod(@2MonthBefore)
SELECT @4MonthBefore=dbo.sfGetPrevPeriod(@3MonthBefore)
SELECT @5MonthBefore=dbo.sfGetPrevPeriod(@4MonthBefore)
SELECT @6MonthBefore=dbo.sfGetPrevPeriod(@5MonthBefore)
SELECT @1YearBefore =@Period-100
SELECT @3YearBefore =@Period-300

SELECT 
	c.contractNumber AS contractNumber
	,CG.ShortName className
	--,cls.ShortName
	,isnull(ctr.ShortName,c.NumStatement) AS ShortName 
	,sum (do.EndRest) AS AllDebet
	,sum (CASE WHEN do.period=@Period THEN do.EndRest ELSE '0,00' END) AS MonthBefore_1
	,sum (CASE WHEN do.period IN(@2MonthBefore,@3MonthBefore) THEN do.EndRest ELSE '0,00' END) AS MonthBefore_2_3
	,sum (CASE WHEN do.period IN(@4MonthBefore,@5MonthBefore,@6MonthBefore) THEN do.EndRest ELSE '0,00' END) AS MonthBefore_4_5_6
	,sum (CASE WHEN do.period<@6MonthBefore AND do.period>=@1YearBefore THEN do.EndRest ELSE '0,00' END) AS YearBefore_6_12
	,sum (CASE WHEN do.period<@1YearBefore THEN do.EndRest ELSE '0,00' END) AS Before
FROM #Operation do
	LEFT JOIN contract c ON c.contractId = do.contractId
	LEFT JOIN Contractor ctr on c.ContractorId = ctr.ContractorId
	JOIN dbo.ClassifierGroupContract CGC ON CGC.ContractId = do.ContractID	 
	JOIN dbo.ClassifierGroup CG ON CG.ClassifierGroupId = CGC.ClassifierGroupId
	JOIN Classifier cls on cls.ClassifierId = CG.ClassifierId
where (@ClsId = 0 or CG.ClassifierId = @ClsId)
GROUP BY 
c.contractNumber,
CG.ShortName,
CG.ClassifierGroupId,
cls.ShortName,
ctr.ShortName,
c.NumStatement
HAVING
	sum (do.EndRest) > @N
ORDER BY CG.ShortName, sum (do.EndRest), CONVERT(int, c.ContractNumber)


--DECLARE @FilialId int 
--select @FilialId = CAST(Value as int)	from   setting	where  Name = 'FilialId'

--select f.ShortName AS Filial,
--	CASE @IsIncome WHEN 0 THEN 'Дебіторська' ELSE 'Кредиторcька' END +
--	' заборгованість на '+'01.'+RIGHT(str(@SetPeriod),2)+'.'+LEFT(str(@SetPeriod,6),4)+ 'p. ' + 
--		(SELECT pk.PKShortName FROM PaymentKind pk WHERE pk.PaymentKindId=@PaymentKindId) AS Title
--FROM Contractor f
--   		inner join Filial fi on fi.ContractorId = f.ContractorId
--	WHERE fi.FilialId = @FilialId
--END
