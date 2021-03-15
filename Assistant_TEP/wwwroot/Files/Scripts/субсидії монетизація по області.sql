DECLARE @per INT; SET @per = CONVERT(VARCHAR(6),DATEADD(mm, -1, GETDATE()),112) -- 1 міс назад
DECLARE @raj INT; SET @raj = 27

DECLARE @tab TABLE (
		raj INT NOT NULL, 
		FullName CHAR(100) NULL, 
		osRah BIGINT NOT NULL, 
		newRah BIGINT NOT NULL, 
		sumSpogyto DECIMAL(10,2) NULL, 
		sumBorg DECIMAL(10,2) NULL
		)

WHILE @raj <=44
	BEGIN
/*вибираємо базу даних*/
		IF @raj = 27
		begin
			use [TR27_Utility] 
		end
		IF @raj = 28
		BEGIN
			USE [TR28_Utility] 
		end
		IF @raj = 29
		BEGIN
			USE [TR29_Utility] 
		end
		IF @raj = 30
		BEGIN
			USE [TR30_Utility] 
		end
		IF @raj = 31
		BEGIN
			USE [TR31_Utility] 
		end
		IF @raj = 32
		BEGIN
			USE [TR32_Utility] 
		end
		IF @raj = 33
		BEGIN
			USE [TR33_Utility] 
		end
		IF @raj = 34
		BEGIN
			USE [TR34_Utility] 
		end
		IF @raj = 35
		BEGIN
			USE [TR35_Utility] 
		end
		IF @raj = 36
		BEGIN
			USE [TR36_Utility] 
		end
		IF @raj = 37
		BEGIN
			USE [TR37_Utility] 
		end
		IF @raj = 38
		BEGIN
			USE [TR38_Utility] 
		end
		IF @raj = 39
		BEGIN
			USE [TR39_Utility] 
		end
		IF @raj = 40
		BEGIN
			USE [TR40_Utility] 
		end
		IF @raj = 41
		BEGIN
			USE [TR41_Utility] 
		end
		IF @raj = 42
		BEGIN
			USE [TR42_Utility] 
		end
		IF @raj = 43
		BEGIN
			USE [TR43_Utility] 
		end
		IF @raj = 44
		begin
			USE [TR44_Utility] 
		END
        
	INSERT @tab
	(
		raj,
		FullName,
		osRah,
		newRah,
		sumSpogyto,
		sumBorg
	)
	SELECT @raj
			, FullName as pip
			, a.AccountNumber as osRah
			, a.AccountNumberNew as newRah
			, fin.ChargedSumm AS sumSpogyto
			, fin.DebetEnd AS sumBorg
	FROM AccountingCommon.Account a
	JOIN AccountingCommon.PhysicalPerson pp ON pp.PhysicalPersonId =a.PhysicalPersonId
	JOIN AccountingCommon.Address ad ON ad.AddressId = a.AddressId
	JOIN AccountingCommon.UsageObject uo ON uo.AccountId = a.AccountId
	JOIN AccountingCommon.Point p ON p.UsageObjectId = uo.UsageObjectId
	join FinanceCommon.SupplierSaldo fin on fin.AccountId = a.AccountId
	WHERE fin.Period = @per AND a.DateTo = CONVERT(datetime, '06.06.2079', 103)

	SET @raj = @raj +1 
END

SELECT * FROM @tab
ORDER BY 1