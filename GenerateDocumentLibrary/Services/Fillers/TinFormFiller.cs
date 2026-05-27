using GenerateDocumentLibrary.Models;
using GenerateDocumentLibrary.Services.Helpers;
using PdfSharp.Pdf.AcroForms;
using Microsoft.Extensions.Logging;

namespace GenerateDocumentLibrary.Services.Fillers
{
    /// <summary>
    /// Strategia uzupełniania dokumentu: 5. TIN Form (if applicable)
    /// </summary>
    public class TinFormFiller : IDocumentFiller
    {
        public DocumentType SupportedType => DocumentType.TinForm;
        public string TemplateFileName => "tin_template.pdf";

        public void FillForm(PdfAcroForm acroForm, UnifiedEmployeeData data, ILogger logger)
        {
            logger.LogInformation("Uruchamianie strategii wypełniania dla: TIN Form");

            // Wypełnianie pól nie-zielonych
            PdfFormHelper.SetFormField(acroForm, "Name", data.FullName, logger);
            PdfFormHelper.SetFormField(acroForm, "Gender", data.Gender, logger);
            PdfFormHelper.SetFormField(acroForm, "DateOfBirth", data.DateOfBirth?.ToString("yyyy-MM-dd") ?? string.Empty, logger);
            PdfFormHelper.SetFormField(acroForm, "CountryOfBirth", data.CountryOfBirth, logger);

            // Wsparcie dla pól paszportowych (pojedyncze pola lub jako pole łączone)
            PdfFormHelper.SetFormField(acroForm, "PassportNumber", data.PassportNumber, logger);
            PdfFormHelper.SetFormField(acroForm, "PassportIssuingCountry", data.PassportIssuingCountry, logger);
            PdfFormHelper.SetFormField(acroForm, "PassportExpirationDate", data.PassportExpirationDate?.ToString("yyyy-MM-dd") ?? string.Empty, logger);

            string combinedPassport = $"{data.PassportNumber}, {data.PassportIssuingCountry}, {data.PassportExpirationDate?.ToString("yyyy-MM-dd") ?? string.Empty}";
            PdfFormHelper.SetFormField(acroForm, "PassportDetailsCombined", combinedPassport, logger);

            // Kompatybilność wsteczna z podstawowym szablonem testowym
            PdfFormHelper.SetFormField(acroForm, "Text1", data.FullName, logger);
            PdfFormHelper.SetFormField(acroForm, "Text2", data.Gender, logger);
            PdfFormHelper.SetFormField(acroForm, "Text3", combinedPassport, logger);
        }
    }
}
