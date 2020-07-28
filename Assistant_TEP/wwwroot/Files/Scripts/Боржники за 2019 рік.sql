/*���������� ���� ������������� �� ������� ������������� ����������,
	��� ������� � 2019 ���� � � ����������� �� ����� ���� */

DECLARE @date_from DATE; SET @date_from = '2019-01-01 00:00:00'
DECLARE @date_to DATE; SET @date_to = '2019-12-01 00:00:00'

SELECT 	acc.AccountNumber AS [��. ���]
		,pp.FullName AS [ϲ� ���������]
		,adr.FullAddress AS [������]
		, SUM(br.RestSumm) AS [���� �����]
		, NULL AS [�����������]
		, NULL AS [������ � ���]
		, CONVERT(varchar(10),dcoff.DateOff,103) AS [����������]
		, NULL AS [������ ��������]
		, NULL AS [�������� �� ����]
FROM FinanceCommon.BillRegular br
LEFT JOIN AccountingCommon.Account acc ON acc.AccountId = br.AccountId
LEFT JOIN AccountingCommon.PhysicalPerson pp ON pp.PhysicalPersonId = acc.PhysicalPersonId
LEFT JOIN AccountingCommon.Address adr ON adr.AddressId = acc.AddressId
LEFT JOIN (
		SELECT a.AccountId,d.DateFrom  DateOff,DisconnectionStatus,
		ROW_NUMBER() OVER (PARTITION BY a.AccountId ORDER BY d.DateFrom DESC) AS RowNumber
		FROM AccountingCommon.Disconnection d
		JOIN AccountingCommon.Point p ON d.PointId = p.PointId
		AND p.DateTo=convert(DATETIME,'06/06/2079',103)
		JOIN AccountingCommon.UsageObject uo ON uo.UsageObjectId = p.UsageObjectId
		JOIN AccountingCommon.Account a ON uo.AccountId = a.AccountId
		) dcoff ON dcoff.AccountId = acc.AccountId 
			AND dcoff.RowNumber = 1 
			AND dcoff.DisconnectionStatus=1
WHERE br.IsDeleted = 0
		AND br.ConsumptionFrom BETWEEN @date_from AND @date_to
		AND br.RestSumm > 0
GROUP BY br.AccountId
		,acc.AccountNumber
		,adr.FullAddress
		,pp.FullName
		,CONVERT(varchar(10),dcoff.DateOff,103)