using System;
using System.Globalization;

namespace Data.Tests.DBQ
{
    public class MbtTickHttpOriginal
    {
        public int _no = 0;
        public readonly DateTime _date;
        public readonly float _price;
        public readonly long _volume;
        public readonly int _condition;
        public readonly int _type;

        public DateTime DateAndTime
        {
            get { return this._date; }
        }
        public double Price
        {
            get { return this._price; }
        }
        public long Volume
        {
            get { return this._volume; }
        }
        public int No
        {
            get { return this._no; }
        }
        public bool IsInInterval(bool isFirstQuote)
        {
            return this._condition == 0 || this._condition == 54 || this._condition == 191 ||
                   this._condition == 196 || (this._condition == 10 && isFirstQuote);
        }

        public MbtTickHttpOriginal(DateTime date, float price, long volume, int condition, int type)
        {
            this._date = date; this._price = Convert.ToSingle(Math.Round(price, 10)); this._volume = volume;
            this._condition = condition; this._type = type;
        }

        public string ToTimeString(CultureInfo ci)
        {
            return this._date.ToString("HH:mm:ss") + "\t" + _price.ToString(ci.NumberFormat) + "\t" +
                   this._volume.ToString() + "\t" + this._condition + "\t" + this._type;
        }
        public string ToString(CultureInfo ci)
        {
            return this._date.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + _price.ToString(ci.NumberFormat) + "\t" +
                   this._volume.ToString() + "\t" + this._condition + "\t" + this._type;
        }
        public override string ToString()
        {
            return this._date.ToString("yyyy-MM-dd HH:mm:ss") + "\t" +
                   _price.ToString(CultureInfo.InvariantCulture.NumberFormat) + "\t" + this._volume.ToString() + "\t" +
                   this._condition + "\t" + this._type;
        }
    }
}
