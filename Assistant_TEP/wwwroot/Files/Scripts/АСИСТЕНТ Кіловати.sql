--declare @SetClsGUID nvarchar(36) = N'F15F0DE7-47A2-43BB-8E74-509F56C12DFA'
--declare @SetPeriod nvarchar(6) = N'202101'
--declare @IsNamedkwta nvarchar(1) = N'0'

DECLARE 
	@Periodkwta INT,
	@PeriodMaxkwta INT,
	@ClassifierGroupGUIDkwta varchar(36),
	@PeriodStartkwta int = CAST(@SetPeriod as int),
	@PaymentKindkwta int = 15,
	@LastPeriodDay date = DATEADD(DAY, -1, DATEADD(MONTH, 1, CONVERT(DATE, cast(@SetPeriod as varchar(6)) + '01')))

SET @Periodkwta = @PeriodStartkwta
SET @PeriodMaxkwta = @PeriodStartkwta
SET @ClassifierGroupGUIDkwta = CAST(@SetClsGUID as varchar(36))


DROP TABLE IF EXISTS [##consumption$cok$]
DROP TABLE IF EXISTS [##ress$cok$]
DROP TABLE IF EXISTS [##all_free_objects$cok$]
DROP TABLE IF EXISTS [##contracts_with_power$cok$]
DROP TABLE IF EXISTS [##saldovka$cok$]
DROP TABLE IF EXISTS [##total$cok$]


create table [##saldovka$cok$](
	ContractId int,
	ContractNumber varchar(50),
	ShortName varchar(100),
	DebetPM money,
	DebetPMVAT money,
	CreditPM money,
	CreditPMVAT money,
	Charged money,
	ChargedVAT money,
	Payment money,
	PaymentVAT money,
	WriteOff money,
	WriteOffVAT money,
	InternalSaldo money,
	InternalSaldoVAT money,
	UsageActive int,
	UsageReactive int,
	UsageGeneration int,
	DebetCM money,
	DebetCMVAT money,
	CreditCM money,
	CreditCMVAT money
)

DECLARE @cur_db varchar(25) = '$cok$'

create table [##consumption$cok$](period int, usage int, contract_id int, summ money, debetkm money)

CREATE TABLE [##ress$cok$](db varchar(25), cn varchar(125), FullName varchar(250), usg int, summa money, debetkm money)

create table [##all_free_objects$cok$](ObjectId int, ContractId int)

create table [##contracts_with_power$cok$](ContractNumber varchar(125), Allowed int, isBudget bit)

create table [##total$cok$](
	[Номер договору] varchar(25),
	Тип varchar(50),
	[К-сть споживачів] int,
	[Обсяг кВт] float,
	[Нараховано грн. без ПДВ] money,
	[Д-Т заборгованість, грн. без ПДВ] money
)

	select @Periodkwta = @PeriodStartkwta
	WHILE @Periodkwta <= @PeriodMaxkwta
	BEGIN
		insert into [##saldovka$cok$]
		exec cpFinancialSaldoRollFilialByContract @CurrentPeriod = @Periodkwta, @PaymentKindId = @PaymentKindkwta, @ShowZeroSaldo = 1
		
		insert into [##consumption$cok$](period, usage, contract_id, summ, debetkm)
		SELECT @Periodkwta, UsageActive, ContractId, (Charged - ChargedVat), (DebetCM - DebetCMVat)
		FROM [##saldovka$cok$]

		delete from [##saldovka$cok$]

	select @Periodkwta = case 
		when SUBSTRING(CAST(@Periodkwta as varchar), 5, 2) = '12' 
		then CAST((CAST((SUBSTRING(CAST(@Periodkwta as varchar), 1, 4) + 1) AS VARCHAR) + '01') AS INT) 
		else @Periodkwta + 1 
		END

	END

	INSERT INTO [##ress$cok$](db, cn, FullName, usg, summa, debetkm)
	SELECT
		@cur_db db,
		c.ContractNumber cn, 
		ct.FullName,
		SUM(cons.usage) usg,
		SUM(cons.summ) summa,
		SUM(cons.debetkm) debetkm
	FROM [##consumption$cok$] cons
	JOIN Contract c on c.ContractId = cons.contract_id
	JOIN Contractor ct on ct.ContractorId = c.ContractorId
	JOIN ContractClassifierHistory cch on cch.KeyId = c.ContractId and cch.PeriodFrom <= @SetPeriod and cch.PeriodTo >= @SetPeriod
	JOIN ClassifierGroup cg on cg.ClassifierGroupId = cch.IntValue
	where cg.ClassifierGroupGUID = @ClassifierGroupGUIDkwta
	GROUP BY c.ContractNumber, ct.FullName

	insert into [##all_free_objects$cok$]
	SELECT obj.ObjectId, c.ContractId
	FROM Object obj
	JOIN ContractObject co on co.ObjectId = obj.ObjectId
	JOIN Contract c on c.ContractId = co.ContractId
	JOIN ContractClassifierHistory cch on cch.KeyId = c.ContractId and cch.PeriodFrom <= @SetPeriod and cch.PeriodTo >= @SetPeriod
	JOIN ClassifierGroup cg on cg.ClassifierGroupId = cch.IntValue
	WHERE 1=1
		and cg.ClassifierGroupGUID = @ClassifierGroupGUIDkwta
		and obj.PeriodEnd >= @LastPeriodDay
		and co.PeriodEnd >= @LastPeriodDay
		and obj.ObjectState not in (4, 6)

	insert into [##contracts_with_power$cok$]
	SELECT 
		c.ContractNumber,
		MAX(obj.AllowPower) as Allowed,
		(
			CASE WHEN cg.ClassifierGroupGUID in (
				'B6182914-75D3-4A2B-9338-8B0BE0F94809',
				'45870DF2-9D44-44C8-9F3D-C1D4839DD60C',
				'A1CDA09A-5498-4744-85A6-442192E47267',
				'E7616E6F-6A36-4447-BB8D-A0DD73BD90F0',
				'4C0E46B5-A2DA-4E2B-94CF-D1EE5F722FA6',
				'B6182814-75D3-4A2B-9338-8B0BE0F94809',
				'E7616E6F-6A36-4447-BB8D-A0DD73BD90F0'
			) THEN 1
			ELSE 0 END
		) as isBudget
	from [##all_free_objects$cok$] af
	JOIN Contract c on c.ContractId = af.ContractId
	JOIN ContractObject co on co.ContractId = c.ContractId
	JOIN Object obj on obj.ObjectId = co.ObjectId
	JOIN ContractClassifierHistory cch on cch.KeyId = c.ContractId and cch.PeriodFrom <= @SetPeriod and cch.PeriodTo >= @SetPeriod
	JOIN ClassifierGroup cg on cg.ClassifierGroupId = cch.IntValue
	JOIN Classifier cls on cls.ClassifierId = cg.ClassifierId
	WHERE 1=1
		and c.ContractState = 1 
		and c.ContractType = 0 
		and co.PeriodEnd >= @LastPeriodDay
		AND obj.PeriodEnd >= @LastPeriodDay
		and obj.ObjectState not in ('4', '6')
		AND cls.ClassifierGUID = '815B6ECE-F317-4A78-BED3-5B1CC6C72B1C'
	GROUP BY c.ContractNumber, cg.ClassifierGroupGUID

	IF @IsNamedkwta = 0
	BEGIN
		insert into [##total$cok$]
		select
			@cur_db as [Номер договору],
			'Менше 50' as 'Тип',
			count(cn) as [К-сть споживачів], 
			sum(usg) as [Обсяг кВт], 
			sum(summa) as [Нараховано грн. без ПДВ],
			sum(debetkm) as [Д-Т заборгованість, грн. без ПДВ]
		from [##ress$cok$]
		JOIN [##contracts_with_power$cok$] cwp on cwp.ContractNumber = [##ress$cok$].cn
		where Allowed <= 50 and cwp.isBudget = 0
		GROUP BY [##ress$cok$].db

		insert into [##total$cok$]
		select 
			@cur_db as [Номер договору],
			'Бюджетні' as 'Тип',
			count(cn) as [К-сть споживачів], 
			sum(usg) as [Обсяг кВт], 
			sum(summa) as [Нараховано грн. без ПДВ],
			sum(debetkm) as [Д-Т заборгованість, грн. без ПДВ]
		from [##ress$cok$]
		JOIN [##contracts_with_power$cok$] cwp on cwp.ContractNumber = [##ress$cok$].cn
		where cwp.isBudget = 1
		GROUP BY [##ress$cok$].db

		insert into [##total$cok$]
		select 
			@cur_db as [Номер договору],
			'Більше 50 і менше 150' as 'Тип',
			count(cn) as [К-сть споживачів], 
			sum(usg) as [Обсяг кВт], 
			sum(summa) as [Нараховано грн. без ПДВ],
			sum(debetkm) as [Д-Т заборгованість, грн. без ПДВ]
		from [##ress$cok$]
		JOIN [##contracts_with_power$cok$] cwp on cwp.ContractNumber = [##ress$cok$].cn
		where Allowed > 50 and Allowed <= 150 and cwp.isBudget = 0
		GROUP BY [##ress$cok$].db

		insert into [##total$cok$]
		select 
			@cur_db as [Номер договору],
			'Більше 150' as 'Тип',
			count(cn) as [К-сть споживачів], 
			sum(usg) as [Обсяг кВт], 
			sum(summa) as [Нараховано грн. без ПДВ],
			sum(debetkm) as [Д-Т заборгованість, грн. без ПДВ]
		from [##ress$cok$]
		JOIN [##contracts_with_power$cok$] cwp on cwp.ContractNumber = [##ress$cok$].cn
		where Allowed > 150 and cwp.isBudget = 0
		GROUP BY [##ress$cok$].db
	END
ELSE
	BEGIN
		INSERT INTO [##total$cok$]
		select
			cn as [Номер договору],
			(
				CASE
					WHEN cwp.Allowed > 150 and cwp.isBudget = 0 THEN 'Більше 150'
					WHEN cwp.Allowed > 50 and cwp.Allowed <= 150 and cwp.isBudget = 0 THEN 'Більше 50 і менше 150'
					WHEN cwp.isBudget = 1 then 'Бюджетні'
					WHEN cwp.Allowed <= 50 and cwp.isBudget = 0 THEN 'Менше 50'
			    END
			) as 'Тип',
			1 as [К-сть споживачів], 
			usg as [Обсяг кВт], 
			summa as [Нараховано грн. без ПДВ],
			debetkm as [Д-Т заборгованість, грн. без ПДВ]
		FROM [##ress$cok$]
		JOIN [##contracts_with_power$cok$] cwp on cwp.ContractNumber = [##ress$cok$].cn
	END
select * from [##total$cok$]

