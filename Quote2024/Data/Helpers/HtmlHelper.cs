using System;
using System.IO;

namespace Data.Helpers
{
    public static class HtmlHelper
    {
        public static void TestFile()
        {
            var filename2 = @"E:\Quote\WebData\Splits\StockSplitHistory\StockSplitHistory_20230304\sshAMST.html";
            var filename3 = @"E:\Quote\WebData\Symbols\Quantumonline\Profiles\Profiles\pA.html";
            // var filename = @"E:\Quote\WebData\Symbols\MorningStar\Data\MSProfiles_20240615\MSProfile_arcx_esba.html";
            var filename = @"E:\Quote\WebData\Symbols\Yahoo\Profile\WA_Data\ZWS_20240226082342.html";
            var s = RemoveUselessTags(File.ReadAllText(filename));
        }

        public static void ProcessFolder()
        {
            var sourceFolder = @"E:\Quote\WebData\Symbols\Yahoo\Profile\WA_Data";
            var destinationFolder = @"E:\Quote\WebData\Symbols\Yahoo\Profile\WA1";

            var files = Directory.GetFiles(sourceFolder, "A*.html");
            foreach (var file in files)
            {
                File.Delete(file);
                continue;
                /*var newContext = RemoveUselessTags(File.ReadAllText(file));
                var newFileName = Path.Combine(destinationFolder, Path.GetFileName(file));
                File.WriteAllText(newFileName, newContext);

                var lastWriteTime = File.GetLastWriteTime(file);
                File.SetLastWriteTime(newFileName, lastWriteTime);
                var lastAccessTime = File.GetLastAccessTime(file);
                File.SetLastWriteTime(newFileName, lastAccessTime);
                var creationTime = File.GetCreationTime(file);
                File.SetCreationTime(newFileName, creationTime);*/
            }
        }

        public static string RemoveUselessTags(string htmlContent)
        {
            var len = htmlContent.Length;
            var i1 = htmlContent.IndexOf("<body ", StringComparison.InvariantCulture);
            var i2 = htmlContent.LastIndexOf("<body ", StringComparison.InvariantCulture);
            if (i1 == -1 && i2 == -1)
            {
                i1 = htmlContent.IndexOf("<body>", StringComparison.InvariantCulture);
                i2 = htmlContent.LastIndexOf("<body>", StringComparison.InvariantCulture);
            }
            var i3 = htmlContent.IndexOf("</body>", StringComparison.InvariantCulture);
            var i4 = htmlContent.LastIndexOf("</body>", StringComparison.InvariantCulture);
            if (i1 != -1 && i3 != -1 && i1 == i2 && i3 == i4)
            {
                htmlContent = htmlContent.Substring(i1, i3 - i1 + 7);
            }

            i1 = htmlContent.IndexOf("</script>", StringComparison.InvariantCulture);
            while (i1 != -1)
            {
                i2 = htmlContent.LastIndexOf("<script", i1, StringComparison.InvariantCulture);
                if (i2 == -1)
                    break;
                htmlContent = htmlContent.Substring(0, i2) + htmlContent.Substring(i1 + 9);
                i1 = htmlContent.IndexOf("</script>", StringComparison.InvariantCulture);
            }

            i1 = htmlContent.IndexOf("</style>", StringComparison.InvariantCulture);
            while (i1 != -1)
            {
                i2 = htmlContent.LastIndexOf("<style", i1, StringComparison.InvariantCulture);
                if (i2 == -1)
                    break;
                htmlContent = htmlContent.Substring(0, i2) + htmlContent.Substring(i1 + 8);
                i1 = htmlContent.IndexOf("</style>", StringComparison.InvariantCulture);
            }

            i1 = htmlContent.IndexOf("</svg>", StringComparison.InvariantCulture);
            while (i1 != -1)
            {
                i2 = htmlContent.LastIndexOf("<svg", i1, StringComparison.InvariantCulture);
                if (i2 == -1)
                    break;
                htmlContent = htmlContent.Substring(0, i2) + htmlContent.Substring(i1 + 6);
                i1 = htmlContent.IndexOf("</svg>", StringComparison.InvariantCulture);
            }

            /* not need: file size is less only ~5%
            i1 = htmlContent.IndexOf("-->", StringComparison.InvariantCulture);
            while (i1 != -1)
            {
                i2 = htmlContent.LastIndexOf("<!--", i1, StringComparison.InvariantCulture);
                if (i2 == -1)
                    break;
                htmlContent = htmlContent.Substring(0, i2) + htmlContent.Substring(i1 + 3);
                i1 = htmlContent.IndexOf("-->", StringComparison.InvariantCulture);
            }

            i1 = htmlContent.IndexOf(" style=\"", StringComparison.InvariantCulture);
            while (i1 != -1)
            {
                i2 = htmlContent.IndexOf("\"", i1+8, StringComparison.InvariantCulture);
                if (i2 == -1)
                    break;
                htmlContent = htmlContent.Substring(0, i1) + htmlContent.Substring(i2 + 1);
                i1 = htmlContent.IndexOf(" style=\"", StringComparison.InvariantCulture);
            }*/

            var len2 = htmlContent.Length;
            return htmlContent;
        }
    }
}
