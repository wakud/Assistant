

Select
	c.ContractNumber [Номер договору]
	,ct.ShortName [Найменування]
	,cg.ShortName [Постачальник]
	,obj.Name [Назва Об'єкту]
	,case
		when obj.ObjectState = 3 then 'Вимкнутий'
		when obj.ObjectState = 5 then 'Частково вимкнутий'
		else 'Увімкнутий'
	end [Стан об'єкту]
	,se.Name [Назва точки обліку]
	,sp.EIC [EIC]
FROM SchemePoint sp
JOIN SchemeElement se on se.SchemeElementId = sp.SchemeElementId
JOIN Object obj on obj.ObjectId = se.ObjectId
JOIN ContractObject co on co.ObjectId = obj.ObjectId
JOIN Contract c on c.ContractId = co.ContractId
JOIN ContractClassifierHistory cch on cch.KeyId = c.ContractId and cch.PeriodTo = 207906
JOIN ClassifierGroup cg on cg.ClassifierGroupId = cch.IntValue
JOIN Contractor ct on ct.ContractorId = c.ContractorId
where 1 = 1
	and c.ContractState > 0
	and c.ContractType = 0
	--and (sp.EIC is null or LEN(sp.EIC) <> 16)
	--and c.ContractNumber = '269'
	and cg.ClassifierGroupGuid in (
		'4297FF9F-9DA2-43F3-AD23-3D47F18D552E',
		'F15F0DE7-47A2-43BB-8E74-509F56C12DFA'
	)
	and se.SchemeElementType = 9
	and co.PeriodEnd = '2079-06-06'
	and obj.ObjectState not in (4, 6, 7)
	and obj.PeriodEnd = '2079-06-06'
	and se.PeriodEnd = 207906
order by obj.ObjectState, isnull(try_cast(c.ContractNumber as bigint), 0)
