using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Data.Helpers;

namespace Data.Models
{
    public class LoaderItem: NotifyPropertyChangedAbstract
    {
        public enum ItemStatus { Disabled, None, Working, Done, Error }

        public static BindingList<LoaderItem> DataGridLoaderItems = new BindingList<LoaderItem>
        {
            // new LoaderItem {Name = "Polygon Minute Scan", Action = Actions.Polygon.PolygonMinuteScan.Start, Checked = false},
            // new LoaderItem {Name = "TradingView Screener", Action = Actions.TradingView.TvScreenerLoader.Start, Checked = false},
            // new LoaderItem {Name = "Nasdaq Stock/Etf Screeners", Action = Actions.Nasdaq.NasdaqScreenerLoader.Start, Checked = false},
            // new LoaderItem {Name = "Nasdaq Stock Screeners from Github", Action = Actions.Nasdaq.NasdaqScreenerGithubLoader.Start, Checked = false},
            // new LoaderItem {Name = "MorningStar Screener", Action = Actions.MorningStar.MorningStarScreenerLoader.Start, Checked = false},
            // new LoaderItem {Name = "Chartmill Screener", Action = Actions.Chartmill.ChartmillScreenerLoader.Start, Checked = false},
            new LoaderItem {Name = "Yahoo sectors", Action = Actions.Yahoo.YahooSectorLoader.Start, Checked = false},
            new LoaderItem {Name = "Index components from Wikipedia", Action = Actions.Wikipedia.WikipediaIndexLoader.Start, Checked = false},
            new LoaderItem {Name = "Eoddata Symbols", Action = Actions.Eoddata.EoddataSymbolsLoader.Start, Checked = false},
            // new LoaderItem {Name = "Yahoo Profiles", Action=Actions.Yahoo.YahooProfileLoader.Start},
            new LoaderItem {Name = "Yahoo Indices & Update Trading days", Action = Actions.Yahoo.YahooIndicesLoader.Start, Checked = false},
            new LoaderItem {Name = "Polygon Symbols (~4 minutes)", Action = Actions.Polygon.PolygonSymbolsLoader.Start, Checked = false},
            new LoaderItem {Name = "Polygon Daily Quotes", Action = Actions.Polygon.PolygonDailyLoader.Start, Checked = false},
            new LoaderItem {Name = "Synchronize Polygon Day and Symbols (~4 minutes)", Action = Actions.Polygon.PolygonSynchronizeDayAndSymbols.Start, Checked = false},
            new LoaderItem {Name = "Eoddata Daily Quotes", Action = Actions.Eoddata.EoddataDailyLoader.Start, Checked = false},
            new LoaderItem {Name = "Eoddata Splits", Action = Actions.Eoddata.EoddataSplitsLoader.Start, Checked = false},
            new LoaderItem {Name = "Investing Splits", Action = Actions.Investing.InvestingSplitsLoader.Start, Checked = false},
            new LoaderItem {Name = "StockAnalysis Corporate Actions", Action = Actions.StockAnalysis.StockAnalysisActions.Start, Checked = false},
            new LoaderItem {Name = "StockAnalysis IPOs", Action = Actions.StockAnalysis.StockAnalysisIPOs.Start, Checked=false},
            new LoaderItem {Name = "Refresh Split data", Action = Models.SplitModel.RefreshSplitData, Checked = false},
            new LoaderItem {Name = "Polygon Minute Quotes (+ one week of last loading)", Action = Actions.Polygon.PolygonMinuteLoader.Start, Checked = false},
            new LoaderItem {Name = "PolygonMinuteLog save to database", Action = Actions.Polygon.PolygonMinuteLogSaveToDb.Start, Checked = false},
            // new LoaderItem {Name = "Polygon Minute Quotes (date range)", Action = Actions.Polygon.PolygonMinuteLoader.StartWithDateRange, Checked = false},*/
            new LoaderItem {Name = "Yahoo Minute Quotes", Action = Actions.Yahoo.YahooMinuteLoader.Start, Checked = false},
        };

        public static System.Drawing.Image GetAnimatedImage() => GetImage(ItemStatus.Working);

        private static Dictionary<string, Bitmap> _imgResources;
        private static string[] _itemStatusImageName = new[] {"Blank", "Blank", "Wait", "Done", "Error"};

        private static void TestLogEvent()
        {
            Logger.AddMessage("Started");
            Thread.Sleep(1200);
            Logger.AddMessage("Finished");
        }

        private static Bitmap GetImage(ItemStatus status)
        {
            if (_imgResources == null)
            {
                _imgResources=new Dictionary<string, Bitmap>();
                var asm = System.Reflection.Assembly.GetExecutingAssembly();
                var rm = new System.Resources.ResourceManager("Data.Images", asm);
                foreach (var o in rm.GetResourceSet(CultureInfo.InvariantCulture, true, false))
                {
                    if (o is DictionaryEntry de && de.Key is string key && de.Value is Bitmap value)
                        _imgResources.Add(key, value);
                }
            }

            return _imgResources[_itemStatusImageName[(int)status]];
        }

        private bool _checked = true;
        public bool Checked
        {
            get => _checked;
            set
            {
                _checked = value;
                OnPropertiesChanged(nameof(Checked));
            }
        }

        public System.Drawing.Bitmap Image => GetImage(Status);

        public DateTime? Started { get; private set; }
        private DateTime? _finished;

        public long? Duration => Started.HasValue && _finished.HasValue
            ? Convert.ToInt64((_finished.Value - Started.Value).TotalSeconds)
            : (Started.HasValue ? Convert.ToInt64((DateTime.Now - Started.Value).TotalSeconds) : (long?) null);

        public string Name { get; private set; }
        public Action Action = TestLogEvent;

        public ItemStatus Status { get; set; } = ItemStatus.None;

        public void Reset()
        {
            Started = null;
            _finished = null;
            Status = ItemStatus.None;
            UpdateUI();
        }
        public async Task Start()
        {
            var t1 = Data.Helpers.CsUtils.MemoryUsedInBytes;
            Debug.Print($"Memory before {Name} {t1:N0} bytes");
            Started = DateTime.Now;
            _finished = null;
            Status = ItemStatus.Working;
            UpdateUI();

            await Task.Factory.StartNew(() => Action?.Invoke());

            _finished = DateTime.Now;
            Checked = false;
            Status = ItemStatus.Done;
            UpdateUI();
            t1 = Data.Helpers.CsUtils.MemoryUsedInBytes;
            Debug.Print($"Memory after {Name} {t1:N0} bytes");
        }

        public override void UpdateUI()
        {
            OnPropertiesChanged(nameof(Started), nameof(ItemStatus), nameof(Duration), nameof(Image));
            // OnPropertiesChanged(nameof(Started));
        }
    }
}
