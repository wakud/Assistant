DECLARE @ForSupplier BIT SET @ForSupplier = 1

if exists (select * from tempdb..sysobjects where id = object_id('tempdb..#ebank'))
drop table #ebank

create table #ebank (
					[ptr] [int] NOT NULL 
					,[numbpers] [bigint] NOT NULL
					,[NEWnumbpers] [bigint] NOT NULL 
					,[street_ptr] [int] NOT NULL 
					,[street] [varchar] (40) NOT NULL 
					,[house] [varchar] (10) NULL 
					,[apartment] [varchar] (5) NULL 
					,[tank] [varchar] (10) NULL 
					,[family] [varchar] (40) NULL 
					,[ldate] [datetime] NULL 
					,[lcount] VARCHAR(30)
					,[billdate] [datetime] NULL 
					,[datestart] [datetime] NULL 
					,[c_start] VARCHAR(30)
					,[dateons] [datetime] NULL 
					,[c_ons] VARCHAR(30)
					,[ecount] [int] NULL 
					,[billsumma] [decimal](14, 2) NOT NULL 
					,[subsyd] [decimal](14, 2) NULL 
					,[borgsumma] [decimal](14, 2) not NULL 
					,[tariff] [decimal](13, 4) NOT NULL 
					,[limit] [int] NULL 
					,[discount] [decimal](5, 2) NULL 
					,[kredyt] [decimal](14, 2) not NULL 
					,[realsumm] [decimal](14, 2)  NULL 
					,[us_subsyd] [decimal](14, 2) NULL 
					,[data_close] [datetime] NULL
					,[LastPayDat] [datetime] NULL 
					,[oplata_pop] [decimal](14, 2) NULL 
					,[oplata_cur] [decimal](14, 2) NULL 
					,[saldo_p] [decimal](14, 2) NULL 
					,[do_oplaty] [decimal](9, 2) NULL 
)
insert #ebank (
				ptr
				,numbpers
				, NEWnumbpers
				,street_ptr
				,street
				,house
				,apartment
				,tank
				,family
				,tariff
				,limit
				,discount
				,borgsumma
				, kredyt
				, billsumma
				,data_close
				)
select A.AccountId AS ptr
		, A.AccountNumber AS numbpers
		, A.AccountNumberNew AS NEWnumbpers
		, ADR.StreetId AS street_ptr
		,LEFT(AD.Name,40) AS name
		,ADR.Building AS house
		, ADR.Apartment AS apartment
		, ADR.BuildingPart AS tank
		,LEFT(PP.LastName + ' ' + PP.FirstName + ' ' +  PP.SecondName,40) AS family
		,(SELECT TOP 1 T.Price 
		  FROM Dictionary.Tariff T 
		  WHERE T.TariffGroupId = TM.TariffGroupId 
				AND TM.Is30km = T.Is30km 
				AND TM.IsAPK = T.IsAPK 
				AND TM.IsHeating = T.IsHeating 
				AND TM.IsHighlander = T.IsHighlander
				AND T.DateTo = '20790606' 
				AND T.TimeZoneId = 1 
		  ORDER BY T.TarifficationBlockLineId
		 ) AS tariff
		,CASE TMI.QuantityTo WHEN 999999 THEN 0 ELSE TMI.QuantityTo END AS limit
		,BC.Discount AS discount
		,0
		,0
		,0
		,CASE WHEN A.DateTo = '20790606' THEN NULL ELSE A.DateTo END AS dateshut  
FROM AccountingCommon.Account A (NOLOCK) JOIN AccountingCommon.PhysicalPerson PP ON A.PhysicalPersonId = PP.PhysicalPersonId
JOIN AccountingCommon.UsageObject UO  (NOLOCK) ON A.AccountId = UO.AccountId
JOIN AccountingCommon.Point P  (NOLOCK) ON UO.UsageObjectId = P.UsageObjectId
JOIN AccountingCommon.TarifficationMethod TM  (NOLOCK) ON P.PointId = TM.PointId AND P.DateTo = TM.DateTo -- !!!!!
JOIN AccountingCommon.Address ADR  (NOLOCK) ON A.AddressId = ADR.AddressId
LEFT JOIN AddressDictionary.Street AD  (NOLOCK) ON ADR.StreetId = AD.StreetId
LEFT JOIN AccountingCommon.TarifficationMethodItem TMI  (NOLOCK) ON TMI.TarifficationMethodId = TM.TarifficationMethodId AND TMI.QuantityFrom = 0
LEFT JOIN AccountingDictionary.BenefitsCategory BC  (NOLOCK) ON TMI.BenefitsCategoryId = BC.BenefitsCategoryId
WHERE A.DateTo = '20790606'
AND ADR.StreetId IS NOT NULL
AND AD.Name IS NOT NULL

if exists (select * from tempdb..sysobjects where id = object_id('tempdb..#b_summa'))
drop table #b_summa

create table #b_summa(fscore_ptr int,billdate datetime,
billsumma decimal(10,2),us_subsyd decimal(10,2),
datestart datetime,dateons datetime,
realsumm decimal(10,2),ecount int)

insert #b_summa (fscore_ptr,billdate,billsumma,us_subsyd,
                 datestart,dateons,realsumm,ecount)
select 
B.AccountId AS fscore_ptr
,max(B.Date) AS billdate
,sum(BD.TotalSumm - BD.UsedSumm) AS billsumma
,SUM(ISNULL(SS.Subsyd,0)) AS us_subsyd --sum(subsyd) --&&&
,MIN(BD.[ConsumptionFrom]) AS datestart --140908 O.Chekh
,MAX(BD.[ConsumptionTo]) as  dateons ----------------max(dateons),
,sum(BD.TOTALSUMM) AS realsumm--sum(summ+fsc_balance+subsyd),
,SUM(BD.Quantity) AS ecount--sum(ecount)
from FinanceMain.Operation B  (NOLOCK) JOIN FinanceMain.OperationRow BD  (NOLOCK) ON B.OperationId = BD.OperationId AND B.IsIncome = 0 AND B.PeriodTo = 207906
CROSS JOIN (SELECT TOP 1 CAST(VALUE AS INT) AS daydebt FROM Services.Setting WHERE Guid = '826C4666-F79C-4558-A0BB-2D5A428FCE1B') S
LEFT JOIN --Subsyd
(SELECT POR.BillOperationRowId, SUM(POR.TotalSumm)  AS Subsyd 
FROM FinanceMain.PayOffRow POR  (NOLOCK)
JOIN FinanceMain.PayOff POF  (NOLOCK)ON POR.PayOffId = POF.PayOffId
JOIN FinanceMain.OperationRow PPP  (NOLOCK) ON POR.PayOperationRowId = PPP.OperationRowId
JOIN FinanceMain.Operation PP  (NOLOCK) ON PPP.OperationId = PP.OperationId AND PP.DocumentTypeId = 5 AND PP.IsIncome =1 
WHERE POF.PeriodTo = 207906
GROUP BY POR.BillOperationRowId) SS ON SS.BillOperationRowId = BD.OperationRowId
WHERE B.Date > DATEADD(DAY,-S.daydebt, getdate())
--AND B.AccountId = 101758
AND (B.CalcMethod IN (1,2,3) OR B.DocumentTypeId = 5) -- 180425 Lemeshko: Added  OR B.DocumentTypeId = 5
AND B.SaldoKind = CASE WHEN @ForSupplier = 1 THEN 4 ELSE 1 END
--SELECT * FROM #b_summa
GROUP BY B.AccountId

if exists (select * from tempdb..sysobjects where id = object_id('tempdb..#b_borg'))
drop table #b_borg
create table #b_borg (fscore_ptr int,borgsumma decimal(10,2),borgdate datetime)
insert #b_borg
select 
B.AccountId AS fscore_ptr
,SUM(BD.TotalSumm - BD.UsedSumm) AS borgsumma
,MAX(BD.ConsumptionTo) AS borgdate
from FinanceMain.Operation B  (NOLOCK) JOIN FinanceMain.OperationRow BD  (NOLOCK) ON B.OperationId = BD.OperationId AND B.IsIncome = 0 AND B.PeriodTo = 207906
CROSS JOIN (SELECT TOP 1 CAST(VALUE AS INT) AS daydebt FROM Services.Setting WHERE Guid = '826C4666-F79C-4558-A0BB-2D5A428FCE1B') S
where B.Date <= DATEADD(DAY,-S.daydebt, getdate())
--AND B.AccountId = 101758
AND B.CalcMethod in (1,2,3) ---- !!!! and avgbill = 0 
AND BD.TotalSumm - BD.UsedSumm > 0
AND B.SaldoKind = CASE WHEN @ForSupplier = 1 THEN 4 ELSE 1 END
GROUP BY B.AccountId

if exists (select * from tempdb..sysobjects where id = object_id('tempdb..#max_kontrol_date'))
drop table #max_kontrol_date
create table #max_kontrol_date (fscore_ptr int,dateons datetime)
insert #max_kontrol_date
select B.AccountId AS fscore_ptr, MAX(BD.ConsumptionTo)--max(bill.dateons)
FROM FinanceMain.Operation B  (NOLOCK) JOIN FinanceMain.OperationRow BD  (NOLOCK) ON B.OperationId = BD.OperationId AND B.IsIncome = 0
WHERE B.PeriodTo = 207906 AND B.CalcMethod in (1,2)
--AND B.AccountId = 101758
AND B.AccountId NOT IN (SELECT B1.AccountId FROM FinanceMain.Operation B1  (NOLOCK)
CROSS JOIN (SELECT TOP 1 CAST(VALUE AS INT) AS daydebt FROM Services.Setting WHERE Guid = '826C4666-F79C-4558-A0BB-2D5A428FCE1B') S
WHERE B.AccountId = B1.AccountId AND B1.Date > DATEADD(DAY,-S.daydebt, getdate()) AND B1.PeriodTo = 207906 AND B1.CalcMethod in (1,2)
)
AND B.SaldoKind = CASE WHEN @ForSupplier = 1 THEN 4 ELSE 1 END
GROUP BY B.AccountId

create index ebank_ptr on #ebank(ptr)
create index ebank_ptr_datestart on #ebank(ptr,datestart)
create index ebank_ptr_dateons on #ebank(ptr,dateons)
create index b_summa_fscore_ptr on #b_summa(fscore_ptr)
create index b_borg_fscore_ptr on #b_borg(fscore_ptr)
create index max_kontrol_date_fscore_ptr on #max_kontrol_date(fscore_ptr)

--СУМА ДО ОПЛАТИ І ДАТи ПОТОЧНИХ РАХУНКІВ ТА ВИКОРИСТАНА СУБСИДІЯ
update e
set  billsumma=bs.billsumma,
     billdate=bs.billdate,
     us_subsyd=bs.us_subsyd,
     datestart=bs.datestart,
     dateons=bs.dateons,
     realsumm=bs.realsumm,
     ecount=bs.ecount
from #ebank e, #b_summa bs
where e.ptr=bs.fscore_ptr
--СУМА ДО ОПЛАТИ ПО БОРГОВИХ РАХУНКАХ
update e
set borgsumma=bg.borgsumma
from #ebank e, #b_borg bg
where e.ptr=bg.fscore_ptr
-- ПРОСТАНОВКА КРЕДИТУ
IF @ForSupplier = 1 -- 190211
UPDATE e
set kredyt= ISNULL(LS.Credit,0) + isnull(b.fsc_balance,0)-ISNULL(ls.subsidy,0)
from #ebank e
left join FinanceMain.LightSaldo LS ON e.ptr = LS.AccountId AND LS.Credit > 0 AND LS.SaldoKind = CASE WHEN @ForSupplier = 1 THEN 4 ELSE 1 END -- 190211 SaldoKind = 4
LEFT JOIN (SELECT B.AccountId, SUM(BD.UsedSumm) AS fsc_balance
 FROM FinanceMain.Operation B  (NOLOCK) JOIN FinanceMain.OperationRow BD  (NOLOCK) ON B.OperationId = BD.OperationId AND B.PeriodTo = 207906 AND B.IsIncome = 0
--140904 AND B.CalcMethod > 2 --непрямі методи
 AND B.CalcMethod = 4 --по оплаті 140904 O.Chekh
GROUP BY b.AccountId
) b ON e.ptr = b.AccountId

--140904 O.Chekh CacheIndexForBill
--DECLARE InsertCacheIndexForBill CURSOR
--FOR
--SELECT BR2.BillId FROM FinanceCommon.BillRegular2 BR2 
--JOIN #ebank LB ON BR2.AccountId = LB.ptr
--WHERE --CalcMethod NOT IN (1,2) AND 
--CalcMethod IN (3) AND --140904 O.Chekh
--NOT EXISTS
--(SELECT * FROM SupportDefined.CacheIndexForBill CIFB WHERE CIFB.Billid = BR2.BillId)
--ORDER BY BR2.AccountId, BR2.ConsumptionFrom

--DECLARE @BillId INT
--OPEN InsertCacheIndexForBill
--FETCH NEXT FROM InsertCacheIndexForBill INTO @BillId
--WHILE @@FETCH_STATUS = 0
--BEGIN
--EXEC SupportDefined.spInsertCacheIndexForBill @BillId 

--FETCH NEXT FROM InsertCacheIndexForBill INTO @BillId
--END

--CLOSE InsertCacheIndexForBill; DEALLOCATE InsertCacheIndexForBill
---- 140904

-- Початковий контрольний
--..--
update e set c_start= GI.CachedIndexes
--st.lcount
FROM #ebank e 
JOIN AccountingCommon.UsageObject UO ON e.ptr = UO.AccountId
JOIN AccountingCommon.Point P ON UO.UsageObjectId = P.UsageObjectId
--JOIN Measuring.GroupIndex GI ON P.PointId = GI.PointId
JOIN AccountingCommon.UsageCalculationMethod ucm ON ucm.PointId = P.PointId
JOIN AccountingMeasuring.GroupIndex gi ON gi.UsageCalculationMethodId = ucm.UsageCalculationMethodId
WHERE GI.Date = e.datestart AND GI.IsForCalculate = 1

-- Кінцевий контрольний
update e set c_ons= GI.CachedIndexes
--st.lcount
FROM #ebank e 
JOIN AccountingCommon.UsageObject UO ON e.ptr = UO.AccountId
JOIN AccountingCommon.Point P ON UO.UsageObjectId = P.UsageObjectId
--JOIN Measuring.GroupIndex GI ON P.PointId = GI.PointId
JOIN AccountingCommon.UsageCalculationMethod ucm ON ucm.PointId = P.PointId
--JOIN AccountingMeasuring.GroupIndex gi ON gi.UsageCalculationMethodId = ucm.UsageCalculationMethodId
CROSS APPLY (SELECT top 1 * FROM AccountingMeasuring.GroupIndex ggi WHERE ggi.UsageCalculationMethodId = ucm.UsageCalculationMethodId
    AND gGI.Date = e.dateons AND gGI.IsForCalculate = 1 ORDER BY GroupIndexId DESC) gi
--WHERE GI.Date = e.dateons AND GI.IsForCalculate = 1

UPDATE e 
SET dateons = g.Date,c_ons = g.CachedIndexes
FROM #ebank e
CROSS APPLY ( SELECT TOP 1 
    uo.AccountId, gi.Date, CONVERT(VARCHAR(30),gi.CachedIndexes) CachedIndexes
	FROM AccountingCommon.UsageObject uo
	JOIN AccountingCommon.Point p ON p.UsageObjectId = uo.UsageObjectId
--	JOIN Measuring.GroupIndex gi ON p.PointId = gi.PointId
    JOIN AccountingCommon.UsageCalculationMethod ucm ON ucm.PointId = P.PointId
    JOIN AccountingMeasuring.GroupIndex gi ON gi.UsageCalculationMethodId = ucm.UsageCalculationMethodId
    WHERE uo.AccountId = e.ptr
    ORDER BY gi.Date DESC
) g 
WHERE e.dateons IS NULL 
	
/*UPDATE e 
SET dateons = g.Date,c_ons = g.CachedIndexes
FROM #ebank e
JOIN (
SELECT uo.AccountId,gi.Date,convert(VARCHAR(30),gi.CachedIndexes) CachedIndexes,
ROW_NUMBER() OVER (PARTITION BY uo.AccountId ORDER BY gi.Date DESC) id
	FROM AccountingCommon.UsageObject uo
	JOIN AccountingCommon.Point p ON p.UsageObjectId = uo.UsageObjectId
--	JOIN Measuring.GroupIndex gi ON p.PointId = gi.PointId
    JOIN AccountingCommon.UsageCalculationMethod ucm ON ucm.PointId = P.PointId
    JOIN AccountingMeasuring.GroupIndex gi ON gi.UsageCalculationMethodId = ucm.UsageCalculationMethodId
) g ON e.ptr = g.AccountId AND g.id = 1
WHERE e.dateons IS NULL */

-- ПРОСТАНОВКА дати зняття контрольного показника для абонентiв без поточки
update e
set datestart=st.dateons
from #ebank e, #max_kontrol_date st
where e.ptr=st.fscore_ptr and e.billdate is null
-- ПРОСТАНОВКА дати зняття контрольного показника для абонентiв без рахункiв
update e
set datestart= GIM.MaxDate
--st.dateexecs
FROM #ebank e 
JOIN AccountingCommon.UsageObject UO ON e.ptr = UO.AccountId
JOIN AccountingCommon.Point P ON UO.UsageObjectId = P.UsageObjectId
--JOIN (SELECT GI.PointId, MAX(GI.Date) AS MaxDate FROM  Measuring.GroupIndex GI 
CROSS APPLY (SELECT MAX(GI.Date) AS MaxDate FROM AccountingCommon.UsageCalculationMethod ucm
    JOIN AccountingMeasuring.GroupIndex gi ON gi.UsageCalculationMethodId = ucm.UsageCalculationMethodId
    WHERE ucm.PointId = P.PointId
    AND GI.IsForCalculate = 1
    ) GIM
WHERE e.billdate is NULL AND e.datestart is NULL
-- ПРОСТАНОВКА значення контрольного показника для абонентiв без поточки (рахункiв)
update e
set c_start= GI.CachedIndexes
--st.lcount
FROM #ebank e 
JOIN AccountingCommon.UsageObject UO ON e.ptr = UO.AccountId
JOIN AccountingCommon.Point P ON UO.UsageObjectId = P.UsageObjectId
--JOIN Measuring.GroupIndex GI ON P.PointId = GI.PointId
JOIN AccountingCommon.UsageCalculationMethod ucm ON ucm.PointId = P.PointId
JOIN AccountingMeasuring.GroupIndex gi ON gi.UsageCalculationMethodId = ucm.UsageCalculationMethodId
WHERE GI.IsForCalculate = 1
and e.billdate is null 
and GI.Date=e.datestart

-- ПРОСТАНОВКА дати останньої оплати
update e
set LastPayDat = o.MAxPayDate
--o.data
from #ebank e
JOIN (SELECT R.AccountId, MAX(R.PayDate) AS MAxPayDate  FROM FinanceCommon.Receipt R
--FinanceMain.Operation P 
WHERE R.IsDeleted = 0 
GROUP BY R.AccountId
) o ON e.ptr = o.AccountId
update #ebank set us_subsyd=0  where us_subsyd is null
update #ebank set realsumm=0   where realsumm is null
update #ebank set oplata_pop=0 where oplata_pop is null
update #ebank set oplata_cur=0 where oplata_cur is null
update #ebank set saldo_p=0    where saldo_p is null

--оплата в попередньому місяцці
update e
set oplata_pop =o.sum_pop
from #ebank e 
JOIN (SELECT R.AccountId, SUM(R.TotalSumm) AS sum_pop  FROM FinanceCommon.Receipt R
    CROSS JOIN (SELECT TOP 1 CAST(VALUE + '01' AS SMALLDATETIME) AS CurenMonth FROM Services.Setting WHERE Guid = 'E6AC6284-6983-46E1-9A9D-D110BE68E954') S
    JOIN FinanceMain.Operation o ON o.DocumentId = r.ReceiptId AND o.DocumentTypeId = 4
    AND o.SaldoKind = CASE WHEN @ForSupplier = 1 THEN 4 ELSE 1 END
--FinanceMain.Operation P 
WHERE R.IsDeleted = 0  
and month(R.PayDate)=month(dateadd(mm,-1,s.CurenMonth))
and year(R.PayDate)=year(dateadd(mm,-1,S.CurenMonth))
GROUP BY R.AccountId) O ON e.ptr = O.AccountId

--оплата в поточному місяці
update e
set oplata_cur =o.sum_cur
from #ebank e 
JOIN (SELECT R.AccountId, SUM(R.TotalSumm) AS sum_cur  FROM FinanceCommon.Receipt R
    CROSS JOIN (SELECT TOP 1 CAST(VALUE + '01' AS SMALLDATETIME) AS CurenMonth FROM Services.Setting WHERE Guid = 'E6AC6284-6983-46E1-9A9D-D110BE68E954') S
    JOIN FinanceMain.Operation o ON o.DocumentId = r.ReceiptId AND o.DocumentTypeId = 4
    AND o.SaldoKind = CASE WHEN @ForSupplier = 1 THEN 4 ELSE 1 END
--FinanceMain.Operation P 
WHERE R.IsDeleted = 0  
and month(R.PayDate)=month(dateadd(mm,0,s.CurenMonth))
and year(R.PayDate)=year(dateadd(mm,0,S.CurenMonth))
GROUP BY R.AccountId) O ON e.ptr = O.AccountId

--1абонент з що мають борговий рахунок(попередній) і поточний ще негасився 
update #ebank
set saldo_p= borgsumma+oplata_pop+oplata_cur-kredyt
where borgsumma>0  and (realsumm-us_subsyd)=billsumma
--2абонент з що мають борговий рахунок(попередній) і поточний гасився частково
update #ebank
set saldo_p=borgsumma+oplata_pop+oplata_cur-(realsumm-us_subsyd-billsumma)-kredyt
where borgsumma>0 and (realsumm-us_subsyd)>billsumma
--3поточний рахунок не погашався,а тільки боргові рахунки погашалися
update #ebank
set saldo_p=oplata_pop+oplata_cur-kredyt
where borgsumma=0 and (realsumm-us_subsyd)=billsumma
--4поточний рахунок  погашався частково ,а борговий рахунок погашений 
update #ebank
set saldo_p=oplata_pop+oplata_cur-kredyt-(realsumm-us_subsyd-billsumma) 
where borgsumma=0 and (realsumm-us_subsyd)>billsumma

update #ebank set do_oplaty=0

update #ebank  set do_oplaty= saldo_p+realsumm-us_subsyd-oplata_pop

-- Початковий контрольний для непрямих методів 140904
update e set c_start= CI.[CachedIndexesFrom]
--st.lcount
FROM #ebank e 
JOIN AccountingCommon.UsageObject UO ON e.ptr = UO.AccountId
JOIN AccountingCommon.Point P ON UO.UsageObjectId = P.UsageObjectId
--JOIN Measuring.GroupIndex GI ON P.PointId = GI.PointId
JOIN FinanceCommon.BillRegular2 BR2 ON BR2.AccountId =UO.AccountId AND BR2.IsDeleted = 0 AND BR2.ConsumptionFrom =  e.datestart
JOIN [SupportDefined].[CacheIndexForBill] CI ON CI.[AccountId] = UO.AccountId AND CI.[Billid] = BR2.BillId
WHERE e.c_start IS NULL 

-- Кінцевий контрольний для непрямих методів 140904
update e set c_ons= CI.[CachedIndexesTo]
--st.lcount
FROM #ebank e 
JOIN AccountingCommon.UsageObject UO ON e.ptr = UO.AccountId
JOIN AccountingCommon.Point P ON UO.UsageObjectId = P.UsageObjectId
JOIN FinanceCommon.BillRegular2 BR2 ON BR2.AccountId =UO.AccountId AND BR2.IsDeleted = 0 AND BR2.ConsumptionTo = e.dateons
JOIN [SupportDefined].[CacheIndexForBill] CI ON CI.[AccountId] = UO.AccountId AND CI.[Billid] = BR2.BillId
WHERE e.c_ons IS NULL

--GI.Date = e.dateons AND GI.IsForCalculate = 1

select ptr
        , numbpers
        , NEWnumbpers
        , street_ptr
        , street
        , house
        , apartment
        , tank
        , family
        , ldate
        , lcount
        , billdate
        , datestart
        , c_start
        , dateons
        , c_ons
        , ecount
        , billsumma
        , subsyd
        , borgsumma
        , tariff
        , limit
        , discount
        , kredyt
        , realsumm
        , us_subsyd
        , data_close
        , lastpaydat
        , oplata_pop
        , oplata_cur
        , saldo_p
        , do_oplaty 
from #ebank