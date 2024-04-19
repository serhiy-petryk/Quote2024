using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Tests.DBQ
{
    public class DbqWriter
    {
        private delegate object dlgGetNextDataElement();

        IList _data;
        List<ArrayList> _arData;
        // C.DataType _dataType;
        bool _reverseFlag;
        int _endListNo;
        int _endElementNo;
        int _cntList;
        int _cntElement;
        int _records;
        internal double _open = 0;
        internal double _high = 0;
        internal double _low = 0;
        internal double _close = 0;
        internal long _volume = 0;
        internal DateTime _from;
        internal DateTime _to;

        internal byte[] GetBytes(IList data, string comment, DateTime timeStamp, out int records)
        {
            records = 0;
            this._records = 0;
            this._open = 0;
            this._high = 0;
            this._low = 0;
            this._close = 0;
            this._volume = 0;
            if (data.Count == 0) return new byte[0];
            this._data = data;
            // this._dataType = C.GetDataType(data[0]);
            this._reverseFlag = false;

            try
            {
                MbtTickHttp t1 = (MbtTickHttp)data[0];
                MbtTickHttp t2 = (MbtTickHttp)data[data.Count - 1];
                if (t2._date < t1._date)
                {
                    this._reverseFlag = true;
                }
                return GetBytes_MbtTickHttp(comment, timeStamp, out records, GetNextElement);
            }
            catch { throw; }
            finally
            {
                this._data = null;
            }
        }

        object GetNextElement()
        {
            int k = this._reverseFlag ? this._data.Count - 1 - this._records : this._records;
            if (k >= 0 && k < this._data.Count)
            {
                this._records++;
                SetOHLCV(this._data[k]);
                return this._data[k];
            }
            else return null;
        }

        void SetOHLCV(object o)
        {
            MbtTickHttp tick1 = (MbtTickHttp)o;
            if (tick1.IsInInterval(this._volume == 0))
            {
                if (this._volume == 0)
                {
                    this._open = tick1._price;
                    this._high = tick1._price;
                    this._low = tick1._price;
                    this._from = tick1._date;
                }
                else
                {
                    if (this._high < tick1._price) this._high = tick1._price;
                    if (this._low > tick1._price) this._low = tick1._price;
                }
                this._close = tick1._price;
                this._to = tick1._date;
                this._volume += tick1.Volume;
            }
        }

        static byte[] GetBytes_MbtTickHttp(string comment, DateTime timeStamp, out int records, dlgGetNextDataElement GetNextDataElement)
        {
            records = 0;
            MbtTickHttp tick = (MbtTickHttp)GetNextDataElement.Invoke();
            if (tick == null) return null; // no data

            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms, Encoding.UTF8))
                {
                    int dp = C.GetDP(tick._price);
                    //          long startPriceFactor = 100000000;
                    long kDP = 1;
                    for (int i = 0; i < dp; i++) kDP *= 10;
                    long lastClose = Convert.ToInt64(tick._price * kDP);
                    int bLastClose = (lastClose > 0xFFFF ? 1 : 0);
                    long lastTime = tick._date.Ticks / C.cTicksInSecond;
                    UInt32 u2 = Convert.ToUInt32(timeStamp.Ticks / C.cTicksInSecond - C.offsetDateTimeInSecs);

                    // Write to stream (timeStamp+comment+dp+startPrice+startTime)
                    if (bLastClose == 0) bw.Write((Convert.ToUInt16(lastClose)));
                    else bw.Write(Convert.ToUInt32(lastClose));
                    bw.Write(u2);// TimeStamp
                    bw.Write(Convert.ToUInt32(lastTime - C.offsetDateTimeInSecs));// lastTime
                    bw.Write(comment);// Comment
                                      // Define the first price factors
                    lastClose *= (C.startPriceFactor / kDP);
                    long priceFactor = C.NOD(lastClose, C.startPriceFactor);
                    //					long priceFactor = startPriceFactor;
                    lastClose /= priceFactor;
                    uint lastVolume = 0;
                    int lastOther = 0;
                    int lastRepeats = 0;
                    long timeSign = 0;

                    while (tick != null)
                    {
                        records++;
                        long secs = tick._date.Ticks / C.cTicksInSecond;
                        if (timeSign == 0 && secs != lastTime)
                        {
                            timeSign = (secs > lastTime ? 1 : -1);
                            if (timeSign == -1)
                            {
                                bw.Write((byte)0);
                                bw.Write((byte)0);
                            }
                        }
                        long newTime = secs;
                        secs = (secs - lastTime) * timeSign;// secs to write in dbq
                                                            // Prices =========
                        long price = Convert.ToInt64(tick._price * C.startPriceFactor);
                        long newPriceFactor = C.NOD(price, priceFactor);
                        if (newPriceFactor != priceFactor)
                        {
                            int i1 = Convert.ToInt32(priceFactor / newPriceFactor);
                            SaveFactor_MbtTickHttp(bw, i1);
                            priceFactor = newPriceFactor;
                            lastClose *= i1;
                        }
                        price /= priceFactor;

                        long newPrice = price;
                        price -= lastClose;

                        UInt32 volume = Convert.ToUInt32(tick._volume);
                        UInt16 other = Convert.ToUInt16((tick._type << 10) | tick._condition);

                        if (secs == 0 && price == 0 && volume == lastVolume && other == lastOther)
                        {
                            lastRepeats++;
                        }
                        else
                        {
                            if (lastRepeats > 0)
                            {
                                while (lastRepeats > 0)
                                {
                                    int recs = Math.Min(lastRepeats, 31);
                                    bw.Write(Convert.ToByte(recs));
                                    lastRepeats -= recs;
                                }
                            }
                            if (price == 0 && secs == 0 && other == lastOther && volume % 100 == 0 && volume <= 32 * 100)
                            {
                                byte bb1 = Convert.ToByte(((volume / 100) - 1) | 0x20);
                                lastVolume = volume;
                                bw.Write(bb1);
                            }
                            else
                            {
                                int bSecs = (secs == 0 ? 0 : (secs == 1 ? 1 : (secs <= C.maxUInt8 ? 2 : 3)));
                                int bPrice = (price == 0 ? 0 : (price <= C.maxInt8 && price >= C.minInt8 ? 1 : (price <= C.maxInt16 && price >= C.minInt16 ? 2 : 3)));
                                int bVolume = (volume == 100 ? 0 : (volume <= C.maxUInt8 ? 1 : ((volume % 100) == 0 && (volume / 100) <= C.maxUInt8 ? 2 : 3)));
                                int bOther = (other == 0 ? 1 : (other == 54 ? 2 : 3));
                                byte b1 = Convert.ToByte((bOther << 6) | (bSecs << 4) | (bPrice << 2) | bVolume);
                                bw.Write(b1);

                                switch (bSecs)
                                {
                                    case 0:// 0 second
                                    case 1:// 1 second
                                        break;
                                    case 2: bw.Write(Convert.ToByte(secs)); break;
                                    case 3: bw.Write(Convert.ToUInt32(secs)); break;
                                }

                                switch (bPrice)
                                {
                                    case 0: break;
                                    case 1: bw.Write(Convert.ToSByte(price)); break;
                                    case 2: bw.Write(Convert.ToInt16(price)); break;
                                    case 3: bw.Write(Convert.ToInt32(price)); break;
                                }

                                switch (bVolume)
                                {
                                    case 0: break;
                                    case 1: bw.Write(Convert.ToByte(volume)); break;
                                    case 2: bw.Write(Convert.ToByte(volume / 100)); break;
                                    case 3: bw.Write(Convert.ToUInt32(volume)); break;
                                }

                                if (bOther == 3) { bw.Write(other); };
                            }//if (price == 0 && secs == 0 && other == lastOther && volume % 100 == 0 && volume <= 32 * 100) {

                            lastClose = newPrice;
                            lastTime = newTime;
                            lastVolume = volume;
                            lastOther = other;
                        }// if (secs == 0 && price == 0 && volume == lastVolume && other == lastOther) {

                        tick = (MbtTickHttp)GetNextDataElement.Invoke();
                    }//while (tick != null) {

                    if (lastRepeats > 0)
                    {
                        while (lastRepeats > 0)
                        {
                            int recs = Math.Min(lastRepeats, 31);
                            bw.Write(Convert.ToByte(recs));
                            lastRepeats -= recs;
                        }
                    }
                    // Write terminator
                    bw.Write((byte)0);
                    bw.Write((byte)1);
                    // Write header
                    List<byte> header = new List<byte>();
                    int bRecs = (records > 0xFFFF ? 1 : 0);
                    uint len = Convert.ToUInt32(4 + 1 + 1 + (2 + bRecs * 2) + ms.Length);
                    byte firstByte = Convert.ToByte((bRecs << 6) | (bLastClose << 5) | dp);
                    // Len + type + firstByte + records + stream
                    header.AddRange(BitConverter.GetBytes(len));
                    header.Add((byte)C.DataType.MbtTickHttp);
                    header.Add(firstByte);
                    if (bRecs == 0) header.AddRange(BitConverter.GetBytes(Convert.ToUInt16(records)));
                    else header.AddRange(BitConverter.GetBytes(records));
                    // Merge arrays
                    header.AddRange(ms.ToArray());
                    bw.Close();
                    ms.Dispose();
                    return header.ToArray();

                }//using (BinaryWriter bw = new BinaryWriter(ms,Encoding.UTF8)) {
            }//using (MemoryStream ms = new MemoryStream()) {
        }

        static void SaveFactor_MbtTickHttp(BinaryWriter bw, int factor)
        {
            bw.Write((byte)0);
            if (factor < 128)
            {
                bw.Write(Convert.ToByte(factor));
            }
            else
            {
                byte b1 = Convert.ToByte(0x80 | ((factor >> 8) & 0x7f));
                byte b2 = Convert.ToByte((factor) & 0xff);
                bw.Write(b1);
                bw.Write(b2);
            }
        }
    }
}
