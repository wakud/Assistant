DECLARE @table TABLE (AccountNumber VARCHAR(10))
$params$

DECLARE @acc TABLE (NewAcc VARCHAR(10), accId INT, acc VARCHAR(10))
INSERT @acc
(
    NewAcc, acc,
    accId
)
SELECT a.AccountNumberNew, a.AccountNumber, 
		accId = a.AccountId
From @table t 
JOIN TR43_Utility.AccountingCommon.Account a ON t.AccountNumber = a.AccountNumber
 
SELECT * FROM @acc