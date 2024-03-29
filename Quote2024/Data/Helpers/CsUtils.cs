﻿using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Data.Helpers
{
    public static class CsUtils
    {
        public static string OpenZipFileDialog(string folder) => OpenFileDialogGeneric(folder, @"zip files (*.zip)|*.zip");
        public static string OpenFileDialogGeneric(string folder, string filter)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.InitialDirectory = folder;
                ofd.RestoreDirectory = true;
                ofd.Multiselect = false;
                ofd.Filter = filter;
                if (ofd.ShowDialog() == DialogResult.OK)
                    return ofd.FileName;
                return null;
            }
        }

        public static string[] OpenFileDialogMultiselect(string folder, string filter, string title = null)
        {
            using (var ofd = new OpenFileDialog())
            {
                if (!string.IsNullOrWhiteSpace(title))
                    ofd.Title = title;
                ofd.InitialDirectory = folder;
                ofd.RestoreDirectory = true;
                ofd.Multiselect = true;
                ofd.Filter = filter;
                if (ofd.ShowDialog() == DialogResult.OK)
                    return ofd.FileNames;
                return null;
            }
        }

        public static string GetString(object o)
        {
            if (o is DateTime dt)
                return dt.ToString(dt.TimeOfDay == TimeSpan.Zero ? "yyyy-MM-dd" : "yyyy-MM-dd HH:mm");
            else if (o is TimeSpan ts)
                return ts.ToString("hh\\:mm");
            else if (Equals(o, null)) return null;
            return o.ToString();
        }

        public static string CurrentArchitecture = IntPtr.Size == 4 ? "x86" : "x64";

        public static bool IsInDesignMode => LicenseManager.UsageMode == LicenseUsageMode.Designtime;

        public static string[] GetApiKeys(string dataProvider)
        {
            const string filename = @"E:\Quote\WebData\ApiKeys.txt";
            var keys = File.ReadAllLines(filename)
                .Where(a => a.StartsWith(dataProvider, StringComparison.InvariantCultureIgnoreCase))
                .Select(a => a.Split('\t')[1].Trim()).ToArray();
            return keys;
        }

        public static int GetFileSizeInKB(string filename) => Convert.ToInt32(new FileInfo(filename).Length / 1024.0);

        public static DateTime RoundDown(this DateTime dt, TimeSpan d)
        { // see answer of redent84 in https://stackoverflow.com/questions/7029353/how-can-i-round-up-the-time-to-the-nearest-x-minutes
            var delta = dt.Ticks % d.Ticks;
            return new DateTime(dt.Ticks - delta, dt.Kind);
        }

        public static long MemoryUsedInBytes
        {
            get
            {
                // clear memory
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                return GC.GetTotalMemory(true);
            }
        }

    }
}
