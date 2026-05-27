using System;

namespace GenerateDocumentLibrary.Models
{
    public class UnifiedEmployeeData
    {
        public string FullName { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public DateTime? DateOfBirth { get; set; }
        public string Nationality { get; set; } = string.Empty;
        public string Occupation { get; set; } = string.Empty; // Used in ED Card
        public string PassportNumber { get; set; } = string.Empty;
        public DateTime? PassportIssueDate { get; set; }
        public DateTime? PassportExpirationDate { get; set; }
        public string PassportIssuingCountry { get; set; } = string.Empty;
        public string PurposeOfVisit { get; set; } = "Work";
        public string NatureOfBusiness { get; set; } = "Oil & Gas";
        public string Address { get; set; } = string.Empty; // For NIS Compliance
        public string ReasonForApplication { get; set; } = "Work";
        public string CountryOfBirth { get; set; } = string.Empty;
    }
}
