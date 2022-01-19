/*		
		��� ���������� ���� ������������� �� ������� ������������� ���������� 
		(�� 1000 ��� ��/��� �� 1000��� � ������� ���������� 3 ����� � �����)
		������ �� __________________
*/
DROP TABLE IF EXISTS ##qwerty$cok$

DECLARE @ExBill INT 
SET @ExBill =       --����� ��������� ����� �� ��������
	(SELECT TOP 1 cast([Value] as int)  FROM [Services].[Setting] WHERE [Guid] = '826C4666-F79C-4558-A0BB-2D5A428FCE1B')
DECLARE @start datetime = '2019-01-01', @finish datetime = GETDATE()

SELECT	a.AccountNumber AS [��.���]
		, pp.FullName AS [ϲ�]		
		, addr.FullAddress AS [������]
		, SUM(o.RestSumm) AS [����]
		, MAX(DATEDIFF(MONTH, 
						CAST(
							o.Date AS DATE
						),
						CAST(@stanom_na AS DATE) 
					) ) AS [�����]
		, CONVERT(varchar(10),dcoff.DateOff,103) as [���� ����]
INTO ##qwerty$cok$
FROM AccountingCommon.Account a
JOIN AccountingCommon.PhysicalPerson pp ON a.PhysicalPersonId = pp.PhysicalPersonId
JOIN AccountingCommon.Address addr ON addr.AddressId = a.AddressId
JOIN (SELECT o.AccountId,SUM(o.RestSumm) RestSumm, o.PeriodFrom, o.Date
	FROM FinanceMain.Operation o
	WHERE PeriodTo=207906
	AND IsIncome=0
	--AND DocumentTypeId IN (15)
	AND o.PeriodFrom >= 201901
	AND o.SaldoKind = 4
	AND o.RestSumm>0
	AND o.Date<=DATEADD(dd,-@ExBill,GETDATE())
	GROUP BY o.AccountId, o.PeriodFrom, o.Date
	) o ON a.AccountId = o.AccountId
LEFT JOIN (
		SELECT a.AccountId,d.DateFrom  DateOff,DisconnectionStatus,
		ROW_NUMBER() OVER (PARTITION BY a.AccountId ORDER BY d.DateFrom DESC) AS RowNumber
		FROM AccountingCommon.Disconnection d
		JOIN AccountingCommon.Point p ON d.PointId = p.PointId
		AND p.DateTo=convert(DATETIME,'06/06/2079',103)
		JOIN AccountingCommon.UsageObject uo ON uo.UsageObjectId = p.UsageObjectId
		JOIN AccountingCommon.Account a ON uo.AccountId = a.AccountId
		) dcoff ON dcoff.AccountId = a.AccountId AND dcoff.RowNumber = 1 AND dcoff.DisconnectionStatus=1
GROUP BY a.AccountNumber,pp.FullName,addr.FullAddress, CONVERT(varchar(10),dcoff.DateOff,103)
ORDER by a.AccountNumber, addr.FullAddress

SELECT * FROM ##qwerty$cok$
WHERE ����>=1000.00 
		OR (����>=100.00 AND ����� BETWEEN 3 AND datediff(month,0,dateadd(ss,DATEDIFF(ss,@start,@finish),0)))
ORDER BY [��.���]