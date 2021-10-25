DECLARE @CntMonth$cok$ INT; SET @CntMonth$cok$=0				-- в≥д к≥лькост≥ м≥с€ц≥в
DECLARE @ExBill$cok$ INT 
SET @ExBill$cok$ =       --“ерм≥н погашенн€ боргу по рахунках
	(SELECT TOP 1 cast([Value] as int)  FROM [Services].[Setting] WHERE [Guid] = '826C4666-F79C-4558-A0BB-2D5A428FCE1B')  
declare @m char(2),@y char(4)
set @y=convert(char(4),year(getdate()))
set @m=convert(varchar(2),month(getdate()))
IF (LEN(@m) =1)
SET @m = '0' + @m

IF Getdate() < convert(datetime,@y + @m + '20')
	SELECT	'38' + pp.MobilePhoneNumber AS [Phone number]
			,a.AccountNumberNew AS [1]
			,pp.FullName AS [2]
			,o.RestSumm AS [3]
	FROM AccountingCommon.Account a
	JOIN AccountingCommon.PhysicalPerson pp ON a.PhysicalPersonId = pp.PhysicalPersonId
	JOIN AccountingCommon.Address addr ON addr.AddressId = a.AddressId
	JOIN (SELECT o.AccountId,SUM(o.RestSumm) RestSumm
		FROM FinanceMain.Operation o
		WHERE PeriodTo=207906
		AND IsIncome=0
		AND DocumentTypeId IN (15)
		AND o.RestSumm>0
		AND o.Date<=DATEADD(dd,-@ExBill$cok$,GETDATE())
		GROUP BY o.AccountId
		HAVING SUM(o.RestSumm)>=@sum_pay) o ON a.AccountId = o.AccountId
	JOIN (SELECT r.AccountId,MAX(r.PayDate) AS PayDate
		FROM FinanceCommon.Receipt r
		WHERE r.IsDeleted=0
		AND r.BillDocumentTypeId IN (15,8,16,14)
		GROUP BY r.AccountId
		HAVING DATEDIFF(mm,MAX(r.PayDate),GETDATE())>=@CntMonth$cok$) pay ON pay.AccountId = a.AccountId
	WHERE (		pp.MobilePhoneNumber IS NOT NULL
				AND LEN(pp.MobilePhoneNumber) = 10
				AND pp.MobilePhoneNumber <> ' '
				AND pp.MobilePhoneNumber NOT LIKE '%-%'
				AND pp.MobilePhoneNumber NOT LIKE '%[ја-€я]%' 
				AND pp.MobilePhoneNumber NOT LIKE '%[ ]%'
	/*робимо перев≥рку на оператор≥в*/
				AND (pp.MobilePhoneNumber LIKE '050%' OR pp.MobilePhoneNumber LIKE'063%' 
					OR pp.MobilePhoneNumber LIKE '066%' OR pp.MobilePhoneNumber LIKE '067%'
					OR pp.MobilePhoneNumber LIKE '068%' OR pp.MobilePhoneNumber LIKE '073%' 
					OR pp.MobilePhoneNumber LIKE '091%' OR pp.MobilePhoneNumber LIKE '092%'
					OR pp.MobilePhoneNumber LIKE '093%' OR pp.MobilePhoneNumber LIKE '094%' 
					OR pp.MobilePhoneNumber LIKE '095%' OR pp.MobilePhoneNumber LIKE '096%'
					OR pp.MobilePhoneNumber LIKE '097%' OR pp.MobilePhoneNumber LIKE '098%' 
					OR pp.MobilePhoneNumber LIKE '099%' OR pp.MobilePhoneNumber LIKE '039%' 
						OR pp.MobilePhoneNumber LIKE '089%' 
					)
				)	
		ORDER by addr.FullAddress,a.AccountNumber
ELSE
	SELECT	'38' + pp.MobilePhoneNumber AS [Phone number]
			,a.AccountNumberNew AS [1]
			,pp.FullName AS [2]
			,o.RestSumm AS [3]
	FROM AccountingCommon.Account a
	JOIN AccountingCommon.PhysicalPerson pp ON a.PhysicalPersonId = pp.PhysicalPersonId
	JOIN AccountingCommon.Address addr ON addr.AddressId = a.AddressId
	JOIN (SELECT o.AccountId,SUM(o.RestSumm) RestSumm
		FROM FinanceMain.Operation o
		WHERE PeriodTo=207906
		AND IsIncome=0
		AND DocumentTypeId IN (15)
		AND o.RestSumm>0
		AND o.Date<=convert(datetime,@y + @m + '20')
		GROUP BY o.AccountId
		HAVING SUM(o.RestSumm)>=@sum_pay) o ON a.AccountId = o.AccountId
	JOIN (SELECT r.AccountId,MAX(r.PayDate) AS PayDate
		FROM FinanceCommon.Receipt r
		WHERE r.IsDeleted=0
		AND r.BillDocumentTypeId IN (15,8,16,14)
		GROUP BY r.AccountId
		HAVING DATEDIFF(mm,MAX(r.PayDate),GETDATE())>=@CntMonth$cok$) pay ON pay.AccountId = a.AccountId
	WHERE (		pp.MobilePhoneNumber IS NOT NULL
				AND LEN(pp.MobilePhoneNumber) = 10
				AND pp.MobilePhoneNumber <> ' '
				AND pp.MobilePhoneNumber NOT LIKE '%-%'
				AND pp.MobilePhoneNumber NOT LIKE '%[ја-€я]%' 
				AND pp.MobilePhoneNumber NOT LIKE '%[ ]%'
	/*робимо перев≥рку на оператор≥в*/
				AND (pp.MobilePhoneNumber LIKE '050%' OR pp.MobilePhoneNumber LIKE'063%' 
					OR pp.MobilePhoneNumber LIKE '066%' OR pp.MobilePhoneNumber LIKE '067%'
					OR pp.MobilePhoneNumber LIKE '068%' OR pp.MobilePhoneNumber LIKE '073%' 
					OR pp.MobilePhoneNumber LIKE '091%' OR pp.MobilePhoneNumber LIKE '092%'
					OR pp.MobilePhoneNumber LIKE '093%' OR pp.MobilePhoneNumber LIKE '094%' 
					OR pp.MobilePhoneNumber LIKE '095%' OR pp.MobilePhoneNumber LIKE '096%'
					OR pp.MobilePhoneNumber LIKE '097%' OR pp.MobilePhoneNumber LIKE '098%' 
					OR pp.MobilePhoneNumber LIKE '099%' OR pp.MobilePhoneNumber LIKE '039%' 
						OR pp.MobilePhoneNumber LIKE '089%' 
					)
				)	
		ORDER by addr.FullAddress,a.AccountNumber