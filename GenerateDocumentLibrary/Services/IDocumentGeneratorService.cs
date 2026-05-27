using System.Threading.Tasks;

namespace GenerateDocumentLibrary.Services
{
    /// <summary>
    /// Interfejs głównego serwisu odpowiedzialnego za generowanie wszystkich dokumentów po kolei.
    /// </summary>
    public interface IDocumentGeneratorService
    {
        /// <summary>
        /// Pobiera dane pracownika i uzupełnia sekwencyjnie po kolei wszystkie dokumenty (fillery).
        /// </summary>
        Task GenerateAllComplianceLettersAsync(string employeeId);
    }
}
