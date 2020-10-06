
drop table if exists ##res_his_mesh

SELECT
	acc.AccountNumber ��,
	FORMAT(apph.DateFrom, 'dd.MM.yyyy', 'uk') '��',
	pp.FullName ϲ�,
	pp.IdentificationCode '���������������� ���',
	(
		case 
		when apph.DateTo <> '2079-06-06'
		then isnull(addr.FullAddress, '�� �������')
		else isnull(addr_account.FullAddress, '�� �������')
		end
	)'�����',
	(pp.PassportSeries + pp.PassportNumber) '�������',
	(
		case
		when apph.DateTo = '2079-06-06'
		then '�������� ���'
		else FORMAT(apph.DateTo, 'dd.MM.yyyy', 'uk')
		end
	) '��'
into ##res_his_mesh
FROM [AccountingCommon].[AccountPhysicalPersonHistory] apph
JOIN AccountingCommon.Account acc on apph.AccountId = acc.AccountId
JOIN AccountingCommon.PhysicalPerson pp on apph.PhysicalPersonId = pp.PhysicalPersonId
LEFT JOIN AccountingCommon.Address addr on pp.AddressId = addr.AddressId
LEFT JOIN AccountingCommon.Address addr_account on acc.AddressId = addr_account.AddressId
--where acc.AccountNumber = '231'

SELECT r.*
FROM ##res_his_mesh r
where r.�� not in (
	select rr.��
	from ##res_his_mesh rr
	group by rr.��
	having count(*) = 1
)
order by ISNULL(TRY_CAST(r.�� as bigint), 0), r.�� DESC, r.�� DESC
