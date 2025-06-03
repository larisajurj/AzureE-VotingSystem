namespace PollingStationAPI.Service.DTOs;

// Namespaces required for these DTOs and the service
using System.Text.Json.Serialization; // For JsonPropertyName
using System.Collections.Generic;
using System.Linq; // For Linq operations like FirstOrDefault

// --- Request DTOs ---
public class GeminiRequest
{
    [JsonPropertyName("contents")]
    public List<ContentItem> Contents { get; set; } = new List<ContentItem>();

    // Optional: Add GenerationConfig and SafetySettings if needed
    // [JsonPropertyName("generation_config")]
    // public GenerationConfig GenerationConfig { get; set; }
    // [JsonPropertyName("safety_settings")]
    // public List<SafetySetting> SafetySettings { get; set; }
}

public class ContentItem
{
    [JsonPropertyName("parts")]
    public List<PartItem> Parts { get; set; } = new List<PartItem>();

    // Optional: Role, e.g., "user" or "model" for conversational context
    // [JsonPropertyName("role")]
    // public string? Role { get; set; }
}

public class PartItem
{
    [JsonPropertyName("text")]
    public string? Text { get; set; }

    [JsonPropertyName("inline_data")]
    public InlineData? InlineData { get; set; }

    // Static factory methods for convenience
    public static PartItem FromText(string text) => new PartItem { Text = text };
    public static PartItem FromInlineData(string mimeType, string base64Data) =>
        new PartItem { InlineData = new InlineData { MimeType = mimeType, Data = base64Data } };
}

public class InlineData
{
    [JsonPropertyName("mime_type")]
    public string MimeType { get; set; }

    [JsonPropertyName("data")]
    public string Data { get; set; } // Base64 encoded string
}

// --- Response DTOs ---
public class GeminiResponse
{
    [JsonPropertyName("candidates")]
    public List<ResponseCandidate>? Candidates { get; set; }

    [JsonPropertyName("promptFeedback")]
    public PromptFeedback? PromptFeedback { get; set; }
}

public class ResponseCandidate // Renamed to avoid conflict if Candidate class exists elsewhere
{
    [JsonPropertyName("content")]
    public ContentItem? Content { get; set; } // The model's response content

    [JsonPropertyName("finishReason")]
    public string? FinishReason { get; set; } // e.g., "STOP", "MAX_TOKENS", "SAFETY", "RECITATION"

    [JsonPropertyName("safetyRatings")]
    public List<SafetyRating>? SafetyRatings { get; set; }

    // Helper to get the first text part, which is common for simple responses
    public string? GetFirstTextPart() => Content?.Parts?.FirstOrDefault(p => !string.IsNullOrEmpty(p.Text))?.Text;
}

public class SafetyRating
{
    [JsonPropertyName("category")]
    public string? Category { get; set; } // e.g., "HARM_CATEGORY_SEXUALLY_EXPLICIT"

    [JsonPropertyName("probability")]
    public string? Probability { get; set; } // e.g., "NEGLIGIBLE", "LOW", "MEDIUM", "HIGH"

    [JsonPropertyName("blocked")] // This property might not always be present if not blocked.
    public bool? Blocked { get; set; }
}

public class PromptFeedback
{
    [JsonPropertyName("blockReason")]
    public string? BlockReason { get; set; } // If the prompt itself was blocked

    [JsonPropertyName("safetyRatings")]
    public List<SafetyRating>? SafetyRatings { get; set; }
}