using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Data.Helpers;
using Microsoft.Data.SqlClient;

namespace Data.Actions.Yahoo
{
    public static class WA_YahooProfile
    {
        // private const string ListUrlTemplate = "https://web.archive.org/cdx/search/cdx?url=https://finance.yahoo.com/quote/{0}&matchType=prefix&limit=100000&from=2024";
        private const string ListUrlTemplate = "https://web.archive.org/cdx/search/cdx?url=https://finance.yahoo.com/quote/{0}/profile&matchType=prefix&limit=100000&from=2020";
        private const string ListDataFolder = @"E:\Quote\WebData\Symbols\Yahoo\Profile\WA_List";
        private const string HtmlDataFolder = @"E:\Quote\WebData\Symbols\Yahoo\Profile\WA_Data";

        public static void ParseAndSaveToDb()
        {
            Logger.AddMessage($"Started");
            var data = new List<(string, string, string, DateTime)>();
            var folder = @"E:\Quote\WebData\Symbols\Yahoo\Profile\Data";
            var zipFiles = Directory.GetFiles(folder, "*.zip");
            foreach (var zipFileName in zipFiles)
            {
                Logger.AddMessage($"Parse data from {Path.GetFileName(zipFileName)}");
                ParseZip(zipFileName);
            }

            var wa_zipFileName = @"E:\Quote\WebData\Symbols\Yahoo\WA_Profile\WA_Data.Short.zip";
            Logger.AddMessage($"Parse data from {Path.GetFileName(wa_zipFileName)}");
            ParseZip(wa_zipFileName);

            Logger.AddMessage($"Save data to database");
            var symbolXref = GetSymbolXref();
            var dbData = new List<DbItem>();
            DbItem lastDbItem = null;
            foreach (var item in data.OrderBy(a => a.Item1).ThenBy(a => a.Item4))
            {
                var symbol = item.Item1;
                var name = item.Item2;
                var sector = item.Item3;
                if (string.IsNullOrEmpty(sector))
                    throw new Exception("Check");
                if (lastDbItem == null || lastDbItem.YahooSymbol != symbol || lastDbItem.Name != name ||
                    lastDbItem.Sector != sector)
                {
                    if (lastDbItem != null)
                        dbData.Add(lastDbItem);
                    lastDbItem = new DbItem()
                        { PolygonSymbol = symbolXref[symbol], YahooSymbol = symbol, Name = name, Sector = sector, StartDate = item.Item4, EndDate = item.Item4 };
                }
                else
                    lastDbItem.EndDate = item.Item4;
            }
            dbData.Add(lastDbItem);
            Logger.AddMessage($"Finished");

            void ParseZip(string zipFileName)
            {
                using (var zip = ZipFile.Open(zipFileName, ZipArchiveMode.Read))
                    foreach (var entry in zip.Entries.Where(a => a.Length > 0))
                    {
                        if (entry.Name.EndsWith("ACT_20230406035606.html") || entry.Name.EndsWith("AXP_20230410144059.html") ||
                            entry.Name.EndsWith("OXM_20230429222332.html") || entry.Name.EndsWith("SONO_20230422222307.html") ||
                            entry.Name.EndsWith("WAB_20230417233217.html") || entry.Name.EndsWith("ILMNV_20240624224841.html"))
                        { // Bad files
                            continue;
                        }

                        var ss = Path.GetFileNameWithoutExtension(entry.Name).Split('_');
                        var dateKey = DateTime.ParseExact(ss[ss.Length - 1],
                            ss[ss.Length - 1].Length == 14 ? "yyyyMMddHHmmss" : "yyyyMMdd",
                            CultureInfo.InvariantCulture);
                        var content = entry.GetContentOfZipEntry();
                        var temp = ParseHtmlContent(content);
                        if (temp.Item1 == "BI")
                        {

                        }
                        if (!string.IsNullOrEmpty(temp.Item3))
                            data.Add((temp.Item1, temp.Item2, temp.Item3, dateKey));
                    }
            }
        }

        public static (string,string,string) ParseHtmlContent(string content)
        {
                var i1 = content.IndexOf(">Fund Overview<", StringComparison.InvariantCulture);
                if (i1 > 1)
                {
                    return (null, null, null);
                }

                // 2020 year format
                i1 = content.IndexOf("<h1 class=\"D(ib) Fz(1", StringComparison.InvariantCulture);
                if (i1 > -1) // Count, from, to: 20652, 2020-01-01, 2024-06-27
                { // AA_20200512043535.html
                    var k2 = content.IndexOf("<h1 class=\"D(ib) Fz(1", i1 + 5, StringComparison.InvariantCulture);
                    if (k2 != -1)
                        throw new Exception("Check YahooProfile parser");

                    var i2 = content.IndexOf(">Sector<", i1, StringComparison.InvariantCulture);
                    if (i2 == -1) i2 = content.IndexOf(">Sector(s)<", i1, StringComparison.InvariantCulture);
                    if (i2 == -1 && CheckSector2020())
                        throw new Exception("Check YahooProfile parser");

                    var symbolAndName = GetSymbolAndName2020();

                    var sector = GetSector2020(i2);
                    return ApplyCorrections(symbolAndName[0], symbolAndName[1], sector);
                }

                // 2024 year format
                i1 = content.IndexOf("<h1 class=\"svelte-", StringComparison.InvariantCulture);
                if (i1 > -1) // Count, from, to: 1328, 2023-11-12, 2024-07-05
                { // AAON_20240503140454.html
                    var k2 = content.IndexOf("<h1 class=\"svelte-", i1 + 5, StringComparison.InvariantCulture);
                    if (k2 != -1)
                        throw new Exception("Check YahooProfile parser");

                    var i2 = content.IndexOf(">Sector: </dt>", i1, StringComparison.InvariantCulture);
                    if (i2 == -1 && CheckSector2024())
                        throw new Exception("Check YahooProfile parser");

                    var symbolAndName = GetSymbolAndName2023();
                    var sector = GetSector2023(i2);
                    return ApplyCorrections(symbolAndName[0], symbolAndName[1], sector);
                }

                i1 = content.IndexOf(">Symbols similar to ", StringComparison.InvariantCulture);
                if (i1 > -1) // Not found
                    return (null, null, null);

                throw new Exception("Check YahooProfile parser");

                bool CheckSector2020()
                {
                    var content1 = content.Replace("https://finance.yahoo.com/sectors\" title=\"Sectors\">Sectors", "\">");
                    content1 = content1.Replace("sector information, ", "");
                    content1 = content1.Replace("finance.yahoo.com/sector/", "x");

                    /*// Below for YP_20230304.1/YP_HUBCZ_20230304.html (full version)
                    var a1 = content1.Length;
                    content1 = content1.Replace("SectorAllocation", "", StringComparison.InvariantCultureIgnoreCase);
                    var a2 = content1.Length;
                    content1 = content1.Replace("morningstarSector", "");
                    var a3 = content1.Length;
                    content1 = content1.Replace("enableSectorIndustryLabelFix", "");
                    var a4 = content1.Length;
                    content1 = content1.Replace("SectorVisitorTrend", "", StringComparison.InvariantCultureIgnoreCase);
                    var a5 = content1.Length;
                    content1 = content1.Replace("sectorsList", "");
                    var a6 = content1.Length;

                    // Below for YP_20230304/YP_CHKEL_20230304.html (full version)
                    content1 = content1.Replace("ZiP8secTORUqEL", "");
                    var a7 = content1.Length;*/
                    if (content1.IndexOf("sector", StringComparison.InvariantCultureIgnoreCase) == -1)
                        return false;

                    return true;
                }

                bool CheckSector2024()
                {
                    var content1 = content;
                    if (content1.IndexOf("sector", StringComparison.InvariantCultureIgnoreCase) == -1)
                        return false;
                    if (content1.IndexOf(">Profile Information Not Available<",
                            StringComparison.InvariantCulture) != -1)
                    { // CNH_20240520210256.html
                        return false;
                    }
                    if (content1.IndexOf(">Profile data is currently not available for ", StringComparison.InvariantCulture) != -1)
                    { // ENERW_20240422165327.html
                        return false;
                    }

                    return true;
                }

                string[] GetSymbolAndName2020()
                {
                    var i2 = content.IndexOf(">", i1 + 5, StringComparison.InvariantCulture);
                    var i3 = content.IndexOf("</h1", i1 + 5, StringComparison.InvariantCulture);
                    if (i2 == -1 || i3 < i2)
                        throw new Exception("Check YahooProfile parser (GetSymbolAndName2020 method)");

                    var s1 = content.Substring(i2 + 1, i3 - i2 - 1);
                    s1 = System.Net.WebUtility.HtmlDecode(s1).Trim();
                    i2 = s1.LastIndexOf('(');
                    if (s1.EndsWith(')') && i2 >= 0)
                    {
                        var name = s1.Substring(0, i2).Trim();
                        var symbol = s1.Substring(i2 + 1, s1.Length - i2 - 2).Trim();
                        return new[] { symbol, name };
                    }

                    i2 = s1.IndexOf('-');
                    if (i2 >= 0)
                    {
                        var symbol = s1.Substring(0, i2 - 1).Trim();
                        var name = s1.Substring(i2 + 1).Trim();
                        return new[] { symbol, name };
                    }

                    throw new Exception("Check YahooProfile parser (GetSymbolAndName2020 method)");
                }

                string[] GetSymbolAndName2023()
                {
                    var i2 = content.IndexOf(">", i1 + 5, StringComparison.InvariantCulture);
                    var i3 = content.IndexOf("</h1", i1 + 5, StringComparison.InvariantCulture);
                    if (i2 == -1 || i3 < i2)
                        throw new Exception("Check YahooProfile parser (GetSymbolAndName2023 method)");

                    var s1 = content.Substring(i2 + 1, i3 - i2 - 1);
                    s1 = System.Net.WebUtility.HtmlDecode(s1).Trim();
                    i2 = s1.LastIndexOf('(');
                    if (s1.EndsWith(')') && i2 >= 0)
                    {
                        var name = s1.Substring(0, i2).Trim();
                        var symbol = s1.Substring(i2 + 1, s1.Length - i2 - 2).Trim();
                        return new[] { symbol, name };
                    }

                    i2 = s1.IndexOf('-');
                    if (i2 >= 0)
                    {
                        var symbol = s1.Substring(0, i2 - 1).Trim();
                        var name = s1.Substring(i2 + 1).Trim();
                        return new[] { symbol, name };
                    }

                    throw new Exception("Check YahooProfile parser (GetSymbolAndName2023 method)");
                }

                string GetSector2020(int sectorKeyPosition)
                {
                    if (sectorKeyPosition == -1) return null;
                    var i2 = content.IndexOf("<span", sectorKeyPosition, StringComparison.InvariantCulture);
                    var i3 = content.IndexOf(">", i2, StringComparison.InvariantCulture);
                    var i4 = content.IndexOf("</span>", i3, StringComparison.InvariantCulture);
                    if (i4 > i3 && i3 > i2 && i2 > 0)
                    {
                        var sector = content.Substring(i3 + 1, i4 - i3 - 1).Trim();
                        return sector;
                    }

                    throw new Exception("Check YahooProfile parser (GetSector2020 method)");
                }

                string GetSector2023(int sectorKeyPosition)
                {
                    if (sectorKeyPosition == -1) return null;
                    var i3 = content.IndexOf("</a>", sectorKeyPosition, StringComparison.InvariantCulture);
                    var i2 = content.LastIndexOf(">", i3, StringComparison.InvariantCulture);
                    if (i3 > i2 && i2 > 0)
                    {
                        var sector = content.Substring(i2 + 1, i3 - i2 - 1).Trim();
                        return sector;
                    }

                    throw new Exception("Check YahooProfile parser (GetSector2020 method)");
                }

                (string, string, string) ApplyCorrections(string symbol, string name, string sector)
                {
                    if (string.IsNullOrEmpty(sector)) return (null, null, null);

                    // Correction of non-standard sectors
                    if (sector == "Industrial Goods" && (symbol == "BLDR" || symbol == "ECOL")) sector = "Industrials";

                    if (sector == "Financial" && symbol == "MFA") sector = "Real Estate";
                    else if (sector == "Financial" && (symbol == "CBSH" || symbol == "ISTR" || symbol == "NAVI" ||
                                                       symbol == "SFE" ||
                                                       symbol == "ZTR")) sector = "Financial Services";

                    if (sector == "Services" && (symbol == "EAT" || symbol == "PZZA")) sector = "Consumer Cyclical";
                    else if (sector == "Services" && (symbol == "EROS" || symbol == "NFLX")) sector = "Communication Services";
                    else if (sector == "Services" && (symbol == "TTEK" || symbol == "ZNH")) sector = "Industrials";

                    if (sector == "Consumer Goods" && symbol == "KNDI") sector = "Consumer Cyclical";

                    if (string.IsNullOrEmpty(name)) name = null;
                    if (string.IsNullOrEmpty(symbol))
                        throw new Exception("Check YahooProfile parser (AddSector method)");

                    return (symbol, name, sector);
                }
        }

        public static void ParseHtml()
        {
            Logger.AddMessage($"Started");

            // var folder = @"E:\Quote\WebData\Symbols\Yahoo\Profile\WA_Data.Short";
            var folder = @"E:\Quote\WebData\Symbols\Yahoo\Profile\Data\YP_20230221.Short";
            var files = Directory.GetFiles(folder, "*.html").OrderBy(a => a).ToArray();
            var cnt = 0;
            var dict = new Dictionary<int, object[]>();
            var sectors = new Dictionary<string, int>();
            var sectors2 = new Dictionary<string, List<(string, string)>>();
            foreach (var file in files)
            {
                cnt++;
                if (cnt % 100 == 0)
                    Logger.AddMessage($"Parsed {cnt:N0} files from {files.Length:N0}. Last file: {Path.GetFileName(file)}");

                if (file.EndsWith("ACT_20230406035606.html") || file.EndsWith("AXP_20230410144059.html") ||
                    file.EndsWith("OXM_20230429222332.html") || file.EndsWith("SONO_20230422222307.html") ||
                    file.EndsWith("WAB_20230417233217.html") || file.EndsWith("ILMNV_20240624224841.html"))
                { // Bad files
                    continue;
                }

                var content = File.ReadAllText(file);

                var i1 = content.IndexOf(">Fund Overview<", StringComparison.InvariantCulture);
                if (i1 > 1)
                {
                    SaveToDict(9);
                    continue;
                }

                // 2020 year format
                i1 = content.IndexOf("<h1 class=\"D(ib) Fz(1", StringComparison.InvariantCulture);
                if (i1 > -1) // Count, from, to: 20652, 2020-01-01, 2024-06-27
                { // AA_20200512043535.html
                    var k2 = content.IndexOf("<h1 class=\"D(ib) Fz(1", i1 + 5, StringComparison.InvariantCulture);
                    if (k2 != -1)
                        throw new Exception("Check YahooProfile parser");

                    var i2 = content.IndexOf(">Sector<", i1, StringComparison.InvariantCulture);
                    if (i2 == -1) i2 = content.IndexOf(">Sector(s)<", i1, StringComparison.InvariantCulture);
                    if (i2 == -1 && CheckSector2020())
                        throw new Exception("Check YahooProfile parser");

                    var symbolAndName = GetSymbolAndName2020();

                    var sector = GetSector2020(i2);
                    AddSector(symbolAndName[0], symbolAndName[1], sector);

                    SaveToDict(1);
                    continue;
                }

                // 2024 year format
                i1 = content.IndexOf("<h1 class=\"svelte-", StringComparison.InvariantCulture);
                if (i1 > -1) // Count, from, to: 1328, 2023-11-12, 2024-07-05
                { // AAON_20240503140454.html
                    var k2 = content.IndexOf("<h1 class=\"svelte-", i1 + 5, StringComparison.InvariantCulture);
                    if (k2 != -1)
                        throw new Exception("Check YahooProfile parser");

                    var i2 = content.IndexOf(">Sector: </dt>", i1, StringComparison.InvariantCulture);
                    if (i2 == -1 && CheckSector2024())
                        throw new Exception("Check YahooProfile parser");

                    var symbolAndName = GetSymbolAndName2023();
                    var sector = GetSector2023(i2);
                    AddSector(symbolAndName[0], symbolAndName[1], sector);

                    SaveToDict(2);
                    continue;
                }

                i1 = content.IndexOf(">Symbols similar to ", StringComparison.InvariantCulture);
                if (i1 > -1) // Not found
                    continue;

                throw new Exception("Check YahooProfile parser");


                void SaveToDict(int id)
                {
                    var ss = Path.GetFileNameWithoutExtension(file).Split('_');
                    DateTime dateKey;
                    if (ss.Length == 2) // web.archive
                        dateKey = DateTime.ParseExact(ss[ss.Length - 1], "yyyyMMddHHmmss", CultureInfo.InvariantCulture).Date;
                    else
                        dateKey = DateTime.ParseExact(ss[2], "yyyyMMdd", CultureInfo.InvariantCulture);

                    if (!dict.ContainsKey(id))
                    {
                        dict.Add(id, new object[] { 1, dateKey, dateKey });
                        return;
                    }

                    var oldData = dict[id];
                    oldData[0] = (int)oldData[0] + 1;
                    var from = (DateTime)oldData[1];
                    if (dateKey < from) oldData[1] = dateKey;
                    var to = (DateTime)oldData[2];
                    if (dateKey > to) oldData[2] = dateKey;
                }

                bool CheckSector2020()
                {
                    var content1 = content.Replace("https://finance.yahoo.com/sectors\" title=\"Sectors\">Sectors", "\">");
                    content1 = content1.Replace("sector information, ", "");
                    content1 = content1.Replace("finance.yahoo.com/sector/", "x");

                    /*// Below for YP_20230304.1/YP_HUBCZ_20230304.html (full version)
                    var a1 = content1.Length;
                    content1 = content1.Replace("SectorAllocation", "", StringComparison.InvariantCultureIgnoreCase);
                    var a2 = content1.Length;
                    content1 = content1.Replace("morningstarSector", "");
                    var a3 = content1.Length;
                    content1 = content1.Replace("enableSectorIndustryLabelFix", "");
                    var a4 = content1.Length;
                    content1 = content1.Replace("SectorVisitorTrend", "", StringComparison.InvariantCultureIgnoreCase);
                    var a5 = content1.Length;
                    content1 = content1.Replace("sectorsList", "");
                    var a6 = content1.Length;

                    // Below for YP_20230304/YP_CHKEL_20230304.html (full version)
                    content1 = content1.Replace("ZiP8secTORUqEL", "");
                    var a7 = content1.Length;*/
                    if (content1.IndexOf("sector", StringComparison.InvariantCultureIgnoreCase) == -1)
                        return false;

                    return true;
                }

                bool CheckSector2024()
                {
                    var content1 = content;
                    if (content1.IndexOf("sector", StringComparison.InvariantCultureIgnoreCase) == -1)
                        return false;
                    if (content1.IndexOf(">Profile Information Not Available<",
                            StringComparison.InvariantCulture) != -1)
                    { // CNH_20240520210256.html
                        return false;
                    }
                    if (content1.IndexOf(">Profile data is currently not available for ", StringComparison.InvariantCulture) != -1)
                    { // ENERW_20240422165327.html
                        return false;
                    }

                    return true;
                }

                string[] GetSymbolAndName2020()
                {
                    var i2 = content.IndexOf(">", i1 + 5, StringComparison.InvariantCulture);
                    var i3 = content.IndexOf("</h1", i1 + 5, StringComparison.InvariantCulture);
                    if (i2 == -1 || i3 < i2)
                        throw new Exception("Check YahooProfile parser (GetSymbolAndName2020 method)");

                    var s1 = content.Substring(i2 + 1, i3 - i2 - 1);
                    s1 = System.Net.WebUtility.HtmlDecode(s1).Trim();
                    i2 = s1.LastIndexOf('(');
                    if (s1.EndsWith(')') && i2 >= 0)
                    {
                        var name = s1.Substring(0, i2).Trim();
                        var symbol = s1.Substring(i2 + 1, s1.Length - i2 - 2).Trim();
                        return new[] { symbol, name };
                    }

                    i2 = s1.IndexOf('-');
                    if (i2 >= 0)
                    {
                        var symbol = s1.Substring(0, i2 - 1).Trim();
                        var name = s1.Substring(i2 + 1).Trim();
                        return new[] { symbol, name };
                    }

                    throw new Exception("Check YahooProfile parser (GetSymbolAndName2020 method)");
                }

                string[] GetSymbolAndName2023()
                {
                    var i2 = content.IndexOf(">", i1 + 5, StringComparison.InvariantCulture);
                    var i3 = content.IndexOf("</h1", i1 + 5, StringComparison.InvariantCulture);
                    if (i2 == -1 || i3 < i2)
                        throw new Exception("Check YahooProfile parser (GetSymbolAndName2023 method)");

                    var s1 = content.Substring(i2 + 1, i3 - i2 - 1);
                    s1 = System.Net.WebUtility.HtmlDecode(s1).Trim();
                    i2 = s1.LastIndexOf('(');
                    if (s1.EndsWith(')') && i2 >= 0)
                    {
                        var name = s1.Substring(0, i2).Trim();
                        var symbol = s1.Substring(i2 + 1, s1.Length - i2 - 2).Trim();
                        return new[] { symbol, name };
                    }

                    i2 = s1.IndexOf('-');
                    if (i2 >= 0)
                    {
                        var symbol = s1.Substring(0, i2 - 1).Trim();
                        var name = s1.Substring(i2 + 1).Trim();
                        return new[] { symbol, name };
                    }

                    throw new Exception("Check YahooProfile parser (GetSymbolAndName2023 method)");
                }

                string GetSector2020(int sectorKeyPosition)
                {
                    if (sectorKeyPosition == -1) return null;
                    var i2 = content.IndexOf("<span", sectorKeyPosition, StringComparison.InvariantCulture);
                    var i3 = content.IndexOf(">", i2, StringComparison.InvariantCulture);
                    var i4 = content.IndexOf("</span>", i3, StringComparison.InvariantCulture);
                    if (i4 > i3 && i3 > i2 && i2 > 0)
                    {
                        var sector = content.Substring(i3 + 1, i4 - i3 - 1).Trim();
                        return sector;
                    }

                    throw new Exception("Check YahooProfile parser (GetSector2020 method)");
                }

                string GetSector2023(int sectorKeyPosition)
                {
                    if (sectorKeyPosition == -1) return null;
                    var i3 = content.IndexOf("</a>", sectorKeyPosition, StringComparison.InvariantCulture);
                    var i2 = content.LastIndexOf(">", i3, StringComparison.InvariantCulture);
                    if (i3 > i2 && i2 > 0)
                    {
                        var sector = content.Substring(i2 + 1, i3 - i2 - 1).Trim();
                        return sector;
                    }

                    throw new Exception("Check YahooProfile parser (GetSector2020 method)");
                }

                void AddSector(string symbol, string name, string sector)
                {
                    if (string.IsNullOrEmpty(sector)) return;

                    // Correction of non-standard sectors
                    if (sector == "Industrial Goods" && (symbol == "BLDR" || symbol == "ECOL")) sector = "Industrials";

                    if (sector == "Financial" && symbol == "MFA") sector = "Real Estate";
                    else if (sector == "Financial" && (symbol == "CBSH" || symbol == "ISTR" || symbol == "NAVI" ||
                                                       symbol == "SFE" ||
                                                       symbol == "ZTR")) sector = "Financial Services";

                    if (sector == "Services" && (symbol == "EAT" || symbol == "PZZA")) sector = "Consumer Cyclical";
                    else if (sector == "Services" && (symbol == "EROS" || symbol == "NFLX")) sector = "Communication Services";
                    else if (sector == "Services" && (symbol == "TTEK" || symbol == "ZNH")) sector = "Industrials";

                    if (sector == "Consumer Goods" && symbol == "KNDI") sector = "Consumer Cyclical";

                    if (string.IsNullOrEmpty(name)) name = null;
                    if (string.IsNullOrEmpty(symbol))
                        throw new Exception("Check YahooProfile parser (AddSector method)");

                    if (!sectors.ContainsKey(sector))
                    {
                        sectors.Add(sector, 0);
                        sectors2.Add(sector, new List<(string, string)>());
                    }

                    sectors[sector]++;
                    sectors2[sector].Add((symbol, name));
                }
            }
            Logger.AddMessage($"Finished");

            foreach (var kvp in dict.OrderBy(a => a.Key))
                Debug.Print($"{kvp.Key}\t{kvp.Value[0]}\t{((DateTime)kvp.Value[1]):yyyy-MM-dd}\t{((DateTime)kvp.Value[2]):yyyy-MM-dd}");
            /*
Dict: id, recs, from, to
1	20652	2020-01-01	2024-06-27 (2020 format)
2	1328	2023-11-12	2024-07-05 (2024 format)
9	144	2020-08-14	2024-03-05
            */
        }

        public static void ParseHtmlZip()
        {
            var zipFileName = @"E:\Quote\WebData\Symbols\Yahoo\Profile\WA_Data.Short.zip";
            using (var zip = ZipFile.Open(zipFileName, ZipArchiveMode.Read))
                foreach (var entry in zip.Entries.Where(a => a.Length > 0))
                {
                    var content = entry.GetContentOfZipEntry();
                    var timeStamp = entry.LastWriteTime.DateTime;
                    //                    var items = new List<SplitModel>();
                    var i1 = content.IndexOf(">Sector(s)<", StringComparison.InvariantCulture);
                    if (i1 == -1)
                    {
                        i1 = content.IndexOf(">Sector: <", StringComparison.InvariantCulture);
                    }

                    if (i1 == -1)
                    {
                        i1 = content.IndexOf(">Sector<", StringComparison.InvariantCulture);
                    }

                    if (i1 == -1)
                    {
                        i1 = content.IndexOf("\"sector\":\"", StringComparison.InvariantCulture);
                    }

                    if (i1 == -1)
                    {
                        i1 = content.IndexOf(">Fund Overview<", StringComparison.InvariantCulture);
                        if (i1 == -1)
                        {

                        }
                    }
                }
        }

        public static async Task DownloadData()
        {
            // Get url and file list
            var listFiles = Directory.GetFiles(ListDataFolder, "*.txt");
            var toDownload = new List<DownloadItem>();
            var fileCount = 0;
            foreach (var listFile in listFiles)
            {
                var items = File.ReadAllLines(listFile).Select(a => new JsonItem(a))
                    .Where(a => a.Type == "text/html" && a.Status == 200).OrderBy(a => a.TimeStamp).ToArray();
                var polygonSymbol = Path.GetFileNameWithoutExtension(listFile);
                var lastDateKey = "";
                foreach (var item in items)
                {
                    var dateKey = item.TimeStamp.Substring(0, 8);
                    if (dateKey == lastDateKey) // skeep the same day
                        continue;

                    var url = $"https://web.archive.org/web/{item.TimeStamp}/{item.Url}";
                    var filename = Path.Combine(HtmlDataFolder, $"{polygonSymbol}_{item.TimeStamp}.html");
                    toDownload.Add(new DownloadItem(url, filename));
                    fileCount++;
                    lastDateKey = dateKey;
                }
            }

            // Download data
            var tasks = new ConcurrentDictionary<DownloadItem, Task<byte[]>>();
            var itemCount = 0;
            var needToDownload = true;
            while (needToDownload)
            {
                needToDownload = false;
                foreach (var urlAndFilename in toDownload)
                {
                    itemCount++;

                    if (!File.Exists(urlAndFilename.Filename))
                    {
                        var task = WebClientExt.DownloadToBytesAsync(urlAndFilename.Url);
                        tasks[urlAndFilename] = task;
                        urlAndFilename.StatusCode = null;
                    }
                    else
                        urlAndFilename.StatusCode = HttpStatusCode.OK;

                    if (tasks.Count >= 10)
                    {
                        await Download(tasks);
                        Helpers.Logger.AddMessage($"Downloaded {itemCount} url from {toDownload.Count:N0}");
                        tasks.Clear();
                        needToDownload = true;
                    }
                }

                if (tasks.Count > 0)
                {
                    await Download(tasks);
                    tasks.Clear();
                    needToDownload = true;
                }

                Helpers.Logger.AddMessage($"Downloaded {itemCount} url from {toDownload.Count:N0}");
            }

            Helpers.Logger.AddMessage($"Downloaded {toDownload.Count:N0} items");
        }

        private static async Task Download(ConcurrentDictionary<DownloadItem, Task<byte[]>> tasks)
        {
            foreach (var kvp in tasks)
            {
                try
                {
                    var data = await kvp.Value;
                    // var htmlContent = HtmlHelper.RemoveUselessTags(System.Text.Encoding.UTF8.GetString(data));
                    // File.WriteAllText(kvp.Key.Filename, htmlContent);
                    File.WriteAllText(kvp.Key.Filename, System.Text.Encoding.UTF8.GetString(data));
                    kvp.Key.StatusCode = HttpStatusCode.OK;
                }
                catch (Exception ex)
                {
                    if (ex is WebException exc && exc.Response is HttpWebResponse response &&
                        (response.StatusCode == HttpStatusCode.InternalServerError ||
                         response.StatusCode == HttpStatusCode.BadGateway))
                    {
                        kvp.Key.StatusCode = response.StatusCode;
                        continue;
                    }

                    throw new Exception($"YahooSectorLoader: Error while download from {kvp.Key}. Error message: {ex.Message}");
                }
            }
        }

        public static void DownloadList()
        {
            Logger.AddMessage($"Started");
            var symbols = GetSymbolXref();
            // var symbols = new Dictionary<string,string>{{"MSFT", "MSFT80"}};
            var count = 0;
            foreach (var symbol in symbols)
            {
                count++;
                    Logger.AddMessage($"Process {count} symbols from {symbols.Count}");
                var filename = Path.Combine($@"{ListDataFolder}", $"{symbol.Value}.txt");
                if (!File.Exists(filename))
                {
                    var url = string.Format(ListUrlTemplate, symbol.Key);
                    var o = Helpers.WebClientExt.GetToBytes(url, false);
                    if (o.Item3 != null)
                        throw new Exception(
                            $"WA_YahooProfile: Error while download from {url}. Error message: {o.Item3.Message}");
                    File.WriteAllBytes(filename, o.Item1);
                    System.Threading.Thread.Sleep(200);
                }
            }
            Logger.AddMessage($"Finished");
        }

        private static Dictionary<string, string> GetSymbolXref()
        {
            var data = new Dictionary<string, string>();
            using (var conn = new SqlConnection(Settings.DbConnectionString))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = "SELECT DISTINCT symbol, YahooSymbol FROM dbQ2024..SymbolsPolygon " +
                                  "WHERE isnull([To],'2099-12-31')>'2020-01-01' and IsTest is null " +
                                  "and MyType not like 'ET%' and MyType not in ('RIGHT') and YahooSymbol is not null order by 1";
                //                "and MyType not like 'ET%' and MyType not in ('FUND', 'RIGHT', 'WARRANT') and YahooSymbol is not null";
                using (var rdr = cmd.ExecuteReader())
                    while (rdr.Read())
                        data.Add((string)rdr["YahooSymbol"], (string)rdr["symbol"]);
            }

            return data;
        }

        #region =========  SubClasses  ===========
        public class DbItem
        {
            public string PolygonSymbol;
            public string YahooSymbol;
            public string Name;
            public string Sector;
            public DateTime StartDate;
            public DateTime EndDate;
            /*public string Symbol;
            public string sDate;
            public string sLastUpdated;
            public string Exchange;
            public string Sector;
            public string Name;
            public DateTime Date => DateTime.ParseExact(sDate, "yyyyMMddHHmmss", CultureInfo.InvariantCulture).Date;
            public DateTime LastUpdated => DateTime.ParseExact(sLastUpdated, "yyyyMMddHHmmss", CultureInfo.InvariantCulture).Date;*/
        }

        private class JsonItem
        {
            public string Url;
            public string TimeStamp;
            public string Type;
            public int Status;

            public JsonItem(string line)
            {
                var ss = line.Split(' ');
                TimeStamp = ss[1];
                Url = ss[2];
                Type = ss[3];
                if (ss[4] != "-")
                    Status = int.Parse(ss[4]);
            }
        }

        public class DownloadItem
        {
            public string Url;
            public string Filename;
            public HttpStatusCode? StatusCode;

            public DownloadItem(string url, string filename)
            {
                Url = url;
                Filename = filename;
            }
        }
        #endregion


    }
}
