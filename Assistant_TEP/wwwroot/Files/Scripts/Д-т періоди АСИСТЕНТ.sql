--declare @SetClsGUID varchar(36) = 'F15F0DE7-47A2-43BB-8E74-509F56C12DFA'
--declare @SelectedPeriod varchar(6) = '202101'
--declare @ClsContract varchar(36) = '72CA9E89-C01E-4BED-98B0-556737E2AAE8' -- borg
----declare @ClsContract varchar(36) = '815B6ECE-F317-4A78-BED3-5B1CC6C72B1C' -- shoden
--declare @ReportType int = 2 -- 2
--declare @SelectedBorg varchar(13) = ''

drop table if exists #debRes$cok$

DECLARE
	@PaymentKindIddeb int = 15
	,@IsIncomedeb bit = 0
	,@Borgdeb decimal(10, 2) = 0.01

IF TRIM(@SelectedBorg) <> ''
SET @Borgdeb = CAST(@SelectedBorg as decimal(10, 2))


DECLARE @SetPeriodDeb int = cast(@SelectedPeriod as int)
DECLARE @ClassifierGroupGUIDdeb varchar(36) = CAST(@SetClsGUID as varchar(36))

DECLARE @CurrentPerioddeb INT , @Perioddeb int, @LastCachePerioddeb int
SET @CurrentPerioddeb = dbo.sfGetCurrentPeriod()

SELECT @SetPeriodDeb=isnull (@SetPeriodDeb,@CurrentPerioddeb)

SELECT @Perioddeb=dbo.sfGetPrevPeriod(@SetPeriodDeb)
select @LastCachePerioddeb = dbo.sfGetPrevPeriod(@CurrentPerioddeb)

DECLARE @2MonthBeforedeb money, @3MonthBeforedeb money,@4MonthBeforedeb money,@5MonthBeforedeb money,@6MonthBeforedeb money
DECLARE @1YearBefore int, @3YearBefore int
SELECT @2MonthBeforedeb=dbo.sfGetPrevPeriod(@Perioddeb)
SELECT @3MonthBeforedeb=dbo.sfGetPrevPeriod(@2MonthBeforedeb)
SELECT @4MonthBeforedeb=dbo.sfGetPrevPeriod(@3MonthBeforedeb)
SELECT @5MonthBeforedeb=dbo.sfGetPrevPeriod(@4MonthBeforedeb)
SELECT @6MonthBeforedeb=dbo.sfGetPrevPeriod(@5MonthBeforedeb)
SELECT @1YearBefore =@Perioddeb-100
SELECT @3YearBefore =@Perioddeb-300


create table #debRes$cok$ (
	contract_id varchar(20),
	className varchar(75),
	AllDebet decimal(10, 2),
	MonthBefore_1 decimal(10, 2),
	MonthBefore_2_3 decimal(10, 2),
	MonthBefore_4_5_6 decimal(10, 2),
	YearBefore_6_12 decimal(10, 2),
	Before decimal(10, 2),
)


IF @Perioddeb<@CurrentPerioddeb
BEGIN
	insert into #debRes$cok$
	SELECT 
		c.ContractId contract_id
		,CG.ShortName className
		,sum (do.EndRest) AS AllDebet
		,sum (CASE WHEN do.period=@Perioddeb THEN do.EndRest ELSE '0,00' END) AS MonthBefore_1
		,sum (CASE WHEN do.period IN(@2MonthBeforedeb,@3MonthBeforedeb) THEN do.EndRest ELSE '0,00' END) AS MonthBefore_2_3
		,sum (CASE WHEN do.period IN(@4MonthBeforedeb,@5MonthBeforedeb,@6MonthBeforedeb) THEN do.EndRest ELSE '0,00' END) AS MonthBefore_4_5_6
		,sum (CASE WHEN do.period<@6MonthBeforedeb AND do.period>=@1YearBefore THEN do.EndRest ELSE '0,00' END) AS YearBefore_6_12
		,sum (CASE WHEN do.period<@1YearBefore THEN do.EndRest ELSE '0,00' END) AS Before
	FROM (
		SELECT 
				SC.ContractId, SC.OperationId, o.CurrentPeriod period
				,SC.EndRest 
		FROM BalanceCache SC
			INNER JOIN operation o ON o.operationId=SC.OperationId
		WHERE SC.CurrentPeriod = @Perioddeb
			AND SC.IsAdmitted = 1 AND SC.IsIncome = @IsIncomedeb AND SC.EndRest <>0.00
			AND o.PaymentKindId = @PaymentKindIddeb AND SC.ContractId IS NOT NULL 
			AND SC.ContractId IN (
				SELECT cch.KeyId
				FROM ContractClassifierHistory cch
				JOIN ClassifierGroup cg on cg.ClassifierGroupId = cch.IntValue
					WHERE cg.ClassifierGroupGUID=@ClassifierGroupGUIDdeb and
					cch.PeriodTo >= @Perioddeb and cch.PeriodFrom <= @Perioddeb
			)
	) do
		LEFT JOIN contract c ON c.contractId = do.contractId
		LEFT JOIN Contractor ctr on c.ContractorId = ctr.ContractorId
		JOIN ContractClassifierHistory cch on cch.KeyId = c.ContractId and cch.PeriodFrom <= @Perioddeb and cch.PeriodTo >= @Perioddeb
		JOIN ClassifierGroup cg on cg.ClassifierGroupId = cch.IntValue
		JOIN Classifier cls on cls.ClassifierId = CG.ClassifierId
	where cls.ClassifierGUID = @ClsContract
	GROUP BY 
		c.ContractId, CG.ShortName
	ORDER BY CG.ShortName
END
ELSE
BEGIN
	INSERT INTO #debRes$cok$
	SELECT 
		c.ContractId contract_id
		,CG.ShortName className
		,sum (do.EndRest) AS AllDebet
		,sum (CASE WHEN do.period=@Perioddeb THEN do.EndRest ELSE '0,00' END) AS MonthBefore_1
		,sum (CASE WHEN do.period IN(@2MonthBeforedeb,@3MonthBeforedeb) THEN do.EndRest ELSE '0,00' END) AS MonthBefore_2_3
		,sum (CASE WHEN do.period IN(@4MonthBeforedeb,@5MonthBeforedeb,@6MonthBeforedeb) THEN do.EndRest ELSE '0,00' END) AS MonthBefore_4_5_6
		,sum (CASE WHEN do.period<@6MonthBeforedeb AND do.period>=@1YearBefore THEN do.EndRest ELSE '0,00' END) AS YearBefore_6_12
		,sum (CASE WHEN do.period<@1YearBefore THEN do.EndRest ELSE '0,00' END) AS Before
	FROM (
		SELECT 
			o.ContractId, o.OperationId,o.CurrentPeriod period
			,sum(R.TotalWithVAT * RT.Sign) EndRest
		FROM Operation O 
			JOIN OperationRow R ON R.OperationId = O.OperationId 
			JOIN OperationRowType RT on R.OperationRowTypeId = RT.OperationRowTypeId
			JOIN OperationType OT on OT.OperationTypeId = O.OperationTypeId	
		WHERE 		r.IsAdmitted = 1 AND o.IsIncome = @IsIncomedeb AND o.Rest <>0.00
			AND o.PaymentKindId = @PaymentKindIddeb AND o.ContractId IS NOT NULL 
			AND O.ContractId IN (
				SELECT cch.KeyId
					FROM ContractClassifierHistory cch
					JOIN ClassifierGroup cg on cg.ClassifierGroupId = cch.IntValue
						WHERE cg.ClassifierGroupGUID=@ClassifierGroupGUIDdeb and
						cch.PeriodTo >= @Perioddeb and cch.PeriodFrom <= @Perioddeb
			)
		GROUP BY o.ContractId, o.OperationId,o.CurrentPeriod
	) do
		LEFT JOIN contract c ON c.contractId = do.contractId
		LEFT JOIN Contractor ctr on c.ContractorId = ctr.ContractorId
		JOIN ContractClassifierHistory cch on cch.KeyId = c.ContractId and cch.PeriodFrom <= @Perioddeb and cch.PeriodTo >= @Perioddeb
		JOIN ClassifierGroup cg on cg.ClassifierGroupId = cch.IntValue
		JOIN Classifier cls on cls.ClassifierId = CG.ClassifierId
	where cls.ClassifierGUID = @ClsContract
	GROUP BY 
		c.ContractId, CG.ShortName
	ORDER BY CG.ShortName
END


IF @ReportType = 1
begin
	select c.ContractNumber, d.AllDebet, d.MonthBefore_1, d.MonthBefore_2_3, d.MonthBefore_4_5_6, d.YearBefore_6_12, d.Before
	FROM #debRes$cok$ d
	JOIN Contract c on c.ContractId = d.contract_id
	where d.AllDebet >= @Borgdeb
end
else
begin
	select 
		d.className, SUM(d.AllDebet) AllDebet, SUM(d.MonthBefore_1) MonthBefore_1, 
		SUM(d.MonthBefore_2_3) MonthBefore_2_3, SUM(d.MonthBefore_4_5_6) MonthBefore_4_5_6, 
		SUM(d.YearBefore_6_12) YearBefore_6_12, SUM(d.Before) Before
	FROM #debRes$cok$ d
	where d.AllDebet >= @Borgdeb
	GROUP BY d.className
end
