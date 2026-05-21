using System;
using System.IO;
using PdfSharp.Fonts;

namespace GenerateDocumentLibrary.Fonts
{
    /// <summary>
    /// Robust cross-platform Font Resolver for PDFsharp 6.x supporting Windows, Linux and local publishing.
    /// </summary>
    public class SystemFontResolver : IFontResolver
    {
        public string DefaultFontName => "Arial";

        public byte[]? GetFont(string faceName)
        {
            var fileName = faceName.ToLowerInvariant() switch
            {
                "arial" => "arial.ttf",
                "arial bold" => "arialbd.ttf",
                "arial italic" => "ariali.ttf",
                "arial bold italic" => "arialbi.ttf",
                "courier new" => "cour.ttf",
                "courier new bold" => "courbd.ttf",
                "courier new italic" => "couri.ttf",
                "courier new bold italic" => "courbi.ttf",
                "times new roman" => "times.ttf",
                "times new roman bold" => "timesbd.ttf",
                "times new roman italic" => "timesi.ttf",
                "times new roman bold italic" => "timesbi.ttf",
                "verdana" => "verdana.ttf",
                "verdana bold" => "verdanab.ttf",
                "calibri" => "calibri.ttf",
                "calibri bold" => "calibrib.ttf",
                _ => faceName.ToLowerInvariant().Replace(" ", "") + ".ttf"
            };

            var data = ReadFontFile(fileName);
            if (data != null) return data;

            // Fallback 1: Try standard Arial
            if (fileName != "arial.ttf")
            {
                data = ReadFontFile("arial.ttf");
                if (data != null) return data;
            }

            // Fallback 2: Try common open-source Linux monospace/sans fonts
            var fallbackLinuxFonts = new[] { "LiberationSans-Regular.ttf", "DejaVuSans.ttf", "Ubuntu-R.ttf", "FreeSans.ttf" };
            foreach (var fallbackName in fallbackLinuxFonts)
            {
                data = ReadFontFile(fallbackName);
                if (data != null) return data;
            }

            // Fallback 3: Return the first available .ttf font file in Linux directories so it never crashes the engine
            var linuxPaths = new[] { "/usr/share/fonts", "/usr/local/share/fonts" };
            foreach (var dir in linuxPaths)
            {
                if (Directory.Exists(dir))
                {
                    try
                    {
                        var ttfFiles = Directory.GetFiles(dir, "*.ttf", SearchOption.AllDirectories);
                        if (ttfFiles.Length > 0)
                            return File.ReadAllBytes(ttfFiles[0]);
                    }
                    catch
                    {
                        // ignore filesystem access errors in sandbox/restricted environments
                    }
                }
            }

            return null;
        }

        private byte[]? ReadFontFile(string fileName)
        {
            try
            {
                // 1. Check local application directory (highly recommended for self-contained/Docker/Azure deployment)
                var localPath = Path.Combine(AppContext.BaseDirectory, fileName);
                if (File.Exists(localPath))
                    return File.ReadAllBytes(localPath);

                var localSubdirPath = Path.Combine(AppContext.BaseDirectory, "fonts", fileName);
                if (File.Exists(localSubdirPath))
                    return File.ReadAllBytes(localSubdirPath);

                // 2. Check Windows System Fonts Folder
                var windowsFontsFolder = Environment.GetFolderPath(Environment.SpecialFolder.Fonts);
                if (!string.IsNullOrEmpty(windowsFontsFolder))
                {
                    var winPath = Path.Combine(windowsFontsFolder, fileName);
                    if (File.Exists(winPath))
                        return File.ReadAllBytes(winPath);
                }

                // 3. Check common Linux Font directories
                var linuxPaths = new[]
                {
                    "/usr/share/fonts",
                    "/usr/local/share/fonts",
                    "/usr/share/fonts/truetype"
                };

                foreach (var dir in linuxPaths)
                {
                    if (Directory.Exists(dir))
                    {
                        var foundFiles = Directory.GetFiles(dir, fileName, SearchOption.AllDirectories);
                        if (foundFiles.Length > 0)
                            return File.ReadAllBytes(foundFiles[0]);
                    }
                }
            }
            catch
            {
                // Gracefully handle any security/IO exceptions in sandboxed environments like Azure
            }

            return null;
        }

        public FontResolverInfo? ResolveTypeface(string familyName, bool isBold, bool isItalic)
        {
            var faceName = familyName;
            if (isBold) faceName += " Bold";
            if (isItalic) faceName += " Italic";

            return new FontResolverInfo(faceName);
        }
    }
}
