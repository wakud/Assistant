/* ���������� ���� ������������� �� ������� �/� �� ��������� ����������, 
	�� �������� �� �� ���� ���������� ������� �� �������������� ��������� (�� 100���)
	*/

DECLARE @ExBill INT 
SET @ExBill =       --����� ��������� ����� �� ��������
            	(SELECT TOP 1 cast([Value] as int)  FROM [Services].[Setting] WHERE [Guid] = '826C4666-F79C-4558-A0BB-2D5A428FCE1B')
				
DECLARE @SummBorh INT; SET @SummBorh=100	--���� ����� �� 100
DECLARE @date_from DATE; SET @date_from = '2019-01-01 00:00:00'		--���� �
DECLARE @date_to DATE; SET @date_to = dateadd(day,1-day(GETDATE()),GETDATE())		--���� ��

declare @vykl_roz TABLE (
		AccountId INT,
		AccountNumber VARCHAR(10),
		pip VARCHAR(300),
		monthBorgu INT,
		dateVykl DATE,
		kvtVykl INT,
		sumaVykl DECIMAL(10,2),
		kvtZvit INT,
		sumaZvit DECIMAL(10,2),
		kvtRizn INT,
		sumaRizn DECIMAL(10,2),
		PayDate DATE,
		PaySum DECIMAL(10,2)
		)

--�������� ���� �������� ���������
INSERT INTO @vykl_roz (AccountId, AccountNumber,pip, dateVykl)
SELECT	
		a.AccountId
		,a.AccountNumber
		,pp.FullName
		,d.DateFrom
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

UPDATE @vykl_roz		--���������� ������� ������
SET PayDate = s.PayDate
	,PaySum = s.PaySum
FROM ( SELECT r.AccountId
				,MAX(r.PayDate) AS PayDate
				,rc.TotalSumm AS PaySum
		FROM FinanceCommon.Receipt r
		INNER JOIN FinanceCommon.Receipt rc ON rc.AccountId = r.AccountId
		WHERE r.IsDeleted=0
			AND r.BillDocumentTypeId IN (15,8,16,14)
		GROUP BY r.AccountId, rc.TotalSumm
		HAVING DATEDIFF(mm,MAX(r.PayDate),GETDATE())>=0 AND MAX(rc.PayDate) = MAX(r.PayDate)
	  ) AS s
WHERE s.AccountId = [@vykl_roz].AccountId

UPDATE @vykl_roz
SET kvtZvit = s.Quantity
	,sumaZvit = s.RestSumm
	,monthBorgu = s.�����
FROM (		--�������� ���� �� ��� ����
				SELECT o.AccountId
						, SUM(o.RestSumm) RestSumm
						,SUM(ro.Quantity) Quantity
						, MAX(DATEDIFF(MONTH, 
							SUBSTRING(CONVERT(CHAR(10),o.PeriodFrom), 1, 4)+ '-' + 
							SUBSTRING(CONVERT(CHAR(10),o.PeriodFrom), 5, 2)+ '-01'
								, @date_to
						) ) AS [�����]
				FROM FinanceMain.Operation o
				LEFT OUTER JOIN (
								SELECT r.OperationId,
										SUM(r.Quantity) AS Quantity
								FROM FinanceMain.OperationRow r 
								GROUP BY r.OperationId
								)ro ON ro.OperationId = o.OperationId
				WHERE o.PeriodTo = '207906'
						AND o.DocumentTypeId = 15
						AND o.RestSumm > 0
						AND o.Date<=DATEADD(dd,-@ExBill,GETDATE())
				GROUP BY o.AccountId
				) AS s
WHERE s.AccountId = [@vykl_roz].AccountId

UPDATE @vykl_roz		--�������� ���� �� ���� ����������
SET kvtVykl = s.Quantity, sumaVykl = s.RestSumm
FROM (		
		SELECT o.AccountId
				,SUM(o.RestSumm) RestSumm
				,SUM(ro.Quantity) Quantity
		FROM FinanceMain.Operation o
		LEFT OUTER JOIN (
							SELECT r.OperationId,
									SUM(r.Quantity) AS Quantity
							FROM FinanceMain.OperationRow r 
							GROUP BY r.OperationId
						)ro ON ro.OperationId = o.OperationId
		LEFT JOIN @vykl_roz n ON n.AccountId = o.AccountId
				WHERE o.PeriodTo = '207906'
						AND o.DocumentTypeId = 15
						AND o.RestSumm > 0
						AND o.Date BETWEEN @date_from AND n.dateVykl
				GROUP BY o.AccountId
	 ) AS s
WHERE s.AccountId = [@vykl_roz].AccountId


UPDATE @vykl_roz		--������ ������ �����
SET kvtRizn = ISNULL(kvtZvit-kvtVykl, kvtZvit)
	,sumaRizn = ISNULL(sumaZvit - sumaVykl, sumaZvit)

SELECT n.AccountNumber AS [��.���]
		,n.pip AS [ϲ�]
		,n.monthBorgu AS [�-�� ������ �����]
		,n.dateVykl AS [���� ����������]
		,n.kvtVykl AS [��� �� ���� ����.]
		,n.sumaVykl AS [���� �� ���� ����.]
		,n.kvtZvit AS [��� �� ���. ����]
		,n.sumaZvit AS [���� �� ���. ����]
		,n.kvtRizn AS [������ ���]
		,n.sumaRizn AS [���� ������]
		,n.PayDate AS [������� ���� ������]
		,n.PaySum AS [���� ������]
FROM @vykl_roz n
JOIN AccountingCommon.UsageObject uo ON n.AccountId = uo.AccountId
JOIN AccountingCommon.Point p ON p.UsageObjectId = uo.UsageObjectId
JOIN AccountingCommon.UsageCalculationMethod cm ON cm.PointId = p.PointId
JOIN AccountingMeasuring.GroupIndex gi ON gi.UsageCalculationMethodId = cm.UsageCalculationMethodId
JOIN Organization.Staff s ON s.StaffId = gi.StaffId
WHERE sumaZvit >= @SummBorh 
	AND gi.GroupIndexSourceId = 18			-- �� ��� ��������������
	AND gi.IsForCalculate = 1
	AND gi.Date BETWEEN n.dateVykl AND @date_to
GROUP BY n.AccountId
		,n.AccountNumber
		,n.pip
		,n.monthBorgu
		,n.dateVykl
		,n.kvtVykl
		,n.sumaVykl
		,n.kvtZvit
		,n.sumaZvit
		,n.kvtRizn
		,n.sumaRizn
		,n.PayDate
		,n.PaySum

