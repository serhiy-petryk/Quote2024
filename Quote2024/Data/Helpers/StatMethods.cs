using System;
using System.Diagnostics;
using System.Globalization;

namespace Data.Helpers
{
    public static class StatMethods
    {
        private static float[] testData = new float[] { 50.25f, 50.675f, 50.7f, 50.66f, 50.45f, 50.48f, 50.5305f, 
            50.37f, 50.35f, 50.24f, 50.24f, 50.27f, 50.355f, 50.39f, 50.22f, 50.3f, 50.3f, 50.35f, 50.285f, 
            50.255f, 50.45f, 50.555f, 50.32f, 50.34f, 50.3984f, 50.49f, 50.47f, 50.35f, 50.3f, 50.14f, 49.905f,
            49.8914f, 49.645f, 49.7907f, 49.62f, 49.57f, 49.66f, 49.73f, 49.71f, 49.58f, 49.46f, 49.4f, 49.145f,
            49.14f, 49.2f, 49.44f, 49.2823f, 49.335f, 49.2849f, 49.285f, 49.28f, 49.285f, 49.3f, 49.3f, 49.28f,
            49.3f, 49.35f, 49.33f, 49.34f, 49.24f, 49.18f, 49.24f, 49.19f, 49.325f, 49.32f, 49.27f, 49.34f,
            49.35f, 49.35f, 49.3f, 49.21f, 49.255f, 49.3f, 49.33f, 49.3299f, 49.28f, 49.32f, 49.2901f, 49.37f,
            49.43f, 49.445f, 49.5099f, 49.55f, 49.53f, 49.528f, 49.39f, 49.4401f, 49.495f, 49.51f, 49.55f,
            49.6099f, 49.56f, 49.59f, 49.52f, 49.497f, 49.3599f, 49.37f, 49.38f, 49.41f, 49.48f, 49.5f, 49.53f,
            49.57f, 49.66f, 49.73f, 49.76f, 49.74f, 49.71f, 49.765f, 49.82f, 49.82f, 49.89f, 49.87f, 49.885f,
            49.9f, 49.805f, 49.72f, 49.66f, 49.665f, 49.7f, 49.69f, 49.7f, 49.745f, 49.845f, 49.97f, 49.9972f,
            49.94f, 49.93f, 49.9099f, 49.91f, 49.8754f, 49.96f, 49.98f, 49.94f, 49.92f, 49.94f, 49.85f, 49.825f,
            49.8401f, 49.795f, 49.8f, 49.89f, 49.75f, 49.685f, 49.7f, 49.603f, 49.65f, 49.645f, 49.7f, 49.655f,
            49.66f, 49.52f, 49.47f, 49.4f, 49.28f, 49.22f, 49.18f, 49.2366f, 49.31f, 49.34f, 49.34f, 49.47f,
            49.3501f, 49.36f, 49.385f, 49.33f, 49.3f, 49.1802f, 49.19f, 49.13f, 49.235f, 49.35f, 49.375f,
            49.37f, 49.34f, 49.3899f, 49.365f, 49.425f, 49.4f, 49.41f, 49.49f, 49.55f, 49.51f, 49.5299f, 49.48f,
            49.46f, 49.49f, 49.485f, 49.3546f, 49.335f, 49.3001f, 49.28f, 49.23f, 49.17f, 49.24f, 49.2301f, 49.24f,
            49.25f, 49.2976f, 49.26f, 49.32f, 49.265f, 49.26f, 49.2f, 49.19f, 49.2f, 49.16f, 49.225f, 49.18f,
            49.215f, 49.23f, 49.15f, 49.14f, 49.04f, 49.04f, 49.06f, 49.05f, 49.03f, 49.01f, 49.015f, 49.015f,
            48.92f, 48.97f, 48.91f, 48.92f, 48.9328f, 49.03f, 49.02f, 48.89f, 48.915f, 48.94f, 49.02f, 48.965f,
            48.9751f, 48.98f, 49.015f, 49.0299f, 49.04f, 49.07f, 48.99f, 49.04f, 49.05f, 49.05f, 48.98f, 49.04f,
            49.08f, 49.04f, 49.02f, 49.01f, 48.94f, 48.93f, 48.96f, 49.025f, 49.07f, 49.05f, 49.06f, 49.05f,
            49.045f, 49.1f, 49.11f, 49.1f, 49.11f, 49.15f, 49.12f, 49.09f, 49.13f, 49.095f, 49.11f, 49.175f,
            49.1999f, 49.1599f, 49.1f, 49.1f, 49.1f, 49.07f, 49.1f, 49.12f, 49.1173f, 49.09f, 49.1f, 49.034f,
            49.04f, 49.06f, 49.05f, 49.0693f, 49.05f, 49.07f, 49.075f, 49.115f, 49.08f, 49.07f, 49.06f, 48.99f,
            48.95f, 49.0101f, 49.01f, 49f, 48.94f, 48.92f, 48.93f, 48.875f, 48.91f, 48.93f, 48.925f, 48.96f,
            49.06f, 49.095f, 49.13f, 49.135f, 49.2224f, 49.16f, 49.155f, 49.1839f, 49.1983f, 49.1f, 49.09f, 49.1058f, 49.1f, 49.1f, 49.12f, 49.125f, 49.13f, 49.12f, 49.12f, 49.087f, 49.02f, 49.03f, 49.02f, 49.045f, 49.045f, 49.03f, 49.06f, 49.04f, 49.015f, 49.0801f, 49.03f, 49.06f, 49.085f, 49.11f, 49.09f, 49.06f, 49.11f, 49.065f, 49.06f, 48.975f, 48.98f, 48.94f, 48.96f, 48.92f, 48.98f, 48.96f, 48.94f, 48.93f, 48.91f, 48.94f, 48.9f, 48.943f, 49.02f, 49f, 49.0101f, 48.93f, 48.905f, 48.92f, 48.87f, 48.905f, 48.92f, 48.87f, 48.8826f, 48.88f, 48.9f, 48.92f, 48.945f, 48.945f, 48.985f, 48.99f, 48.96f, 48.93f, 48.93f, 48.865f, 48.8685f, 48.98f, 49.045f, 49.0712f, 49.12f, 49.1186f, 49.07f, 49.065f, 49.1f, 49.075f, 49.13f };

        public static void Tests()
        {
            var aa = Wma(testData, 20);
            var aa2 = EMA(testData, 20);
            var aa3 = EMA2(testData, 20);
            for (var k = 0; k < aa.GetLength(0); k++)
            {
                var ff = aa[k];
                Debug.Print(ff.ToString(CultureInfo.CurrentCulture));
            }
        }

        public static float[] Ema2(float[] data, int period)
        {
            var result = new float[data.Length];
            var prevValue = 0f;
            for (var k1 = 0; k1 < data.Length; k1++)
            {
                var f = data[k1];
                if (k1 == 0)
                    result[k1] = f;
                else
                {
                    var n = Convert.ToSingle(Math.Min(k1, period));
                    var o = f / n + prevValue * (1f - 1f / n);
                    result[k1] = o;
                }
                prevValue = result[k1];
            }

            return result;
            /* // Expotential Moving average ' Second method by formula: EMA= pi/n + prevEma*(1-1/n)
             //      if (recs <= 1) return newValue; // changed at 2010-05-06
             if (double.IsNaN(prevValue)) return newValue;
             double n = Convert.ToDouble(Math.Min(recs, period));
             return newValue / n + prevValue * (1 - 1 / n);*/
        }

        public static float?[,] EMA2(float[] data, int period)
        {
            var result = new float?[data.Length, 2];

            var prevValue = (float?)null;
            var prevValue2 = (float?)null;
            for (var k1 = 0; k1 < data.Length; k1++)
            {
                var f = data[k1];
                if (float.IsNaN(f))
                {
                    result[k1, 0] = prevValue;
                    result[k1, 1] = prevValue2;
                    continue;
                }
                else if (!prevValue.HasValue)
                {
                    result[k1, 0] = f;
                    result[k1, 1] = prevValue2;
                }
                else
                {
                    var n = Convert.ToSingle(Math.Min(k1, period));
                    var o = f / n + prevValue * (1f - 1f / n);
                    result[k1, 0] = o;
                    result[k1, 1] = prevValue;
                }

                prevValue = result[k1, 0];
                prevValue2 = result[k1, 1];
            }

            return result;
            /* // Expotential Moving average ' Second method by formula: EMA= pi/n + prevEma*(1-1/n)
             //      if (recs <= 1) return newValue; // changed at 2010-05-06
             if (double.IsNaN(prevValue)) return newValue;
             double n = Convert.ToDouble(Math.Min(recs, period));
             return newValue / n + prevValue * (1 - 1 / n);*/
        }

        public static float[] Ema(float[] data, int period)
        {
            var result = new float[data.Length];
            var prevValue = 0f;
            for (var k1 = 0; k1 < data.Length; k1++)
            {
                var f = data[k1];
                if (k1 == 0)
                    result[k1] = f;
                else
                {
                    var o = prevValue + 2 / Convert.ToSingle(period + 1) * (f - prevValue);
                    result[k1] = o;
                }

                prevValue = result[k1];
            }

            return result;
            /*// Expotential Moving average
            // EMA = EMA(i-1) + 2/(n+1) * (pi - EMA(i-1))
            // Start point: == average (iQChart)
            //aEMA = prevEma + 2 / (n + 1) * (pi - prevEma)
            if (double.IsNaN(prevValue)) return newValue;
            return prevValue + 2 / Convert.ToDouble(period + 1) * (newValue - prevValue);*/
        }

        public static float?[,] EMA(float[] data, int period)
        {
            var result = new float?[data.Length, 2];

            var prevValue = (float?)null;
            var prevValue2 = (float?)null;
            for (var k1 = 0; k1 < data.Length; k1++)
            {
                var f = data[k1];
                if (float.IsNaN(f))
                {
                    result[k1, 0] = prevValue;
                    result[k1, 1] = prevValue2;
                    continue;
                }
                else if (!prevValue.HasValue)
                {
                    result[k1, 0] = f;
                    result[k1, 1] = prevValue2;
                }
                else
                {
                    var o = prevValue + 2 / Convert.ToSingle(period + 1) * (f - prevValue);
                    result[k1, 0] = o;
                    result[k1, 1] = prevValue;
                }

                prevValue = result[k1, 0];
                prevValue2 = result[k1, 1];
            }

            return result;
            /*// Expotential Moving average
            // EMA = EMA(i-1) + 2/(n+1) * (pi - EMA(i-1))
            // Start point: == average (iQChart)
            //aEMA = prevEma + 2 / (n + 1) * (pi - prevEma)
            if (double.IsNaN(prevValue)) return newValue;
            return prevValue + 2 / Convert.ToDouble(period + 1) * (newValue - prevValue);*/
        }

        public static float[] Wma(float[] data, int period)
        {
            var result = new float[data.Length];
            for (var k1 = 0; k1 < data.Length; k1++)
            {
                var cnt = 0;
                var rez = 0f;
                var rezK = 0f;
                for (var k2 = k1; k2 >= 0 && (cnt < period); k2--)
                {
                    var f = data[k2];
                    var fPeriods = Convert.ToSingle(period - cnt);
                    rez += f * fPeriods;
                    rezK += fPeriods;
                    cnt++;
                }

                result[k1] = rez / rezK;
            }
            return result;
            /*double rez = 0;
            double rezK = 0;
            int cnt = 0;
            for (int i = startNo; (i >= 0) && (cnt < period); i--)
            {
                double x = delItemType(data[i]);
                if (!double.IsNaN(x))
                {
                    rez += x * Convert.ToDouble(period - cnt);
                    rezK += Convert.ToDouble(period - cnt);
                }
                cnt++;
            }
            return (cnt == 0 ? double.NaN : rez / rezK);*/
        }

        public static float?[,] MAWeighted(float[] data, int period)
        {
            var result = new float?[data.Length, 2];
            var prevValue = (float?)null;
            var prevValue2 = (float?)null;
            for (var k1 = 0; k1 < data.Length; k1++)
            {
                var f = data[k1];
                if (float.IsNaN(f))
                {
                    result[k1, 0] = prevValue;
                    result[k1, 1] = prevValue2;
                    continue;
                }

                var cnt = 0;
                var rez = 0f;
                var rezK = 0f;
                for (var k2 = k1; k2 >= 0 && (cnt < period); k2--)
                {
                    f = data[k2];
                    if (!float.IsNaN(f))
                    {
                        var fPeriods = Convert.ToSingle(period - cnt);
                        rez += f * fPeriods;
                        rezK += fPeriods;
                        cnt++;
                    }
                }

                result[k1, 0] = cnt == 0 ? prevValue : rez / rezK;
                result[k1, 1] = cnt == 0 ? prevValue2 : prevValue;

                if (cnt > 0)
                {
                    prevValue2 = prevValue;
                    prevValue = result[k1, 0];
                }
            }
            return result;
            /*double rez = 0;
            double rezK = 0;
            int cnt = 0;
            for (int i = startNo; (i >= 0) && (cnt < period); i--)
            {
                double x = delItemType(data[i]);
                if (!double.IsNaN(x))
                {
                    rez += x * Convert.ToDouble(period - cnt);
                    rezK += Convert.ToDouble(period - cnt);
                }
                cnt++;
            }
            return (cnt == 0 ? double.NaN : rez / rezK);*/
        }


    }
}
