DECLARE @ExBill INT 
SET @ExBill =       --����� ��������� ����� �� ��������
            	(SELECT TOP 1 cast([Value] as int)  FROM [Services].[Setting] WHERE [Guid] = '826C4666-F79C-4558-A0BB-2D5A428FCE1B')
				
DECLARE @vykl TABLE (
		[��� ���] SMALLINT,
		[��������] VARCHAR(10),
		[accId] INT,
		[ϲ�] VARCHAR(292),
		[�������] VARCHAR(150),
		[��'�] VARCHAR(70),
		[�� �������] VARCHAR(70),
		[����� ������] VARCHAR(300),
		[�����.���] VARCHAR(10),
		[�������] VARCHAR(12),
		[���.���] VARCHAR(100),
		[UtilityAddressId] INT,
		[������] CHAR(5),
		[�������] VARCHAR(40),
		[�����] VARCHAR(40),
		[��� ������] VARCHAR(400),
		[��� �.�.] VARCHAR(40),
		[��������] VARCHAR(50),
		[���.�����] VARCHAR(50),
		--[��� ���.������] SMALLINT,
		[��� �] VARCHAR(10),
		[��� ���] VARCHAR(50),
		[������] VARCHAR(50),
		[�������] VARCHAR(9),
		[������] VARCHAR(10),
		[��������] VARCHAR(5),
		[���� �����] DECIMAL(10,2),
		[���. ���� ���.] DATE,
		[���� ����.] DATE,
		[���� �����.] DATE,
		[� ���������] VARCHAR(40),
		[Ų�] VARCHAR(16)
		)

--�������� ���� �������� ���������
INSERT INTO @vykl ([accId], [��� ���], [��������],[ϲ�], [�������], [��'�], [�� �������], [�����.���], [�������], [���.���], [���� ����.])
SELECT	
		  a.AccountId
		, a.OrganizationUnitId
		, a.AccountNumber
		, pp.FullName
		, pp.LastName
		, pp.FirstName
		, pp.SecondName
		, pp.IdentificationCode
		, pp.PassportSeries +' '+ pp.PassportNumber
		, pp.MobilePhoneNumber
		, d.DateFrom
	FROM AccountingCommon.Account a
	JOIN AccountingCommon.PhysicalPerson pp ON pp.PhysicalPersonId = a.PhysicalPersonId
	JOIN AccountingCommon.UsageObject uo ON uo.AccountId = a.AccountId
	JOIN AccountingCommon.Point p ON p.UsageObjectId = uo.UsageObjectId
		AND p.DateTo=convert(DATETIME,'06/06/2079',103)
	JOIN AccountingCommon.Disconnection d ON p.PointId = d.PointId AND d.DisconnectionStatus=1
	JOIN (
			SELECT
				 d2.[DisconnectionId],
				ROW_NUMBER() OVER (PARTITION BY d2.PointId ORDER BY d2.DateFrom DESC) AS RowNumber
			FROM [AccountingCommon].[Disconnection] as d2
		)
			AS dtemp ON dtemp.[DisconnectionId] = d.[DisconnectionId] AND dtemp.RowNumber = 1
	WHERE a.DateTo = '2079-06-06' -- ����� �������� ��
-- ���������� ������
UPDATE @vykl
SET [����� ������] = s.[����� ������], [UtilityAddressId] = s.AddressId, [������] = s.������, [�������] = s.[�������], ����� = s.�����, [��� ������] = s.[��� ������]
	, [��� �.�.] = s.[��� �.�.], [��������] = s.��������, [���.�����] = s.[���.�����], /*[��� ���.������] = s.[��� ���.������],*/ [��� �] = s.[��� �], [��� ���] = s.[��� ���], ������ = s.������
	, ������� = s.�������, ������ = s.������, �������� = s.��������
FROM (
		SELECT a.AccountId
				, addr.FullAddress AS [����� ������]
				, addr.AddressId
				, z.ZipCode AS [������]
				,'������������' AS [�������]
				,'�������������' AS [�����]
				, ct.Name AS [��� ������],
				ct.ShortName AS [��� �.�.],
				napr.ShortName AS [��������],
				c.Name AS [���.�����],
				--c.CityId AS [��� ���.������],
				st.ShortName AS [��� �],
				st.Name AS [��� ���],
				s.Name AS [������],
				addr.Building AS [�������],
				addr.BuildingPart AS [������],
				addr.Apartment AS [��������]
		FROM AccountingCommon.Account a
		LEFT JOIN AccountingCommon.Address addr ON addr.AddressId = a.AddressId
		LEFT JOIN [TR_Organization].[AddressDictionary].[Zip] AS z ON z.ZipId = addr.ZipId
		LEFT JOIN [TR_Organization].[AddressDictionary].[City] AS c ON c.CityId = addr.CityId
		LEFT JOIN [TR_Organization].[AddressDictionary].[CityType] AS ct ON ct.CityTypeId = c.CityTypeId
		LEFT JOIN [TR_Organization].[AddressDictionary].Region AS r ON r.RegionId = c.RegionId
		LEFT JOIN [TR_Organization].[AddressDictionary].[District] AS ad ON ad.DistrictId = c.DistrictId
		LEFT JOIN [TR_Organization].[AddressDictionary].[Street] AS s ON s.StreetId = addr.StreetId
		LEFT JOIN [TR_Organization].[AddressDictionary].[StreetType]AS st ON st.StreetTypeId = s.StreetTypeId
		JOIN [AccountingCommon].[ClassifierGroupAccount] cl on cl.[AccountId]=a.AccountId  -- ��� ��'����, ��� ������� �������� ����
	JOIN [Dictionary].[ClassifierGroup] napr ON  napr.ClassifierGroupId = cl.ClassifierGroupId and napr.[ClassifierId] = 5 -- ������� �������� ���� ��������
		) AS s
WHERE accId = s.AccountId
-- ���������� ����
UPDATE @vykl
SET [���� �����] = s.RestSumm
FROM (
		SELECT a.accId
				, o.RestSumm
		FROM @vykl a
		JOIN (SELECT o.AccountId,SUM(o.RestSumm) RestSumm
				FROM FinanceMain.Operation o
				WHERE PeriodTo=207906
						AND IsIncome=0
						AND DocumentTypeId IN (15)
						AND o.Date<=DATEADD(dd,-@ExBill,GETDATE())
				GROUP BY o.AccountId
			) o ON a.accId = o.AccountId
	) AS s
WHERE [@vykl].accId = s.accId
--���������� � ��������� � Ų�
UPDATE @vykl
SET [� ���������] = s.CounterNumber, Ų� = s.Ų�
FROM (
		SELECT v.accId
				, cch.CounterNumber CounterNumber
				, p.EIC AS [Ų�]
		FROM @vykl v
		JOIN AccountingCommon.UsageObject uo ON uo.AccountId = v.accId
		JOIN AccountingCommon.Point p ON p.UsageObjectId = uo.UsageObjectId
		JOIN AccountingCommon.UsageCalculationMethod AS ucm ON ucm.PointId = p.PointId 
		JOIN Counter.CounterMeasuring cm ON ucm.CounterMeasuringId=cm.CounterMeasuringId AND ucm.DateTo = '2079-06-06'
		JOIN Counter.CounterHistory cch ON cch.CounterHistoryId=cm.CounterHistoryId AND cch.DateTo='2079-06-06'
	) AS s
WHERE [@vykl].accId = s.accId
--���������� ������� ������
UPDATE @vykl
SET [���. ���� ���.] = s.PayDate
FROM (
		SELECT v.accId
				,MAX(r.PayDate) AS PayDate
	FROM @vykl v
	LEFT JOIN FinanceCommon.Receipt r ON r.AccountId = v.accId
	WHERE r.IsDeleted=0
	AND r.BillDocumentTypeId IN (15,8,16,14)--Solenko20190515
	GROUP BY v.accId
	HAVING DATEDIFF(mm,MAX(r.PayDate),GETDATE())>=0
	) AS s
WHERE [@vykl].accId = s.accId
-- ���������� ���� ������ ������ ������������
UPDATE @vykl
SET [���� �����.] = s.DeliverDate
FROM (
		SELECT v.accId
				, dw.DeliverDate
		FROM @vykl v
		LEFT JOIN LawCommon.DisconnectWarning dw ON dw.AccountId = v.accId
		WHERE dw.Date = (SELECT MAX(date) 
						 FROM LawCommon.DisconnectWarning
						 WHERE AccountId = dw.AccountId
						 AND IsActive=1
						 AND IsDelivered=1
						 )
			AND dw.Summ-dw.UsedSumm>0
	) AS s
WHERE [@vykl].accId = s.accId

SELECT * FROM @vykl
