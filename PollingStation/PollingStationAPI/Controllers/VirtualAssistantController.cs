namespace PollingStationAPI.Controllers;

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http; // Required for StatusCodes
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging; // Optional: for logging
using System;
using System.Threading;
using System.Threading.Tasks;

// Make sure your IVirtualAssistantService and AssistantQuestionRequest are in accessible namespaces
// using YourProject.Services; // If IVirtualAssistantService is there
// using YourProject.Models;  // If AssistantQuestionRequest is there

[ApiController]
[Route("api/assistant")] // Base route for this controller
public class VirtualAssistantController : ControllerBase
{
    private readonly IVirtualAssistantService _virtualAssistantService;
    private readonly ILogger<VirtualAssistantController> _logger; // Optional but recommended

    public VirtualAssistantController(
        IVirtualAssistantService virtualAssistantService,
        ILogger<VirtualAssistantController> logger = null) // Logger can be optional
    {
        _virtualAssistantService = virtualAssistantService ?? throw new ArgumentNullException(nameof(virtualAssistantService));
        _logger = logger;
    }

    /// <summary>
    /// Asks a question to the virtual assistant.
    /// </summary>
    /// <param name="request">The request containing the question.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The assistant's answer.</returns>
    [HttpPost("ask")] // Endpoint: POST /api/assistant/ask
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)] // Using object for { Answer = "..." }
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AskQuestion(
        [FromBody] AssistantQuestionRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) // Checks for [Required], [MinLength] etc. from DTO
        {
            return BadRequest(ModelState);
        }

        _logger?.LogInformation("Received question for virtual assistant: {Question}", request.Question);

        try
        {
            string answer = await _virtualAssistantService.GetAnswer(request.Question, cancellationToken);

            // The service itself might return a string indicating an error (e.g., "API key missing").
            // The controller will return this as part of a 200 OK response unless an exception was thrown.
            // You could add more sophisticated checks here if the service had specific error codes/objects.
            return Ok(new { Answer = answer }); // Wrapping the string in an object for a structured JSON response
        }
        catch (ArgumentNullException ex) // Example of catching specific exceptions from the service
        {
            _logger?.LogWarning(ex, "A required argument was null while processing question: {UserQuestion}", request.Question);
            return BadRequest(new { Error = ex.Message });
        }
        catch (InvalidOperationException ex) // Example for configuration errors from the service
        {
            _logger?.LogError(ex, "Invalid operation (e.g., configuration error) while processing question: {UserQuestion}", request.Question);
            return StatusCode(StatusCodes.Status500InternalServerError, new { Error = $"Service configuration error: {ex.Message}" });
        }
        catch (TaskCanceledException ex) when (ex.CancellationToken == cancellationToken)
        {
            _logger?.LogInformation("Request was canceled by the client for question: {UserQuestion}", request.Question);
            return StatusCode(StatusCodes.Status499ClientClosedRequest, new { Error = "Request was canceled by the client." });
        }
        catch (TaskCanceledException ex) // Typically from HttpClient timeout in the service
        {
            _logger?.LogError(ex, "The operation timed out while processing question: {UserQuestion}", request.Question);
            return StatusCode(StatusCodes.Status504GatewayTimeout, new { Error = "The request to an upstream service timed out." });
        }
        catch (Exception ex) // Catch-all for other unexpected errors from the service
        {
            _logger?.LogError(ex, "An unexpected error occurred while processing question: {UserQuestion}", request.Question);
            return StatusCode(StatusCodes.Status500InternalServerError, new { Error = "An unexpected error occurred. Please try again later." });
        }
    }
}

public class AssistantQuestionRequest
{
    [Required(ErrorMessage = "A question is required.")]
    [MinLength(1, ErrorMessage = "Question cannot be empty.")]
    public string Question { get; set; }
}