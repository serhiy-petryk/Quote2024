using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests.TradesCompressor
{
    class PolygonTradesLoader
    {
        public class cRoot
        {
            public cResult[] results;
            public string status;
            // public string request_id;
            public string next_url;
        }

        public class cResult
        {
            public int Price2;

            public int Size2;

            public ushort Seconds2;

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
