--declare @SetContractNumber varchar(25) = ''
--declare @IsOnlyAnoutherSuppliers varchar(1) = 1

IF @SetContractNumber = ''
	SET @SetContractNumber = '*'

drop table if exists ##contractsnpst$cok$
drop table if exists ##resnpst$cok$

create table ##resnpst$cok$(
	contract_number varchar(20),
	new_supplier varchar(150),
	period_from varchar(6),
	period_to varchar(6)
)

select c.ContractNumber 
into ##contractsnpst$cok$
from Contract c
where 
	c.ContractState = 1 and 
	c.ContractType = 0 and
	c.ContractNumber like REPLACE(REPLACE(@SetContractNumber, '*', '%'), '?', '_')
ORDER BY ISNULL(TRY_CAST(c.ContractNumber as bigint), 0)

declare @cur_cn_npst varchar(25) = ''

WHILE ((SELECT COUNT(*) FROM ##contractsnpst$cok$) > 0)
BEGIN
	select @cur_cn_npst = (SELECT TOP 1 ContractNumber from ##contractsnpst$cok$)
	
	INSERT INTO ##resnpst$cok$
	SELECT 
		ct.ContractNumber as contract_number,
		cg.ShortName as new_supplier,
		cch.PeriodFrom as period_from,
		cch.PeriodTo as period_to
	FROM ContractClassifierHistory cch
	JOIN Contract ct on ct.ContractId = cch.KeyId
	JOIN ClassifierGroup cg on cch.IntValue = cg.ClassifierGroupId
	WHERE 
		ct.ContractNumber = @cur_cn_npst
		and IntValue in (
			SELECT cg.ClassifierGroupId
			FROM ClassifierGroup cg
			where
			cg.ClassifierId in (
				SELECT c.ClassifierId from Classifier c where c.ClassifierGUID = 'E76A591D-9242-41D6-91B9-F87F6856F119'
			) and (
				@IsOnlyAnoutherSuppliers <> 1 or
				cg.ClassifierGroupGUID not in (
					'F15F0DE7-47A2-43BB-8E74-509F56C12DFA',
					'4297FF9F-9DA2-43F3-AD23-3D47F18D552E'
				)
			)
		)
	ORDER BY cch.PeriodFrom

	IF EXISTS(SELECT TOP 1 * from ##resnpst$cok$ where contract_number = @cur_cn_npst)
		begin
			insert into ##resnpst$cok$
			select '------------', '-----------------------------------', '------', '------'
		end
	delete TOP(1) from ##contractsnpst$cok$
END

select 
	contract_number [Номер договору],
	new_supplier [Новий постачальник],
	CASE WHEN TRY_CAST(CAST(period_from as varchar(6)) + '01' as date) IS NULL
		THEN '--------'
		ELSE
		FORMAT(
			TRY_CAST(CAST(period_from as varchar(6)) + '01' as date),
			'Y',
			'uk-ua'
		)
	END
	[Період з],
	CASE
		WHEN period_to = '207906'
		THEN 'Триває'
	    WHEN TRY_CAST(CAST(period_to as varchar(6)) + '01' as date) IS NULL
		THEN '--------'
		ELSE
		FORMAT(
			TRY_CAST(CAST(period_to as varchar(6)) + '01' as date),
			'Y',
			'uk-ua'
		)
	END [Період до]
from ##resnpst$cok$

---- 'E76A591D-9242-41D6-91B9-F87F6856F119' - Класифікатор постачальник
---- 'F15F0DE7-47A2-43BB-8E74-509F56C12DFA' - вц
---- '4297FF9F-9DA2-43F3-AD23-3D47F18D552E' - пуп
