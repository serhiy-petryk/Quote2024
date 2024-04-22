using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Data.Helpers;
using ProtoBuf;

namespace Data.Actions.Polygon
{
    public static class PolygonTradesLoader
    {
        private const string UrlTemplate = @"https://api.polygon.io/v3/trades/{0}?timestamp={1}&limit=50000";
        private const string DataFolder = @"E:\Quote\WebData\Trades\Polygon\Data";
        private static readonly string ZipFileNameTemplate = Path.Combine(DataFolder, @"{2}\TradesPolygon_{1}_{0}.zip");

        public static void Test()
        {
            var items = 0;
            var seqCount = 0;
            var seqCount1 = 0;
            var seqCount2 = 0;
            var sizes = new Dictionary<int, int>();
            var conditions = new Dictionary<byte, int> { { 0, 0 } };
            var sConditions = new Dictionary<string, int> { { "", 0 } };
            var files = Directory.GetFiles(@"E:\Quote\WebData\Trades\Polygon\Data\2024-04-05", "*.zip");
            foreach (var zipFileName in files)
            {
                Logger.AddMessage($"File: {zipFileName}");
                //var zipFileName = @"E:\Quote\WebData\Trades\Polygon\Data\2024-04-05\TradesPolygon_20240405_AA.zip";
                using (var zip = ZipFile.Open(zipFileName, ZipArchiveMode.Read))
                    foreach (var entry in zip.Entries.Where(a => a.Length > 0))
                    {
                        var oo = ZipUtils.DeserializeZipEntry<cRoot>(entry);

                        items += oo.results.Length;
                        var time = oo.results.Max(a => a.sip_timestamp - a.participant_timestamp);
                        var time1 = oo.results.Min(a => a.sip_timestamp - a.participant_timestamp);

                        var lastSeq = int.MaxValue;
                        foreach (var item in oo.results)
                        {
                            if (lastSeq < item.sequence_number)
                            {
                                seqCount1++;
                            }

                            lastSeq = item.sequence_number;

                            if (item.conditions == null)
                            {
                              conditions[0]++;
                              sConditions[""]++;

                            }
                            else
                            {
                                foreach (var c in item.conditions)
                                {
                                    if (!conditions.ContainsKey(c))
                                        conditions.Add(c, 0);
                                    conditions[c]++;
                                }

                                var key = $"{item.conditions.Length};" + string.Join(',', item.conditions.OrderBy(a=>a).Select(a=>a.ToString()));
                                if (!sConditions.ContainsKey(key))
                                    sConditions.Add(key, 0);
                                sConditions[key]++;
                            }
                        }

                        /*var oo1 = oo.results
                            .Where(a => a.ParticipantTime > new TimeSpan(9, 30, 0) &&
                                        a.ParticipantTime < new TimeSpan(16, 00, 0))
                            .OrderByDescending(a => a.sip_timestamp - a.participant_timestamp).ToArray();*/
                        var oo2 = oo.results.OrderBy(a => a.sequence_number).ToArray();

                        foreach (var item in oo2)
                        {
                            var size = Convert.ToInt32(item.size);
                            if (!sizes.ContainsKey(size))
                                sizes.Add(size, 0);
                            sizes[size]++;
                        }

                        var lastSize = 0;
                        long lastTimestamp = 0;
                        foreach (var item in oo2)
                        {
                            if (item.sip_timestamp < lastTimestamp)
                            {
                                seqCount++;
                                Debug.Print($"Bad seq: {item.SipTime}");
                            }

                            if (Convert.ToInt32(item.size) == lastSize && lastSize!=100 && lastSize!=1)
                            {
                                seqCount2++;
                            }

                            lastSize = Convert.ToInt32(item.size);
                            lastTimestamp = item.sip_timestamp;
                        }
                    }
            }

            var aa1 = sizes.OrderByDescending(a => a.Value).ToDictionary(a => a.Key, a => a.Value);
            var aa21 = conditions.OrderByDescending(a => a.Value).ToDictionary(a => a.Key, a => a.Value);
            var aa22 = sConditions.OrderByDescending(a => a.Value).ToDictionary(a => a.Key, a => a.Value);
        }

        public static void Start()
        {
            var symbols = new string[]
            {
                "A", "AA", "AAL", "AAP", "AAPL", "ABBV", "ABEV", "ABIO", "ABNB", "ABT", "ACB", "ACGL", "ACHC", "ACI",
                "ACLS", "ACM", "ACMR", "ACN", "ACWI", "ADBE", "ADC", "ADI", "ADM", "ADP", "ADSK", "ADT", "ADTX", "AEE",
                "AEM", "AEO", "AEP", "AER", "AES", "AFL", "AFRM", "AG", "AGCO", "AGG", "AGI", "AGNC", "AGO", "AGQ",
                "AI", "AIG", "AINC", "AIZ", "AJG", "AKAM", "AL", "ALAB", "ALB", "ALC", "ALGN", "ALIT", "ALK", "ALKS",
                "ALL", "ALLE", "ALLY", "ALNY", "ALPN", "ALSN", "ALT", "ALTM", "ALUR", "ALV", "AMAT", "AMC", "AMCR",
                "AMD", "AME", "AMGN", "AMH", "AMLP", "AMN", "AMP", "AMT", "AMZN", "AN", "ANET", "ANF", "ANSS", "AON",
                "AOS", "APA", "APD", "APG", "APGE", "APH", "APLS", "APO", "APP", "APPF", "APTV", "AR", "ARCB", "ARCC",
                "ARDX", "ARE", "ARES", "ARKB", "ARKG", "ARKK", "ARM", "ARMK", "AROC", "ARRY", "AS", "ASML", "ASO",
                "ASX", "ATI", "ATKR", "ATMU", "ATO", "AU", "AUB", "AVB", "AVGO", "AVTR", "AVY", "AWI", "AWK", "AXNX",
                "AXON", "AXP", "AXS", "AXTA", "AYI", "AZEK", "AZN", "AZO", "BA", "BABA", "BAC", "BAH", "BALL", "BAM",
                "BAX", "BB", "BBD", "BBIO", "BBJP", "BBWI", "BBY", "BC", "BCE", "BCS", "BDX", "BE", "BEAM", "BECN",
                "BEKE", "BEN", "BERY", "BF.B", "BG", "BHC", "BHP", "BHVN", "BIDU", "BIIB", "BIL", "BILI", "BILL",
                "BIRK", "BITB", "BITF", "BITI", "BITO", "BITX", "BJ", "BK", "BKLN", "BKNG", "BKR", "BL", "BLD", "BLDP",
                "BLDR", "BLK", "BLMN", "BMRN", "BMY", "BN", "BND", "BNDX", "BNS", "BOIL", "BOOT", "BOX", "BP", "BPMC",
                "BR", "BRBR", "BRK.A", "BRK.B", "BRO", "BROG", "BROS", "BRX", "BRZE", "BSX", "BSY", "BTE", "BTG", "BTI",
                "BTU", "BUD", "BURL", "BWA", "BWXT", "BX", "BXP", "BYD", "BYON", "BZ", "C", "CADL", "CAG", "CAH",
                "CALF", "CALM", "CAR", "CARR", "CART", "CASY", "CAT", "CAVA", "CB", "CBOE", "CBRE", "CBRL", "CC",
                "CCCS", "CCEP", "CCI", "CCJ", "CCK", "CCL", "CDE", "CDNS", "CDW", "CE", "CEG", "CELH", "CERE", "CF",
                "CFG", "CFLT", "CG", "CGC", "CHD", "CHDN", "CHH", "CHK", "CHKP", "CHRD", "CHRW", "CHTR", "CHWY", "CHX",
                "CI", "CIEN", "CINF", "CIVI", "CL", "CLDX", "CLF", "CLS", "CLSK", "CLX", "CM", "CMA", "CMC", "CMCSA",
                "CME", "CMG", "CMI", "CMS", "CNC", "CNHI", "CNI", "CNK", "CNM", "CNP", "CNQ", "CNX", "CNXC", "COF",
                "COHR", "COIN", "COLD", "CONL", "COO", "COP", "COPX", "COR", "COST", "COTY", "COWZ", "CP", "CPAY",
                "CPB", "CPG", "CPNG", "CPRT", "CPT", "CRBG", "CRC", "CRH", "CRI", "CRL", "CRM", "CRNX", "CROX", "CRS",
                "CRSP", "CRWD", "CSCO", "CSGP", "CSL", "CSX", "CTAS", "CTLT", "CTRA", "CTSH", "CTVA", "CUBE", "CVE",
                "CVNA", "CVS", "CVX", "CX", "CXAI", "CXM", "CYBR", "CYTK", "CZOO", "CZR", "D", "DAL", "DAR", "DASH",
                "DAY", "DB", "DBX", "DD", "DDOG", "DE", "DECK", "DELL", "DEO", "DFS", "DG", "DGRO", "DGX", "DHI", "DHR",
                "DIA", "DINO", "DIS", "DJT", "DK", "DKNG", "DKS", "DLR", "DLTR", "DNB", "DNUT", "DOC", "DOCS", "DOCU",
                "DOV", "DOW", "DOX", "DPST", "DPZ", "DRI", "DT", "DTE", "DTM", "DUK", "DUOL", "DUST", "DV", "DVA",
                "DVN", "DXCM", "DXJ", "DXYZ", "EA", "EAT", "EBAY", "EC", "ECL", "ED", "EDR", "EDU", "EDV", "EEM", "EFA",
                "EFV", "EFX", "EG", "EGO", "EH", "EHC", "EIX", "EL", "ELAN", "ELF", "ELS", "ELV", "EMB", "EME", "EMN",
                "EMR", "EMXC", "ENB", "ENPH", "ENTG", "EOG", "EPAM", "EPD", "EQH", "EQIX", "EQNR", "EQR", "EQT", "ERF",
                "ERIC", "ERJ", "ES", "ESPR", "ESTC", "ET", "ETN", "ETR", "ETRN", "ETSY", "EVRG", "EW", "EWBC", "EWG",
                "EWJ", "EWT", "EWU", "EWW", "EWY", "EWZ", "EXAS", "EXC", "EXEL", "EXLS", "EXP", "EXPD", "EXPE", "EXR",
                "EZU", "F", "FAF", "FANG", "FAS", "FAST", "FAZ", "FBIN", "FBTC", "FCX", "FDMT", "FDS", "FDX", "FE",
                "FERG", "FEZ", "FFIV", "FHN", "FI", "FIS", "FITB", "FIVE", "FIVN", "FIX", "FL", "FLEX", "FLR", "FLS",
                "FMC", "FMX", "FN", "FND", "FNF", "FNGD", "FNGU", "FNV", "FOUR", "FOX", "FOXA", "FOXF", "FRO", "FROG",
                "FRPT", "FRSH", "FSK", "FSLR", "FSM", "FTAI", "FTI", "FTNT", "FTV", "FUSN", "FUTU", "FWONK", "FXI", "G",
                "GBTC", "GBX", "GCT", "GCTS", "GD", "GDDY", "GDX", "GDXJ", "GE", "GEHC", "GEN", "GEO", "GES", "GEV",
                "GFI", "GFS", "GGB", "GGG", "GH", "GILD", "GIS", "GKOS", "GL", "GLBE", "GLD", "GLDM", "GLOB", "GLPI",
                "GLW", "GM", "GME", "GMED", "GNRC", "GNTX", "GOEV", "GOLD", "GOOG", "GOOGL", "GOVT", "GPC", "GPK",
                "GPN", "GPS", "GRAB", "GRMN", "GS", "GSK", "GTES", "GTLB", "GTLS", "GWRE", "GWW", "GXO", "H", "HAL",
                "HALO", "HAS", "HBAN", "HBI", "HCA", "HCC", "HCP", "HD", "HDB", "HEI", "HES", "HIG", "HII", "HIMS",
                "HL", "HLF", "HLN", "HLT", "HMY", "HOG", "HOLO", "HOLX", "HON", "HOOD", "HPE", "HPQ", "HQY", "HRL",
                "HSBC", "HSIC", "HST", "HSY", "HTHT", "HTZ", "HUBB", "HUBC", "HUBS", "HUM", "HUN", "HWM", "HXL", "HYG",
                "HYLB", "IAG", "IAU", "IBB", "IBIT", "IBKR", "IBM", "IBN", "IBP", "ICE", "ICLN", "ICLR", "IDCC", "IDXX",
                "IEF", "IEFA", "IEMG", "IEUR", "IFF", "IGLB", "IGV", "IHI", "IJH", "IJR", "ILMN", "INCY", "INDA",
                "INFA", "INFN", "INFY", "ING", "INSM", "INSP", "INTC", "INTU", "INVH", "IONS", "IOT", "IOVA", "IP",
                "IPG", "IPGP", "IQV", "IR", "IRBT", "IRDM", "IREN", "IRM", "IRON", "IRTC", "ISRG", "IT", "ITB", "ITOT",
                "ITRI", "ITT", "ITUB", "ITW", "IVE", "IVV", "IVW", "IVZ", "IWD", "IWF", "IWM", "IWN", "IWP", "IYR", "J",
                "JAZZ", "JBHT", "JBL", "JBLU", "JCI", "JD", "JDST", "JEF", "JEPI", "JEPQ", "JETS", "JHG", "JKHY", "JNJ",
                "JNK", "JNPR", "JNUG", "JPM", "JWN", "JXN", "K", "KBE", "KBH", "KBR", "KDP", "KEX", "KEY", "KEYS",
                "KGC", "KHC", "KIM", "KKR", "KLAC", "KMB", "KMI", "KMX", "KNSL", "KNX", "KO", "KOLD", "KOS", "KR",
                "KRE", "KRUS", "KSS", "KULR", "KVUE", "KWEB", "L", "LABD", "LABU", "LBRDK", "LBRT", "LCID", "LDOS",
                "LEA", "LEGN", "LEN", "LEVI", "LH", "LHX", "LI", "LIN", "LITE", "LKQ", "LLY", "LMT", "LNC", "LNG",
                "LNT", "LNW", "LOW", "LPG", "LPLA", "LPX", "LQD", "LRCX", "LSCC", "LSXMA", "LSXMK", "LULU", "LUNR",
                "LUV", "LVS", "LW", "LYB", "LYFT", "LYV", "M", "MA", "MAA", "MANH", "MAR", "MARA", "MAS", "MASI", "MAT",
                "MBB", "MBLY", "MCD", "MCHI", "MCHP", "MCK", "MCO", "MDB", "MDC", "MDGL", "MDIA", "MDLZ", "MDT", "MDU",
                "MDY", "MEDP", "MELI", "MESO", "MET", "META", "MFC", "MGA", "MGM", "MGNX", "MGY", "MHK", "MIDD", "MKC",
                "MKSI", "MKTX", "MLM", "MMC", "MMM", "MMYT", "MNDY", "MNMD", "MNST", "MO", "MOD", "MODV", "MOH", "MOS",
                "MP", "MPC", "MPLX", "MPW", "MPWR", "MQ", "MRK", "MRNA", "MRO", "MRVL", "MS", "MSCI", "MSFT", "MSI",
                "MSM", "MSOS", "MSTR", "MTB", "MTCH", "MTDR", "MTG", "MTN", "MTSI", "MTUM", "MTZ", "MU", "MUB", "MUR",
                "MUSA", "MXL", "NARI", "NBIX", "NCLH", "NCNO", "NDAQ", "NE", "NEE", "NEM", "NET", "NFLX", "NI", "NICE",
                "NIO", "NKE", "NKLA", "NLY", "NNN", "NOC", "NOG", "NOK", "NOV", "NOVA", "NOW", "NRG", "NSC", "NTAP",
                "NTES", "NTNX", "NTR", "NTRA", "NTRS", "NU", "NUE", "NUGT", "NUVL", "NVDA", "NVDL", "NVDX", "NVEI",
                "NVO", "NVS", "NVST", "NVT", "NWSA", "NXE", "NXPI", "NXT", "NYCB", "NYT", "O", "OC", "ODD", "ODFL",
                "OGE", "OHI", "OKE", "OKTA", "OLLI", "OLN", "OMC", "OMF", "ON", "ONB", "ONON", "ONTO", "ORCL", "ORI",
                "ORLY", "OSK", "OTIS", "OUST", "OVV", "OWL", "OXM", "OXY", "PAA", "PAAS", "PAGP", "PANW", "PARA",
                "PATH", "PAVE", "PAYC", "PAYX", "PBA", "PBF", "PBR", "PBR.A", "PCAR", "PCG", "PCOR", "PCTY", "PCVX",
                "PDBC", "PDD", "PEG", "PEN", "PENN", "PEP", "PFE", "PFF", "PFG", "PFGC", "PG", "PGR", "PGX", "PH",
                "PHM", "PII", "PINS", "PK", "PKG", "PLAY", "PLD", "PLNT", "PLTR", "PLUG", "PM", "PNC", "PNR", "PNW",
                "PODD", "POOL", "POST", "PPG", "PPL", "PR", "PRGO", "PRGS", "PRIM", "PRKS", "PRU", "PSA", "PSLV", "PSN",
                "PSQ", "PSTG", "PSX", "PTC", "PTEN", "PTON", "PVH", "PWR", "PXD", "PYPL", "PZZA", "QCOM", "QDEL",
                "QGEN", "QID", "QLD", "QQQ", "QQQM", "QRVO", "QSR", "QUAL", "QYLD", "RACE", "RBA", "RBLX", "RCL", "RCM",
                "RDDT", "RDN", "REG", "REGN", "RELX", "REVG", "REXR", "RF", "RGEN", "RGLD", "RH", "RHI", "RHP", "RIG",
                "RIO", "RIOT", "RIVN", "RJF", "RL", "RMBS", "RMD", "RNA", "RNG", "RNR", "ROIV", "ROK", "ROKU", "ROL",
                "ROOT", "ROP", "ROST", "RPM", "RPRX", "RRC", "RRR", "RSG", "RSP", "RTO", "RTX", "RUM", "RUN", "RVTY",
                "RXRX", "RY", "RYAN", "S", "SAIA", "SAP", "SBAC", "SBLK", "SBSW", "SBUX", "SCCO", "SCHD", "SCHF",
                "SCHG", "SCHW", "SCHX", "SCI", "SCZ", "SDOW", "SDRL", "SDS", "SE", "SEDG", "SEE", "SFM", "SG", "SGOV",
                "SH", "SHAK", "SHEL", "SHOP", "SHV", "SHW", "SHY", "SIG", "SIL", "SILJ", "SIRI", "SITE", "SJM", "SJNK",
                "SKX", "SLB", "SLG", "SLV", "SM", "SMAR", "SMCI", "SMG", "SMH", "SMPL", "SMR", "SMTC", "SN", "SNA",
                "SNAP", "SNDL", "SNOW", "SNPS", "SNV", "SNX", "SNY", "SO", "SOFI", "SOLV", "SONY", "SOUN", "SOXL",
                "SOXS", "SOXX", "SPG", "SPGI", "SPLG", "SPLV", "SPMD", "SPOT", "SPR", "SPTL", "SPXL", "SPXS", "SPXU",
                "SPY", "SPYG", "SPYV", "SQ", "SQM", "SQQQ", "SRE", "SRPT", "SSNC", "SSO", "ST", "STAA", "STE", "STLA",
                "STLD", "STM", "STNE", "STNG", "STT", "STX", "STZ", "SU", "SUI", "SVIX", "SVXY", "SWAV", "SWK", "SWKS",
                "SWN", "SYF", "SYK", "SYY", "T", "TAN", "TAP", "TCBP", "TCOM", "TD", "TDG", "TDOC", "TDW", "TEAM",
                "TECH", "TECK", "TECL", "TECS", "TEL", "TER", "TEVA", "TFC", "TFX", "TGT", "THC", "THO", "TIP", "TJX",
                "TKO", "TLRY", "TLT", "TM", "TMDX", "TME", "TMF", "TMHC", "TMO", "TMUS", "TMV", "TNA", "TNDM", "TOL",
                "TOST", "TPR", "TPST", "TPX", "TQQQ", "TREX", "TRGP", "TRIP", "TRMB", "TRNO", "TROW", "TRP", "TRU",
                "TRV", "TS", "TSCO", "TSLA", "TSLL", "TSLQ", "TSLS", "TSLT", "TSLZ", "TSM", "TSN", "TT", "TTC", "TTD",
                "TTE", "TTEK", "TTWO", "TU", "TVGN", "TW", "TWLO", "TXG", "TXN", "TXRH", "TXT", "TYL", "TZA", "U",
                "UAA", "UAL", "UBER", "UBS", "UCO", "UDOW", "UDR", "UEC", "UGI", "UHS", "UL", "ULTA", "UMC", "UNG",
                "UNH", "UNM", "UNP", "UPRO", "UPS", "UPST", "URA", "URBN", "URI", "USB", "USFD", "USHY", "USMV", "USO",
                "UTHR", "UVIX", "UVXY", "V", "VAC", "VAL", "VALE", "VB", "VCIT", "VCLT", "VCSH", "VEA", "VEEV", "VERV",
                "VFC", "VGK", "VGLT", "VGT", "VICI", "VIG", "VIPS", "VIXY", "VKTX", "VLO", "VLTO", "VMC", "VNDA", "VNO",
                "VNQ", "VNT", "VO", "VOD", "VONG", "VOO", "VOYA", "VRNS", "VRSK", "VRSN", "VRT", "VRTX", "VSCO", "VSH",
                "VST", "VSTE", "VT", "VTEB", "VTI", "VTLE", "VTR", "VTRS", "VTV", "VTWO", "VUG", "VVPR", "VVV", "VWO",
                "VXUS", "VXX", "VYM", "VZ", "W", "WAB", "WAL", "WAT", "WBA", "WBD", "WBS", "WCC", "WCN", "WDAY", "WDC",
                "WEC", "WELL", "WEN", "WFC", "WFRD", "WGO", "WH", "WHR", "WING", "WLK", "WM", "WMB", "WMG", "WMS",
                "WMT", "WOLF", "WPC", "WPM", "WRB", "WRK", "WSC", "WSM", "WSO", "WST", "WTRG", "WTW", "WWD", "WY",
                "WYNN", "X", "XBI", "XEL", "XHB", "XLB", "XLC", "XLE", "XLF", "XLI", "XLK", "XLP", "XLRE", "XLU", "XLV",
                "XLY", "XME", "XOM", "XOP", "XP", "XPEV", "XPO", "XRAY", "XRT", "XRX", "XTIA", "XXII", "XYL", "YETI",
                "YINN", "YMM", "YPF", "YUM", "YUMC", "Z", "ZBH", "ZBRA", "ZETA", "ZG", "ZGN", "ZI", "ZIM", "ZION", "ZM",
                "ZS", "ZTO", "ZTS"
            };

            foreach (var symbol in symbols)
                Download(symbol, new DateTime(2024, 4, 5));

            Logger.AddMessage($"Finished!");
        }

        public static void Download(string symbol, DateTime date)
        {
            var zipFileName = string.Format(ZipFileNameTemplate, symbol, date.ToString("yyyyMMdd"),
                date.ToString("yyyy-MM-dd"));
            var folder = Path.GetDirectoryName(zipFileName);
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            var cnt = 0;
            var url = string.Format(UrlTemplate, symbol, date.ToString("yyyy-MM-dd"));
            var virtualFileEntries = new List<VirtualFileEntry>();
            if (!File.Exists(zipFileName))
            {
                while (url != null)
                {
                    Logger.AddMessage($"Downloading {cnt} data chunk into {Path.GetFileName(zipFileName)} for {symbol}/{date:yyyy-MM-dd}");

                    url = url + "&apiKey=" + PolygonCommon.GetApiKey2003();
                    // var filename = folder + $@"\PolygonTrades_{cnt:D2}_{date:yyyyMMdd}.json";
                    // Logger.AddMessage($"Downloading {cnt} data chunk into {Path.GetFileName(filename)} for {date:yyyy-MM-dd}");

                    var o = Data.Helpers.Download.DownloadToBytes(url, true);
                    if (o.Item2 != null)
                        throw new Exception(
                            $"PolygonTradesLoader: Error while download from {url}. Error message: {o.Item2.Message}");

                    // var entry = new VirtualFileEntry(Path.Combine(Path.GetFileName(folder), $@"PolygonTrades_{cnt:D2}_{date:yyyyMMdd}.json"), o.Item1);
                    var entry = new VirtualFileEntry($@"PolygonTrades_{cnt:D2}_{date:yyyyMMdd}.json", o.Item1);
                    virtualFileEntries.Add(entry);

                    var oo = ZipUtils.DeserializeBytes<cRoot>(entry.Content);
                    //File.WriteAllText(Path.Combine(folder, $@"PolygonTrades_{cnt:D2}_{date:yyyyMMdd}.json"), (byte[])o);

                    //var oo = JsonConvert.DeserializeObject<cRoot>((string)o);
                    if (oo.status != "OK")
                        throw new Exception($"Wrong status of response! Status is '{oo.status}'");

                    url = oo.next_url;
                    cnt++;
                }

                // Save to zip file
                ZipUtils.ZipVirtualFileEntries(zipFileName, virtualFileEntries);
            }
        }

        public class cRoot
        {
            public cResult[] results;
            public string status;
            // public string request_id;
            public string next_url;
        }

        [ProtoContract]
        public class cResult
        {
            [ProtoMember(3)]
            public int Price2;

            [ProtoMember(2)]
            public int Size2;

            [ProtoMember(1)]
            public ushort Seconds2;

            [ProtoMember(4)]
            public byte[] conditions;
            public byte exchange;
            public string id;
            public long participant_timestamp;
            public float price;
            public int sequence_number;
            public long sip_timestamp; // syncronized with sequence_number
            public double size;
            public byte type;
            public int trf_id;
            public long trf_timestamp;

            public DateTime ParticipantDt => TimeHelper.GetEstDateTimeFromUnixMilliseconds(participant_timestamp / 1000000);
            public DateTime SipDt => TimeHelper.GetEstDateTimeFromUnixMilliseconds(sip_timestamp / 1000000);
            public DateTime TrfDt => TimeHelper.GetEstDateTimeFromUnixMilliseconds(trf_timestamp / 1000000);
            public TimeSpan ParticipantTime => TimeHelper.GetEstDateTimeFromUnixMilliseconds(participant_timestamp / 1000000).TimeOfDay;
            public TimeSpan SipTime => TimeHelper.GetEstDateTimeFromUnixMilliseconds(sip_timestamp / 1000000).TimeOfDay;
            // public DateTime TrfDt => TimeHelper.GetEstDateTimeFromUnixMilliseconds(trf_timestamp / 1000000);
        }
    }
}
