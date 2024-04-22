using System;
using System.Globalization;

namespace Data.Tests.DBQ
{
    public static class C
    {
        public readonly static NumberFormatInfo fiNumberInvariant = CultureInfo.InvariantCulture.NumberFormat;
        internal const long cTicksInSecond = 10000 * 1000;
        public static DateTime minDateTime = new DateTime(1970, 1, 1); // MaxDate: 2038 year-int; 2106 year-uint
        public static long offsetDateTimeInSecs = minDateTime.Ticks / cTicksInSecond;
        internal const long startPriceFactor = 100000000;

        internal const long maxUInt32 = UInt32.MaxValue;
        internal const long maxUInt16 = UInt16.MaxValue;
        internal const long maxUInt8 = Byte.MaxValue;
        internal const long maxInt32 = Int32.MaxValue;
        internal const long minInt32 = Int32.MinValue;
        internal const long maxInt16 = Int16.MaxValue;
        internal const long minInt16 = Int16.MinValue;
        internal const long maxInt8 = SByte.MaxValue;
        internal const long minInt8 = SByte.MinValue;

        public enum DataType : byte { NotDefined = 0, Quote = 1, MbtTick = 2, MbtTickHttp = 3 };

        internal static long NOD(long n1, long n2)
        {//Наибольший общий делитель
            long k1 = (n1 < n2 ? n1 : n2);
            long k2 = (n1 < n2 ? n2 : n1);
            if (k1 == 0) return k2;
            long k3 = k2 % k1;
            while (k3 != 0)
            {
                k2 = k1;
                k1 = k3;
                k3 = k2 % k1;
            }
            return k1;
        }

        internal static int GetDP(float x)
        {
            string s = x.ToString(C.fiNumberInvariant);
            int k = s.IndexOf(C.fiNumberInvariant.NumberDecimalSeparator);
            if (k > 15)
            {
                throw new Exception("Program can not support " + k + " decimal places for number " + s + ". 15 is maximum number of dp.");
            }
            if (k == -1) return 0;
            else return s.Length - (k + 1);
        }


    }
}
