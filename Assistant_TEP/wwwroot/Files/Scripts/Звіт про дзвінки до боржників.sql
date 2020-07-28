
DECLARE @date_from DATE; SET @date_from = '2019-01-01 00:00:00'
DECLARE @d DATETIME; SET @d=convert(char(8),getdate(),112)
DECLARE @date_to DATE; SET @date_to = dateadd(day,1-day(@d),@d)		--���� ��
--DECLARE @SummBorh INT; SET @SummBorh = 100

declare @borh TABLE (
		AccountId INT,
		AccountNumber VARCHAR(10),
		pip VARCHAR(300),
		monthBorgu INT,
		kvtZvit INT,
		sumaZvit DECIMAL(10,2),
		mobileNumber VARCHAR(25),
		anulKt INT,
		kvtAnul INT,
		sumaAnul DECIMAL(10,2),
		PayDate DATE,
		PaySum DECIMAL(10,2)
		)

--�������� ���� �������� ���������
INSERT INTO @borh (AccountId, AccountNumber, pip, sumaZvit, kvtZvit, monthBorgu, mobileNumber)
SELECT 	acc.AccountId
		,acc.AccountNumber AS [��. ���]
		,pp.FullName AS [ϲ� ���������]
		,SUM(br.RestSumm) AS [���� �����]
		,SUM(br.ConsumptionQuantity) AS [���]
		,MAX(DATEDIFF(MONTH, 
							SUBSTRING(CONVERT(CHAR(10),br.CalculatePeriod), 1, 4)+ '-' + 
							SUBSTRING(CONVERT(CHAR(10),br.CalculatePeriod), 5, 2)+ '-01'
								, @date_to
						) ) AS [�����]
		, pp.MobilePhoneNumber
FROM FinanceCommon.BillRegular br
LEFT JOIN AccountingCommon.Account acc ON acc.AccountId = br.AccountId
LEFT JOIN AccountingCommon.PhysicalPerson pp ON pp.PhysicalPersonId = acc.PhysicalPersonId
WHERE br.IsDeleted = 0
		AND br.ConsumptionFrom BETWEEN @date_from AND @date_to
		AND br.RestSumm > 0
GROUP BY acc.AccountId
		,acc.AccountNumber
		,pp.FullName
		,pp.MobilePhoneNumber

UPDATE @borh		-- �������� ������� ������
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
WHERE s.AccountId = [@borh].AccountId

UPDATE @borh		--�������� �� ���������� ������ � ����
SET kvtAnul = s.kvtAnul, sumaAnul = s.sumaAnul
FROM (
		SELECT  br.accountid,
				SUM(br.ConsumptionQuantity) AS kvtAnul,
				SUM(br.totalsumm) AS sumaAnul
        FROM FinanceCommon.BillRegular br
			,FinanceMain.Operation fo
        WHERE isdeleted=1 AND br.date>CONVERT(DATETIME,'01.01.2019',103)
				AND fo.AccountId=br.AccountId
				AND br.BillId=fo.DocumentId
				AND fo.PeriodTo=(SELECT value FROM Services.Setting s WHERE s.SettingId=1)
				AND DocumentTypeId=15
		GROUP BY br.accountid
		) AS s
WHERE s.AccountId = [@borh].AccountId

SELECT b.AccountNumber AS [��. ���.]
		,b.pip AS [ϲ�]
		,b.monthBorgu AS [�-�� ������]
		,b.kvtZvit AS [��� �� ��� ����]
		,b.sumaZvit AS [���� �� ��� ����]
		,b.mobileNumber AS [� ��������]
		,b.anulKt AS [�-�� ��������������]
		, b.kvtAnul AS [��� ��������������]
		,b.sumaAnul AS [���� ��������������]
		,b.PayDate AS [���� ������]
		,b.PaySum AS [���� ������]
FROM @borh b
WHERE b.sumaZvit >= @SummBorh

