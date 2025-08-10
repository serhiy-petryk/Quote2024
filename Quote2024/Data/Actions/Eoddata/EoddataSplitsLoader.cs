﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Data.Helpers;
using Data.Models;

namespace Data.Actions.Eoddata
{
    public class EoddataSplitsLoader
    {
        private const string URL = @"https://eoddata.com/splits.aspx";

        public static void Start()
        {
            Logger.AddMessage($"Started");

            var timeStamp = TimeHelper.GetTimeStamp();
            var zipFileName = Settings.DataFolder + $@"Splits\Eoddata\EoddataSplits_{timeStamp.Item2}.zip";

            // Download html data
            var o = WebClientExt.GetToBytes(URL, false, true);
            if (o.Item3 != null)
                throw new Exception($"EoddataSplitsLoader: Error while download from {URL}. Error message: {o.Item3.Message}");

            var htmlContent = System.Text.Encoding.UTF8.GetString(o.Item1);

            // Convert data to text format
            var now = DateTime.Now;
            var items = new Dictionary<(string, string, DateTime), SplitModel>();
            var fileLines = new List<string> { "Exchange\tSymbol\tDate\tRatio" };

            var i1 = htmlContent.IndexOf("<th>Ratio</th>", StringComparison.InvariantCulture);
            var i2 = htmlContent.IndexOf("</table>", i1 + 14, StringComparison.InvariantCulture);
            var rows = htmlContent.Substring(i1 + 14, i2 - i1 - 14).Trim().Split(new[] { "</tr>" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var row in rows)
            {
                if (string.Equals(row.Trim(), "</tbody>")) continue;

                var cells = row.Trim().Split(new[] { "</td>" }, StringSplitOptions.RemoveEmptyEntries);
                var exchange = GetCellValue(cells[0]);
                var symbol = GetCellValue(cells[1]);
                var sDate = GetCellValue(cells[2]);
                // var date = DateTime.ParseExact(sDate, "MM/dd/yyyy", CultureInfo.InvariantCulture);
                var date = DateTime.ParseExact(sDate, "dd MMM yyyy", CultureInfo.InvariantCulture); // Added at 2025-08-10
                var ratio = GetCellValue(cells[3]);
                fileLines.Add($"{exchange}\t{symbol}\t{sDate}\t{ratio}");
                ratio = ratio.Replace('-', ':');
                var item = new SplitModel(exchange, symbol, date, ratio, now);
                var key = (item.Exchange, item.Symbol, item.Date);
                if (!items.ContainsKey(key))
                    items.Add(key, item);
            }

            // Save data to zip file
            var entry = new VirtualFileEntry($"{Path.GetFileNameWithoutExtension(zipFileName)}.txt",
                System.Text.Encoding.UTF8.GetBytes(string.Join(Environment.NewLine, fileLines)));
            ZipUtils.ZipVirtualFileEntries(zipFileName, new[] { entry });

            // Save data to database
            var maxDate = timeStamp.Item1;
            DbHelper.ClearAndSaveToDbTable(items.Values.Where(a => a.Date <= maxDate), "dbQ2024..Bfr_SplitEoddata", "Exchange", "Symbol",
                "Date", "Ratio", "K", "TimeStamp");
            DbHelper.ExecuteSql("INSERT INTO dbQ2024..SplitEoddata (Exchange,Symbol,[Date],Ratio,K,[TimeStamp]) " +
                               "SELECT a.Exchange, a.Symbol, a.[Date], a.Ratio, a.K, a.[TimeStamp] FROM dbQ2024..Bfr_SplitEoddata a " +
                               "LEFT JOIN dbQ2024..SplitEoddata b ON a.Exchange=b.Exchange AND a.Symbol = b.Symbol AND a.Date = b.Date " +
                               "WHERE b.Symbol IS NULL");

            Logger.AddMessage($"!Finished. Items: {items.Count:N0}. Zip file size: {CsUtils.GetFileSizeInKB(zipFileName):N0}KB. Filename: {zipFileName}");
        }

        private static string GetCellValue(string cell)
        {
            var s = cell.Replace("</a>", "").Trim();
            var i1 = s.LastIndexOf('>');
            s = System.Net.WebUtility.HtmlDecode(s.Substring(i1 + 1)).Trim();
            return s;
        }
    }
}
