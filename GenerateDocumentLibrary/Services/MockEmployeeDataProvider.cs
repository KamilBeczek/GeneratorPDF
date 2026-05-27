using System;
using System.Threading.Tasks;
using GenerateDocumentLibrary.Models;

namespace GenerateDocumentLibrary.Services
{
    /// <summary>
    /// Testowa (mockowana) implementacja dostawcy danych pracownika.
    /// </summary>
    public class MockEmployeeDataProvider : IEmployeeDataProvider
    {
        public Task<UnifiedEmployeeData> GetEmployeeDataAsync(string employeeId)
        {
            // Zwracamy pełne testowe dane (z polskimi znakami do weryfikacji Unicode)
            return Task.FromResult(new UnifiedEmployeeData
            {
                FullName = "Kamil Ząbkowski",
                Gender = "Male",
                DateOfBirth = new DateTime(1993, 11, 24),
                Nationality = "Polish",
                Occupation = "Senior Software Developer",
                PassportNumber = "PL88776655",
                PassportIssueDate = new DateTime(2021, 05, 12),
                PassportExpirationDate = new DateTime(2031, 05, 12),
                PassportIssuingCountry = "Poland",
                PurposeOfVisit = "Work - IT Consulting",
                NatureOfBusiness = "Oil & Gas Engineering Services",
                Address = "ul. Marszałkowska 120/4, 00-001 Warszawa, Polska",
                ReasonForApplication = "Employment Contract - Work Permit",
                CountryOfBirth = "Poland"
            });
        }
    }
}
