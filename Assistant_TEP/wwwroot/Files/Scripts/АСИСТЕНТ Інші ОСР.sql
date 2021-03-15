
declare @classifierTOEAnoutherDistributors varchar(36) = 'C8BFE7D5-DFE8-47C7-88FB-9FD2E095CB06'
declare @classifierDistributors varchar(36) = '6F8E1057-0996-45AA-BE47-341F47155E62'

select c.ContractNumber Номер_Договору, ct.ShortName Найменування, cg.ShortName ОСР
FROM Contract c
JOIN Contractor ct on ct.ContractorId = c.ContractorId
JOIN ContractClassifierHistory cch on cch.KeyId = c.ContractId
JOIN ClassifierGroup cg on cg.ClassifierGroupId = cch.IntValue
JOIN Classifier cls on cls.ClassifierId = cg.ClassifierId
where 1 = 1
	and c.ContractState = 1
	and c.ContractType > 0
	and cls.ClassifierGUID = @classifierDistributors
	and cg.ClassifierGroupGUID <> @classifierTOEAnoutherDistributors
