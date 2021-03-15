
Select
	c.ContractNumber [Номер договору]
	,ct.ShortName [Найменування]
	,obj.Name [Назва Об'єкту]
	,se.Name [Назва точки обліку]
	,sp.EIC [EIC]
FROM SchemePoint sp
JOIN SchemeElement se on se.SchemeElementId = sp.SchemeElementId
JOIN Object obj on obj.ObjectId = se.ObjectId
JOIN ContractObject co on co.ObjectId = obj.ObjectId
JOIN Contract c on c.ContractId = co.ContractId
JOIN Contractor ct on ct.ContractorId = c.ContractorId
where 1 = 1
	and c.ContractState > 0
	and c.ContractType = 0
	and (sp.EIC is null or LEN(sp.EIC) <> 16)
	--and c.ContractNumber = '269'
	and se.SchemeElementType = 9
	and co.PeriodEnd = '2079-06-06'
	and obj.ObjectState not in (4, 6, 7)
	and obj.PeriodEnd = '2079-06-06'
	and se.PeriodEnd = 207906
