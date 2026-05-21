using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PdfSharp.Fonts;
using Microsoft.Extensions.Logging;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using PdfSharp.Pdf.AcroForms;
using GenerateDocumentLibrary.Models;

namespace GenerateDocumentLibrary.Services
{
    public class DocumentGeneratorService
    {
        private readonly ILogger _logger;

        public DocumentGeneratorService(ILogger logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Generuje tablicę bajtów PDF na podstawie identyfikatora pracownika. Szablon 'template.pdf' jest ładowany automatycznie.
        /// </summary>
        public async Task<byte[]> GenerateContractPdfBytesAsync(string employeeId)
        {
            _logger.LogInformation("Rozpoczęto generowanie PDF w bibliotece dla EmployeeId: {EmployeeId}", employeeId);

            // 1. Automatyczne ustalenie ścieżki do szablonu PDF
            string templatePath = Path.Combine(AppContext.BaseDirectory, "template.pdf");
            if (!File.Exists(templatePath))
            {
                string alternativePath = Path.Combine(Directory.GetCurrentDirectory(), "template.pdf");
                if (File.Exists(alternativePath))
                {
                    templatePath = alternativePath;
                }
                else
                {
                    throw new FileNotFoundException($"Brak szablonu 'template.pdf' w katalogu aplikacji. Ścieżka poszukiwań: {templatePath}");
                }
            }

            _logger.LogInformation("Używanie szablonu PDF ze ścieżki: {TemplatePath}", templatePath);

            // 2. Pobranie zamakietowanych danych pracownika
            ContractData employeeData = GetMockContractData(employeeId);
            _logger.LogInformation("Pobrano zamakietowane dane pracownika: {Imie} {Nazwisko}", employeeData.Imie, employeeData.Nazwisko);

            // 3. Otwarcie i modyfikacja szablonu PDF za pomocą PdfSharpCore
            byte[] pdfBytes;
            using (var document = PdfReader.Open(templatePath, PdfDocumentOpenMode.Modify))
            {
                PdfAcroForm acroForm = document.AcroForm;
                if (acroForm == null)
                {
                    _logger.LogError("Wybrany plik PDF nie posiada interaktywnych pól formularza (AcroForms)!");
                    throw new InvalidOperationException("Plik szablonu PDF nie posiada interaktywnych pól formularza (AcroForms).");
                }

                // Log all available form field names for debugging
                foreach (var fieldName in acroForm.Fields.Names)
                {
                    _logger.LogInformation("PDF field available: {FieldName}", fieldName);
                }

                // Ensure NeedAppearances is set so values are rendered
                if (acroForm.Elements.ContainsKey("/NeedAppearances"))
                {
                    acroForm.Elements["/NeedAppearances"] = new PdfBoolean(true);
                }
                else
                {
                    acroForm.Elements.Add("/NeedAppearances", new PdfBoolean(true));
                }

                // Fill primary fields mapping to the actual template field names: Text1, Text2, Text3
                SetFormField(acroForm, "Text1", employeeData.Imie);
                SetFormField(acroForm, "Text2", employeeData.Nazwisko);
                SetFormField(acroForm, "Text3", employeeData.NumerPaszportu);



                // Zapis dokumentu do MemoryStream
                using (var outputStream = new MemoryStream())
                {
                    document.Save(outputStream, false);
                    pdfBytes = outputStream.ToArray();
                }
            }

            _logger.LogInformation("Dokument PDF został pomyślnie wygenerowany w bibliotece. Rozmiar: {Size} bajtów.", pdfBytes.Length);
            
            // Asynchroniczne wywołanie zapisu (symulowane)
            await SaveFileToDatabaseAsync(employeeId, pdfBytes);

            return pdfBytes;
        }

        /// <summary>
        /// Pomocnicza metoda bezpiecznie wstrzykująca dane do pola formularza PDF.
        /// </summary>
        private void SetFormField(PdfAcroForm acroForm, string fieldName, string value)
        {
            try
            {
                // Find field case‑insensitively
                var matchingKey = acroForm.Fields.Names
                    .FirstOrDefault(k => string.Equals(k, fieldName, StringComparison.OrdinalIgnoreCase));

                if (matchingKey != null)
                {
                    var field = acroForm.Fields[matchingKey];
                    if (field is PdfTextField textField)
                    {
                        // Use Unicode encoding for Polish characters
                        textField.Value = new PdfString(value ?? string.Empty, PdfStringEncoding.Unicode);
                        textField.ReadOnly = true;

                        // Remove border by setting BS width to 0
                        var bs = textField.Elements.GetDictionary("/BS");
                        if (bs == null)
                        {
                            bs = new PdfDictionary();
                            textField.Elements["/BS"] = bs;
                        }
                        bs.Elements["/W"] = new PdfInteger(0);

                        // Remove background and border colors from MK characteristics
                        var mk = textField.Elements.GetDictionary("/MK");
                        if (mk == null)
                        {
                            mk = new PdfDictionary();
                            textField.Elements["/MK"] = mk;
                        }
                        mk.Elements.Remove("/BG");
                        mk.Elements.Remove("/BC");

                        _logger.LogInformation("Biblioteka -> Zapisano pole: '{FieldName}' = '{Value}'", matchingKey, value);
                    }
                    else
                    {
                        _logger.LogWarning("Pole '{FieldName}' istnieje, ale nie jest typu tekstowego. Typ: {Type}", matchingKey, field.GetType().Name);
                    }
                }
                else
                {
                    _logger.LogDebug("Pole '{FieldName}' nie występuje w bieżącym szablonie PDF (pomijanie).", fieldName);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Błąd podczas próby zapisu pola '{FieldName}'", fieldName);
            }
        }

        /// <summary>
        /// Zwraca zamakietowane dane pracownika na podstawie identyfikatora.
        /// </summary>
        public ContractData GetMockContractData(string employeeId)
        {
            return new ContractData
            {
                Imie = "Jon",
                Nazwisko = "Kamil",
                NumerPaszportu = "777",
                ElementyTabeli = new List<TableItem>
                {
                    new TableItem { Opis = "Usługi konsultingowe IT - Maj 2026", Kwota = "12 500,00 PLN" },
                    new TableItem { Opis = "Wsparcie wdrożeniowe Azure Cloud", Kwota = "4 200,00 PLN" },
                    new TableItem { Opis = "Refaktoryzacja kodu do .NET 8 Isolated", Kwota = "3 800,00 PLN" }
                }
            };
        }

        /// <summary>
        /// Pusta asynchroniczna metoda przygotowana pod przyszłą integrację zapisu do bazy danych.
        /// </summary>
        public async Task SaveFileToDatabaseAsync(string employeeId, byte[] fileData)
        {
            _logger.LogInformation("[DB_SAVE] Zapisywanie pliku do bazy z poziomu biblioteki dla EmployeeId: {EmployeeId}", employeeId);
            
            // Miejsce na Twoją przyszłą logikę biznesową zapisu (np. Azure SQL, Blob Storage, Dataverse)
            
            await Task.CompletedTask;
        }
    }
}
