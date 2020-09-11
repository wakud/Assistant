DECLARE @date$cok$ DATETIME; SET @date$cok$ = dateadd(DAY, 1 - DAY(GETDATE()), GETDATE());
DECLARE @date_from$cok$ DATETIME; SET @date_from$cok$ = dateadd( month, datediff( MONTH, 0, DATEADD(month,-1,GETDATE())), 0);
DECLARE @date_to$cok$ DATE; SET @date_to$cok$ = dateadd(month,1,dateadd(day,1-day(@date_from$cok$),@date_from$cok$))-1

DECLARE @m char(2),@y char(4)
select @y=convert(char(4),year(@date_to$cok$))
select @m=convert(varchar(2),month(@date_to$cok$))
IF LEN(@m) = 1 
SELECT @m = '0' + @m
DECLARE @period$cok$ INT; SET @period$cok$ = @y+@m

DECLARE @analiza TABLE (
						AccountId INT,
						AccountNumber VARCHAR(10),
						zona INT,
						pip VARCHAR(300),
						[Status] VARCHAR(300),
						debetend DECIMAL (9, 2),
						kil INT,
						kVt DECIMAL(18, 9),
						spog DECIMAL (9, 2),
						ser DECIMAL(18, 9),
						minus DECIMAL (9, 2),
						err VARCHAR(300),
						tarif DECIMAL(18, 9),
						whoFiled CHAR(100),
						pokaz CHAR(20)
	)

----- ��������� ���� �������� - �� ���� ���� ���������� �� �������� ��� ������ �������
INSERT INTO @analiza
(
    AccountId,
    AccountNumber,
    pip,
    [Status]
)
SELECT astat.AccountId
		, astat.AccountNumber
		, astat.Name AS 'ϲ�',
(CASE WHEN len(astat.status_zakr) > 0 THEN astat.status_zakr
     WHEN len(astat.status_zakr) = 0 AND len(astat.status_vidkl) > 0 THEN astat.status_vidkl
     WHEN len(astat.status_zakr) = 0 AND len(astat.status_vidkl) = 0 AND len(astat.status_bord) > 0 THEN astat.status_bord
     ELSE '�������� ��������' END) AS [Status] 
FROM (SELECT a.AccountId, a.AccountNumber,pp.LastName + ' ' + pp.FirstName + ' ' + pp.SecondName [Name],
		(CASE WHEN a.DateTo = CONVERT(datetime, '06.06.2079', 103) THEN '' ELSE '�������� - ' + CONVERT(varchar(10), a.DateTo, 103) END) status_zakr,
		(CASE WHEN vida.LastOffDate IS NULL THEN '' ELSE '³��������� - ' + CONVERT(varchar(10), vida.LastOffDate, 103) END) AS status_vidkl,
		(CASE WHEN sc.DebetEndExpired IS NULL THEN '' ELSE '������� - ' + CONVERT(varchar(15),sc.DebetEndExpired) END) AS status_bord
		FROM AccountingCommon.Account a
		JOIN AccountingCommon.PhysicalPerson pp ON pp.PhysicalPersonId = a.PhysicalPersonId
		LEFT JOIN SupportDefined.V_IsDisconnectAccounts vida ON a.AccountId = vida.AccountId
		LEFT JOIN FinanceCommon.SupplierSaldoCurrent sc ON sc.AccountId = a.AccountId AND sc.DebetEndExpired > 0.0
		WHERE a.DateTo = '2079-06-06' -- ����� �������� ��
) astat

----��������� ����i ��������� �� �����������
UPDATE @analiza
SET [@analiza].zona = s.zona
FROM (
		SELECT accountid
				,MAX(ucm.TimeZonalId) zona 
		FROM AccountingCommon.UsageCalculationMethod ucm, AccountingCommon.Point p,AccountingCommon.UsageObject uo
		WHERE uo.UsageObjectId=p.UsageObjectId
				AND ucm.PointId=p.PointId
				AND ucm.DateTo>GETDATE()
		GROUP BY accountid
	) AS s
WHERE [@analiza].AccountId = s.AccountId

---- ������ � ������� �� ����������
UPDATE @analiza
SET kil = s.kil
	, debetend = s.[�� ������]
	, kVt = s.[����.���]
	, spog = s.[����.���]
	, minus = s.̳���
	, ser = s.ser
FROM (SELECT a.accountid
			,accountnumber
			,pp.FullName,
  ---- ������ �� �� ������
			  (SELECT debetend 
				FROM FinanceCommon.SupplierSaldoCurrent_Light sscl
				WHERE sscl.accountid=a.AccountId
			) AS '�� ������',

			  (SELECT count(billid)  ---- ������ � ������� �� ����������
				FROM FinanceCommon.BillRegular BR (NOLOCK)
				WHERE br.consumptionfrom >=convert(datetime,'01.01.19',3) AND IsDeleted = 0
				AND calcmethod in (3) and a.accountid=br.accountid
			) AS kil,

			  (SELECT ROUND(SUM(br.ConsumptionQuantity),0) --- ������ ��� ���� � ��������� ����� ���������
			   FROM FinanceCommon.BillRegular br
			   WHERE br.AccountId=a.AccountId
				AND br.IsDeleted=0
				AND br.ConsumptionFrom = @date_from$cok$
			) AS '����.���',

			  (SELECT SUM(br.TotalSumm)		-- ������ ���������� ���.
				FROM FinanceCommon.BillRegular br
				WHERE br.AccountId=a.AccountId
					AND br.IsDeleted=0
					AND br.ConsumptionFrom = @date_from$cok$
					--AND br.CalculatePeriod=@period$cok$
			) AS '����.���',

			  (SELECT avg(br.ConsumptionQuantity) ---����������� ������ ����������
			  FROM FinanceCommon.BillRegular br
				WHERE br.AccountId=a.AccountId
				AND br.IsDeleted=0
				AND  br.ConsumptionFrom BETWEEN DATEADD(mm,-7,@date$cok$) AND @date_from$cok$
			) AS ser, ---�� ����� ����� ������� ������

			  (SELECT  ISNULL(sum(br5.totalsumm ),0)   --- ������ ��� ���� ������ � ��������� �����
                           FROM FinanceCommon.BillRegular br5,FinanceMain.Operation fo
                           WHERE isdeleted=1 AND br5.date > CONVERT(DATETIME,'01.01.2019',103)
                           AND fo.AccountId=br5.AccountId  AND br5.BillId=fo.DocumentId
                           AND fo.PeriodTo=@period$cok$ AND DocumentTypeId=15
                           AND br5.AccountId=a.AccountId
			   ) AS '̳���'
 
  FROM AccountingCommon.Account a
		,AccountingCommon.PhysicalPerson pp
  WHERE pp.PhysicalPersonId=a.PhysicalPersonId 
		)s
WHERE [@analiza].AccountId = s.AccountId

UPDATE @analiza
SET tarif = s.tarif, err = s.err
FROM (
		SELECT AccountId, spog/kVt AS tarif
				,CASE
					WHEN kVt > 2000 THEN '������� ����� 2000'
					WHEN spog/kVt < 0.90 AND zona = 1 THEN 'T���� ����� 0,90'
					WHEN Status LIKE '%������%' THEN '������ � ������'
					WHEN kVt > (ser * 1.5) AND debetend > 100 THEN '����. ����� ����������.'
					WHEN spog > 2000 THEN '�� ������ ����� 2000'
					WHEN kil > 5 THEN '����� 5 �� ����������'
					WHEN status LIKE '%³���%' AND kVt > 0 THEN '³��������� � ����.'
					ELSE ''
				END AS err
		FROM @analiza
		WHERE kVt > 0 
	) AS s
WHERE [@analiza].AccountId = s.AccountId

UPDATE @analiza
SET whoFiled = s.LastName, pokaz = s.CachedIndexes
FROM(
		SELECT * FROM (
						SELECT  a.AccountId
						, a.AccountNumber
						, s.LastName + ' ' + s.FirstName + ' ' + s.SecondName AS LastName
						, gi.UpdateDate
						, gi.CachedIndexes
						, ROW_NUMBER() OVER(PARTITION BY a.AccountNumber ORDER BY gi.UpdateDate DESC) num
						FROM AccountingCommon.Account a
						JOIN AccountingCommon.UsageObject uo ON uo.AccountId = a.AccountId
						JOIN AccountingCommon.Point p ON p.UsageObjectId = uo.UsageObjectId
						JOIN AccountingCommon.UsageCalculationMethod cm ON cm.PointId = p.PointId
						JOIN AccountingMeasuring.GroupIndex gi ON gi.UsageCalculationMethodId = cm.UsageCalculationMethodId
						JOIN Organization.Staff s ON s.StaffId = gi.StaffId
						WHERE gi.IsForCalculate = 1
							AND gi.Date BETWEEN @date_from$cok$ AND @date_to$cok$
					 ) v
					WHERE v.num = 1
	) AS s
WHERE [@analiza].AccountId = s.AccountId

SELECT a.AccountNumber AS [��. ���], a.pip AS [ϲ�], a.tarif AS [�����], a.pokaz AS [��������], a.whoFiled AS [��� �����], a.err AS [�������]
FROM @analiza a
WHERE a.err <> ''