DROP TABLE IF EXISTS #Benefits
DROP TABLE IF EXISTS #dbf2
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
--GO

DECLARE @Period INT,
		@BudgetId  TINYINT
		, @withGroup BIT = 1,
		@xml XML = null
BEGIN
SET NOCOUNT ON;
SET @period = 202012        --потрібно буде тягнути з програми

DECLARE @CDPR NUMERIC(12)
DECLARE @pc VARCHAR(10) --поки що залишаю,
DECLARE @IsMultiBudjet BIT --поки що залишаю,

DECLARE @StartSystem VARCHAR(6)
SELECT  @StartSystem = s.[value] FROM    Services.Setting AS s WHERE   s.Guid = '60CA4304-9E2F-4798-9826-6C1A6D05D173'
 
SET @pc = 2  --1--:Pc /*1- Державний бюджет, 2 - Міський бюджет, NULL - всі бюджети*/

SET @IsMultiBudjet = 0
DECLARE @TariffNotBlock TABLE (TarifficationBlockLineId INT) 
INSERT @TariffNotBlock
        ( TarifficationBlockLineId )
SELECT TarifficationBlockLineId FROM Dictionary.TarifficationBlockLine
WHERE ShortName LIKE '%не обмежено%'
OR ShortName LIKE 'до%' 

--Параметри ЦОК
SELECT @CDPR = 42145798

DECLARE @BenefitsCategory TABLE  (BenefitsCategoryId INT)
IF @xml IS NULL
	INSERT @BenefitsCategory 
	SELECT BenefitsCategoryId
	FROM AccountingDictionary.BenefitsCategory WHERE IsBenefits=1 OR Name LIKE '%оплата%'
ELSE 
	INSERT @BenefitsCategory 
	SELECT BenefitsCategoryId
	FROM AccountingDictionary.BenefitsCategory 
	WHERE  BenefitsCategoryId IN (SELECT t.c.value('@BenefitsId','int')as StaffId FROM @xml.nodes('/Benefits') AS T(c));	
	WITH    Benefits ( isBlock,TariffGroupId, /*20120905 Tariffid, */ TimeZoneId, AccountId, BenefitsCategoryId, Tariff, IsForHeatingSeason, ConsumptionFrom, ConsumptionTo, Discount, Summ, Quantity, BenefitsCertificateId, BeneficiaryQuantity )
          AS ( SELECT   
          CASE WHEN tnb.TarifficationBlockLineId IS NULL THEN 1 ELSE 0 END AS isBlock,
          tm.TariffGroupId,
                        T.TimeZoneId, A.AccountId, BD.BenefitsCategoryId,
                        BD.TariffPrice * ( 1 + BD.VATRate / 100 ) AS Tariff, ISNULL(tmi.IsForHeatingSeason, 0),
                        CASE WHEN ISNULL(tmi.IsForHeatingSeason, 0) = 1
                             THEN MIN(CASE WHEN (b.PeriodFrom < @StartSystem OR BRP.PreviousSystem = 1)
                                                AND DATEDIFF(m, BD.ConsumptionFrom, BD.ConsumptionTo) > 0
                                           THEN BD.ConsumptionFrom + 1
                                           ELSE BD.ConsumptionFrom
                                      END)
                             WHEN DATEDIFF(MONTH,
                                           MIN(CASE WHEN (b.PeriodFrom < @StartSystem OR BRP.PreviousSystem = 1)
                                                         AND DATEDIFF(m, BD.ConsumptionFrom, BD.ConsumptionTo) > 0
                                                    THEN BD.ConsumptionFrom + 1
                                                    ELSE BD.ConsumptionFrom
                                               END), MIN(TM.DateFrom)) = 0 THEN MIN(TM.DateFrom)
                             ELSE DATEADD(M,
                                          DATEDIFF(M, 0,
                                                   MIN(CASE WHEN (b.PeriodFrom < @StartSystem OR BRP.PreviousSystem = 1)
                                                                 AND DATEDIFF(m, BD.ConsumptionFrom, BD.ConsumptionTo) > 0
                                                            THEN BD.ConsumptionFrom + 1
                                                            ELSE BD.ConsumptionFrom
                                                       END)), 0)
                        END AS ConsumptionFrom,
                        CASE WHEN ISNULL(tmi.IsForHeatingSeason, 0) = 1 THEN MAX(BD.ConsumptionTo) - 1 -- 181207 Lemeshko, додав фрагмент "- 1"
                             WHEN DATEDIFF(M,
                                           MAX(CASE WHEN (b.PeriodFrom >= @StartSystem AND isnull(BRP.PreviousSystem,0) = 0)
                                                         AND DATEDIFF(m, BD.ConsumptionFrom, BD.ConsumptionTo) > 0
                                                    THEN BD.ConsumptionTo - 1
                                                    ELSE BD.ConsumptionTo
                                               END), MAX(TM.DateTo)) = 0 THEN MAX(TM.DateTo) - 1 -- якщо місяць співпадає з місяцем початку дії тарифікації, то беремо дату тарифікації
                             ELSE (MAX(CASE WHEN (b.PeriodFrom >= @StartSystem AND isnull(BRP.PreviousSystem,0) = 0)
                                                                        AND DATEDIFF(m, BD.ConsumptionFrom,
                                                                                     BD.ConsumptionTo) > 0
                                                                   THEN BD.ConsumptionTo - 1
                                                                   ELSE BD.ConsumptionTo
                                                              END))
                        END AS ConsumptionTo, BD.Discount,
                        SUM(CASE WHEN B.PeriodTo = @period
                                      OR ( b.DocumentTypeId IN ( 9 )
                                           AND b.isincome = 1
                                         ) THEN -BD.DiscountSumm --end as ConsumptionTo, BD.Discount, SUM(CASE when B.PeriodTo = @period then -BD.DiscountSumm
                                 ELSE BD.DiscountSumm
                            END) AS Summ,
                        SUM(CASE WHEN B.PeriodTo = @period
                                      OR ( b.DocumentTypeId IN ( 9 )
                                           AND b.isincome = 1
                                         ) THEN -BD.Quantity --end) as Summ, SUM(CASE when B.PeriodTo = @period then -BD.Quantity
                                 ELSE BD.Quantity
                            END) AS ecount, BD.BenefitsCertificateId AS BenefitsCertificateId, TM.BeneficiaryQuantity
               FROM     FinanceMain.OperationRow BD WITH ( NOLOCK )
                        JOIN [Dictionary].Tariff AS T ON BD.TariffId = t.TariffId
                        JOIN FinanceMain.[Operation] B WITH ( NOLOCK ) ON BD.OperationId = B.OperationId
                        JOIN AccountingDictionary.BenefitsCategory PC ON PC.BenefitsCategoryId = BD.BenefitsCategoryId
                        JOIN @BenefitsCategory bcat ON bcat.BenefitsCategoryId = pc.BenefitsCategoryId
                        JOIN AccountingCommon.Account A WITH ( NOLOCK ) ON B.AccountId = A.AccountId
                        JOIN AccountingCommon.UsageObject AS UO WITH ( NOLOCK ) ON A.AccountId = UO.AccountId
                        JOIN AccountingCommon.Address ADR WITH ( NOLOCK ) ON UO.AddressId=ADR.AddressId
                        JOIN AccountingCommon.Point AS P WITH ( NOLOCK ) ON UO.UsageObjectId = P.UsageObjectId
                        LEFT JOIN AccountingCommon.TarifficationMethod AS tm WITH ( NOLOCK ) ON P.PointId = tm.PointId
                                                                                                AND BD.ConsumptionFrom BETWEEN TM.DateFrom AND DATEADD(day,
                                                                                                        -1, TM.DateTo)
                        LEFT JOIN AccountingDictionary.HeatingSeason AS hs ON bd.ConsumptionFrom >= hs.DateFrom
                                                                              AND bd.ConsumptionFrom < hs.DateTo
                        LEFT JOIN AccountingCommon.TarifficationMethodItem AS tmi WITH ( NOLOCK ) ON tm.TarifficationMethodId = tmi.TarifficationMethodId
                                                                                                     AND pc.BenefitsCategoryId = tmi.BenefitsCategoryId
                                                                                                     AND tmi.Discount > 0
                                                                                                     AND tmi.IsForHeatingSeason = CASE
                                                                                                        WHEN hs.HeatingSeasonId IS NULL
                                                                                                        THEN 0
                                                                                                        ELSE 1
                                                                                                        END
                LEFT JOIN FinanceCommon.BillRegularParams BRP  ( NOLOCK ) ON  BRP.BillId = B.OperationId --140602 O.Chekh
                LEFT JOIN @TariffNotBlock tnb ON tnb.TarifficationBlockLineId = t.TarifficationBlockLineId
                WHERE    ( ( b.IsIncome = 0
                            AND B.DocumentTypeId IN ( 1, 15 )
                          )
                          OR B.DocumentTypeId IN ( 9 )
                        ) --and b.AccountId = 79030--B.IsIncome = 0
                        AND BD.DiscountSumm != 0
                        AND ( B.PeriodFrom = @period
                              OR B.PeriodTo = @period
                            )
                        AND PC.BudgetId = ISNULL(@BudgetId,PC.BudgetId)
               GROUP BY A.AccountId,CASE WHEN tnb.TarifficationBlockLineId IS NULL THEN 1 ELSE 0 END,P.PointId, tm.TariffGroupId, BD.TariffPrice * ( 1 + BD.VATRate / 100 ),
                        BD.BenefitsCategoryId, ISNULL(tmi.IsForHeatingSeason, 0),
                        MONTH(CASE WHEN (b.PeriodFrom < @StartSystem OR BRP.PreviousSystem = 1)
                                        AND DATEDIFF(m, BD.ConsumptionFrom, BD.ConsumptionTo) > 0
                                   THEN BD.ConsumptionFrom + 1
                                   ELSE BD.ConsumptionFrom
                              END),
                        YEAR(CASE WHEN (b.PeriodFrom < @StartSystem OR BRP.PreviousSystem = 1)
                                       AND DATEDIFF(m, BD.ConsumptionFrom, BD.ConsumptionTo) > 0
                                  THEN BD.ConsumptionFrom + 1
                                  ELSE BD.ConsumptionFrom
                             END), BD.Discount, BD.BenefitsCertificateId, TM.BeneficiaryQuantity,
                        --bd.tariffid , -- 20120905 при зміні тарифу виводилося два записи, треба групувати по ціні
                        T.TimeZoneId
               HAVING   SUM(CASE WHEN B.PeriodTo = @period
                                      OR ( b.DocumentTypeId IN ( 9 )
                                           AND b.isincome = 1
                                         ) THEN -BD.DiscountSumm --having SUM(CASE when B.PeriodTo = @period then -BD.DiscountSumm
                                 ELSE BD.DiscountSumm
                            END) != 0),
        PrevBenefits ( AccountId, BenefitsCategoryId, Tariff,/*20120905 TariffId, */ TimeZoneId, IsForHeatingSeason, discount, BenefitsCertificateId, YearConsumptionFrom, MonthConsumptionFrom, MinConsumptionFrom, MaxConsumptionFrom, DiscountEcount, [Days], [DiscountSumm], BeneficiaryQuantity )
          AS ( SELECT   B.AccountId, BD.BenefitsCategoryId, BD.TariffPrice * ( 1 + BD.VATRate / 100 ) AS Tariff, --20120905
                        --BD.TariffId ,
                        T.TimeZoneId, ISNULL(tmi.IsForHeatingSeason, 0), BD.discount, BD.BenefitsCertificateId,
                        YEAR(CASE WHEN (b.PeriodFrom < @StartSystem OR BRP.PreviousSystem = 1)
                                       AND DATEDIFF(m, BD.ConsumptionFrom, BD.ConsumptionTo) > 0
                                  THEN BD.ConsumptionFrom + 1
                                  ELSE BD.ConsumptionFrom
                             END) AS 'Y_Date',
                        MONTH(CASE WHEN (b.PeriodFrom < @StartSystem OR BRP.PreviousSystem = 1)
                                        AND DATEDIFF(m, BD.ConsumptionFrom, BD.ConsumptionTo) > 0
                                   THEN BD.ConsumptionFrom + 1
                                   ELSE BD.ConsumptionFrom
                              END) AS 'M_Date',
                        MIN(CASE WHEN (b.PeriodFrom < @StartSystem OR BRP.PreviousSystem = 1)
                                      AND DATEDIFF(m, BD.ConsumptionFrom, BD.ConsumptionTo) > 0
                                 THEN BD.ConsumptionFrom + 1
                                 ELSE BD.ConsumptionFrom
                            END) AS 'MinConsumptionFrom',
                        MAX(CASE WHEN (b.PeriodFrom >= @StartSystem AND isnull(BRP.PreviousSystem,0) = 0)
                                      AND DATEDIFF(m, BD.ConsumptionFrom, BD.ConsumptionTo) > 0
                                 THEN BD.ConsumptionTo - 1
                                 ELSE BD.ConsumptionTo
                            END) AS 'MaxConsumptionFrom', SUM(BD.Quantity) AS 'DiscountEcount',
                        DATEDIFF(DAY,
                                 MIN(CASE WHEN (b.PeriodFrom < @StartSystem OR BRP.PreviousSystem = 1)
                                               AND DATEDIFF(m, BD.ConsumptionFrom, BD.ConsumptionTo) > 0
                                          THEN BD.ConsumptionFrom + 1
                                          ELSE BD.ConsumptionFrom
                                     END),
                                 MAX(CASE WHEN (b.PeriodFrom >= @StartSystem AND isnull(BRP.PreviousSystem,0) = 0)
                                               AND DATEDIFF(m, BD.ConsumptionFrom, BD.ConsumptionTo) > 0
                                          THEN BD.ConsumptionTo - 1
                                          ELSE BD.ConsumptionTo
                                     END)) AS 'Days', SUM(BD.DiscountSumm) AS [DiscountSumm], tm.BeneficiaryQuantity
               FROM     FinanceMain.[Operation] B WITH ( NOLOCK )
                        JOIN FinanceMain.OperationRow BD WITH ( NOLOCK ) ON B.OperationId = BD.OperationId
                        JOIN [Dictionary].Tariff AS T ON BD.TariffId = t.TariffId
                        JOIN AccountingDictionary.BenefitsCategory BC ON BD.BenefitsCategoryId = BC.BenefitsCategoryId
                        JOIN @BenefitsCategory bcat ON bcat.BenefitsCategoryId = bc.BenefitsCategoryId
                        JOIN AccountingCommon.UsageObject AS UO WITH ( NOLOCK ) ON B.AccountId = UO.AccountId
                        JOIN AccountingCommon.Point AS P WITH ( NOLOCK ) ON UO.UsageObjectId = P.UsageObjectId
                        LEFT JOIN AccountingCommon.TarifficationMethod AS tm WITH ( NOLOCK ) ON P.PointId = tm.PointId
                                                                                                AND BD.ConsumptionFrom BETWEEN TM.DateFrom AND DATEADD(day,
                                                                                                        -1, TM.DateTo)
                        LEFT JOIN AccountingDictionary.HeatingSeason AS hs ON bd.ConsumptionFrom >= hs.DateFrom
                                                                              AND bd.ConsumptionFrom < hs.DateTo
                        LEFT JOIN AccountingCommon.TarifficationMethodItem AS tmi WITH ( NOLOCK ) ON tm.TarifficationMethodId = tmi.TarifficationMethodId
                                                                                                     AND BC.BenefitsCategoryId = tmi.BenefitsCategoryId
                                                                                                     AND tmi.Discount > 0
                                                                                                     AND tmi.IsForHeatingSeason = CASE
                                                                                                        WHEN hs.HeatingSeasonId IS NULL
                                                                                                        THEN 0
                                                                                                        ELSE 1
                                                                                                        END
LEFT JOIN FinanceCommon.BillRegularParams BRP  ( NOLOCK ) ON  BRP.BillId = B.OperationId  --140602 O.Chekh
                        JOIN benefits bn WITH ( NOLOCK ) ON bn.AccountId = b.AccountId
                                                            AND bn.BenefitsCategoryId = bd.BenefitsCategoryId
                                                            AND BD.TariffPrice * ( 1 + BD.VATRate / 100 ) = bn.Tariff /*20120905 AND bn.TariffId = bd.TariffId*/
                                                            AND bn.TimeZoneId = T.TimeZoneId
                                                            AND bn.discount = bd.discount
                                                            AND YEAR(bn.ConsumptionFrom) = YEAR(CASE WHEN (b.PeriodFrom < @StartSystem OR BRP.PreviousSystem = 1)
                                                                                                        AND DATEDIFF(m,
                                                                                                        BD.ConsumptionFrom,
                                                                                                        BD.ConsumptionTo) > 0
                                                                                                     THEN BD.ConsumptionFrom
                                                                                                        + 1
                                                                                                     ELSE BD.ConsumptionFrom
                                                                                                END)
                                                            AND MONTH(bn.ConsumptionFrom) = MONTH(CASE WHEN (b.PeriodFrom < @StartSystem OR BRP.PreviousSystem = 1)
                                                                                                        AND DATEDIFF(m,
                                                                                                        BD.ConsumptionFrom,
                                                                                                        BD.ConsumptionTo) > 0
                                                                                                       THEN BD.ConsumptionFrom
                                                                                                        + 1
                                                                                                       ELSE BD.ConsumptionFrom
                                                                                                  END)
                                                            AND bn.BenefitsCertificateId = bd.BenefitsCertificateId
                                                            AND bn.IsForHeatingSeason = ISNULL(tmi.IsForHeatingSeason, 0)
                                                            AND tm.BeneficiaryQuantity = bn.BeneficiaryQuantity
               WHERE    ( B.PeriodTo = 207906
                          OR B.PeriodTo > @period
                        )
                        AND ( b.PeriodFrom < @period
                              OR b.PeriodFrom = @period
                            )
                          /*чи потрібно враховувати корекцію періоду нарахованого в минулих звітних місяцях*/
                        AND ( ( b.IsIncome = 0
                                AND B.DocumentTypeId IN ( 1, 15 )
                              )
                              OR B.DocumentTypeId IN ( 9 )
                            )--B.IsIncome = 0
                        AND ( BD.Discount > 0
                              OR ( BC.Discount = 0
                                   AND BC.IsBenefits = 1
                                 )
                            )
               GROUP BY B.AccountId, BD.BenefitsCertificateId, BD.TariffPrice * ( 1 + BD.VATRate / 100 ), -- 20120905
                        --BD.TariffId , --20120905
                        T.TimeZoneId, ISNULL(tmi.IsForHeatingSeason, 0), BD.BenefitsCategoryId, BD.discount,
                        tm.BeneficiaryQuantity,
                        YEAR(CASE WHEN (b.PeriodFrom < @StartSystem OR BRP.PreviousSystem = 1)
                                       AND DATEDIFF(m, BD.ConsumptionFrom, BD.ConsumptionTo) > 0
                                  THEN BD.ConsumptionFrom + 1
                                  ELSE BD.ConsumptionFrom
                             END),
                        MONTH(CASE WHEN (b.PeriodFrom < @StartSystem OR BRP.PreviousSystem = 1)
                                        AND DATEDIFF(m, BD.ConsumptionFrom, BD.ConsumptionTo) > 0
                                   THEN BD.ConsumptionFrom + 1
                                   ELSE BD.ConsumptionFrom
                              END))
                              
    SELECT  @CDPR AS CDPR, 
    CASE 
    WHEN LTRIM(RTRIM(ISNULL(BP.IdentificationCode, '')))=''
    THEN BP.PassportSeries+' '+bp.PassportNumber
    ELSE ISNULL(BP.IdentificationCode, '')
    end    
     AS idcode,
            BP.LastName + ' ' + ISNULL(BP.FirstName, '') + ' ' + ISNULL(BP.SecondName, '') AS FIO,
            ISNULL(LTRIM(RTRIM(BC.BCSeries)), '') + ISNULL(LTRIM(RTRIM(BC.BCNumber)), '') AS PPOS,
            LTRIM(RTRIM(A.AccountNumber)) AS RS
			, LEFT(@period, 4) AS YEARIN, RIGHT(@period, 2) AS MONTHIN,
            CASE WHEN bn.TimeZoneId IN ( 3, 6 ) THEN 514
                 WHEN bn.TimeZoneId IN ( 2, 4 ) THEN 524
                 WHEN bn.TimeZoneId = 5 THEN 534
                 WHEN pc.MinistryCode = 40
                      OR PC.MinistryCode = 41
                      OR PC.MinistryCode = 42 -- 2017.03.03 Лемешко П.М. додано категорію для виду пільг "Освітлення" (Спеціаліст з захисту рослин)
                      OR PC.MinistryCode = 48 -- 2017.03.03 Лемешко П.М. додано категорію для виду пільг "Освітлення" (Працівники культури (пенсіонери))
                      OR pc.MinistryCode = 43 THEN 508
                 ELSE 504
            END AS LGCODE, BN.ConsumptionFrom AS DATA1, BN.ConsumptionTo AS DATA2, BN.BeneficiaryQuantity AS LGKOL,
            PC.MinistryCode AS LGKAT,
            CASE WHEN @IsMultiBudjet=1 THEN CASE WHEN BN.BenefitsCategoryId IN ( 77, 79, 80, 82, 84 ) THEN ABS(CASE WHEN @Pc=1 THEN BN.discount
                                                                                                                    ELSE 0
                                                                                                               END)-25
                                                 WHEN BN.BenefitsCategoryId IN ( 78, 83 ) THEN ABS(CASE WHEN @Pc=1 THEN BN.discount
                                                                                                        ELSE 0
                                                                                                   END)-50
                                                 WHEN BN.BenefitsCategoryId=81 THEN ABS(CASE WHEN @Pc=1 THEN BN.discount
                                                                                             ELSE 0
                                                                                        END)-15
                                                 ELSE BN.discount
                                            END
                 ELSE BN.discount
            END AS LGPRC,
            CASE WHEN @IsMultiBudjet=1
                 THEN CASE WHEN BN.BenefitsCategoryId IN ( 77, 79, 80, 82, 84 )
                           THEN ( CASE WHEN @pc=1 THEN 0
                                       ELSE BN.summ
                                  END-ROUND(BN.summ*( BN.discount-25 )/BN.discount, 2) )*CASE WHEN @pc=1 THEN -1
                                                                                              ELSE 1
                                                                                         END
                           WHEN BN.BenefitsCategoryId IN ( 78, 83 ) THEN ( CASE WHEN @pc=1 THEN 0
                                                                                ELSE BN.summ
                                                                           END-ROUND(BN.summ*( BN.discount-50 )/BN.discount, 2) )*CASE WHEN @pc=1 THEN -1
                                                                                                                                       ELSE 1
                                                                                                                                  END
                           WHEN BN.BenefitsCategoryId=81 THEN ( CASE WHEN @pc=1 THEN 0
                                                                     ELSE BN.summ
                                                                END-ROUND(BN.summ*( BN.discount-15 )/BN.discount, 2) )*CASE WHEN @pc=1 THEN -1
                                                                                                                            ELSE 1
                                                                                                                       END
                           ELSE BN.summ
                      END
                 ELSE BN.summ
            END AS SUMM, CAST(ISNULL(PB.DiscountEcount, 0) AS INT) AS FACT, BN.tariff AS tarif,
            CASE WHEN BN.Quantity != ISNULL(PB.DiscountEcount, 0)
           OR --131030 A.Chekh
           EXISTS (SELECT TOP 1 RR.OperationRowId 
           FROM FinanceMain.OperationRow RR WITH ( NOLOCK ) JOIN FinanceMain.Operation O WITH ( NOLOCK ) ON RR.OperationId = O.OperationId
           JOIN FinanceCommon.BillRegularParams BRP ON O.OperationId = BRP.BillId
           WHERE O.AccountId = A.AccountId AND O.PeriodFrom < @Period AND RR.BenefitsCategoryId = BN.BenefitsCategoryId AND RR.DiscountSumm > 0
           AND            CASE WHEN (O.PeriodFrom < @StartSystem OR BRP.PreviousSystem = 1) THEN   YEAR(RR.ConsumptionTo)*100 + MONTH(RR.ConsumptionTo)
        ELSE   YEAR(DATEADD(hh, - 1, RR.ConsumptionTo))*100 + MONTH(DATEADD(hh, - 1, RR.ConsumptionTo))        END
         --140219 O.Chekh
           = YEAR(BN.ConsumptionTo) *100 + MONTH(BN.ConsumptionTo)
           )
            THEN 1
            ELSE 0
            END AS Flag,
            bn.isBlock
			, adc.Name AS NasPunkt
			--, ADS.Name
        into #Benefits
		FROM AccountingCommon.Account A WITH ( NOLOCK )
			JOIN AccountingCommon.UsageObject AS UO WITH ( NOLOCK ) ON A.AccountId = UO.AccountId
            JOIN AccountingCommon.Address Adr WITH ( NOLOCK ) ON UO.AddressId = Adr.AddressId
			LEFT JOIN AddressDictionary.City adc ON adc.CityId = Adr.CityId
            JOIN Benefits BN WITH ( NOLOCK ) ON BN.AccountId = A.AccountId
            JOIN AccountingDictionary.BenefitsCategory PC WITH ( NOLOCK ) ON BN.BenefitsCategoryId = PC.BenefitsCategoryId
            JOIN @BenefitsCategory bcat ON bcat.BenefitsCategoryId = pc.BenefitsCategoryId
            LEFT JOIN AccountingCommon.BenefitsCertificate BC WITH ( NOLOCK ) ON BN.BenefitsCertificateId = BC.BenefitsCertificateId
            LEFT JOIN AccountingCommon.PhysicalPerson BP WITH ( NOLOCK ) ON BC.PhysicalPersonId = BP.PhysicalPersonId
            LEFT JOIN prevBenefits pb WITH ( NOLOCK ) ON bn.AccountId = pb.AccountId
                                                         AND bn.BenefitsCategoryId = pb.BenefitsCategoryId
                                                         AND bn.Tariff = pb.Tariff /*20120905 bn.TariffId = pb.TariffId*/
                                                         AND bn.TimeZoneId = pb.TimeZoneId
                                                         AND bn.discount = pb.discount
                                                         AND YEAR(bn.ConsumptionFrom) = pb.YearConsumptionFrom
                                                         AND MONTH(bn.ConsumptionFrom) = pb.MonthConsumptionFrom
                                                         AND bn.BenefitsCertificateId = pb.BenefitsCertificateId
                                                         AND bn.IsForHeatingSeason = pb.IsForHeatingSeason
                                                         AND pb.BeneficiaryQuantity = bn.BeneficiaryQuantity
    ORDER BY A.AccountNumber
    
--SELECT * FROM #Benefits 
--WHERE rs = 101020
    
/* перевірка

SELECT * FROM #Benefits 
COMPUTE SUM(summ)
    SELECT *,CONVERT(DECIMAL(10,2),tarif*fact*LGPRC/100.00) FROM #Benefits
WHERE ABS(CONVERT(DECIMAL(10,2),tarif*fact*LGPRC/100.00)-summ)>0.05
AND flag=0
COMPUTE SUM(summ)
*/    
/*Структура згідно
http://zakon2.rada.gov.ua/laws/show/z1172-07
 */
CREATE TABLE #dbf2 (CDPR NUMERIC(12) --Код за ЄДРПОУ організації
,IDCODE CHARACTER (10)				 --131007A.Chekh: --Ідентифікаційний   номер   пільговика
,FIO CHARACTER   (50)				 -- П.І.Б.  пільговика - поле FIO
, PPOS CHARACTER (15)				 -- Серія   та   номер  пільгового  посвідчення  -  поле  PPO
, RS CHARACTER (25)					 --Особовий рахунок 
, YEARIN NUMERIC (4)				 --рік в якому завантажується пільга
, MONTHIN NUMERIC (2)				 --місяць в якому завантажується пільга
, LGCODE NUMERIC (4)				 -- Код  пільги  -  поле  LGCODE.  Заповнюється з використанням довідника ЄДАРП "Види послуг".
, DATA1   DATETIME					 --131007A.Chekh:
, DATA2   DATETIME					 --131007A.Chekh:
, LGKOL   NUMERIC (2)				 --к-ть осіб, що отримують пільгу
, LGKAT   NUMERIC (3)				 --код категорії пільговика
, LGPRC   NUMERIC (3)				 --розмір пільги у відсотках
, SUMM     NUMERIC (8,2)			 --сума нарахованої пільги
, FACT     NUMERIC (19,6)			 --пільговий обсяг фактичного споживання
 ,TARIF    NUMERIC (14,7)			 --тариф за одиницю послуги з ПДВ  ----!!!!!
, FLAG     NUMERIC (1)				 --ознака для перерахунків
,isBlock NUMERIC(1)					 --заблокована вже пільга
, NasPunkt CHAR(50)					 --населений пункт пільговика
)
IF @withGroup =   1
BEGIN
INSERT INTO #dbf2
        ( CDPR, IDCODE, FIO, PPOS, RS, YEARIN, MONTHIN, LGCODE, DATA1, DATA2, LGKOL, LGKAT, LGPRC, SUMM, FACT, TARIF, FLAG , isBlock, NasPunkt )
SELECT  CDPR, IDCODE, FIO, PPOS, RS, YEARIN, MONTHIN, LGCODE, MAX(DATA1), MAX(DATA2), MAX(LGKOL), LGKAT, LGPRC, SUM(SUMM), SUM(FACT)
                  , TARIF, FLAG ,isBlock, NasPunkt
FROM #Benefits
GROUP BY CDPR, IDCODE, FIO, PPOS, RS, YEARIN, MONTHIN, LGCODE, YEAR(DATA1)*100 + MONTH(DATA1), YEAR(DATA2) *100 + MONTH (DATA2)
		, LGKAT, LGPRC, TARIF, FLAG, isBlock, NasPunkt

--------видалення блочних записів 
DECLARE @x TABLE 
(
cdpr	INT,
idcode	char(10),
fio	char(50),
ppos	char(15),
rs	char(25),
yearin	INT,
monthin	INT,
lgcode	INT,
data1	DATETIME,
data2	DATETIME,
lgkol	INT,
lgkat	char(3),
lgprc	INT,
summ	DECIMAL(10,2),
fact	DECIMAL(10,2),
tarif	DECIMAL(14,7),
flag	INT,
isblock INT,
NasPunkt CHAR(50),
id INT) 

INSERT @x 
SELECT cdpr,idcode,fio,ppos,rs,yearin,monthin,lgcode,
		data1,data2,lgkol,lgkat,lgprc,
		summ,fact,tarif,flag,isblock, NasPunkt,
		ROW_NUMBER() OVER (PARTITION BY lgcode,idcode,rs,MONTH(data1),YEAR(data1),flag ORDER BY lgcode,isblock,tarif) id
FROM #dbf2 z
WHERE EXISTS (
				SELECT * FROM #dbf2 zz
				WHERE z.IDCODE = zz.IDCODE
				AND z.rs = zz.rs 
				AND zz.isBlock>0
				and z.lgcode=zz.lgcode
				AND MONTH(zz.data1) = MONTH(z.data1)
				AND YEAR(zz.data1) = YEAR(z.data1)
			)

UPDATE x SET x.fact = x.fact+z.fact+ISNULL(z3.fact,0)+ISNULL(z4.fact,0),x.summ = x.summ+z.summ+ISNULL(z3.summ,0)+ISNULL(z4.summ,0)
--SELECT x.*
FROM #dbf2 x 
JOIN @x y ON x.cdpr = y.cdpr 
AND x.idcode COLLATE Ukrainian_CI_AS= y.idcode COLLATE Ukrainian_CI_AS
AND x.rs COLLATE Ukrainian_CI_AS= y.rs COLLATE Ukrainian_CI_AS
AND x.data1 = y.data1
AND x.data2 = y.data2
AND x.summ = y.summ
AND x.fact = y.fact
AND x.flag=y.flag
and x.lgcode = y.lgcode
AND y.id = 1
JOIN @x z ON z.cdpr= x.cdpr
AND z.idcode COLLATE Ukrainian_CI_AS= x.idcode COLLATE Ukrainian_CI_AS
AND z.rs COLLATE Ukrainian_CI_AS= x.rs COLLATE Ukrainian_CI_AS
AND MONTH(x.data1) = MONTH(z.data1) 
AND YEAR(x.data1) = YEAR(z.data1)
AND z.lgcode = x.lgcode
AND z.id =2
LEFT JOIN @x z3 ON z3.cdpr= x.cdpr
AND z3.idcode COLLATE Ukrainian_CI_AS= x.idcode COLLATE Ukrainian_CI_AS
AND z3.rs COLLATE Ukrainian_CI_AS= x.rs COLLATE Ukrainian_CI_AS
AND MONTH(x.data1) = MONTH(z3.data1) 
AND YEAR(x.data1) = YEAR(z3.data1)
AND z3.lgcode = x.lgcode
AND z3.id =3
LEFT JOIN @x z4 ON z4.cdpr= x.cdpr
AND z4.idcode COLLATE Ukrainian_CI_AS= x.idcode COLLATE Ukrainian_CI_AS
AND z4.rs COLLATE Ukrainian_CI_AS= x.rs COLLATE Ukrainian_CI_AS
AND MONTH(x.data1) = MONTH(z4.data1) 
AND YEAR(x.data1) = YEAR(z4.data1)
AND z4.id =4
and z4.lgcode = x.lgcode

DELETE FROM #dbf2 
WHERE EXISTS (SELECT * FROM @x
	WHERE cdpr = #dbf2.cdpr 
	AND idcode COLLATE Ukrainian_CI_AS= #dbf2.idcode COLLATE Ukrainian_CI_AS
	AND rs COLLATE Ukrainian_CI_AS= #dbf2.rs COLLATE Ukrainian_CI_AS
	AND summ = #dbf2.summ
	AND fact = #dbf2.fact 
	AND data1 = #dbf2.data1
	AND data2 = #dbf2.data2
	AND id IN (2,3,4)
    and flag = #dbf2.flag
)

SELECT * FROM #dbf2
ORDER BY RS  ASC
END
END


