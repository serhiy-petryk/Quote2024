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
            var filename = @"E:\Quote\WebData\Symbols\Yahoo\Profile\ok\ABNB_20230519031517.html";
            var s = RemoveUselessTags(File.ReadAllText(filename));
        }

        public static void ProcessFolder()
        {
            Logger.AddMessage($"Started");
            ProcessFolder(@"E:\Quote\WebData\Symbols\MorningStar\WA_Profile");
            Logger.AddMessage($"Finished");
        }

        private static void ProcessFolder(string sourceFolder)
        {
            var destinationFolder = sourceFolder +  @".Short";
            if (!Directory.Exists(destinationFolder))
                Directory.CreateDirectory(destinationFolder);

            var files = Directory.GetFiles(sourceFolder, "*.html");
            var count = 0;
            foreach (var file in files)
            {
                count++;
                if (count % 10 == 0)
                    Logger.AddMessage($"Parsed {count:N0} files from {files.Length:N0}. Last file: {Path.GetFileName(file)}");

                var newFileName = Path.Combine(destinationFolder, Path.GetFileName(file));
                if (!File.Exists(newFileName))
                {
                    var newContext = RemoveUselessTags(File.ReadAllText(file));
                    File.WriteAllText(newFileName, newContext);

                    var lastWriteTime = File.GetLastWriteTime(file);
                    File.SetLastWriteTime(newFileName, lastWriteTime);
                    var lastAccessTime = File.GetLastAccessTime(file);
                    File.SetLastWriteTime(newFileName, lastAccessTime);
                    var creationTime = File.GetCreationTime(file);
                    File.SetCreationTime(newFileName, creationTime);
                }
            }
            Helpers.Logger.AddMessage($"Finished. Proccessed {count:N0} files");
        }

        public static string RemoveUselessTags(string htmlContent)
        {
            var len = htmlContent.Length;
            var i1 = htmlContent.IndexOf("<!-- BEGIN WAYBACK TOOLBAR INSERT -->", StringComparison.InvariantCulture);
            var i2 = htmlContent.LastIndexOf("<!-- END WAYBACK TOOLBAR INSERT -->", StringComparison.InvariantCulture);
            if (i1 > -1 && i2 > i1) // file from web.archive.org
            {
                htmlContent = htmlContent.Substring(0, i1) + htmlContent.Substring(i2 + 35);
            }

            i1 = htmlContent.IndexOf("<body", StringComparison.InvariantCulture);
            i2 = htmlContent.LastIndexOf("</body>", StringComparison.InvariantCulture);
            if (i2>i1 && i1>-1)
            {
                htmlContent = htmlContent.Substring(i1, i2 - i1 + 7);
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
