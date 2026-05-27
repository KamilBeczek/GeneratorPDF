using System.Threading.Tasks;
using GenerateDocumentLibrary.Models;

namespace GenerateDocumentLibrary.Services
{
    /// <summary>
    /// Interfejs dostawcy danych o pracownikach do generowania dokumentów.
    /// </summary>
    public interface IEmployeeDataProvider
    {
        /// <summary>
        /// Pobiera zjednoczone dane pracownika na podstawie jego identyfikatora.
        /// </summary>
        Task<UnifiedEmployeeData> GetEmployeeDataAsync(string employeeId);
    }
}
