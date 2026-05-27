using GenerateDocumentLibrary.Models;
using PdfSharp.Pdf.AcroForms;
using Microsoft.Extensions.Logging;

namespace GenerateDocumentLibrary.Services.Fillers
{
    /// <summary>
    /// Interfejs dla strategii uzupełniania określonego formularza PDF.
    /// </summary>
    public interface IDocumentFiller
    {
        /// <summary>
        /// Typ dokumentu obsługiwany przez tę strategię.
        /// </summary>
        DocumentType SupportedType { get; }

        /// <summary>
        /// Nazwa pliku szablonu powiązanego z danym formularzem.
        /// </summary>
        string TemplateFileName { get; }

        /// <summary>
        /// Wypełnia formularz PDF AcroForm danymi pracownika.
        /// </summary>
        void FillForm(PdfAcroForm acroForm, UnifiedEmployeeData data, ILogger logger);
    }
}
