--declare @SetContractNumber varchar(100) = '*612'

DROP TABLE IF EXISTS ##t9abo$cok$
DROP TABLE IF EXISTS ##resabo$cok$

IF @SetContractNumber = ''
	set @SetContractNumber = '*'

CREATE TABLE ##t9abo$cok$(contract_number varchar(50))

INSERT INTO ##t9abo$cok$(contract_number)
SELECT ContractNumber as contract_number
from Contract
where ContractNumber like REPLACE(@SetContractNumber, '*', '%')

DECLARE @ContractNumberabo varchar(50) = '123213122311231232'

SELECT
	c.ContractNumber as 'Номер договору',
	ct.ShortName as 'Найменування споживача', 
	obj.name as 'Назва обєкту', 
	obj.AllowPower as 'Дозволена потужність'
INTO ##resabo$cok$
FROM Object obj
JOIN Contractor ct on ct.ContractorId = obj.ContractorId
JOIN Contract c on c.ContractorId = ct.ContractorId
WHERE 1 = 0


WHILE (SELECT COUNT(*) FROM ##t9abo$cok$) <> 0
BEGIN
	SELECT TOP 1 @ContractNumberabo = ##t9abo$cok$.contract_number FROM ##t9abo$cok$
	
	INSERT INTO ##resabo$cok$
	SELECT
		c.ContractNumber as 'Номер договору',
		ct.ShortName as 'Найменування споживача', 
		obj.name as 'Назва обєкту', 
		obj.AllowPower as 'Дозволена потужність'
	FROM Object obj
	JOIN Contractor ct on ct.ContractorId = obj.ContractorId
	JOIN Contract c on c.ContractorId = ct.ContractorId
	WHERE obj.PeriodEnd = '2079-06-06' and c.ContractState = 1 and c.ContractType = 0
	AND (@ContractNumberabo is NULL or c.ContractNumber = @ContractNumberabo)
	UNION
	SELECT 
		c.ContractNumber as 'Номер договору',
		' ЗАГАЛЬНА' as 'Найменування споживача', 
		' ДОЗВОЛЕНА ПОТУЖНІСТЬ' as 'Назва обєкту', 
		SUM(obj.AllowPower) as 'Дозволена потужність'
	FROM Object obj
	JOIN Contractor ct on ct.ContractorId = obj.ContractorId
	JOIN Contract c on c.ContractorId = ct.ContractorId
	WHERE obj.PeriodEnd = '2079-06-06' and c.ContractState = 1 and c.ContractType = 0
	AND (@ContractNumberabo is NULL or c.ContractNumber = @ContractNumberabo)
	GROUP BY c.ContractNumber

	INSERT INTO ##resabo$cok$ VALUES ('------------------', '------------------', '-----------------------------------', 0)

	DELETE TOP(1) FROM ##t9abo$cok$
END

SELECT * FROM ##resabo$cok$
