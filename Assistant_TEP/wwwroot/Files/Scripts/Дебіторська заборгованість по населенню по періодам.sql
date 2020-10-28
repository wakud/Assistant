DECLARE @per_start$cok$ INT;set @per_start$cok$ = 201901
DECLARE @per_end$cok$ INT; SET @per_end$cok$ = (SELECT value FROM Services.Setting s WHERE settingid=1)

DECLARE @mis1$cok$ INT; SET @mis1$cok$ = CONVERT(VARCHAR(6),DATEADD(mm, -1, GETDATE()),112) -- �� 1 ��
DECLARE @mis2$cok$ INT; SET @mis2$cok$ = CONVERT(VARCHAR(6),DATEADD(mm, -2, GETDATE()),112)	-- �� 2 ��
DECLARE @mis3$cok$ INT; SET @mis3$cok$ = CONVERT(VARCHAR(6),DATEADD(mm, -3, GETDATE()),112)	-- �� 3 ��
DECLARE @mis4$cok$ INT; SET @mis4$cok$ = CONVERT(VARCHAR(6),DATEADD(mm, -4, GETDATE()),112)	-- �� 4 ��
DECLARE @mis6$cok$ INT; SET @mis6$cok$ = CONVERT(VARCHAR(6),DATEADD(mm, -6, GETDATE()),112)	-- �� 6
DECLARE @mis7$cok$ INT; SET @mis7$cok$ = CONVERT(VARCHAR(6),DATEADD(mm, -7, GETDATE()),112)	-- �� 7
DECLARE @mis12$cok$ INT; SET @mis12$cok$ = CONVERT(VARCHAR(6),DATEADD(mm, -12, GETDATE()),112)	-- �� 12

DECLARE @debitorka$cok$ TABLE
					(
						AccountId INT,
						DebetBegin DECIMAL (10,2),
						narah DECIMAL (10,2),
						kompensacia DECIMAL (10,2),
						DebetEnd DECIMAL (10,2),
						Period INT,
						vid1do3 DECIMAL (10,2),
						vid3do6 DECIMAL (10,2),
						vid6do12 DECIMAL (10,2),
						vid1roku DECIMAL (10,2)
					)

INSERT INTO @debitorka$cok$ (AccountId, DebetEnd, Period, narah, kompensacia)
SELECT	fs.AccountId
		, fs.DebetEnd
		, fs.Period
		, fs.ChargedSumm
		, fs.ChargedSummBudget
FROM FinanceCommon.SupplierSaldo AS fs
WHERE fs.Period BETWEEN @per_start$cok$ AND @per_end$cok$

--�������� ���� �� 1 �� 3 ��
UPDATE @debitorka$cok$
SET vid1do3 = s.RestSumm
FROM (
		SELECT o.AccountId
				,SUM(o.RestSumm) RestSumm
		FROM FinanceMain.Operation o
		WHERE o.PeriodFrom BETWEEN @mis3$cok$ AND @mis2$cok$
				AND o.PeriodTo = 207906
				AND o.IsIncome = 0
				AND o.DocumentTypeId IN (15)
		GROUP BY o.AccountId
		) AS s
WHERE [@debitorka$cok$].AccountId = s.AccountId 
		AND Period = @mis2$cok$

--�������� ���� �� 3 �� 6 ��
UPDATE @debitorka$cok$
SET vid3do6 = s.RestSumm
FROM (
		SELECT o.AccountId
				,SUM(o.RestSumm) RestSumm
		FROM FinanceMain.Operation o
		WHERE o.PeriodFrom BETWEEN @mis6$cok$ AND @mis4$cok$
				AND o.PeriodTo = 207906
				AND o.IsIncome = 0
				AND o.DocumentTypeId IN (15)
		GROUP BY o.AccountId
		) AS s
WHERE [@debitorka$cok$].AccountId = s.AccountId 
		AND Period = @mis4$cok$

--�������� ���� �� 6 �� 12 ��
UPDATE @debitorka$cok$
SET vid6do12 = s.RestSumm
FROM (
		SELECT o.AccountId
				,SUM(o.RestSumm) RestSumm
		FROM FinanceMain.Operation o
		WHERE o.PeriodFrom BETWEEN @mis12$cok$ AND @mis7$cok$
				AND o.PeriodTo = 207906
				AND o.IsIncome = 0
				AND o.DocumentTypeId IN (15)
		GROUP BY o.AccountId
		) AS s
WHERE [@debitorka$cok$].AccountId = s.AccountId 
		AND Period = @mis7$cok$

--�������� ���� �� 6 �� 12 ��
UPDATE @debitorka$cok$
SET vid1roku = s.RestSumm
FROM (
		SELECT o.AccountId
				,SUM(o.RestSumm) RestSumm
		FROM FinanceMain.Operation o
		WHERE o.PeriodFrom < @mis12$cok$
				AND o.PeriodTo = 207906
				AND o.IsIncome = 0
				AND o.DocumentTypeId IN (15)
		GROUP BY o.AccountId
		) AS s
WHERE [@debitorka$cok$].AccountId = s.AccountId 
		AND Period = @mis12$cok$

SELECT 
		 SUM(CASE WHEN o.Period = @mis1$cok$ THEN o.DebetEnd ELSE 0.00 END) AS [������]
		,SUM (CASE WHEN o.Period=@mis1$cok$ THEN o.narah + o.kompensacia ELSE 0.00 END) AS [�� 1 ��]
    -- ������ �� ���� ���� ���
		,SUM(o.vid1do3) AS [�� 1 �� 3]
		,SUM(o.vid3do6) AS [�� 3 �� 6]
		,SUM(o.vid6do12) AS [�� 6 �� 12]
		,SUM(o.vid1roku) AS [�� 1 ����]
FROM @debitorka$cok$ o