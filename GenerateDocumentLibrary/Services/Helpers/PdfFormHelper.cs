using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using PdfSharp.Pdf;
using PdfSharp.Pdf.AcroForms;

namespace GenerateDocumentLibrary.Services.Helpers
{
    /// <summary>
    /// Klasa pomocnicza do bezpiecznego wprowadzania danych do pól formularzy PDF AcroForms z zachowaniem kodowania Unicode.
    /// </summary>
    public static class PdfFormHelper
    {
        public static void SetFormField(PdfAcroForm acroForm, string fieldName, string value, ILogger logger)
        {
            try
            {
                // Wyszukiwanie pola bez względu na wielkość liter
                var matchingKey = acroForm.Fields.Names
                    .FirstOrDefault(k => string.Equals(k, fieldName, StringComparison.OrdinalIgnoreCase));

                if (matchingKey != null)
                {
                    var field = acroForm.Fields[matchingKey];
                    if (field != null)
                    {
                        if (field is PdfTextField textField)
                        {
                            // Wymuszenie kodowania Unicode (obsługa polskich znaków)
                            textField.Value = new PdfString(value ?? string.Empty, PdfStringEncoding.Unicode);
                            textField.ReadOnly = true;

                            // Ukrycie obramowania pola przez ustawienie grubości BS na 0
                            var bs = textField.Elements.GetDictionary("/BS");
                            if (bs == null)
                            {
                                bs = new PdfDictionary();
                                textField.Elements["/BS"] = bs;
                            }
                            bs.Elements["/W"] = new PdfInteger(0);

                            // Usunięcie domyślnego koloru tła i obramowania z cech charakterystycznych MK
                            var mk = textField.Elements.GetDictionary("/MK");
                            if (mk == null)
                            {
                                mk = new PdfDictionary();
                                textField.Elements["/MK"] = mk;
                            }
                            mk.Elements.Remove("/BG");
                            mk.Elements.Remove("/BC");

                            logger.LogInformation("Biblioteka -> Zapisano pole: '{FieldName}' = '{Value}'", matchingKey, value);
                        }
                        else
                        {
                            logger.LogWarning("Pole '{FieldName}' istnieje, ale nie jest typu tekstowego. Typ: {Type}", matchingKey, field.GetType().Name);
                        }
                    }
                }
                else
                {
                    logger.LogDebug("Pole '{FieldName}' nie występuje w bieżącym szablonie PDF (pomijanie).", fieldName);
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Błąd podczas próby zapisu pola '{FieldName}'", fieldName);
            }
        }
    }
}
