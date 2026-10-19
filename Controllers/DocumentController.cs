/*with api key

using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using UglyToad.PdfPig;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;

namespace RAG_API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DocumentController : ControllerBase
{
    private readonly IConfiguration _configuration;

    // In-memory placeholder for the document text until a vector database is connected
    private static string _uploadedDocumentText = @"
        Enterprise Service Level Agreement (SLA)
        1. Service Availability: The Provider guarantees a 99.9% uptime for the API services per calendar month.
        2. Penalties for Non-Compliance: If the uptime falls below 99.9% but is above 99.0%, the Customer is entitled to a 10% credit. If the uptime falls below 99.0%, the Customer is entitled to a 30% credit.
        3. Support Response Times: Priority 1 (Critical) issues will receive a response within 1 hour. Priority 2 (High) within 4 hours. Priority 3 (Standard) within 24 hours.
        4. Termination: Either party may terminate this agreement with 30 days written notice.";

    public DocumentController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpPost("upload")]
    public IActionResult UploadDocument(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("No file was uploaded.");
        }

        string extractedText = string.Empty;

        try
        {
            using (var stream = file.OpenReadStream())
            using (var pdfDocument = PdfDocument.Open(stream))
            {
                foreach (var page in pdfDocument.GetPages())
                {
                    extractedText += ContentOrderTextExtractor.GetText(page) + "\n";
                }
            }

            // Store extracted text in memory so chat can reference it
            _uploadedDocumentText = extractedText;

            return Ok(new
            {
                Message = "Document uploaded and parsed successfully.",
                FileName = file.FileName,
                TotalCharacters = extractedText.Length,
                ExtractedTextPreview = extractedText.Substring(0, Math.Min(extractedText.Length, 300)) + "..."
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Failed to parse PDF: {ex.Message}");
        }
    }

    [HttpPost("chat")]
    public async Task<IActionResult> ChatWithDocument([FromBody] string userQuery)
    {
        var apiKey = _configuration["OpenAI:ApiKey"];

        if (string.IsNullOrWhiteSpace(apiKey) || apiKey == "sk-your-actual-api-key-here")
        {
            return BadRequest("A valid OpenAI API key must be configured in appsettings.Development.json.");
        }

        var builder = Kernel.CreateBuilder();
        builder.AddOpenAIChatCompletion(
            modelId: "gpt-4o-mini",
            apiKey: apiKey
        );

        var kernel = builder.Build();
        var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

        var chatHistory = new ChatHistory();
        chatHistory.AddSystemMessage(
            "You are a strict enterprise document assistant. " +
            "You will be provided with document text. " +
            "You must answer the user's question USING ONLY the provided document text. " +
            "If the answer is not contained in the provided text, you must reply EXACTLY with: " +
            "'I cannot answer this based on the provided document.' " +
            "Do not use any outside knowledge or make up information."
        );

        // Grounding with whatever document text is currently held in memory
        chatHistory.AddSystemMessage($"DOCUMENT TEXT:\n{_uploadedDocumentText}");
        chatHistory.AddUserMessage(userQuery);

        try
        {
            var response = await chatCompletionService.GetChatMessageContentAsync(
                chatHistory,
                kernel: kernel
            );

            return Ok(new
            {
                Query = userQuery,
                Answer = response.Content
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error communicating with OpenAI: {ex.Message}");
        }
    }
}*/

// Note: Below code is for local testing (without apikey)
using Microsoft.AspNetCore.Mvc;
using UglyToad.PdfPig;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;

namespace RAG_API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DocumentController : ControllerBase
{
    // In-memory placeholder for the document text
    private static string _uploadedDocumentText = @"
        Enterprise Service Level Agreement (SLA)
        1. Service Availability: The Provider guarantees a 99.9% uptime for the API services per calendar month.
        2. Penalties for Non-Compliance: If the uptime falls below 99.9% but is above 99.0%, the Customer is entitled to a 10% credit. If the uptime falls below 99.0%, the Customer is entitled to a 30% credit.
        3. Support Response Times: Priority 1 (Critical) issues will receive a response within 1 hour. Priority 2 (High) within 4 hours. Priority 3 (Standard) within 24 hours.
        4. Termination: Either party may terminate this agreement with 30 days written notice.";

    [HttpPost("upload")]
    public IActionResult UploadDocument(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("No file was uploaded.");
        }

        string extractedText = string.Empty;

        try
        {
            using (var stream = file.OpenReadStream())
            using (var pdfDocument = PdfDocument.Open(stream))
            {
                foreach (var page in pdfDocument.GetPages())
                {
                    extractedText += ContentOrderTextExtractor.GetText(page) + "\n";
                }
            }

            _uploadedDocumentText = extractedText;

            return Ok(new
            {
                Message = "Document uploaded and parsed successfully.",
                FileName = file.FileName,
                TotalCharacters = extractedText.Length,
                ExtractedTextPreview = extractedText.Substring(0, Math.Min(extractedText.Length, 300)) + "..."
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Failed to parse PDF: {ex.Message}");
        }
    }

    [HttpPost("chat")]
    public IActionResult ChatWithDocument([FromBody] string userQuery)
    {
        if (string.IsNullOrWhiteSpace(_uploadedDocumentText))
        {
            return BadRequest("No document context available.");
        }

        // SIMULATED AI: Extract words from the user query longer than 4 characters to use as search terms
        var queryKeywords = userQuery.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                                     .Where(w => w.Length > 4)
                                     .ToList();

        // Check if any of these keywords exist in the uploaded PDF text
        bool foundContext = queryKeywords.Any(word => 
            _uploadedDocumentText.Contains(word, StringComparison.OrdinalIgnoreCase));

        if (foundContext)
        {
            return Ok(new
            {
                Query = userQuery,
                Answer = "[SIMULATED AI] I found relevant information in your document to answer this query. (In a real application, the LLM would summarize the exact data here)."
            });
        }
        else
        {
            // Grounding simulation: Keyword not found in document
            return Ok(new
            {
                Query = userQuery,
                Answer = "I cannot answer this based on the provided document."
            });
        }
    }
}