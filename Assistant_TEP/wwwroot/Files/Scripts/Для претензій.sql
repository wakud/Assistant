DECLARE @date_from$cok$ DATE; SET @date_from$cok$ = '2019-01-01 00:00:00'
DECLARE @d$cok$ DATETIME; SET @d$cok$=convert(char(8),getdate(),112)
DECLARE @date_to$cok$ DATE; SET @date_to$cok$ = dateadd(day,1-day(@d$cok$),@d$cok$)		--дата по

DECLARE @ExBill INT 
SET @ExBill =       --Термін погашення боргу по рахунках
	(SELECT TOP 1 cast([Value] as int)  FROM [Services].[Setting] WHERE [Guid] = '826C4666-F79C-4558-A0BB-2D5A428FCE1B')  

DECLARE @pretensia TABLE (
		AccountId INT,
		AccountNumber VARCHAR(10),
		PIP VARCHAR(150),
		Addressa VARCHAR(300),
		MonthBorgu INT,
		DateFrom DATE,
		SummaBorgu DECIMAL(10,2),
		Vykl VARCHAR(10)
)

INSERT INTO @pretensia
(
    AccountId,
    AccountNumber,
    PIP,
    Addressa,
    SummaBorgu,
    Vykl
)
SELECT	a.AccountId,
		a.AccountNumber AS [Особовий],
		pp.FullName AS [ПІП],
		addr.FullAddress AS [Адреса],
		o.RestSumm AS [Борг],
		CONVERT(varchar(10),dcoff.DateOff,103) AS [Дата викл.]
FROM AccountingCommon.Account a
JOIN AccountingCommon.PhysicalPerson pp ON a.PhysicalPersonId = pp.PhysicalPersonId
JOIN AccountingCommon.Address addr ON addr.AddressId = a.AddressId
JOIN (SELECT o.AccountId,SUM(o.RestSumm) RestSumm
	FROM FinanceMain.Operation o
	WHERE PeriodTo=207906
	AND IsIncome=0
	AND DocumentTypeId IN (15)
	AND o.RestSumm>0
	AND o.Date<=DATEADD(dd,-@ExBill,GETDATE())
	GROUP BY o.AccountId
	HAVING SUM(o.RestSumm) >= @SummBorh) o ON a.AccountId = o.AccountId
LEFT JOIN (
		SELECT a.AccountId,d.DateFrom  DateOff, DisconnectionStatus,
		ROW_NUMBER() OVER (PARTITION BY a.AccountId ORDER BY d.DateFrom DESC) AS RowNumber
		FROM AccountingCommon.Disconnection d
		JOIN AccountingCommon.Point p ON d.PointId = p.PointId
		AND p.DateTo=convert(DATETIME,'06/06/2079',103)
		JOIN AccountingCommon.UsageObject uo ON uo.UsageObjectId = p.UsageObjectId
		JOIN AccountingCommon.Account a ON uo.AccountId = a.AccountId
		) dcoff ON dcoff.AccountId = a.AccountId AND dcoff.RowNumber = 1 AND dcoff.DisconnectionStatus=1

UPDATE @pretensia
SET MonthBorgu = s.місяць, DateFrom = s.datefrom
FROM (SELECT MAX(DATEDIFF(MONTH, 
			  	 SUBSTRING(CONVERT(CHAR(10), CalculatePeriod), 1, 4)+ '-' + 
				 SUBSTRING(CONVERT(CHAR(10), CalculatePeriod), 5, 2)+ '-01'
				 , @date_to$cok$
				) ) AS [місяць]
				, AccountId
				, MIN(ConsumptionFrom) AS datefrom
		FROM FinanceCommon.BillRegular
		WHERE IsDeleted = 0
			AND ConsumptionFrom BETWEEN @date_from$cok$ AND @date_to$cok$
			AND RestSumm > 0
		GROUP BY AccountId
)AS s
WHERE s.AccountId = [@pretensia].AccountId

SELECT * 
FROM @pretensia
WHERE MonthBorgu >= @CntMonth