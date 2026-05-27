using GenerateDocumentLibrary.Models;
using GenerateDocumentLibrary.Services.Helpers;
using PdfSharp.Pdf.AcroForms;
using Microsoft.Extensions.Logging;

namespace GenerateDocumentLibrary.Services.Fillers
{
    /// <summary>
    /// Strategia uzupełniania dokumentu: 3. NIS Registration Form
    /// </summary>
    public class NisRegistrationFormFiller : IDocumentFiller
    {
        public DocumentType SupportedType => DocumentType.NisRegistrationForm;
        public string TemplateFileName => "nis_registration_template.pdf";

        public void FillForm(PdfAcroForm acroForm, UnifiedEmployeeData data, ILogger logger)
        {
            logger.LogInformation("Uruchamianie strategii wypełniania dla: NIS Registration Form");

            // Wypełnianie pól nie-zielonych
            PdfFormHelper.SetFormField(acroForm, "Name", data.FullName, logger);
            PdfFormHelper.SetFormField(acroForm, "Gender", data.Gender, logger);
            PdfFormHelper.SetFormField(acroForm, "DateOfBirth", data.DateOfBirth?.ToString("yyyy-MM-dd") ?? string.Empty, logger);
            PdfFormHelper.SetFormField(acroForm, "PassportNumber", data.PassportNumber, logger);
            PdfFormHelper.SetFormField(acroForm, "NatureOfBusiness", data.NatureOfBusiness, logger);

            // Kompatybilność wsteczna z podstawowym szablonem testowym
            PdfFormHelper.SetFormField(acroForm, "Text1", data.FullName, logger);
            PdfFormHelper.SetFormField(acroForm, "Text2", data.Gender, logger);
            PdfFormHelper.SetFormField(acroForm, "Text3", data.PassportNumber, logger);
        }
    }
}
