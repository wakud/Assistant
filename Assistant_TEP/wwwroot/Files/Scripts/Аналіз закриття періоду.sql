DROP TABLE IF EXISTS ##tmp$cok$

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

----- вибириемо стан абонента - він може бути відключений чи закритий або просто боржник
INSERT INTO @analiza
(
    AccountId,
    AccountNumber,
    pip,
    [Status]
)
SELECT astat.AccountId
		, astat.AccountNumber
		, astat.Name AS 'ПІБ',
(CASE WHEN len(astat.status_zakr) > 0 THEN astat.status_zakr
     WHEN len(astat.status_zakr) = 0 AND len(astat.status_vidkl) > 0 THEN astat.status_vidkl
     WHEN len(astat.status_zakr) = 0 AND len(astat.status_vidkl) = 0 AND len(astat.status_bord) > 0 THEN astat.status_bord
     ELSE 'Основний споживач' END) AS [Status] 
FROM (SELECT a.AccountId, a.AccountNumber,pp.LastName + ' ' + pp.FirstName + ' ' + pp.SecondName [Name],
		(CASE WHEN a.DateTo = CONVERT(datetime, '06.06.2079', 103) THEN '' ELSE 'Закритий - ' + CONVERT(varchar(10), a.DateTo, 103) END) status_zakr,
		(CASE WHEN vida.LastOffDate IS NULL THEN '' ELSE 'Відключений - ' + CONVERT(varchar(10), vida.LastOffDate, 103) END) AS status_vidkl,
		(CASE WHEN sc.DebetEndExpired IS NULL THEN '' ELSE 'Боржник - ' + CONVERT(varchar(15),sc.DebetEndExpired) END) AS status_bord
		FROM AccountingCommon.Account a
		JOIN AccountingCommon.PhysicalPerson pp ON pp.PhysicalPersonId = a.PhysicalPersonId
		LEFT JOIN SupportDefined.V_IsDisconnectAccounts vida ON a.AccountId = vida.AccountId
		LEFT JOIN FinanceCommon.SupplierSaldoCurrent sc ON sc.AccountId = a.AccountId AND sc.DebetEndExpired > 0.0
		WHERE a.DateTo = '2079-06-06' -- тільки незакриті ОР
) astat

----вибираемо зоннi лічильники по тарифікації
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

---- скільки має до оплати
UPDATE @analiza
SET debetend = s.DebetEnd
FROM (
		SELECT debetend, sscl.AccountId 
		FROM FinanceCommon.SupplierSaldoCurrent_Light sscl
		)s
WHERE [@analiza].AccountId = s.AccountId

--вибираємо дані з вюшки для подальшого наповнення
SELECT * 
INTO ##tmp$cok$
FROM FinanceCommon.BillRegular (NOLOCK)
WHERE consumptionfrom >= CONVERT(datetime,'01.01.2019', 103)

---- скільки є рахунків по середньому
UPDATE @analiza
SET kil = s.kil
FROM(
		SELECT count(billid) AS kil, AccountId
		FROM ##tmp$cok$
		WHERE consumptionfrom >= CONVERT(datetime,'01.01.2019', 103) AND IsDeleted = 0 AND calcmethod in (3) 
		GROUP BY AccountId
) s
WHERE [@analiza].AccountId = s.AccountId

--- скільки квт буде в поточному періоді враховано i скільки нараховано грн.
UPDATE @analiza
SET kVt = s.kwt, spog = s.summ
FROM(
		SELECT ROUND(SUM(ConsumptionQuantity),0) AS kwt,
				SUM(TotalSumm) AS summ,
				AccountId
		FROM ##tmp$cok$
		WHERE IsDeleted=0 AND ConsumptionFrom = @date_from$cok$
		GROUP BY AccountId
)s
WHERE [@analiza].AccountId = s.AccountId

---вириховуемо середнє споживання
UPDATE @analiza
SET ser = s.ser
FROM(
		SELECT avg(ConsumptionQuantity)AS ser, AccountId
		FROM ##tmp$cok$
		WHERE IsDeleted=0 AND  ConsumptionFrom BETWEEN DATEADD(mm,-7,@date$cok$) AND @date_from$cok$
		GROUP BY AccountId
)s
WHERE [@analiza].AccountId = s.AccountId

--- скільки квт пішло мінусом в поточному періоді
--UPDATE @analiza
--SET minus = s.minus
--FROM (
--		SELECT ISNULL(sum(br.totalsumm ),0) minus
--				, br.AccountId
--		FROM ##tmp$cok$ br, FinanceMain.Operation fo
--		WHERE isdeleted=1 AND br.date > CONVERT(DATETIME,'01.01.2019',103)
--		AND fo.AccountId = br.AccountId  AND br.BillId=fo.DocumentId
--		AND fo.PeriodTo = @period$cok$ AND DocumentTypeId = 15
--		GROUP BY br.AccountId
--)s
--WHERE [@analiza].AccountId = s.AccountId

--Вираховуємо помилки
UPDATE @analiza
SET tarif = s.tarif, err = s.err
FROM (
		SELECT AccountId, spog/kVt AS tarif
				,CASE
					WHEN kVt > 2000 THEN 'Спожито більше 2000'
					WHEN spog/kVt < 1.68 AND zona = 1 THEN 'Tариф менше 1,68'
					WHEN Status LIKE '%закрит%' THEN 'Закриті з боргом'
					WHEN kVt > (ser * 2) AND debetend > 100 THEN 'Спож. більше середнього.'
					WHEN spog > 2000 THEN 'До оплати більше 2000'
					WHEN kil > 5 THEN 'більше 5 по середньому'
					WHEN status LIKE '%Відкл%' AND kVt > 0 THEN 'Відключений і спож.'
					ELSE ''
				END AS err
		FROM @analiza
		WHERE kVt > 0 
	) AS s
WHERE [@analiza].AccountId = s.AccountId

-- виводимо хто подав показник і який
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

--виводимо результат
SELECT a.AccountNumber AS [Ос. рах], a.pip AS [ПІП], a.tarif AS [Тариф], a.pokaz AS [показник], a.whoFiled AS [хто подав], a.err AS [помилка]
FROM @analiza a
WHERE a.err <> ''