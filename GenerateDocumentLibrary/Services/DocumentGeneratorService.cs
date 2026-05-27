using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using PdfSharp.Pdf.AcroForms;
using GenerateDocumentLibrary.Models;
using GenerateDocumentLibrary.Services.Fillers;

namespace GenerateDocumentLibrary.Services
{
    /// <summary>
    /// Główny serwis odpowiedzialny za sekwencyjne generowanie wszystkich dokumentów PDF.
    /// </summary>
    public class DocumentGeneratorService : IDocumentGeneratorService
    {
        private readonly ILogger<DocumentGeneratorService> _logger;
        private readonly IEnumerable<IDocumentFiller> _fillers;
        private readonly IEmployeeDataProvider _employeeDataProvider;

        /// <summary>
        /// Konstruktor serwisu wykorzystujący wstrzykiwanie zależności (Dependency Injection).
        /// </summary>
        public DocumentGeneratorService(
            ILogger<DocumentGeneratorService> logger, 
            IEnumerable<IDocumentFiller> fillers,
            IEmployeeDataProvider employeeDataProvider)
        {
            _logger = logger;
            _fillers = fillers;
            _employeeDataProvider = employeeDataProvider;
        }

        /// <summary>
        /// Konstruktor zapewniający kompatybilność wsteczną dla wywołań bez DI.
        /// </summary>
        public DocumentGeneratorService(ILogger logger)
        {
            _logger = new LoggerFactory().CreateLogger<DocumentGeneratorService>();
            _employeeDataProvider = new MockEmployeeDataProvider();
            _fillers = new List<IDocumentFiller>
            {
                new EdCardFiller(),
                new GraComplianceFormFiller(),
                new NisRegistrationFormFiller(),
                new NisComplianceFormFiller(),
                new TinFormFiller(),
                new VisaFormFiller()
            };
        }

        /// <summary>
        /// Pobiera dane pracownika i uzupełnia sekwencyjnie po kolei wszystkie dokumenty (fillery).
        /// </summary>
        public async Task GenerateAllComplianceLettersAsync(string employeeId)
        {
            _logger.LogInformation("Rozpoczęto sekwencyjne generowanie wszystkich dokumentów dla EmployeeId: {EmployeeId}", employeeId);

            // 1. Pobranie zjednoczonych danych pracownika
            UnifiedEmployeeData employeeData = await _employeeDataProvider.GetEmployeeDataAsync(employeeId);
            _logger.LogInformation("Pobrano dane pracownika do wypełnienia dokumentów: {FullName}", employeeData.FullName);

            // 2. Sekwencyjne uruchamianie każdego fillera po kolei
            foreach (var filler in _fillers)
            {
                _logger.LogInformation("Wymuszanie uzupełnienia dokumentu typu: {DocType} za pomocą szablonu: {Template}", filler.SupportedType, filler.TemplateFileName);

                try
                {
                    // Generowanie bajtów pliku PDF dla konkretnego dokumentu
                    byte[] pdfBytes = await GenerateSinglePdfBytesAsync(employeeData, filler);

                    // Zapis wygenerowanego dokumentu do bazy danych / magazynu
                    await SaveFileToDatabaseAsync(employeeId, filler.SupportedType, pdfBytes);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Wystąpił błąd podczas generowania dokumentu {DocType}", filler.SupportedType);
                    throw;
                }
            }

            _logger.LogInformation("Pomyślnie ukończono sekwencyjne generowanie wszystkich {Count} dokumentów dla EmployeeId: {EmployeeId}", _fillers.Count(), employeeId);
        }

        /// <summary>
        /// Pomocnicza metoda generująca bajty pojedynczego pliku PDF.
        /// </summary>
        private async Task<byte[]> GenerateSinglePdfBytesAsync(UnifiedEmployeeData employeeData, IDocumentFiller filler)
        {
            string templateName = filler.TemplateFileName;
            string templatePath = Path.Combine(AppContext.BaseDirectory, templateName);

            // Sprawdzenie obecności dedykowanego szablonu, w przypadku braku - automatyczny fallback do template.pdf
            if (!File.Exists(templatePath))
            {
                string alternativePath = Path.Combine(Directory.GetCurrentDirectory(), templateName);
                if (File.Exists(alternativePath))
                {
                    templatePath = alternativePath;
                }
                else
                {
                    // Fallback do standardowego szablonu template.pdf
                    string defaultTemplatePath = Path.Combine(AppContext.BaseDirectory, "template.pdf");
                    if (!File.Exists(defaultTemplatePath))
                    {
                        defaultTemplatePath = Path.Combine(Directory.GetCurrentDirectory(), "template.pdf");
                    }

                    if (File.Exists(defaultTemplatePath))
                    {
                        _logger.LogWarning("Nie odnaleziono dedykowanego szablonu '{TemplateName}'. Użycie szablonu domyślnego: {DefaultTemplate}", templateName, defaultTemplatePath);
                        templatePath = defaultTemplatePath;
                    }
                    else
                    {
                        throw new FileNotFoundException($"Brak szablonu '{templateName}' oraz domyślnego 'template.pdf' w aplikacji.");
                    }
                }
            }

            using (var document = PdfReader.Open(templatePath, PdfDocumentOpenMode.Modify))
            {
                PdfAcroForm acroForm = document.AcroForm;
                if (acroForm == null)
                {
                    throw new InvalidOperationException($"Plik szablonu {templatePath} nie zawiera pól formularza (AcroForms).");
                }

                // Odświeżanie wyglądu pól formularza w przeglądarkach PDF
                if (acroForm.Elements.ContainsKey("/NeedAppearances"))
                {
                    acroForm.Elements["/NeedAppearances"] = new PdfBoolean(true);
                }
                else
                {
                    acroForm.Elements.Add("/NeedAppearances", new PdfBoolean(true));
                }

                // Wypełnienie pól za pomocą dedykowanej strategii
                filler.FillForm(acroForm, employeeData, _logger);

                using (var outputStream = new MemoryStream())
                {
                    document.Save(outputStream, false);
                    return outputStream.ToArray();
                }
            }
        }

        /// <summary>
        /// Zapisywanie pliku do bazy danych wraz z typem dokumentu.
        /// </summary>
        public async Task SaveFileToDatabaseAsync(string employeeId, DocumentType documentType, byte[] fileData)
        {
            _logger.LogInformation("[DB_SAVE] Zapisywanie pliku typu: {DocType} (Rozmiar: {Size} bajtów) dla EmployeeId: {EmployeeId}", documentType, fileData.Length, employeeId);
            
            // Miejsce na integrację z Azure SQL, CosmosDB, Blob Storage itp.
            
            await Task.CompletedTask;
        }
    }
}
