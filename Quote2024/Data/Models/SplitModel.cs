using System;
using System.Globalization;
using Data.Helpers;

namespace Data.Models
{
    public class SplitModel
    {
        public static void RefreshSplitData()
        {
            DbUtils.RunProcedure("dbQ2023Others..pRefreshSplits");
            Logger.AddMessage($"!Run RefreshSplitData");
        }

        public string Key => Symbol + Date.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
        public string EoddataKey => Exchange + Symbol + Date.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
        public string Exchange;
        public string Symbol;
        public DateTime Date;
        public string Name;
        public string Ratio;
        public string Title;
        public float K;
        public DateTime TimeStamp;

        // Investing.com
        public SplitModel(string symbol, DateTime date, string name, string ratio, string title, DateTime timeStamp)
        {
            Symbol = symbol;
            Date = date;
            Name = name;
            Ratio = ratio;
            Title = title;
            TimeStamp = timeStamp;
            SetK();
        }

        // Eoddata
        public SplitModel(string exchange, string symbol, DateTime date, string ratio, DateTime timeStamp)
        {
            Exchange = exchange;
            Symbol = symbol;
            Date = date;
            Ratio = ratio;
            TimeStamp = timeStamp;
            SetK();
        }

        // StockSplitHistory
        public SplitModel(string symbol, DateTime date, string ratio, DateTime timeStamp)
        {
            Symbol = symbol;
            Date = date;
            Ratio = ratio;
            TimeStamp = timeStamp;
            SetK();
        }

        private void SetK()
        {
            var ss = Ratio.Split(':');
            K = float.Parse(ss[0], CultureInfo.InvariantCulture);
            K /= float.Parse(ss[1], CultureInfo.InvariantCulture);
        }

        public override string ToString() => $"{Symbol}\t{Date:yyyy-MM-dd}\t{Ratio}";
    }
}
