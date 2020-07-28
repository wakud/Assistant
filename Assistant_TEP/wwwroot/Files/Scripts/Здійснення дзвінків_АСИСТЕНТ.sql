DROP TABLE IF EXISTS ##contracts
DROP TABLE IF EXISTS ##saldovka
DROP TABLE IF EXISTS ##lastPayment
DROP TABLE IF EXISTS ##results
DROP TABLE IF EXISTS ##periods

DECLARE @CurPer int
exec @CurPer = sfGetCurrentPeriod
DECLARE @PaymentKind int  = 15
DECLARE @ZeroSaldo int = 1
DECLARE @StartPeriod int = 201901
DECLARE @CurrentPeriodFill int = @StartPeriod
DECLARE @EndPeriod int = @CurPer

--IF @minBorgForInterval <= 0.00
--	SET @minBorgForInterval = 0.01

create table ##saldovka(
	CurrentPeriod int NULL,
	ContractId int NULL,
	ContractNumber varchar(50) NULL,
	ShortName varchar(100) NULL,
	DebetPM money NULL,
	DebetPMVAT money NULL,
	CreditPM money NULL,
	CreditPMVAT money NULL,
	Charged money NULL,
	ChargedVAT money NULL,
	Payment money NULL,
	PaymentVAT money NULL,
	WriteOff money NULL,
	WriteOffVAT money NULL,
	InternalSaldo money NULL,
	InternalSaldoVAT money NULL,
	UsageActive int NULL,
	UsageReactive int NULL,
	UsageGeneration int NULL,
	DebetCM money NULL,
	DebetCMVAT money NULL,
	CreditCM money NULL,
	CreditCMVAT money NULL
)

INSERT INTO ##saldovka(
		ContractId,
		ContractNumber,
		ShortName,
		DebetPM,
		DebetPMVAT,
		CreditPM,
		CreditPMVAT,
		Charged,
		ChargedVAT,
		Payment,
		PaymentVAT,
		WriteOff,
		WriteOffVAT,
		InternalSaldo,
		InternalSaldoVAT,
		UsageActive,
		UsageReactive,
		UsageGeneration,
		DebetCM,
		DebetCMVAT,
		CreditCM,
		CreditCMVAT
	)
	EXEC cpFinancialSaldoRollFilialByContract @CurPer, @PaymentKind, @ZeroSaldo
	UPDATE ##saldovka
		SET CurrentPeriod = @CurPer
	where CurrentPeriod is null

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
	opl.summ,
	opl.OperationDate dt
INTO ##lastPayment
FROM ##contracts c
outer apply (
	SELECT TOP 1 o.OperationDate, o.TotalWithVAT * ot.Sign summ
	FROM Operation o
	JOIN OperationType ot on ot.OperationTypeId = o.OperationTypeId
	where 
		o.PaymentKindId = 15 and o.ContractId = c.ContractId and
		ot.IsPayment = 1  and ot.IsIncome = 1 and o.CurrentPeriod < @CurPer
	ORDER BY o.OperationDate DESC
) opl
	
select
	c.ContractId,
	c.ContractNumber 'Номер договору',
	c.FullName 'Назва споживача',
	(
		SELECT COUNT(*)
		FROM Operation o
		where 
		o.Rest >= @minBorgForInterval and 
		o.ContractId = c.ContractId and 
		o.CurrentPeriod > @StartPeriod and 
		o.PaymentKindId = @PaymentKind
	)'Тривалість виникнення боргу',
	(
		CASE
			WHEN kwtBorg.debetKwt is not Null and kwtBorg.debetKwt >= 1
			THEN 
				kwtBorg.debetKwt
			ELSE 0.00
		END
	) 'Заборгованість кВт',
	s.DebetPM 'Заборгованість грн.',
	lp.dt 'Дата останньої оплати',
	lp.summ 'Сума останньої оплати'
INTO ##results
from ##contracts c
JOIN ##saldovka s on s.ContractId = c.ContractId and s.CurrentPeriod = @CurPer
JOIN ##lastPayment lp on lp.ContractId = c.ContractId
JOIN Contract ctd on ctd.ContractId = c.ContractId
outer apply (
	SELECT
		SUM(r.Quantity * rt.Sign) debetKwt
	FROM Operation o
	JOIN OperationRow r on r.OperationId = o.OperationId
	JOIN OperationRowType rt on rt.OperationRowTypeId = r.OperationRowTypeId
	JOIN OperationType ot on ot.OperationTypeId = o.OperationTypeId
	where
		o.PaymentKindId = 15 and
		r.IsAdmitted = 1 and
		o.ContractId = c.ContractId	and
		r.CurrentPeriod < @CurPer
) kwtBorg
outer apply (
	SELECT MIN(CurrentPeriod) CurrentPeriod
	FROM ##saldovka
	where
		ContractId = c.ContractId and
		DebetPM is not NULL
	group by ContractId
) minMonth
where s.DebetPM >= @minBorg

select * 
from ##results
where [Тривалість виникнення боргу] >= @minIntervalBorg
ORDER BY ISNULL(TRY_CAST([Номер договору] as int), 0)
