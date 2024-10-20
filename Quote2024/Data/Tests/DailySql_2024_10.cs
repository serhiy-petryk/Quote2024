using System;
using System.Diagnostics;
using System.Text;
using Microsoft.Data.SqlClient;

namespace Data.Tests
{
    public static class DailySql_2024_10
    {
        public static void Start()
        {
            var opens = new string[]
                { "O0935", "O0940", "O0945", "O0950", "O0955", "O1000", "O1005", "O1010", "O1015", "O1020", "O1025"};
            var closes = new string[]
                { "O1520", "O1525", "O1530", "O1535", "O1540", "O1545", "O1550", "O1555"};

            var sqlTemplate = ";with data as\n" +
                              "(\n" +
                              "select (a.[Open]-a.[Close])/a.[Open]*100 PrevOpenRate,\n" +
                              "(a.[Open]-a.[Close])/(a.[Open]+a.[Close])*200 PrevCOProfit,\n" +
                              "(a.[High]-a.[Low])/(a.High+a.Low)*200 PrevHLProfit,\n" +
                              "a.*, b.MyType, b.Sector from dbQ2024..DayPolygon a \n" +
                              "inner join dbQ2024..SymbolsPolygon b on a.Symbol=b.Symbol and a.Date between b.Date and isnull(b.[To],'2099-12-31')\n" +
                              "where a.IsTest is null and year(a.Date) in (2022,2023) and\n" +
                              "a.Volume*a.[Close]>=50000000 and a.TradeCount>=10000 --and a.Low>=5.0\n" +
                              "-- !!?? and a.[Close] between a.Low*1.05 and a.High*0.95  -- !!?? is worse\n" +
                              "),\n" +
                              "data2 as (\n" +
                              "select (OpenIn-CloseIn)/OpenIn*100 Profit, (OpenIn-CloseIn) ProfitValue,\n" +
                              "iif (HighIn-OpenIn<0.01,OpenIn-CloseIn,-0.01)/OpenIn*100 Amt1,\n" +
                              "iif (HighIn-OpenIn<0.02,OpenIn-CloseIn,-0.02)/OpenIn*100 Amt2,\n" +
                              "iif (HighIn-OpenIn<0.05,OpenIn-CloseIn,-0.05)/OpenIn*100 Amt5,\n" +
                              "iif (HighIn-OpenIn<OpenIn*0.01,OpenIn-CloseIn,-OpenIn*0.01)/OpenIn*100 Amt1P,\n" +
                              "iif (HighIn-OpenIn<OpenIn*0.02,OpenIn-CloseIn,-OpenIn*0.02)/OpenIn*100 Amt2P,\n" +
                              "iif (HighIn-OpenIn<OpenIn*0.05,OpenIn-CloseIn,-OpenIn*0.05)/OpenIn*100 Amt5P,\n" +
                              "iif (HighIn-OpenIn<0.01,1,0) Cnt1,\n" +
                              "iif (HighIn-OpenIn<0.02,1,0) Cnt2,\n" +
                              "iif (HighIn-OpenIn<0.05,1,0) Cnt5,\n" +
                              "iif (HighIn-OpenIn<OpenIn*0.01,1,0) Cnt1P,\n" +
                              "iif (HighIn-OpenIn<OpenIn*0.02,1,0) Cnt2P,\n" +
                              "iif (HighIn-OpenIn<OpenIn*0.05,1,0) Cnt5P,\n" +
                              "a.* from data a\n" +
                              "inner join dbQ2024..TradingDays b on a.Date=b.Prev1\n" +
                              "inner join (select *\n" +
                              "from (select *, {0} OpenIn, {3} HighIn, {1} CloseIn\n" +
                              "from dbQ2024MinuteScanner..DailyBy5Minutes WHERE H1025 is not null and O0930 is not null {2}) x\n" +
                              "where OpenIn>=5.0 and CloseIn is not null) c on a.Symbol=c.Symbol and a.Date=c.PrevDate\n" +
                              "--and c.[Open] between a.Low and a.High -- is worse\n" +
                              "--and (c.[Open] <= a.Low or c.[Open]>=a.High) -- is worse\n" +
                              "where b.IsShortened is null\n" +
                              "),\n" +
                              "data3 as (\n" +
                              "       SELECT RN = ROW_NUMBER() OVER (PARTITION BY Date ORDER BY PrevHLProfit DESC), *\n" +
                              "   FROM data2\n" +
                              ")\n" +
                              "\n" +
                              "select cast(ROUND(avg(Profit),3) as real) Profit, count(*) Recs, ROUND(avg(PrevHLProfit),2) PrevHLProfit,\n" +
                              "ROUND(sum(ProfitValue),0) ProfitValue,\n" +
                              "ROUND(avg(Amt1),3) Amt1,\n" +
                              "ROUND(avg(Amt2),3) Amt2,\n" +
                              "ROUND(avg(Amt5),3) Amt5,\n" +
                              "ROUND(avg(Amt1P),3) Amt1P,\n" +
                              "ROUND(avg(Amt2P),3) Amt2P,\n" +
                              "ROUND(avg(Amt5P),3) Amt5P,\n" +
                              "cast(ROUND(100.0*sum(Cnt1)/count(*),1) as real) Cnt1,\n" +
                              "cast(ROUND(100.0*sum(Cnt2)/count(*),1) as real) Cnt2,\n" +
                              "cast(ROUND(100.0*sum(Cnt5)/count(*),1) as real) Cnt5,\n" +
                              "cast(ROUND(100.0*sum(Cnt1P)/count(*),1) as real) Cnt1P,\n" +
                              "cast(ROUND(100.0*sum(Cnt2P)/count(*),1) as real) Cnt2P,\n" +
                              "cast(ROUND(100.0*sum(Cnt5P)/count(*),1) as real) Cnt5P\n" +
                              "from data3\n" +
                              "    WHERE RN<=5 and PrevHLProfit>15";

            using (var conn = new SqlConnection(Settings.DbConnectionString))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandTimeout = 300;
                for (var k2 = 0; k2 < closes.Length; k2++)
                for (var k1 = 0; k1 < opens.Length; k1++)
                {
                    var sb1 = new StringBuilder();
                    var sb2 = new StringBuilder();
                    for (var k21 = k2; k21 < closes.Length; k21++)
                    {
                        sb1.Append("ISNULL(" + closes[k21]+", ");
                        sb2.Append(")");
                    }

                    var closeInString = sb1.ToString() + "O1600" + sb2.ToString();

                    var openIn = opens[k1];
                    var highIn = openIn.Replace("O", "HG");

                    sb1.Clear();
                    for (var k11 = 0; k11 <= k1; k11++)
                    {
                        sb1.Append($"and {opens[k11]} is not null ");
                    }
                    var s2 = sb1.ToString();

                    var sql = string.Format(sqlTemplate, openIn, closeInString, s2, highIn);

                    cmd.CommandText = sql;
                    using (var rdr = cmd.ExecuteReader())
                        while (rdr.Read())
                        {
                            Debug.Print(
                                $"{opens[k1]}-{closes[k2]}\t{rdr["Profit"]}\t{rdr["Recs"]}\t{rdr["PrevHLProfit"]}\t{rdr["ProfitValue"]}\t{rdr["Amt1"]}\t{rdr["Amt2"]}\t{rdr["Amt5"]}\t{rdr["Amt1P"]}\t{rdr["Amt2P"]}\t{rdr["Amt5P"]}\t{rdr["Cnt1"]}\t{rdr["Cnt2"]}\t{rdr["Cnt5"]}\t{rdr["Cnt1P"]}\t{rdr["Cnt2P"]}\t{rdr["Cnt5P"]}");
                        }
                }
            }

        }
    }
}
