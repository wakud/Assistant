DECLARE @CntMonth$cok$ INT; SET @CntMonth$cok$=0				-- від кількості місяців
DECLARE @ExBill$cok$ INT 
SET @ExBill$cok$ =       --Термін погашення боргу по рахунках
	(SELECT TOP 1 cast([Value] as int)  FROM [Services].[Setting] WHERE [Guid] = '826C4666-F79C-4558-A0BB-2D5A428FCE1B')  


SELECT	pp.MobilePhoneNumber AS [моб.тел]
		,a.AccountNumber AS [особовий рахунок]
		,pp.FullName AS [ПІП абонента]
		,o.RestSumm AS [сума до оплати]
FROM AccountingCommon.Account a
JOIN AccountingCommon.PhysicalPerson pp ON a.PhysicalPersonId = pp.PhysicalPersonId
JOIN AccountingCommon.Address addr ON addr.AddressId = a.AddressId
JOIN (SELECT o.AccountId,SUM(o.RestSumm) RestSumm
	FROM FinanceMain.Operation o
	WHERE PeriodTo=207906
	AND IsIncome=0
	AND DocumentTypeId IN (15)
	AND o.RestSumm>0
	--AND o.Date<=DATEADD(dd,-@ExBill$cok$,GETDATE())
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
			AND pp.MobilePhoneNumber NOT LIKE '%[Аа-яЯ]%' 
			AND pp.MobilePhoneNumber NOT LIKE '%[ ]%'
/*робимо перевірку на операторів*/
			AND (pp.MobilePhoneNumber LIKE '050%' OR pp.MobilePhoneNumber LIKE'063%' 
				OR pp.MobilePhoneNumber LIKE '066%' OR pp.MobilePhoneNumber LIKE '067%'
				OR pp.MobilePhoneNumber LIKE '068%' OR pp.MobilePhoneNumber LIKE '073%' 
				OR pp.MobilePhoneNumber LIKE '091%' OR pp.MobilePhoneNumber LIKE '092%'
				OR pp.MobilePhoneNumber LIKE '093%' OR pp.MobilePhoneNumber LIKE '094%' 
				OR pp.MobilePhoneNumber LIKE '095%' OR pp.MobilePhoneNumber LIKE '096%'
				OR pp.MobilePhoneNumber LIKE '097%' OR pp.MobilePhoneNumber LIKE '098%' 
				OR pp.MobilePhoneNumber LIKE '099%'
				)
			)	
	ORDER by addr.FullAddress,a.AccountNumber