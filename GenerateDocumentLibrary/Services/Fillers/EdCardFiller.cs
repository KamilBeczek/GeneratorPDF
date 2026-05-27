using GenerateDocumentLibrary.Models;
using GenerateDocumentLibrary.Services.Helpers;
using PdfSharp.Pdf.AcroForms;
using Microsoft.Extensions.Logging;

namespace GenerateDocumentLibrary.Services.Fillers
{
    /// <summary>
    /// Strategia uzupełniania dokumentu: 1. ED Card
    /// </summary>
    public class EdCardFiller : IDocumentFiller
    {
        public DocumentType SupportedType => DocumentType.EdCard;
        public string TemplateFileName => "ed_card_template.pdf";

        public void FillForm(PdfAcroForm acroForm, UnifiedEmployeeData data, ILogger logger)
        {
            logger.LogInformation("Uruchamianie strategii wypełniania dla: ED Card");

            // Wypełnianie pól specyficznych dla ED Card
            PdfFormHelper.SetFormField(acroForm, "Name", data.FullName, logger);
            PdfFormHelper.SetFormField(acroForm, "Gender", data.Gender, logger);
            PdfFormHelper.SetFormField(acroForm, "DateOfBirth", data.DateOfBirth?.ToString("yyyy-MM-dd") ?? string.Empty, logger);
            PdfFormHelper.SetFormField(acroForm, "Nationality", data.Nationality, logger);
            PdfFormHelper.SetFormField(acroForm, "Occupation", data.Occupation, logger);
            PdfFormHelper.SetFormField(acroForm, "PassportNumber", data.PassportNumber, logger);
            PdfFormHelper.SetFormField(acroForm, "PassportIssueDate", data.PassportIssueDate?.ToString("yyyy-MM-dd") ?? string.Empty, logger);
            PdfFormHelper.SetFormField(acroForm, "PurposeOfVisit", data.PurposeOfVisit, logger);

            // Kompatybilność wsteczna z podstawowym szablonem testowym (template.pdf)
            PdfFormHelper.SetFormField(acroForm, "Text1", data.FullName, logger);
            PdfFormHelper.SetFormField(acroForm, "Text2", data.Gender, logger);
            PdfFormHelper.SetFormField(acroForm, "Text3", data.PassportNumber, logger);
        }
    }
}
