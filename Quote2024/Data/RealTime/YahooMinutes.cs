using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Data.Helpers;

namespace Data.RealTime
{
    public class YahooMinutes
    {
        // private const string UrlTemplate = @"https://query2.finance.yahoo.com/v8/finance/chart/{0}?period1={1}&period2={2}&interval=1m&events=history";
        private const string UrlTemplate = @"https://query2.finance.yahoo.com/v8/finance/chart/{0}?period1={1}&period2={2}&interval=1m&includePrePost=true&events=history";
        
        private const string FileTemplate = @"{0}.json";

        public static async void Start()
        {
            var from = new DateTimeOffset(DateTime.UtcNow.AddMinutes(-30)).ToUnixTimeSeconds();
            var to = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();

            var urlsAndFileNames = _symbols
                .Select(s => (string.Format(UrlTemplate, s, from, to), string.Format(FileTemplate, s)));
            // var virtualFileEntries = new List<VirtualFileEntry>();
            var sw = new Stopwatch();
            sw.Start();

            var tasks = new ConcurrentDictionary<string, Task<byte[]>>();
            foreach (var symbol in _symbols)
            {
                var task = Download.DownloadToBytesAsync(string.Format(UrlTemplate, symbol, from, to));
                tasks[symbol]= task;
            }

            // await Task.WhenAll(tasks.Values);
            var results = new Dictionary<string, byte[]>();
            foreach (var kvp in tasks)
            {
                try
                {
                    // release control to the caller until the task is done, which will be near immediate for each task following the first
                    results.Add(kvp.Key, await kvp.Value);
                }
                catch (Exception e)
                {
                    results.Add(kvp.Key, Array.Empty<byte>());
                    Debug.Print($"{DateTime.Now.TimeOfDay}. Error: {e.Message}");
                }
            }

            sw.Stop();
            Debug.Print($"SW: {sw.ElapsedMilliseconds}. From: {from}. To: {to}");

            var virtualFileEntries =
                results.Select(kvp => new VirtualFileEntry(string.Format(FileTemplate, kvp.Key), kvp.Value));
            ZipUtils.ZipVirtualFileEntries($@"E:\Quote\WebData\RealTime\YahooMinute\RTYahooMinutes_{DateTime.Now:yyyyMMddHHmmss}.zip", virtualFileEntries);

            var aa1 = CsUtils.MemoryUsedInBytes / 1024 / 1024;
            virtualFileEntries = null;
            tasks.Clear();
            var aa2 = CsUtils.MemoryUsedInBytes / 1024 / 1024;
        }

        public static string[] _symbols = new[]
        {
            "A", "AA", "AAL", "AAOI", "AAON", "AAP", "AAPL", "ABBV", "ABEV", "ABNB", "ABT", "ACAD", "ACGL", "ACI",
            "ACLS", "ACMR", "ACN", "ACWI", "ADBE", "ADC", "ADI", "ADIL", "ADM", "ADP", "ADSK", "ADT", "AEE", "AEM",
            "AEO", "AEP", "AER", "AES", "AFL", "AFRM", "AGCO", "AGG", "AGNC", "AI", "AIG", "AJG", "AKAM", "AKRO", "ALB",
            "ALC", "ALGM", "ALGN", "ALK", "ALKS", "ALL", "ALLE", "ALLY", "ALNY", "ALSN", "ALT", "AMAT", "AMC", "AMCR",
            "AMD", "AME", "AMGN", "AMH", "AMP", "AMPH", "AMT", "AMZN", "AN", "ANET", "ANF", "ANSS", "AON", "AOS", "APA",
            "APD", "APG", "APH", "APLS", "APO", "APP", "APTV", "AR", "ARCC", "ARCH", "ARE", "ARES", "ARGX", "ARKB",
            "ARKG", "ARKK", "ARM", "ARMK", "ARQT", "ARRY", "ARW", "ARWR", "AS", "ASHR", "ASML", "ASO", "ASX", "ATI",
            "ATKR", "ATMU", "ATO", "AVB", "AVDL", "AVDX", "AVGO", "AVTR", "AWI", "AWK", "AXNX", "AXON", "AXP", "AXSM",
            "AXTA", "AZEK", "AZN", "BA", "BABA", "BAC", "BAH", "BALL", "BAM", "BAX", "BBAI", "BBWI", "BBY", "BCE",
            "BCS", "BDX", "BE", "BEAM", "BEKE", "BEN", "BERY", "BF-B", "BG", "BHC", "BHP", "BHVN", "BIDU", "BIIB",
            "BIL", "BILI", "BILL", "BIO", "BITB", "BITF", "BITI", "BITO", "BITX", "BIVI", "BJ", "BK", "BKLN", "BKNG",
            "BKR", "BLD", "BLDR", "BLK", "BLMN", "BMRN", "BMY", "BN", "BND", "BNDX", "BNS", "BNTX", "BOIL", "BOOT",
            "BP", "BPMC", "BR", "BRBR", "BRK-A", "BRK-B", "BRKR", "BRO", "BROS", "BRX", "BSX", "BSY", "BTE", "BTI",
            "BTU", "BUD", "BURL", "BWA", "BWXT", "BX", "BXP", "BYD", "BYND", "BZ", "C", "CAG", "CAH", "CALF", "CAR",
            "CARR", "CART", "CAT", "CAVA", "CB", "CBAY", "CBOE", "CBRE", "CC", "CCCS", "CCEP", "CCI", "CCJ", "CCK",
            "CCL", "CCOI", "CDNS", "CDW", "CE", "CEG", "CEIX", "CELH", "CERE", "CF", "CFG", "CFLT", "CG", "CHD", "CHH",
            "CHK", "CHKP", "CHRD", "CHRW", "CHTR", "CHWY", "CI", "CIEN", "CINF", "CIVI", "CL", "CLDX", "CLF", "CLS",
            "CLSK", "CLVT", "CLX", "CM", "CMA", "CMC", "CMCSA", "CME", "CMG", "CMI", "CMS", "CNC", "CNHI", "CNI", "CNK",
            "CNM", "CNP", "CNQ", "CNX", "COF", "COHR", "COIN", "COLB", "COLD", "CONL", "COO", "COP", "COR", "COST",
            "CP", "CPB", "CPG", "CPNG", "CPRT", "CPRX", "CPT", "CRBG", "CRC", "CRDF", "CRDO", "CRH", "CRI", "CRL",
            "CRM", "CRNX", "CROX", "CRSP", "CRWD", "CSCO", "CSGP", "CSL", "CSX", "CTAS", "CTLT", "CTRA", "CTSH", "CTVA",
            "CUBE", "CUZ", "CVE", "CVNA", "CVS", "CVX", "CXM", "CYBR", "CYTK", "CZR", "D", "DAL", "DAR", "DASH", "DAVA",
            "DAY", "DBX", "DCI", "DD", "DDOG", "DE", "DECK", "DELL", "DEO", "DFS", "DG", "DGRO", "DGX", "DHI", "DHR",
            "DIA", "DINO", "DIS", "DKNG", "DKS", "DLR", "DLTR", "DNA", "DOCU", "DOV", "DOW", "DOX", "DPST", "DPZ",
            "DRI", "DT", "DTE", "DUK", "DUOL", "DUST", "DV", "DVA", "DVN", "DWAC", "DXC", "DXCM", "DYN", "EA", "EBAY",
            "EC", "ECL", "ED", "EDR", "EDU", "EEM", "EFA", "EFX", "EIX", "EL", "ELAN", "ELF", "ELS", "ELV", "EMB",
            "EME", "EMN", "EMR", "EMXC", "ENB", "ENPH", "ENTG", "EOG", "EPAM", "EPD", "EPRT", "EQH", "EQIX", "EQNR",
            "EQR", "EQT", "ERF", "ERIC", "ERJ", "ES", "ESS", "ESTC", "ET", "ETN", "ETR", "ETSY", "EVBG", "EVH", "EVRG",
            "EW", "EWBC", "EWJ", "EWT", "EWW", "EWY", "EWZ", "EXAS", "EXC", "EXEL", "EXPD", "EXPE", "EXR", "EZU", "F",
            "FANG", "FAST", "FBIN", "FBTC", "FCN", "FCX", "FDX", "FE", "FERG", "FFIV", "FHN", "FI", "FIS", "FITB",
            "FIVE", "FIVN", "FIX", "FL", "FLEX", "FLR", "FLT", "FLYW", "FMC", "FN", "FND", "FNGU", "FNV", "FOUR",
            "FOXA", "FR", "FRO", "FROG", "FRPT", "FRT", "FSLR", "FSLY", "FSR", "FSS", "FTI", "FTNT", "FTV", "FUBO",
            "FUTU", "FWONK", "FWRD", "FXI", "G", "GBTC", "GCT", "GD", "GDDY", "GDX", "GDXJ", "GE", "GEHC", "GFI", "GFL",
            "GFS", "GGB", "GGG", "GILD", "GIS", "GKOS", "GLD", "GLOB", "GLPI", "GLW", "GM", "GME", "GNRC", "GOLD",
            "GOOG", "GOOGL", "GOTU", "GPC", "GPK", "GPN", "GPS", "GRBK", "GRMN", "GS", "GSG", "GSK", "GTLB", "GTLS",
            "GWRE", "H", "HAL", "HALO", "HAS", "HAYW", "HBAN", "HBI", "HCA", "HCP", "HD", "HDB", "HES", "HGV", "HIG",
            "HIMS", "HLT", "HMY", "HOG", "HOLX", "HON", "HOOD", "HPE", "HPQ", "HRL", "HSBC", "HSIC", "HST", "HSY",
            "HTHT", "HUBB", "HUBG", "HUBS", "HUM", "HWM", "HYG", "IAU", "IBB", "IBIT", "IBKR", "IBM", "IBN", "ICE",
            "ICLR", "IDCC", "IDXX", "IEF", "IEFA", "IEMG", "IEX", "IFF", "IGT", "IGV", "IJH", "IJR", "ILMN", "INCY",
            "INDA", "INFY", "INSM", "INSP", "INSW", "INTC", "INTU", "INVH", "IONQ", "IONS", "IOT", "IOVA", "IP", "IPG",
            "IQV", "IR", "IRM", "IRWD", "ISRG", "ITB", "ITCI", "ITOT", "ITUB", "ITW", "IVV", "IVW", "IWD", "IWF", "IWM",
            "IWN", "IWO", "IXUS", "IYR", "J", "JANX", "JAZZ", "JBHT", "JBL", "JCI", "JD", "JDST", "JEPI", "JEPQ",
            "JKHY", "JMIA", "JNJ", "JNK", "JNPR", "JNUG", "JPM", "JPST", "JWN", "K", "KBE", "KBH", "KBR", "KDP", "KEY",
            "KEYS", "KGC", "KHC", "KIM", "KKR", "KLAC", "KMB", "KMI", "KMX", "KNX", "KO", "KODK", "KOLD", "KR", "KRC",
            "KRE", "KRG", "KRYS", "KSS", "KVUE", "KVYO", "KWEB", "LABD", "LABU", "LAMR", "LBRDK", "LBTYK", "LCID",
            "LDOS", "LEA", "LEN", "LH", "LHX", "LI", "LIN", "LITE", "LKQ", "LLY", "LMT", "LNG", "LNT", "LNW", "LOW",
            "LPLA", "LPX", "LQD", "LRCX", "LSCC", "LSTR", "LSXMK", "LULU", "LUV", "LVS", "LW", "LYB", "LYFT", "LYV",
            "M", "MA", "MAA", "MAR", "MARA", "MAS", "MASI", "MBB", "MBLY", "MCD", "MCHI", "MCHP", "MCK", "MCO", "MDB",
            "MDGL", "MDLZ", "MDT", "MDY", "MELI", "MET", "META", "MFC", "MGA", "MGM", "MGY", "MHK", "MINM", "MKC",
            "MKSI", "MKTX", "MLCO", "MLM", "MMC", "MMM", "MNDY", "MNST", "MO", "MOD", "MOH", "MOS", "MPC", "MPLX",
            "MPW", "MPWR", "MQ", "MRK", "MRNA", "MRO", "MRUS", "MRVL", "MS", "MSCI", "MSFT", "MSI", "MSTR", "MTB",
            "MTC", "MTCH", "MTDR", "MTH", "MTN", "MTUM", "MTZ", "MU", "MUB", "MUR", "NARI", "NBIX", "NCLH", "NDAQ",
            "NDSN", "NE", "NEE", "NEM", "NET", "NFE", "NFLX", "NI", "NIO", "NKE", "NLY", "NNN", "NOC", "NOG", "NOV",
            "NOW", "NRG", "NSC", "NTAP", "NTES", "NTLA", "NTNX", "NTR", "NTRA", "NTRS", "NU", "NUE", "NUGT", "NVAX",
            "NVDA", "NVDL", "NVO", "NVS", "NVST", "NVT", "NVTS", "NWSA", "NXE", "NXPI", "NXT", "NXU", "NYCB", "NYT",
            "O", "OC", "ODFL", "OGE", "OKE", "OKTA", "OLN", "OMC", "OMF", "ON", "ONON", "ONTO", "OPRA", "ORCL", "ORLY",
            "OSK", "OTIS", "OVV", "OWL", "OXY", "PAAS", "PANW", "PARA", "PARR", "PATH", "PAYC", "PAYX", "PBA", "PBF",
            "PBR", "PBR-A", "PCAR", "PCG", "PCOR", "PCTY", "PCVX", "PDD", "PEAK", "PEG", "PENN", "PEP", "PFE", "PFF",
            "PFG", "PFGC", "PG", "PGNY", "PGR", "PH", "PHM", "PINS", "PK", "PKG", "PLD", "PLNT", "PLTR", "PLUG", "PM",
            "PNC", "PNM", "PNR", "PNW", "PODD", "POOL", "POWL", "PPG", "PPL", "PR", "PRGO", "PRKS", "PRU", "PSA", "PSN",
            "PSTG", "PSX", "PTC", "PTCT", "PTEN", "PVH", "PWR", "PXD", "PYPL", "PZZA", "QCOM", "QID", "QLD", "QLYS",
            "QQQ", "QQQM", "QRVO", "QSR", "QYLD", "RBA", "RBLX", "RCL", "RCM", "RDNT", "REAL", "REG", "REGN", "REXR",
            "RF", "RGEN", "RGLD", "RH", "RIG", "RILY", "RIO", "RIOT", "RITM", "RIVN", "RJF", "RL", "RMBS", "RMD",
            "ROIV", "ROK", "ROKU", "ROL", "ROOT", "ROP", "ROST", "RPM", "RPRX", "RRC", "RSG", "RSP", "RTO", "RTX",
            "RUN", "RVTY", "RXRX", "RYAN", "S", "SAIA", "SANA", "SAP", "SATS", "SBAC", "SBUX", "SCCO", "SCHD", "SCHG",
            "SCHW", "SCO", "SDGR", "SDOW", "SDRL", "SDS", "SE", "SEDG", "SEE", "SFM", "SG", "SGOV", "SHAK", "SHC",
            "SHEL", "SHLS", "SHOP", "SHV", "SHW", "SHY", "SIG", "SIRI", "SJM", "SJNK", "SKX", "SLB", "SLG", "SLM",
            "SLV", "SM", "SMAR", "SMCI", "SMG", "SMH", "SN", "SNAP", "SNOW", "SNPS", "SNV", "SNX", "SNY", "SO", "SOFI",
            "SONY", "SOUN", "SOXL", "SOXS", "SOXX", "SPG", "SPGI", "SPLG", "SPLK", "SPLV", "SPOT", "SPR", "SPXL",
            "SPXS", "SPXU", "SPY", "SPYG", "SPYV", "SQ", "SQM", "SQQQ", "SRE", "SRPT", "SSNC", "SSO", "ST", "STE",
            "STLA", "STLD", "STM", "STNE", "STNG", "STRL", "STT", "STX", "STZ", "SU", "SUI", "SVXY", "SWAV", "SWK",
            "SWKS", "SWN", "SWTX", "SYF", "SYK", "SYM", "SYY", "T", "TAL", "TAP", "TARS", "TCOM", "TD", "TDG", "TDOC",
            "TDW", "TEAM", "TECH", "TECK", "TECL", "TEL", "TER", "TEVA", "TFC", "TGT", "TGTX", "THC", "TIP", "TJX",
            "TKO", "TLT", "TM", "TMDX", "TME", "TMF", "TMO", "TMUS", "TMV", "TNA", "TOL", "TOST", "TPR", "TPX", "TQQQ",
            "TREX", "TRGP", "TRIP", "TRMB", "TRNO", "TROW", "TRP", "TRU", "TRV", "TS", "TSCO", "TSLA", "TSLL", "TSLQ",
            "TSLX", "TSM", "TSN", "TT", "TTD", "TTE", "TTWO", "TW", "TWLO", "TXN", "TXRH", "TXT", "TZA", "U", "UAA",
            "UAL", "UBER", "UBS", "UCO", "UDOW", "UDR", "UEC", "UHS", "UL", "ULTA", "UMC", "UNG", "UNH", "UNM", "UNP",
            "UPRO", "UPS", "UPST", "URA", "URBN", "URI", "USB", "USFD", "USHY", "USMV", "USO", "UTHR", "UVIX", "UVXY",
            "V", "VAL", "VALE", "VB", "VCIT", "VEA", "VEEV", "VERA", "VFC", "VGK", "VGT", "VICI", "VIG", "VIPS", "VIXY",
            "VKTX", "VLO", "VLTO", "VMC", "VNO", "VNQ", "VO", "VOD", "VOO", "VOYA", "VRRM", "VRSK", "VRSN", "VRT",
            "VRTX", "VSCO", "VST", "VSTO", "VT", "VTI", "VTR", "VTRS", "VTV", "VTWO", "VTYX", "VUG", "VWO", "VXUS",
            "VXX", "VYM", "VYX", "VZ", "W", "WAB", "WAL", "WAT", "WBA", "WBD", "WBS", "WCC", "WCN", "WDAY", "WDC",
            "WEC", "WELL", "WEN", "WFC", "WFRD", "WHR", "WING", "WIX", "WM", "WMB", "WMS", "WMT", "WOLF", "WPC", "WPM",
            "WRB", "WRK", "WSC", "WSM", "WST", "WTW", "WY", "WYNN", "X", "XBI", "XEL", "XHB", "XLB", "XLC", "XLE",
            "XLF", "XLI", "XLK", "XLP", "XLRE", "XLU", "XLV", "XLY", "XME", "XMTR", "XOM", "XOP", "XP", "XPEV", "XPO",
            "XPOF", "XRAY", "XRT", "XYL", "YINN", "YOU", "YUM", "YUMC", "Z", "ZBH", "ZBRA", "ZI", "ZIM", "ZION", "ZM",
            "ZS", "ZTO", "ZTS"
        };
    }
}
