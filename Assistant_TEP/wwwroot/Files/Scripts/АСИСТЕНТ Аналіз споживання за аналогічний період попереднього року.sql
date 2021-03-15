--declare @perAnSp int = 202102
--declare @reportTypeAnSp int = 1   -- 1 - ���� �������������, 2 - �������� �� ���������
--declare @isNamedAnSp bit = 1 -- ������ �� ��������
--declare @goToSupplierNow bit = 0 -- ������ �� �������� �� ������ ������������� ��������� �� ����� ������


declare @PerAsDate date = convert(date, cast(@perAnSp as varchar(6)) + '01')
declare @AnalogPeriodPrevYear int = @perAnSp - 100
declare @AnalogPerAsDate date = convert(date, cast(@AnalogPeriodPrevYear as varchar(6)) + '01')

drop table if exists ##resultsAnSp
drop table if exists ##AnoutherSupplierContracts
drop table if exists ##CurContracts
drop table if exists ##PrevContracts
drop table if exists ##saldovka

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
insert into ##saldovka
exec cpFinancialSaldoRollFilialByContract @CurrentPeriod = @AnalogPeriodPrevYear, @PaymentKindId = 15, @ShowZeroSaldo = 1


create table ##resultsAnSp(
	[����� ��������] varchar(20),
	[������������] varchar(255),
	[������������ � ��.����� ���. ����] varchar(255),
	[�������� ������������] varchar(255),
	[���������� � ��. ����� ���. ����] int,
	[�-��� �������� �� � ����� �����] int,
	[�-��� �������� �� � ��. ����� ���. ����] int,
)

-- current period

select 
	c.ContractId,
	c.ContractNumber,
	cg.ShortName SupplierName,
	cg.ClassifierGroupGUID,
	cch.PeriodFrom SupplierStart,
	ct.ShortName ContractorName,
	COUNT(se.SchemeElementId) points
into ##CurContracts
from Contract c
JOIN Contractor ct on ct.ContractorId = c.ContractorId
JOIN ContractClassifierHistory cch on cch.KeyId = c.ContractId
JOIN ClassifierGroup cg on cg.ClassifierGroupId = cch.IntValue
JOIN Classifier cls on cls.ClassifierId = cg.ClassifierId
JOIN ContractObject co on co.ContractId = c.ContractId and co.PeriodEnd >= @PerAsDate
JOIN Object o on o.ObjectId = co.ObjectId
JOIN SchemeElement se on se.ObjectId = o.ObjectId and se.PeriodEnd >= @perAnSp
where 1 = 1
	and c.ContractType = 0
	and c.ContractState in (1, 3)
	and o.PeriodEnd >= @PerAsDate
	and cch.PeriodFrom <= @perAnSp
	and cch.PeriodTo >= @perAnSp
	and cls.ClassifierGUID = 'E76A591D-9242-41D6-91B9-F87F6856F119'  --������������
GROUP BY c.ContractId, c.ContractNumber, ct.ShortName, cg.ShortName, cch.PeriodFrom, cg.ClassifierGroupGUID, cch.PeriodTo


-- analog prev period

select
	c.ContractId,
	c.ContractNumber,
	cg.ShortName SupplierName,
	cch.PeriodFrom SupplierStart,
	cch.PeriodTo SupplierEnd,
	cg.ClassifierGroupGUID,
	COUNT(se.SchemeElementId) points
into ##PrevContracts
from Contract c
JOIN ContractClassifierHistory cch on cch.KeyId = c.ContractId
JOIN ClassifierGroup cg on cg.ClassifierGroupId = cch.IntValue
JOIN Classifier cls on cls.ClassifierId = cg.ClassifierId
JOIN ContractObject co on co.ContractId = c.ContractId and co.PeriodEnd >= @AnalogPerAsDate
JOIN Object o on o.ObjectId = co.ObjectId
JOIN SchemeElement se on se.ObjectId = o.ObjectId and se.PeriodEnd >= @AnalogPeriodPrevYear
where 1 = 1
	and c.ContractType = 0
	and c.ContractState in (1, 3)
	and o.PeriodEnd >= @AnalogPerAsDate
	and cch.PeriodFrom <= @AnalogPeriodPrevYear
	and cch.PeriodTo >= @AnalogPeriodPrevYear
	and cls.ClassifierGUID = 'E76A591D-9242-41D6-91B9-F87F6856F119'  --������������
GROUP BY c.ContractId, c.ContractNumber, cg.ShortName, cch.PeriodFrom, cch.PeriodTo, cg.ClassifierGroupGUID


-- main
insert into ##resultsAnSp
select
	cc.ContractNumber [����� ��������]
	,cc.ContractorName [������������]
	,pc.SupplierName [������������ � ��.����� ���. ����]
	,cc.SupplierName [�������� ������������]
	,s.UsageActive [���������� � ��. ����� ���. ����]
	,cc.points [�-��� �������� �� � ����� �����]
	,pc.points [�-��� �������� �� � ��. ����� ���. ����]
FROM ##CurContracts cc
JOIN ##PrevContracts pc on pc.ContractId = cc.ContractId
JOIN ##saldovka s on s.ContractId = cc.ContractId
where 1 = 1
	and (
		(
			@reportTypeAnSp = 1
			and cc.ClassifierGroupGUID not in (
				'F15F0DE7-47A2-43BB-8E74-509F56C12DFA' -- ��� ��
				,'4297FF9F-9DA2-43F3-AD23-3D47F18D552E' -- ��� ��
			)
			and pc.ClassifierGroupGUID in (
				'F15F0DE7-47A2-43BB-8E74-509F56C12DFA' -- ��� ��
				,'4297FF9F-9DA2-43F3-AD23-3D47F18D552E' -- ��� ��
			)
		)
		or (
			@reportTypeAnSp = 2
			and cc.ClassifierGroupGUID in (
				'F15F0DE7-47A2-43BB-8E74-509F56C12DFA' -- ��� ��
				,'4297FF9F-9DA2-43F3-AD23-3D47F18D552E' -- ��� ��
			)
			and pc.ClassifierGroupGUID in (
				'F15F0DE7-47A2-43BB-8E74-509F56C12DFA' -- ��� ��
				,'4297FF9F-9DA2-43F3-AD23-3D47F18D552E' -- ��� ��
			)
			and pc.ClassifierGroupGUID <> cc.ClassifierGroupGUID
		)
	)
	and (@goToSupplierNow = 0 or (@goToSupplierNow = 1 and cc.SupplierStart = @perAnSp))


insert into ##resultsAnSp
select
	'������' [����� ��������]
	,'' [������������]
	,'' [������������ � ��.����� ���. ����]
	,'' [�������� ������������]
	,sum(r.[���������� � ��. ����� ���. ����]) [���������� � ��. ����� ���. ����]
	,sum(r.[�-��� �������� �� � ����� �����]) [�-��� �������� �� � ����� �����]
	,sum(r.[�-��� �������� �� � ��. ����� ���. ����]) [�-��� �������� �� � ��. ����� ���. ����]
from ##resultsAnSp r

select * 
from ##resultsAnSp
