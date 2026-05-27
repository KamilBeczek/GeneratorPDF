using System;
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
    /// <summary>
    /// Klasa funkcji Azure obsługująca żądanie generowania wszystkich dokumentów po kolei dla pracownika.
    /// </summary>
    public class GenerateDocumentFunction
    {
        private readonly ILogger<GenerateDocumentFunction> _logger;
        private readonly IDocumentGeneratorService _documentGeneratorService;

        /// <summary>
        /// Konstruktor wykorzystujący automatyczne wstrzykiwanie zależności (DI).
        /// </summary>
        public GenerateDocumentFunction(
            ILogger<GenerateDocumentFunction> logger,
            IDocumentGeneratorService documentGeneratorService)
        {
            _logger = logger;
            _documentGeneratorService = documentGeneratorService;
        }

        [Function("GenerateDocument")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
        {
            _logger.LogInformation("Rozpoczęto wywołanie Azure Function 'GenerateDocument'.");

            // 1. Odczyt i deserializacja żądania JSON
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var reqData = JsonSerializer.Deserialize<GenerateDocumentRequest>(requestBody);

            if (reqData == null || string.IsNullOrWhiteSpace(reqData.EmployeeId))
            {
                var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badResponse.WriteStringAsync("Błąd: Request body musi zawierać pole 'employeeId'.");
                return badResponse;
            }

            try
            {
                // 2. Wywołanie głównego serwisu w celu sekwencyjnego uzupełnienia wszystkich dokumentów w jednej funkcji
                await _documentGeneratorService.GenerateAllComplianceLettersAsync(reqData.EmployeeId);

                // 3. Zwrócenie prostego komunikatu o pomyślnym wygenerowaniu
                var response = req.CreateResponse(HttpStatusCode.OK);
                await response.WriteStringAsync($"Sukces: Wygenerowano wszystkie dokumenty po kolei dla pracownika: {reqData.EmployeeId}.");
                return response;
            }
            catch (FileNotFoundException fnfEx)
            {
                _logger.LogError(fnfEx, "Brak wymaganego szablonu PDF.");
                var errorResponse = req.CreateResponse(HttpStatusCode.NotFound);
                await errorResponse.WriteStringAsync($"Błąd: Nie znaleziono żadnego szablonu PDF na serwerze. Szczegóły: {fnfEx.Message}");
                return errorResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Wystąpił nieoczekiwany błąd podczas generowania dokumentów.");
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteStringAsync($"Wystąpił błąd podczas generowania dokumentów: {ex.Message}");
                return errorResponse;
            }
        }
    }

    /// <summary>
    /// Klasa reprezentująca strukturę wejściową żądania generowania.
    /// </summary>
    public class GenerateDocumentRequest
    {
        [JsonPropertyName("employeeId")]
        public string? EmployeeId { get; set; }
    }
}
