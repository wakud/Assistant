DECLARE @date$cok$ DATETIME; SET @date$cok$ = CONVERT(DATE, '2019-01-01', 20);
DECLARE @date_from$cok$ DATE; SET @date_from$cok$ = CONVERT(DATE, '2019-01-01', 20);
DECLARE @date_to$cok$ DATE; SET @date_to$cok$ = dateadd(month,0,dateadd(day,1-day(GETDATE()),GETDATE()))-1

DECLARE @pokaz$cok$ TABLE (
						AccountId INT,
						AccountNumber VARCHAR(10),
						pip VARCHAR(300),
						[Status] VARCHAR(300),
						zona INT,
						date_from DATETIME,
						date_to DATETIME,
						pokaz_znach INT,
						kVt DECIMAL(18, 9),
						suma DECIMAL (9, 2)
	)

----- ��������� ���� �������� - �� ���� ���� ���������� �� �������� ��� ������ �������
INSERT INTO @pokaz$cok$
(
    AccountId,
    AccountNumber,
    pip
)
SELECT astat.AccountId
		, astat.AccountNumber
		, astat.Name AS 'ϲ�'
FROM (SELECT a.AccountId, a.AccountNumber,pp.LastName + ' ' + pp.FirstName + ' ' + pp.SecondName [Name]
	  FROM AccountingCommon.Account a
		JOIN AccountingCommon.PhysicalPerson pp ON pp.PhysicalPersonId = a.PhysicalPersonId
		LEFT JOIN SupportDefined.V_IsDisconnectAccounts vida ON a.AccountId = vida.AccountId
		LEFT JOIN FinanceCommon.SupplierSaldoCurrent sc ON sc.AccountId = a.AccountId AND sc.DebetEndExpired > 0.0
		WHERE a.DateTo = '2079-06-06' -- ����� �������� ��
) astat

----��������� ����i ��������� �� �����������
UPDATE @pokaz$cok$
SET zona = s.zona
		,date_from = @date_from$cok$, date_to = @date_to$cok$
FROM (
		SELECT accountid
				,MAX(ucm.TimeZonalId) zona 
		FROM AccountingCommon.UsageCalculationMethod ucm, AccountingCommon.Point p,AccountingCommon.UsageObject uo
		WHERE uo.UsageObjectId=p.UsageObjectId
				AND ucm.PointId=p.PointId
				AND ucm.DateTo>GETDATE()
		GROUP BY accountid
	) AS s
WHERE [@pokaz$cok$].AccountId = s.AccountId

--�������� ����� �������� �� ���� ����� �������
UPDATE @pokaz$cok$
SET pokaz_znach = s.usage
FROM(
		SELECT  a.AccountId
		, SUM(uc.Usage) usage
		FROM AccountingCommon.Account a
		JOIN AccountingCommon.UsageObject uo ON uo.AccountId = a.AccountId
		JOIN AccountingCommon.Point p ON p.UsageObjectId = uo.UsageObjectId
		JOIN AccountingCommon.UsageCalculationMethod cm ON cm.PointId = p.PointId
		JOIN AccountingMeasuring.UsageCache uc ON uc.UsageCalculationMethodId = cm.UsageCalculationMethodId
		WHERE uc.DateFrom BETWEEN @date_from$cok$ AND @date_to$cok$
		GROUP BY a.AccountId
	) AS s
WHERE [@pokaz$cok$].AccountId = s.AccountId


-- �������� ������ ���� ����������� ��� � ����
UPDATE @pokaz$cok$
SET  kVt = s.[����.���]
	, suma = s.[����.���]
FROM (SELECT a.accountid
			,accountnumber
			,pp.FullName,
  ---- ������ �� �� ������
			  
			  (SELECT ROUND(SUM(br.ConsumptionQuantity),0) --- ������ ��� ���� � ��������� ����� ���������
			   FROM FinanceCommon.BillRegular br
			   WHERE br.AccountId=a.AccountId
				AND br.IsDeleted=0
				AND br.ConsumptionFrom BETWEEN @date_from$cok$ AND @date_to$cok$
			) AS '����.���',

			  (SELECT SUM(br.TotalSumm)		-- ������ ���������� ���.
				FROM FinanceCommon.BillRegular br
				WHERE br.AccountId=a.AccountId
					AND br.IsDeleted=0
					AND br.ConsumptionFrom BETWEEN @date_from$cok$ AND @date_to$cok$

			) AS '����.���'

  FROM AccountingCommon.Account a
		,AccountingCommon.PhysicalPerson pp
  WHERE pp.PhysicalPersonId=a.PhysicalPersonId 
		)s
WHERE [@pokaz$cok$].AccountId = s.AccountId

SELECT  AccountNumber AS [��. ���]
		, pip AS [ϲ�]
		, zona AS [�-�� ���]
		, date_from AS [���� �]
		, date_to AS [���� ��]
		, ISNULL(pokaz_znach, 0.00) AS [������� �����]
		, ISNULL(kVt, 0.00) AS [��������� ���]
		, ISNULL(suma, 0.00) AS [����]
FROM @pokaz$cok$
WHERE ISNULL(pokaz_znach, 0.00) <> ISNULL(kVt, 0.00)
