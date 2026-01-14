using System;
using System.Collections.Generic;
using System.Globalization;

namespace Data.Models
{
    public class ActionStockAnalysis
    {
        public enum Action { None, Listed, Delisted, Split, Change, Spinoff, Bankruptcy, Acquisition };

        private static Dictionary<string, Action> _keys = new Dictionary<string, Action>
            {
                {" ticker symbol changed to ", Action.Change}, {" Listed - ", Action.Listed},
                {" was listed", Action.Listed}, {" Delisted - ", Action.Delisted}, {" was delisted", Action.Delisted},
                {" stock split: ", Action.Split}, {" spun off from ", Action.Spinoff},
                {" was acquired by ", Action.Acquisition}, {" was liquidated due to bankruptcy.", Action.Bankruptcy}
            };

        public DateTime Date;
        public Action Type;
        public string Symbol;
        public string OtherSymbolOrName = "";
        public string Name;
        public string Description;
        public string SplitRatio;
        public float? SplitK;
        public DateTime TimeStamp;
        public bool IsBad;
        public Action DescriptionAction;

        public ActionStockAnalysis(Actions.StockAnalysis.StockAnalysisActions.cItem item, DateTime timestamp)
        {
            TimeStamp = timestamp;
            Date = item.pDate;
            Name = item.pName;
            Description = item.text;

            SetProperties(item.pSymbol, item.type);
        }

        public ActionStockAnalysis(string row, DateTime timestamp)
        {
            TimeStamp = timestamp;

            var cells = row.Split(new[] { "</td>" }, StringSplitOptions.RemoveEmptyEntries);
            if (cells.Length != 3 && cells.Length != 4)
                throw new Exception("Check StockAnalysis.WebArchiveActions parser");

            Date = DateTime.Parse(GetCellContent(cells[0]), CultureInfo.InvariantCulture);
            var symbol = cells.Length == 4 ? GetCellContent(cells[1]) : null;
            var action = GetCellContent(cells[cells.Length - 2]);
            Description = GetCellContent(cells[cells.Length - 1]);

            SetProperties(symbol, action);

            // ====  local methods
            string GetCellContent(string cell)
            {
                cell = cell.Replace("</a>", "");
                var i1 = cell.IndexOf("<a", StringComparison.InvariantCultureIgnoreCase);
                while (i1 != -1)
                {
                    var i2 = cell.IndexOf(">", i1 + 2);
                    cell = cell.Substring(0, i1) + cell.Substring(i2 + 1);
                    i1 = cell.IndexOf("<a", StringComparison.InvariantCultureIgnoreCase);
                }

                i1 = cell.LastIndexOf(">", StringComparison.InvariantCultureIgnoreCase);
                return System.Net.WebUtility.HtmlDecode(cell.Substring(i1 + 1).Trim());
            }
        }

        private void SetProperties(string symbol, string action)
        {
            if (Description == "")
            {
                Description = null;
                DescriptionAction = Action.None;
            }
            else
            {
                foreach (var kvp in _keys)
                    if (Description.Contains(kvp.Key))
                    {
                        DescriptionAction = kvp.Value;
                        break;
                    }
            }

            if (action == "Symbol Change")
            {
                Type = Action.Change;
                OtherSymbolOrName = GetFirstWord(Description);
                Symbol = GetLastWord(Description);
            }
            else if (action == "Listed")
            {
                Type = Action.Listed;
                if (Description.EndsWith(" was listed"))
                {
                    Symbol = symbol;
                    if (string.IsNullOrWhiteSpace(Name))
                        Name = Description.Substring(0, Description.Length - 11).Trim();
                }
                else if (Description.Contains(" Listed - "))
                {
                    Symbol = GetFirstWord(Description);
                    if (string.IsNullOrWhiteSpace(Name))
                    {
                        var ss = Description.Split(new[] {" Listed - "}, StringSplitOptions.None);
                        if (!string.IsNullOrWhiteSpace(ss[1]))
                            Name = ss[1].Trim();
                    }
                }
                else
                    throw new Exception("Check Models.ActionStockAnalysis parser");

            }
            else if (action == "Delisted")
            {
                Type = Action.Delisted;
                if (Description.EndsWith(" was delisted"))
                {
                    Symbol = symbol;
                    if (string.IsNullOrWhiteSpace(Name))
                        Name = Description.Substring(0, Description.Length - 13).Trim();
                }
                else if (Description.Contains(" Delisted - "))
                {
                    Symbol = GetFirstWord(Description);
                    if (string.IsNullOrWhiteSpace(Name))
                    {
                        var ss = Description.Split(new[] { " Delisted - " }, StringSplitOptions.None);
                        if (!string.IsNullOrWhiteSpace(ss[1]))
                            Name = ss[1].Trim();
                    }
                }
                else
                    throw new Exception("Check Models.ActionStockAnalysis parser");
            }
            else if (action == "Stock Split")
            {
                Type = Action.Split;
                if (DescriptionAction == Action.Split)
                {
                    Symbol = GetFirstWord(Description);
                    var ss = Description.Split(new[] { " stock split: ", " for " }, StringSplitOptions.None);
                    SplitRatio = ss[1] + ":" + ss[2];
                    var d1 = float.Parse(ss[1], CultureInfo.InvariantCulture);
                    var d2 = float.Parse(ss[2], CultureInfo.InvariantCulture);
                    SplitK = d1 / d2;
                }
                else
                    Symbol = symbol;
            }
            else if (action == "Spinoff")
            {
                Type = Action.Spinoff;
                OtherSymbolOrName = GetFirstWord(Description);
                Symbol = GetLastWord(Description);
            }
            else if (action == "Acquisition")
            {
                Type = Action.Acquisition;
                Symbol = GetFirstWord(Description);
                var i1 = Description.LastIndexOf(" was acquired by ", StringComparison.InvariantCulture);
                OtherSymbolOrName = Description.Substring(i1 + 17);
                if (OtherSymbolOrName.EndsWith(".") && !OtherSymbolOrName.EndsWith(".."))
                    OtherSymbolOrName = OtherSymbolOrName.Substring(0, OtherSymbolOrName.Length - 1);
            }
            else if (action == "Bankruptcy")
            {
                Type = Action.Bankruptcy;
                Symbol = GetFirstWord(Description);
            }
            else
            {
                throw new Exception("Check action");
            }

            if (Type != DescriptionAction)
            {
                IsBad = true;
                return;
            }

            if (string.IsNullOrWhiteSpace(Symbol))
                throw new Exception("Check symbol");
            if (!string.IsNullOrWhiteSpace(symbol) && !string.Equals(symbol, Symbol) && !symbol.StartsWith(@"!otc/SVUHF"))
            {
                if (Type == Action.Split)
                {
                    IsBad = true;
                    return;
                }

                throw new Exception("Check symbol");
            }

            // =======  Local methods
            string GetFirstWord(string s)
            {
                var i1 = s.IndexOf(' ');
                return s.Substring(0, i1).Trim();
            }
            string GetLastWord(string s)
            {
                var i1 = s.LastIndexOf(' ');
                return s.Substring(i1).Trim();
            }
        }
    }
}
