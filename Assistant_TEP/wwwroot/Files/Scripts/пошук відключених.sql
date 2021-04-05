SELECT	*
FROM (
		SELECT	
		a.AccountNumber AS [Особовий]
		--,a.AccountId
		,pp.FullName AS [ПІП]
		,addr.FullAddress AS [Адреса]
		--,d.[DisconnectionId]
        ,dp.[Name] AS [Де відключений]
        ,d.[DisconnectionStatus] AS [Статус викл]
        ,CONVERT (VARCHAR, d.[DateFrom], 104) AS [Дата викл]
        ,d.[Note]
		,ISNULL((SELECT SUM(Usage)
				FROM Measuring.UsageCache
				WHERE PointId = p.PointId
				AND DateFrom>=d.DateFrom
			),0) AS [кВт]
		,CAST((CASE WHEN d.DateFrom>= '2019-01-01' THEN 
				(SELECT DebetEnd FROM FinanceCommon.SupplierSaldo 
					WHERE Period = 
					(CAST(
						CAST(YEAR(d.DateFrom) AS CHAR(4)) + 
						(CASE 
							WHEN MONTH(d.DateFrom) > 9 
							THEN CAST(MONTH(d.DateFrom) AS CHAR(2)) 
							ELSE '0' + CAST(MONTH(d.DateFrom) AS CHAR(2))
						END)
					AS CHAR(6)))
					AND AccountId = a.AccountId
				)
			WHEN d.DateFrom BETWEEN '2010-11-01' AND '2018-12-01' 
				THEN (
					SELECT DebetEnd FROM FinanceCommon.Saldo 
						WHERE Period = 
						(CAST(
							CAST(YEAR(d.DateFrom) AS CHAR(4)) + 
							(CASE 
								WHEN MONTH(d.DateFrom) > 9 
								THEN CAST(MONTH(d.DateFrom) AS CHAR(2)) 
								ELSE '0' + CAST(MONTH(d.DateFrom) AS CHAR(2))
							END)
						AS CHAR(6)))
						AND AccountId = a.AccountId
					)
			ELSE NULL
			END
		) AS CHAR(100)) [Дт викл]
		,ss.DebetEnd AS [Дебет]
FROM AccountingCommon.Account a
JOIN AccountingCommon.PhysicalPerson pp ON pp.PhysicalPersonId = a.PhysicalPersonId
JOIN AccountingCommon.Address addr ON addr.AddressId = a.AddressId
JOIN AccountingCommon.UsageObject uo ON uo.AccountId = a.AccountId
JOIN AccountingCommon.Point p ON p.UsageObjectId = uo.UsageObjectId
JOIN FinanceCommon.SupplierSaldo ss ON ss.AccountId = a.AccountId 
AND ss.Period = @period
JOIN AccountingCommon.Disconnection d ON p.PointId = d.PointId AND d.DisconnectionStatus=1
JOIN (SELECT
             d2.[DisconnectionId],
			ROW_NUMBER() OVER (PARTITION BY d2.PointId ORDER BY d2.DateFrom DESC) AS RowNumber
      FROM [AccountingCommon].[Disconnection] as d2)
             AS dtemp ON dtemp.[DisconnectionId] = d.[DisconnectionId] 
			AND dtemp.RowNumber = 1
LEFT JOIN [AccountingDictionary].[DisconnectionPlace] dp ON dp.DisconnectionPlaceId = d.DisconnectionPlaceId
		WHERE a.DateTo = '2079-06-06' -- тільки незакриті ОР
	) x
--WHERE kwt>0 
WHERE CONVERT(DATETIME, x.[Дата викл], 103) <= CONVERT (DATETIME, '2020-03-01', 103) 
ORDER BY x.[Дата викл], x.Особовий
