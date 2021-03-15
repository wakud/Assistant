--declare @SetClsGUIDKstVn varchar(36) = 'F15F0DE7-47A2-43BB-8E74-509F56C12DFA' -- vc
--declare @SetClsGUIDKstVn varchar(36) = '4297FF9F-9DA2-43F3-AD23-3D47F18D552E' -- pup
--declare @SetClsGUIDKstVn varchar(36) = 'all' -- all
--declare @ReportTypeKstVn int = 3 -- 1, 2, 3
declare @ClsContractKstVn varchar(36) = '815B6ECE-F317-4A78-BED3-5B1CC6C72B1C' -- shoden

IF @SetClsGUIDKstVn = 'all'
	SET @SetClsGUIDKstVn = NULL

drop table if exists #ContractsCountKstVn$cok$

create table #ContractsCountKstVn$cok$ (
	clsName varchar(150) null,
	contract_number varchar(150),
	tariff_group varchar(150) null
)

insert into #ContractsCountKstVn$cok$
SELECT distinct null clsName, c.ContractNumber contract_number, null tariff_group
FROM Contract c
JOIN Object obj on c.ContractorId = obj.ContractorId
JOIN ClassifierGroupContract cgc on cgc.ContractId = c.ContractId
JOIN ClassifierGroup cg on cg.ClassifierGroupId = cgc.ClassifierGroupId
WHERE 1=1
	and c.ContractType = 0
	and c.ContractState = 1
	and (@SetClsGUIDKstVn is null or cg.ClassifierGroupGUID = @SetClsGUIDKstVn)
	and obj.ObjectState not in (4, 6)
	and obj.PeriodEnd = '2079-06-06'

update #ContractsCountKstVn$cok$
	set tariff_group = tg.TariffGroupName,
		clsName = cg.ShortName
FROM #ContractsCountKstVn$cok$ cc
JOIN Contract c on cc.contract_number = c.ContractNumber
JOIN ClassifierGroupContract cgc on cgc.ContractId = c.ContractId
JOIN ClassifierGroup cg on cg.ClassifierGroupId = cgc.ClassifierGroupId
JOIN Classifier cls on cls.ClassifierId = cg.ClassifierId
JOIN Object obj on obj.ContractorId = c.ContractorId
JOIN SchemeElement se on se.ObjectId = obj.ObjectId
JOIN SchemeCollector sc on se.SchemeElementId = sc.SchemeElementId
JOIN SchemeCollectorTariffGroup sctg on sctg.SchemeCollectorId = sc.SchemeCollectorId
JOIN TariffGroup tg on tg.TariffGroupId = sctg.TariffGroupId and tg.PaymentKindId = '15'
where 
	tg.TariffGroupName is not NULL
	and se.SchemeElementType = 11 and
	(se.PeriodEnd = 207906) and
	contract_number = c.ContractNumber and
	cls.ClassifierGUID = @ClsContractKstVn and
	(
		(@SetClsGUIDKstVn = 'F15F0DE7-47A2-43BB-8E74-509F56C12DFA' and tg.TariffGroupName not like '%універ%')
		or
		(@SetClsGUIDKstVn = '4297FF9F-9DA2-43F3-AD23-3D47F18D552E' and tg.TariffGroupName like '%універ%')
		or
		@SetClsGUIDKstVn is null
	)

IF @ReportTypeKstVn = 1
	select clsName [Класифікатор], contract_number [Номер договору], tariff_group [Тарифна група]
	from #ContractsCountKstVn$cok$
ELSE IF @ReportTypeKstVn = 2
	SELECT cc.clsName [Класифікатор], COUNT(cc.contract_number) [К-сть]
	FROM #ContractsCountKstVn$cok$ cc
	GROUP BY cc.clsName
ELSE
	SELECT cc.tariff_group [Тарифна група], COUNT(cc.contract_number) [К-сть]
	FROM #ContractsCountKstVn$cok$ cc
	GROUP BY cc.tariff_group
