--use TR40_Juridical
--declare @perDistrib int = 202101

drop table if exists ##saldovkaDistrib$cok$
create table ##saldovkaDistrib$cok$(
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
insert into ##saldovkaDistrib$cok$
exec cpFinancialSaldoRollFilialByContract @CurrentPeriod = @perDistrib, @PaymentKindId = 15, @ShowZeroSaldo = 1


select distinct 
	c.ContractNumber [Номер договору],
	ct.ShortName [Назва споживача],
	sd.UsageActive [Споживання, кВт*год],
	sd.Charged [Нараховано, грн]
from Contract c
JOIN Contractor ct on ct.ContractorId = c.ContractorId
JOIN ##saldovkaDistrib$cok$ sd on sd.ContractId = c.ContractId
WHERE 1 = 1
	and c.ContractType = 0
	and c.ContractState > 0
	and c.IsCalculateByDistribution = 1
