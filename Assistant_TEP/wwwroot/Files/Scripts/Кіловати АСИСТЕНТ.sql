--declare @SetClsGUID varchar(36) = 'F15F0DE7-47A2-43BB-8E74-509F56C12DFA'
--declare @SetPeriod varchar(6) = '202007'

DECLARE 
	@Period INT,
	@PeriodMax INT,
	@ClassifierGroupGUID varchar(36),
	@PeriodStart int = CAST(@SetPeriod as int),
	@PaymentKind int = 15

SET @Period = @PeriodStart
SET @PeriodMax = CAST(@SetPeriod as int)
SET @ClassifierGroupGUID = CAST(@SetClsGUID as varchar(36))

DROP TABLE IF EXISTS ##new_suppliers
DROP TABLE IF EXISTS ##consumption
DROP TABLE IF EXISTS ##ress
DROP TABLE IF EXISTS ##databases
DROP TABLE IF EXISTS ##all_free_objects
DROP TABLE IF EXISTS ##contracts_with_power
DROP TABLE IF EXISTS ##saldovka
DROP TABLE IF EXISTS ##total


create table ##saldovka(
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

create table ##consumption(period int, usage int, contract_id int, summ money, debetkm money)

CREATE TABLE ##ress(db varchar(25), cn varchar(125), FullName varchar(250), usg int, summa money, debetkm money)

create table ##all_free_objects(ObjectId int, ContractId int)

create table ##contracts_with_power(ContractNumber varchar(125), Allowed int, isBudget bit)

create table ##total(
	db varchar(25),
	Тип varchar(50),
	[К-сть споживачів] int,
	[Обсяг кВт] float,
	[Нараховано грн. без ПДВ] money,
	[Д-Т заборгованість, грн. без ПДВ] money
)

	select @Period = @PeriodStart
	WHILE @Period <= @PeriodMax
	BEGIN
		insert into ##saldovka
		exec cpFinancialSaldoRollFilialByContract @CurrentPeriod = @Period, @PaymentKindId = @PaymentKind, @ShowZeroSaldo = 1
		
		insert into ##consumption(period, usage, contract_id, summ, debetkm)
		SELECT @Period, UsageActive, ContractId, (Charged - ChargedVat), (DebetCM - DebetCMVat)
		FROM ##saldovka

		delete from ##saldovka

	select @Period = case 
		when SUBSTRING(CAST(@Period as varchar), 5, 2) = '12' 
		then CAST((CAST((SUBSTRING(CAST(@Period as varchar), 1, 4) + 1) AS VARCHAR) + '01') AS INT) 
		else @Period + 1 
		END

	END

	INSERT INTO ##ress(db, cn, FullName, usg, summa, debetkm)
	SELECT
		@cur_db db,
		c.ContractNumber cn, 
		ct.FullName,
		AVG(cons.usage) usg,
		SUM(cons.summ) summa,
		SUM(cons.debetkm) debetkm
	FROM ##consumption cons
	JOIN Contract c on c.ContractId = cons.contract_id
	JOIN Contractor ct on ct.ContractorId = c.ContractorId
	JOIN ClassifierGroupContract cgc on cgc.ContractId = c.ContractId
	JOIN ClassifierGroup cg on cg.ClassifierGroupId = cgc.ClassifierGroupId
	where cg.ClassifierGroupGUID = @ClassifierGroupGUID
	GROUP BY c.ContractNumber, ct.FullName

	insert into ##all_free_objects
	SELECT obj.ObjectId, c.ContractId
	FROM Object obj
	JOIN Contract c on c.ContractorId = obj.ContractorId
	JOIN ClassifierGroupContract cgc on cgc.ContractId = c.ContractId
	JOIN ClassifierGroup cg on cg.ClassifierGroupId = cgc.ClassifierGroupId
	WHERE 1=1
		and cg.ClassifierGroupGUID = @ClassifierGroupGUID
		and obj.PeriodEnd = '2079-06-06'
		and obj.ObjectState not in (4, 6)

	insert into ##contracts_with_power
	SELECT 
		c.ContractNumber,
		MAX(obj.AllowPower) as Allowed,
		(
			CASE WHEN cg.ClassifierGroupGUID in (
				'B6182814-75D3-4A2B-9338-8B0BE0F94809',
				'45870DF2-9D44-44C8-9F3D-C1D4839DD60C',
				'A1CDA09A-5498-4744-85A6-442192E47267',
				'E7616E6F-6A36-4447-BB8D-A0DD73BD90F0' 
			) THEN 1
			ELSE 0 END
		) as isBudget
	from ##all_free_objects af
	JOIN Contract c on c.ContractId = af.ContractId
	JOIN Object obj on obj.ContractorId = c.ContractorId
	JOIN ClassifierGroupContract cgc on c.ContractId = cgc.ContractId
	JOIN ClassifierGroup cg on cg.ClassifierGroupId = cgc.ClassifierGroupId
	WHERE 1=1
		and c.ContractState = 1 
		and c.ContractType = 0 
		AND obj.PeriodEnd = '2079-06-06'
		and obj.ObjectState not in ('4', '6')
		AND cg.ClassifierId = 137
	GROUP BY c.ContractNumber, cg.ClassifierGroupGUID

	insert into ##total
	select
		@cur_db as db,
		'Менше 50' as 'Тип',
		count(cn) as [К-сть споживачів], 
		sum(usg) as [Обсяг кВт], 
		sum(summa) as [Нараховано грн. без ПДВ],
		sum(debetkm) as [Д-Т заборгованість, грн. без ПДВ]
	from ##ress
	JOIN ##contracts_with_power cwp on cwp.ContractNumber = ##ress.cn
	where Allowed <= 50 and cwp.isBudget = 0
	GROUP BY ##ress.db

	insert into ##total
	select 
		@cur_db as db,
		'Бюджетні' as 'Тип',
		count(cn) as [К-сть споживачів], 
		sum(usg) as [Обсяг кВт], 
		sum(summa) as [Нараховано грн. без ПДВ],
		sum(debetkm) as [Д-Т заборгованість, грн. без ПДВ]
	from ##ress
	JOIN ##contracts_with_power cwp on cwp.ContractNumber = ##ress.cn
	where cwp.isBudget = 1
	GROUP BY ##ress.db

	insert into ##total
	select 
		@cur_db as db,
		'Більше 50 і менше 150' as 'Тип',
		count(cn) as [К-сть споживачів], 
		sum(usg) as [Обсяг кВт], 
		sum(summa) as [Нараховано грн. без ПДВ],
		sum(debetkm) as [Д-Т заборгованість, грн. без ПДВ]
	from ##ress
	JOIN ##contracts_with_power cwp on cwp.ContractNumber = ##ress.cn
	where Allowed > 50 and Allowed <= 150 and cwp.isBudget = 0
	GROUP BY ##ress.db

	insert into ##total
	select 
		@cur_db as db,
		'Більше 150' as 'Тип',
		count(cn) as [К-сть споживачів], 
		sum(usg) as [Обсяг кВт], 
		sum(summa) as [Нараховано грн. без ПДВ],
		sum(debetkm) as [Д-Т заборгованість, грн. без ПДВ]
	from ##ress
	JOIN ##contracts_with_power cwp on cwp.ContractNumber = ##ress.cn
	where Allowed > 150 and cwp.isBudget = 0
	GROUP BY ##ress.db

select * from ##total

