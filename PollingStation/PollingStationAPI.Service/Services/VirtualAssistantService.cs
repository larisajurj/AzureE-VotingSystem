using Microsoft.Extensions.Configuration; // For IConfiguration
using PollingStationAPI.Service.DTOs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers; // For MediaTypeWithQualityHeaderValue
using System.Text;             // For Encoding
using System.Text.Json;        // For JsonSerializer
using System.Threading;
using System.Threading.Tasks;

public interface IVirtualAssistantService
{
    Task<string> GetAnswer(string question, CancellationToken cancellationToken = default);
}

public class VirtualAssistantService : IVirtualAssistantService
{
    private readonly HttpClient _httpClient; // Used for both Drive and Gemini
    private readonly List<string> _documentUrls;
    private readonly string? _geminiApiKey;
    private readonly string _geminiModelName;
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    private readonly Dictionary<string, byte[]> _pdfCache = new Dictionary<string, byte[]>();
    private static bool _staticPdfsLoaded = false;
    private static readonly SemaphoreSlim _pdfLoadLock = new SemaphoreSlim(1, 1);

    public VirtualAssistantService(IConfiguration configuration, IHttpClientFactory httpClientFactory)
    {
        _geminiApiKey = configuration["GeminiApiKey"];
        if (string.IsNullOrEmpty(_geminiApiKey))
        {
            throw new InvalidOperationException("Gemini API Key ('GeminiApiKey') not found or empty in configuration.");
        }

        _geminiModelName = configuration["GeminiModelName"] ?? "gemini-1.5-flash-latest"; // Allow model to be configured

        _httpClient = httpClientFactory.CreateClient("VirtualAssistantClient"); // General purpose client

        _documentUrls = configuration.GetSection("VirtualAssistant:DocumentUrls").Get<List<string>>() ?? new List<string>();
        if (!_documentUrls.Any())
        {
            Console.WriteLine("Warning: No document URLs configured under 'VirtualAssistant:DocumentUrls'.");
        }

        _jsonSerializerOptions = new JsonSerializerOptions
        {
            // PropertyNameCaseInsensitive for deserializing if needed, though not strictly for serializing here
            // DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull // Useful to omit null properties in request
        };
    }

    private async Task EnsurePdfsAreLoadedAsync(CancellationToken cancellationToken)
    {
        if (_staticPdfsLoaded && _pdfCache.Count == _documentUrls.Count && _pdfCache.Values.All(v => v != null))
        {
            return;
        }
        await _pdfLoadLock.WaitAsync(cancellationToken);
        try
        {
            if (_staticPdfsLoaded && _pdfCache.Count == _documentUrls.Count && _pdfCache.Values.All(v => v != null)) return;

            Console.WriteLine($"Loading/Verifying {_documentUrls.Count} PDF(s) from Google Drive URLs...");
            foreach (var url in _documentUrls)
            {
                if (cancellationToken.IsCancellationRequested) break;
                if (!_pdfCache.ContainsKey(url) || _pdfCache[url] == null || _pdfCache[url].Length == 0)
                {
                    try
                    {
                        Console.WriteLine($"Downloading PDF from: {url}");
                        byte[] pdfBytes = await _httpClient.GetByteArrayAsync(url, cancellationToken);
                        _pdfCache[url] = pdfBytes;
                        Console.WriteLine($"Successfully downloaded and cached PDF: {url} ({pdfBytes.Length} bytes)");
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine($"Error downloading PDF from {url}: {ex.Message}. This PDF will be skipped.");
                        _pdfCache[url] = Array.Empty<byte>(); // Mark as failed
                    }
                }
            }
            if (_pdfCache.Count == _documentUrls.Count) _staticPdfsLoaded = true;
        }
        finally
        {
            _pdfLoadLock.Release();
        }
    }

    public async Task<string> GetAnswer(string question, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(question))
        {
            return "Vă rugăm să furnizați o întrebare.";
        }

        await EnsurePdfsAreLoadedAsync(cancellationToken);

        var requestPayload = new GeminiRequest();
        var contentItem = new ContentItem(); // Typically one content item for a single prompt with multiple parts

        int successfullyLoadedPdfs = 0;
        if (_documentUrls.Any())
        {
            foreach (var url in _documentUrls)
            {
                if (_pdfCache.TryGetValue(url, out byte[]? pdfBytes) && pdfBytes != null && pdfBytes.Length > 0)
                {
                    contentItem.Parts.Add(PartItem.FromInlineData(
                        mimeType: "application/pdf",
                        base64Data: Convert.ToBase64String(pdfBytes)
                    ));
                    successfullyLoadedPdfs++;
                }
            }
        }

        if (successfullyLoadedPdfs > 0)
        {
            Console.WriteLine($"Se utilizează {successfullyLoadedPdfs} PDF-uri ca și context pentru Gemini.");
            contentItem.Parts.Add(PartItem.FromText(
                $"Pe baza documentului(elor) furnizate ({successfullyLoadedPdfs} PDF-uri încărcate), răspundeți la următoarea întrebare: {question}"
            ));
        }
        else
        {
            Console.WriteLine("Niciun PDF încărcat sau disponibil. Se trimite întrebarea către Gemini fără context PDF.");
            contentItem.Parts.Add(PartItem.FromText(
                question + " (Notă: Nu au fost furnizate documente specifice pentru context.)"
            ));
        }

        if (!contentItem.Parts.Any())
        {
            return "Nu se poate genera un răspuns. Nicio întrebare furnizată sau nicio parte de conținut nu a putut fi formată (de ex. toate descărcările PDF au eșuat).";
        }

        requestPayload.Contents.Add(contentItem);

        // Construct the Gemini API endpoint
        string apiUrl = $"https://generativelanguage.googleapis.com/v1beta/models/{_geminiModelName}:generateContent?key={_geminiApiKey}";

        try
        {
            string jsonRequestString = JsonSerializer.Serialize(requestPayload, _jsonSerializerOptions);
            var httpContent = new StringContent(jsonRequestString, Encoding.UTF8, "application/json");

            Console.WriteLine($"Se trimite cererea către Gemini: {apiUrl}");

            HttpResponseMessage response = await _httpClient.PostAsync(apiUrl, httpContent, cancellationToken);
            string responseJson = await response.Content.ReadAsStringAsync(cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var geminiResponse = JsonSerializer.Deserialize<GeminiResponse>(responseJson, _jsonSerializerOptions);
                var firstCandidate = geminiResponse?.Candidates?.FirstOrDefault();
                string? responseText = firstCandidate?.GetFirstTextPart(); // Uses your extension method

                if (!string.IsNullOrEmpty(responseText))
                {
                    return responseText;
                }
                else
                {
                    string failureReason = "Modelul nu a returnat text în structura așteptată.";
                    if (firstCandidate?.FinishReason != null)
                    {
                        failureReason += $" Motiv finalizare: {firstCandidate.FinishReason}.";
                    }
                    if (firstCandidate?.SafetyRatings?.Any(sr => sr.Blocked == true) == true)
                    {
                        var blockedCategories = string.Join(", ", firstCandidate.SafetyRatings.Where(sr => sr.Blocked == true).Select(sr => sr.Category));
                        failureReason += $" Conținutul ar putea fi blocat din cauza evaluărilor de siguranță: {blockedCategories}.";
                    }
                    if (geminiResponse?.PromptFeedback?.BlockReason != null)
                    {
                        failureReason += $" Motiv blocare feedback prompt: {geminiResponse.PromptFeedback.BlockReason}.";
                    }
                    Console.WriteLine($"Răspuns Gemini gol sau malformat. {failureReason}. Răspuns complet (fragment): {responseJson.Substring(0, Math.Min(responseJson.Length, 500))}");
                    return $"Nu s-a putut genera un răspuns. {failureReason.Trim()}";
                }
            }
            else
            {
                Console.Error.WriteLine($"Eroare de la API-ul Gemini. Status: {response.StatusCode}. Răspuns (fragment): {responseJson.Substring(0, Math.Min(responseJson.Length, 500))}");
                return $"Eroare de la asistentul AI: {response.ReasonPhrase} (Status: {(int)response.StatusCode}). Verificați log-urile pentru detalii.";
            }
        }
        catch (JsonException jsonEx)
        {
            Console.Error.WriteLine($"Eroare procesare JSON: {jsonEx.ToString()}");
            return "A apărut o eroare la procesarea datelor pentru asistentul AI.";
        }
        catch (HttpRequestException httpEx)
        {
            Console.Error.WriteLine($"Eroare cerere HTTP către API-ul Gemini: {httpEx.ToString()}");
            return "A apărut o problemă de rețea la contactarea asistentului AI.";
        }
        catch (TaskCanceledException ex) when (cancellationToken.IsCancellationRequested)
        {
            Console.WriteLine("Apelul către API-ul Gemini a fost anulat de către utilizator.");
            return "Cererea către asistentul AI a fost anulată.";
        }
        catch (TaskCanceledException ex) // Typically from HttpClient timeout
        {
            Console.Error.WriteLine($"Apelul către API-ul Gemini a expirat (timeout): {ex.Message}");
            return "Cererea către asistentul AI a expirat (timeout).";
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Eroare neașteptată în GetAnswer: {ex.ToString()}");
            return $"A apărut o eroare neașteptată: {ex.Message}";
        }
    }

}