using System.Collections.Generic;

namespace GenerateDocumentLibrary.Models
{
    public class ContractData
    {
        public string Imie { get; set; } = string.Empty;
        public string Nazwisko { get; set; } = string.Empty;
        public string NumerPaszportu { get; set; } = string.Empty;
        public List<TableItem> ElementyTabeli { get; set; } = new List<TableItem>();
    }
}
