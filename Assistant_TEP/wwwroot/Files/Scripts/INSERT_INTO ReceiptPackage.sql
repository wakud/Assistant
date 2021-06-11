--USE TR40_Utility 
INSERT INTO FinanceCommon.ReceiptPackage (
		Number, 
		Guid, 
		ReceiveDate, 
        ReceiptQuantity, 
		TotalSumm, 
		ReceiptSourceId
) 
select '{0}',newid(),convert(datetime,'{1}',103),{2},{3},{4}
select @@identity AS Id