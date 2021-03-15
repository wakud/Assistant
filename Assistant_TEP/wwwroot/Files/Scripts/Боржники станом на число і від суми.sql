SELECT    	pp.MobilePhoneNumber AS [Phone number]
			,a.AccountNumber AS [1]
			,pp.FullName AS [2]
			,o.RestSumm AS [3]
        FROM AccountingCommon.Account a
        JOIN (SELECT o.AccountId,SUM(o.RestSumm) RestSumm,CEILING(SUM(o.RestSumm)) as RestSummRound
            FROM FinanceMain.Operation o
            WHERE
            PeriodTo=207906
            and PeriodFrom < @period
            AND IsIncome=0
            AND DocumentTypeId IN (15)---1,9/15
            AND o.RestSumm>0
            AND o.Date<= @date
            GROUP BY o.AccountId
            HAVING SUM(o.RestSumm) >= @sum_pay) o ON a.AccountId = o.AccountId
        JOIN AccountingCommon.PhysicalPerson pp ON pp.PhysicalPersonId = a.PhysicalPersonId
        JOIN AccountingCommon.UsageObject uo ON uo.AccountId = a.AccountId
        LEFT JOIN (SELECT o.AccountId,r.PayDate,r.TotalSumm,
                        ROW_NUMBER() OVER (PARTITION BY o.AccountId ORDER BY r.PayDate DESC) id
            FROM FinanceMain.Operation o
            JOIN FinanceCommon.Receipt r ON r.ReceiptId =o.DocumentId
            and IsIncome=1
            AND r.PaymentFormId IN (1,2)
            AND o.PeriodTo=207906) op ON op.AccountId = a.AccountId
            AND op.id = 1
        WHERE (        pp.MobilePhoneNumber IS NOT NULL
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
                        OR pp.MobilePhoneNumber LIKE '099%'
                        )
                    )