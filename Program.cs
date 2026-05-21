using Microsoft.Extensions.Hosting;
using GenerateDocumentLibrary.Fonts;
using PdfSharp.Fonts;
var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .Build();

// Register system font resolver for PDFsharp 6.x on .NET Core/Windows
GlobalFontSettings.FontResolver = new SystemFontResolver();

await host.RunAsync();
