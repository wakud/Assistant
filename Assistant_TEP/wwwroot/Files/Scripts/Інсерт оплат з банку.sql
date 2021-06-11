--USE TR40_Utility 
declare @ReceiptId int
DECLARE @SpecialAccountId INT 
SET @SpecialAccountId = (SELECT CAST(Value AS INT) FROM Services.Setting 
WHERE Guid = 'C4C981AF-AEB0-4518-B8D4-DF292EE7378E')
INSERT FinanceCommon.Receipt(ReceiptPackageId, AccountId,
PaymentFormId, BillDocumentTypeId, PayDate, TotalSumm, PaymentPeriodFrom, PaymentPeriodTo, DocumentDate, DocumentNumber,
SubsidySumm, ScaleQuantity, Note,
IsDeleted, InsertEmployeeGuid, UpdateEmployeeGuid, InsertDate, UpdateDate)
select {0} as ReceiptPackageId, CASE WHEN {1} = 0 THEN @SpecialAccountId ELSE {1}
END as AccountId,
1 as PaymentFormId, 15 as BillDocumentTypeId,
convert(datetime, '{2}', 103) as PayDate,convert(decimal(10, 2), '{3}') as TotalSumm,{4} as PaymentPeriodFrom,
{5} as PaymentPeriodTo,getdate() as DocumentDate,'' as DocumentNumber,0 as SubsidySumm,
0 as ScaleQuantity, CASE WHEN {1} = 0 THEN '{6} {7}' ELSE '' END as Note,
0 as IsDeleted,newid(),newid(),getdate(),getdate()
set @ReceiptId = @@identity
if {8} > -1
begin
INSERT FinanceCommon.ReceiptIndex(Guid, ReceiptId, ScaleNumber, OldValue, NewValue, Consumption)
SELECT NEWID(),@ReceiptId,1, 0 as OldValue,
case when {8}> -1 then {8} else 0 end as NewValue,{8}
end
if {8}> -1
begin
update FinanceCommon.Receipt
set ScaleQuantity = 1 
where ReceiptId = @ReceiptId
end