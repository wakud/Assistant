SELECT	 d.[���������� ����]
		,d.����������
		,d.��������
		,d.[ʳ������ ����]
		,a.[������]
		,b.[�-�� 1]
		,c.[�-�� 3]
FROM (	SELECT	1 ID
		,COUNT(AccountNumber) [������]
		FROM AccountingCommon.Account
		WHERE DateTo = convert(DATETIME,'6/6/2079',103)) [a]
LEFT JOIN (
			--������������� 1 �����
		SELECT	1 ID
				,COUNT(a.AccountNumber) [�-�� 1]
				,SUM(o.RestSumm) [����]
		FROM FinanceMain.Operation o
		LEFT JOIN AccountingCommon.Account a ON a.AccountId = o.AccountId
		LEFT JOIN AccountingCommon.PhysicalPerson p ON p.PhysicalPersonId = a.PhysicalPersonId
		WHERE	PeriodTo=207906
				AND IsIncome=0
				AND DocumentTypeId IN (15)
				AND o.RestSumm>0
				AND o.Date<=DATEADD(mm, -1, GETDATE())
				) [b] ON b.ID = a.ID
LEFT JOIN (
			--������������� 3 ����� � �����
			SELECT	1 ID
					,COUNT(a.AccountNumber) [�-�� 3]
					,SUM(o.RestSumm) [����]
			FROM FinanceMain.Operation o
			LEFT JOIN AccountingCommon.Account a ON a.AccountId = o.AccountId
			LEFT JOIN AccountingCommon.PhysicalPerson p ON p.PhysicalPersonId = a.PhysicalPersonId
			WHERE	PeriodTo=207906
					AND IsIncome=0
					AND DocumentTypeId IN (15)
					AND o.RestSumm>0
					AND o.Date<=DATEADD(mm,-3,GETDATE())) [c] ON c.ID = a.ID
LEFT JOIN(
			--����� 1-4
		SELECT	1 ID
				,SUM(fs.DebetBegin) AS [���������� ����]
				,SUM(fs.ChargedSumm + fs.ChargedSummBudget) [����������]
				,SUM(fs.PaidCashSumm + fs.PaidSubsidySumm + fs.PaidWriteOffSumm) [��������]
				,SUM(fs.DebetEnd) [ʳ������ ����]
		FROM AccountingCommon.Account acc
		LEFT JOIN FinanceCommon.SupplierSaldo fs ON fs.AccountId = acc.AccountId
		WHERE	fs.Period = CONVERT(VARCHAR(6),DATEADD(mm,-1, GETDATE()),112) 
		) [d] ON d.ID = a.ID