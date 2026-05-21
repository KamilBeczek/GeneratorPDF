using System.IO;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using GenerateDocumentLibrary.Services;

namespace GeneratorPDF.Functions
{
    public class GenerateDocumentFunction
    {
        private readonly ILogger<GenerateDocumentFunction> _logger;

        public GenerateDocumentFunction(ILogger<GenerateDocumentFunction> logger)
        {
            _logger = logger;
        }

        [Function("GenerateDocument")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
        {
            _logger.LogInformation("Azure Function 'GenerateDocument' triggered.");

            // 1. Odczyt identyfikatora pracownika z żądania JSON
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var reqData = JsonSerializer.Deserialize<GenerateDocumentRequest>(requestBody);

            if (reqData == null || string.IsNullOrWhiteSpace(reqData.EmployeeId))
            {
                var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badResponse.WriteStringAsync("Request body must contain 'employeeId'.");
                return badResponse;
            }

            try
            {
                // 2. Wywołanie dedykowanej biblioteki biznesowej
                var service = new DocumentGeneratorService(_logger);
                byte[] pdfBytes = await service.GenerateContractPdfBytesAsync(reqData.EmployeeId);

                // 3. Zwrócenie strumienia PDF
                var response = req.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", "application/pdf");
                response.Headers.Add("Content-Disposition", $"attachment; filename=\"kontrakt_{reqData.EmployeeId}.pdf\"");
                await response.Body.WriteAsync(pdfBytes, 0, pdfBytes.Length);

                return response;
            }
            catch (FileNotFoundException fnfEx)
            {
                _logger.LogError(fnfEx, "Nie znaleziono pliku szablonu.");
                var errorResponse = req.CreateResponse(HttpStatusCode.NotFound);
                await errorResponse.WriteStringAsync($"Błąd: Nie odnaleziono szablonu PDF. Szczegóły: {fnfEx.Message}");
                return errorResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Wystąpił błąd podczas generowania dokumentu PDF.");
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteStringAsync($"Wystąpił błąd serwera podczas generowania PDF: {ex.Message}");
                return errorResponse;
            }
        }
    }

    public class GenerateDocumentRequest
    {
        [JsonPropertyName("employeeId")]
        public string? EmployeeId { get; set; }
    }
}
