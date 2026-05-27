using GenerateDocumentLibrary.Models;
using GenerateDocumentLibrary.Services.Helpers;
using PdfSharp.Pdf.AcroForms;
using Microsoft.Extensions.Logging;

namespace GenerateDocumentLibrary.Services.Fillers
{
    /// <summary>
    /// Strategia uzupełniania dokumentu: 4. NIS Compliance form
    /// </summary>
    public class NisComplianceFormFiller : IDocumentFiller
    {
        public DocumentType SupportedType => DocumentType.NisComplianceForm;
        public string TemplateFileName => "nis_compliance_template.pdf";

        public void FillForm(PdfAcroForm acroForm, UnifiedEmployeeData data, ILogger logger)
        {
            logger.LogInformation("Uruchamianie strategii wypełniania dla: NIS Compliance form");

            // Wypełnianie pól nie-zielonych
            PdfFormHelper.SetFormField(acroForm, "Name", data.FullName, logger);
            PdfFormHelper.SetFormField(acroForm, "Address", data.Address, logger);
            PdfFormHelper.SetFormField(acroForm, "DateOfBirth", data.DateOfBirth?.ToString("yyyy-MM-dd") ?? string.Empty, logger);
            PdfFormHelper.SetFormField(acroForm, "ReasonForApplication", data.ReasonForApplication, logger);

            // Kompatybilność wsteczna z podstawowym szablonem testowym
            PdfFormHelper.SetFormField(acroForm, "Text1", data.FullName, logger);
            PdfFormHelper.SetFormField(acroForm, "Text2", data.Address, logger);
            PdfFormHelper.SetFormField(acroForm, "Text3", data.ReasonForApplication, logger);
        }
    }
}
