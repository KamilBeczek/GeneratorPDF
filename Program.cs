using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using GenerateDocumentLibrary.Fonts;
using GenerateDocumentLibrary.Services;
using GenerateDocumentLibrary.Services.Fillers;
using PdfSharp.Fonts;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services =>
    {
        // Rejestracja strategii uzupełniania pól formularzy (IDocumentFiller)
        services.AddSingleton<IDocumentFiller, EdCardFiller>();
        services.AddSingleton<IDocumentFiller, GraComplianceFormFiller>();
        services.AddSingleton<IDocumentFiller, NisRegistrationFormFiller>();
        services.AddSingleton<IDocumentFiller, NisComplianceFormFiller>();
        services.AddSingleton<IDocumentFiller, TinFormFiller>();
        services.AddSingleton<IDocumentFiller, VisaFormFiller>();

        // Rejestracja dostawcy danych o pracownikach
        services.AddSingleton<IEmployeeDataProvider, MockEmployeeDataProvider>();

        // Rejestracja głównej usługi generującej dokumenty PDF
        services.AddSingleton<IDocumentGeneratorService, DocumentGeneratorService>();
    })
    .Build();

// Rejestracja systemowego resolvera czcionek dla PDFsharp 6.x na .NET Core/Windows
GlobalFontSettings.FontResolver = new SystemFontResolver();

await host.RunAsync();
