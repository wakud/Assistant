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

----- вибириемо стан абонента - він може бути відключений чи закритий або просто боржник
INSERT INTO @pokaz$cok$
(
    AccountId,
    AccountNumber,
    pip
)
SELECT astat.AccountId
		, astat.AccountNumber
		, astat.Name AS 'ПІБ'
FROM (SELECT a.AccountId, a.AccountNumber,pp.LastName + ' ' + pp.FirstName + ' ' + pp.SecondName [Name]
	  FROM AccountingCommon.Account a
		JOIN AccountingCommon.PhysicalPerson pp ON pp.PhysicalPersonId = a.PhysicalPersonId
		LEFT JOIN SupportDefined.V_IsDisconnectAccounts vida ON a.AccountId = vida.AccountId
		LEFT JOIN FinanceCommon.SupplierSaldoCurrent sc ON sc.AccountId = a.AccountId AND sc.DebetEndExpired > 0.0
		WHERE a.DateTo = '2079-06-06' -- тільки незакриті ОР
) astat

----вибираемо зоннi лічильники по тарифікації
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

--вибираємо подані показнки за весь період постача
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


-- вибираємо скільки було вистпавлено кВт і суму
UPDATE @pokaz$cok$
SET  kVt = s.[Спож.кВт]
	, suma = s.[Спож.грн]
FROM (SELECT a.accountid
			,accountnumber
			,pp.FullName,
  ---- скільки має до оплати
			  
			  (SELECT ROUND(SUM(br.ConsumptionQuantity),0) --- скільки квт буде в поточному періоді враховано
			   FROM FinanceCommon.BillRegular br
			   WHERE br.AccountId=a.AccountId
				AND br.IsDeleted=0
				AND br.ConsumptionFrom BETWEEN @date_from$cok$ AND @date_to$cok$
			) AS 'Спож.кВт',

			  (SELECT SUM(br.TotalSumm)		-- скільки нараховано грн.
				FROM FinanceCommon.BillRegular br
				WHERE br.AccountId=a.AccountId
					AND br.IsDeleted=0
					AND br.ConsumptionFrom BETWEEN @date_from$cok$ AND @date_to$cok$

			) AS 'Спож.грн'

  FROM AccountingCommon.Account a
		,AccountingCommon.PhysicalPerson pp
  WHERE pp.PhysicalPersonId=a.PhysicalPersonId 
		)s
WHERE [@pokaz$cok$].AccountId = s.AccountId

SELECT  AccountNumber AS [ос. рах]
		, pip AS [ПІП]
		, zona AS [к-ть зон]
		, date_from AS [дата з]
		, date_to AS [дата по]
		, ISNULL(pokaz_znach, 0.00) AS [Поданий показ]
		, ISNULL(kVt, 0.00) AS [нараховані кВт]
		, ISNULL(suma, 0.00) AS [Сума]
FROM @pokaz$cok$
WHERE ISNULL(pokaz_znach, 0.00) <> ISNULL(kVt, 0.00)
