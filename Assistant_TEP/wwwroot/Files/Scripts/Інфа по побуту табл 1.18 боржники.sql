DROP TABLE IF EXISTS ##qw
DECLARE @ExBill INT 
SET @ExBill =       --����� ��������� ����� �� ��������
	(SELECT TOP 1 cast([Value] as int)  FROM [Services].[Setting] WHERE [Guid] = '826C4666-F79C-4558-A0BB-2D5A428FCE1B')  

SELECT	 AccountId [qwer]
		,SUM(o.RestSumm) [����]
		,MAX(DATEDIFF(MONTH, CAST(o.Date AS DATE), GETDATE())) AS [�����]
INTO ##qw
FROM FinanceMain.Operation o
WHERE PeriodTo=207906
	AND IsIncome=0
	AND DocumentTypeId IN (15)
	AND o.RestSumm>0
	AND o.Date<=DATEADD(dd,-@ExBill,GETDATE())
GROUP BY o.AccountId

SELECT	d.[���������� ����], d.����������, d.��������, d.[ʳ������ ����], a.[������], b.[����� ����], c.[3 � �����]
FROM (	SELECT	1 ID
		,COUNT(DISTINCT AccountId) [������]
		FROM AccountingCommon.Account
		WHERE DateTo = convert(DATETIME,'6/6/2079',103)) AS [a]
LEFT JOIN (
			-- �� ��� ����� �������������
			SELECT	1 ID
					, COUNT(qwer) [����� ����]
					,SUM(����) [����]
			FROM ##qw
			) AS [b] ON b.ID = a.ID
LEFT JOIN (
			--������������� 3 ����� � �����
			SELECT	1 ID
					, COUNT(qwer) [3 � �����]
					,SUM(����) [����]
			FROM ##qw
			WHERE ����� >= 3
			) [c] ON c.ID = a.ID
LEFT JOIN(
			--����� 1-4
		SELECT	1 ID
				,SUM(fs.DebetBegin) [���������� ����]
				,SUM(fs.ChargedSumm) [����������]
				,SUM(fs.PaidCashSumm) [��������]
				,SUM(fs.DebetEnd) [ʳ������ ����]
		FROM AccountingCommon.Account acc
		LEFT JOIN FinanceCommon.SupplierSaldo fs ON fs.AccountId = acc.AccountId
		WHERE	fs.Period = CONVERT(VARCHAR(6),DATEADD(mm, -1, GETDATE()),112) AND acc.DateTo = convert(DATETIME,'6/6/2079',103)
		) [d] ON d.ID = a.ID