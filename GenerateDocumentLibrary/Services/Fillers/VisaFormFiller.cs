using GenerateDocumentLibrary.Models;
using GenerateDocumentLibrary.Services.Helpers;
using PdfSharp.Pdf.AcroForms;
using Microsoft.Extensions.Logging;

namespace GenerateDocumentLibrary.Services.Fillers
{
    /// <summary>
    /// Strategia uzupełniania dokumentu: 6. Visa Form (if applicable)
    /// </summary>
    public class VisaFormFiller : IDocumentFiller
    {
        public DocumentType SupportedType => DocumentType.VisaForm;
        public string TemplateFileName => "visa_template.pdf";

        public void FillForm(PdfAcroForm acroForm, UnifiedEmployeeData data, ILogger logger)
        {
            logger.LogInformation("Uruchamianie strategii wypełniania dla: Visa Form");

            // Wypełnianie pól nie-zielonych
            PdfFormHelper.SetFormField(acroForm, "Name", data.FullName, logger);
            PdfFormHelper.SetFormField(acroForm, "Gender", data.Gender, logger);
            PdfFormHelper.SetFormField(acroForm, "DateOfBirth", data.DateOfBirth?.ToString("yyyy-MM-dd") ?? string.Empty, logger);
            PdfFormHelper.SetFormField(acroForm, "Nationality", data.Nationality, logger);

            // Wsparcie dla pól paszportowych (pojedyncze lub scalone)
            PdfFormHelper.SetFormField(acroForm, "PassportNumber", data.PassportNumber, logger);
            PdfFormHelper.SetFormField(acroForm, "PassportIssueDate", data.PassportIssueDate?.ToString("yyyy-MM-dd") ?? string.Empty, logger);
            PdfFormHelper.SetFormField(acroForm, "PassportIssuingCountry", data.PassportIssuingCountry, logger);

            string combinedPassport = $"{data.PassportNumber}, {data.PassportIssueDate?.ToString("yyyy-MM-dd") ?? string.Empty}, {data.PassportIssuingCountry}";
            PdfFormHelper.SetFormField(acroForm, "PassportDetailsCombined", combinedPassport, logger);

            PdfFormHelper.SetFormField(acroForm, "PurposeOfVisit", data.PurposeOfVisit, logger);

            // Kompatybilność wsteczna z podstawowym szablonem testowym
            PdfFormHelper.SetFormField(acroForm, "Text1", data.FullName, logger);
            PdfFormHelper.SetFormField(acroForm, "Text2", data.Gender, logger);
            PdfFormHelper.SetFormField(acroForm, "Text3", combinedPassport, logger);
        }
    }
}
