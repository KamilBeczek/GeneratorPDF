using GenerateDocumentLibrary.Models;
using GenerateDocumentLibrary.Services.Helpers;
using PdfSharp.Pdf.AcroForms;
using Microsoft.Extensions.Logging;

namespace GenerateDocumentLibrary.Services.Fillers
{
    /// <summary>
    /// Strategia uzupełniania dokumentu: 2. GRA Compliance Form
    /// </summary>
    public class GraComplianceFormFiller : IDocumentFiller
    {
        public DocumentType SupportedType => DocumentType.GraComplianceForm;
        public string TemplateFileName => "gra_compliance_template.pdf";

        public void FillForm(PdfAcroForm acroForm, UnifiedEmployeeData data, ILogger logger)
        {
            logger.LogInformation("Uruchamianie strategii wypełniania dla: GRA Compliance Form");

            // GRA Compliance Form: tylko Name nie jest zaznaczone na zielono
            PdfFormHelper.SetFormField(acroForm, "Name", data.FullName, logger);

            // Kompatybilność wsteczna z podstawowym szablonem testowym
            PdfFormHelper.SetFormField(acroForm, "Text1", data.FullName, logger);
        }
    }
}
