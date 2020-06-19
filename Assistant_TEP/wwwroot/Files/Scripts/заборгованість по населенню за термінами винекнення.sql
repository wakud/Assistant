/*		
		��� ���������� ���� ������������� �� ������� ������������� ���������� 
		(�� 1000 ��� ��/��� �� 1000��� � ������� ���������� 3 ����� � �����)
		������ �� __________________
*/
DROP TABLE IF EXISTS ##qwerty

DECLARE @ExBill INT 
SET @ExBill =       --����� ��������� ����� �� ��������
	(SELECT TOP 1 cast([Value] as int)  FROM [Services].[Setting] WHERE [Guid] = '826C4666-F79C-4558-A0BB-2D5A428FCE1B')  
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
INTO ##qwerty
FROM AccountingCommon.Account a
JOIN AccountingCommon.PhysicalPerson pp ON a.PhysicalPersonId = pp.PhysicalPersonId
JOIN AccountingCommon.Address addr ON addr.AddressId = a.AddressId
JOIN (SELECT o.AccountId,SUM(o.RestSumm) RestSumm, o.PeriodFrom, o.Date
	FROM FinanceMain.Operation o
	WHERE PeriodTo=207906
	AND IsIncome=0
	AND DocumentTypeId IN (15)
	AND o.RestSumm>0
	AND o.Date<=DATEADD(dd,-@ExBill,GETDATE())
	GROUP BY o.AccountId, o.PeriodFrom, o.Date
	) o ON a.AccountId = o.AccountId
GROUP BY a.AccountNumber,pp.FullName,addr.FullAddress
ORDER by a.AccountNumber, addr.FullAddress

SELECT * FROM ##qwerty
WHERE ����>=1000.00 
		OR (����>=100.00 AND ����� BETWEEN 3 AND 30)
ORDER BY [��.���]