
drop table if exists ##res_his_mesh

SELECT
	acc.AccountNumber ОР,
	FORMAT(apph.DateFrom, 'dd.MM.yyyy', 'uk') 'Із',
	pp.FullName ПІБ,
	pp.IdentificationCode 'Ідентифікаційний код',
	(
		case 
		when apph.DateTo <> '2079-06-06'
		then isnull(addr.FullAddress, 'Не внесено')
		else isnull(addr_account.FullAddress, 'Не внесено')
		end
	)'Адрес',
	(pp.PassportSeries + pp.PassportNumber) 'Паспорт',
	(
		case
		when apph.DateTo = '2079-06-06'
		then 'теперішній час'
		else FORMAT(apph.DateTo, 'dd.MM.yyyy', 'uk')
		end
	) 'По'
into ##res_his_mesh
FROM [AccountingCommon].[AccountPhysicalPersonHistory] apph
JOIN AccountingCommon.Account acc on apph.AccountId = acc.AccountId
JOIN AccountingCommon.PhysicalPerson pp on apph.PhysicalPersonId = pp.PhysicalPersonId
LEFT JOIN AccountingCommon.Address addr on pp.AddressId = addr.AddressId
LEFT JOIN AccountingCommon.Address addr_account on acc.AddressId = addr_account.AddressId
--where acc.AccountNumber = '231'

SELECT r.*
FROM ##res_his_mesh r
where r.ОР not in (
	select rr.ОР
	from ##res_his_mesh rr
	group by rr.ОР
	having count(*) = 1
)
order by ISNULL(TRY_CAST(r.ОР as bigint), 0), r.По DESC, r.Із DESC
