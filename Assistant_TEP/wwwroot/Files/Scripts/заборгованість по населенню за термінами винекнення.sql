/*		
		��� ���������� ���� ������������� �� ������� ������������� ���������� 
		(�� 1000 ��� ��/��� �� 1000��� � ������� ���������� 3 ����� � �����)
		������ �� __________________
*/

DROP TABLE IF EXISTS ##qwerty

SELECT	acc.AccountNumber AS [��.���]
		, pers.FullName AS [ϲ�]
		, ad.FullAddress AS [������]
		, SUM(CASE WHEN br.CalculatePeriod BETWEEN @per_start AND @per_end THEN br.RestSumm ELSE 0 END) AS [����]
		, MAX(DATEDIFF(MONTH, 
						CAST(
							CAST(br.CalculatePeriod AS CHAR(6)) + '01' AS DATE
						),
						CAST(@stanom_na AS DATE) 
					) )AS [�����]
INTO ##qwerty
FROM FinanceCommon.BillRegular br
LEFT JOIN AccountingCommon.Account acc ON acc.AccountId = br.AccountId
JOIN AccountingCommon.PhysicalPerson as pers ON pers.PhysicalPersonId = acc.PhysicalPersonId
JOIN AccountingCommon.Address as ad ON ad.AddressId = acc.AddressId 
WHERE	br.IsDeleted = 0 
		AND br.CalculatePeriod BETWEEN @per_start AND @per_end
		AND br.RestSumm > 0.00
GROUP BY acc.AccountNumber
		, pers.FullName
		, ad.FullAddress

SELECT [��.���]
		,[ϲ�]
		,[������]
		,[����]
		,[�����] - 1 AS [�����]
FROM ##qwerty
WHERE ����>=1000.00
		OR (����>=100.00 AND ����� BETWEEN 3 AND 30)
ORDER BY [��.���], ������