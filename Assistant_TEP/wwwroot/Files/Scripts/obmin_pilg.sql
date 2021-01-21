declare @m char(2),@y char(4)
set @y=convert(char(4),year(getdate()))
set @m=convert(varchar(2),month(getdate()))
IF (LEN(@m) =1)
SET @m = '0' + @m

IF Getdate() < convert(datetime,@y + @m + '20')
SELECT a.AccountNumber
		,SUM(o.RestSumm) RestSumm
    FROM FinanceMain.Operation o
	JOIN AccountingCommon.Account a ON o.AccountId = a.AccountId
	WHERE PeriodTo=207906
    AND IsIncome=0
    AND DocumentTypeId IN (15)
    AND o.RestSumm > 0
    AND o.Date<=DATEADD(mm,-4,GETDATE())
    GROUP BY a.AccountNumber
    --HAVING SUM(o.RestSumm) > 340
else
SELECT a.AccountNumber
		,SUM(o.RestSumm) RestSumm
    FROM FinanceMain.Operation o
	JOIN AccountingCommon.Account a ON o.AccountId = a.AccountId
	WHERE PeriodTo=207906
    AND IsIncome=0
    AND DocumentTypeId IN (15)
    AND o.RestSumm > 0
    AND o.Date<=DATEADD(mm,-3,GETDATE())
    GROUP BY a.AccountNumber
    --HAVING SUM(o.RestSumm) > 340