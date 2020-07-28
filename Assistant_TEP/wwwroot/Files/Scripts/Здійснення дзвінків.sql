
DROP TABLE IF EXISTS ##contracts
DROP TABLE IF EXISTS ##saldovka
DROP TABLE IF EXISTS ##lastPayment
DROP TABLE IF EXISTS ##results

DECLARE @CurPer int
--SET @CurPer = 201905
exec @CurPer = sfGetCurrentPeriod
DECLARE @PaymentKind int  = 15
DECLARE @ZeroSaldo int = 1

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
EXEC cpFinancialSaldoRollFilialByContract @CurPer, @PaymentKind, @ZeroSaldo

SELECT 
	c.ContractId,
	c.ContractNumber,
	ct.FullName
INTO ##contracts
FROM Contract c
JOIN Contractor ct on ct.ContractorId = c.ContractorId
WHERE 
	c.ContractState = 1 and 
	c.ContractType = 0

select 
	c.ContractId, 
	(
		SELECT TOP 1 o.TotalWithVAT * ot.Sign summ
		FROM Operation o
		JOIN OperationType ot on ot.OperationTypeId = o.OperationTypeId
		where 
		  o.PaymentKindId = 15 and o.ContractId = c.ContractId and
		  ot.IsPayment = 1  and ot.IsIncome = 1
		ORDER BY o.OperationDate DESC
	) summ,
	(
		SELECT TOP 1 o.OperationDate
		FROM Operation o
		JOIN OperationType ot on ot.OperationTypeId = o.OperationTypeId
		where 
			o.PaymentKindId = 15 and o.ContractId = c.ContractId and
			ot.IsPayment = 1  and ot.IsIncome = 1
		ORDER BY o.OperationDate DESC
	) dt
INTO ##lastPayment
FROM ##contracts c

select 
	--c.ContractId,
	c.ContractNumber 'Номер договору',
	c.FullName 'Назва споживача',
	ISNULL(
		CASE 
			WHEN monthBorg.CurrentPeriod IS NOT NULL
			THEN DATEDIFF(MONTH, CAST(monthBorg.CurrentPeriod as varchar) + '01', CAST(@CurPer as varchar) + '01') - 1
			ELSE DATEDIFF(MONTH, CAST(minMonth.CurrentPeriod as varchar) + '01', CAST(@CurPer as varchar) + '01')
		END
	, 0)'Тривалість виникнення боргу',
	(
		CASE 
			WHEN kwtBorg.debetKwt is not Null and kwtBorg.debetKwt >= 0
			THEN kwtBorg.debetKwt
			WHEN kwtBorg.debetKwt is not Null and kwtBorg.debetKwt < 0
			THEN 0.00
			ELSE s.UsageActive 
		END
	) 'Заборгованість кВт',
	s.DebetCM 'Заборгованість грн.',
	lp.dt 'Дата останньої оплати',
	lp.summ 'Сума останньої оплати'
INTO ##results
from ##contracts c
JOIN ##saldovka s on s.ContractId = c.ContractId
JOIN ##lastPayment lp on lp.ContractId = c.ContractId
outer apply (
		SELECT 
			ROUND(SUM(r.Quantity * rt.Sign), 2) debetKwt
		FROM [TR40_Juridical].[dbo].[Operation] o
		JOIN OperationRow r on r.OperationId = o.OperationId
		JOIN OperationRowType rt on rt.OperationRowTypeId = r.OperationRowTypeId
		JOIN OperationType ot on ot.OperationTypeId = o.OperationTypeId
		where 
			o.PaymentKindId = 15 and 
			r.IsAdmitted = 1 and 
			o.ContractId = c.ContractId	and
			r.CurrentPeriod <= @CurPer
) kwtBorg
outer apply (
	SELECT TOP 1 bcc.CurrentPeriod
	FROM BalanceCache bcc
	JOIN Operation oo on oo.OperationId = bcc.OperationId
	JOIN OperationType oot on oot.OperationTypeId = oo.OperationTypeId
	where 
		bcc.ContractId = c.ContractId and 
		oo.PaymentKindId = 15 and
		bcc.CurrentPeriod <= @CurPer
	GROUP BY bcc.CurrentPeriod
	HAVING SUM(bcc.EndRest * oot.SaldoSign) >= 0.00
	ORDER BY bcc.CurrentPeriod desc
) monthBorg
outer apply (
	SELECT TOP 1 bcc.CurrentPeriod
	FROM BalanceCache bcc
	JOIN Operation oo on oo.OperationId = bcc.OperationId
	where 
		bcc.ContractId = c.ContractId and
		oo.PaymentKindId = 15
	ORDER BY bcc.CurrentPeriod
) minMonth
where s.DebetCM > 0.00
ORDER BY CAST(c.ContractNumber as int)


select * 
from ##results