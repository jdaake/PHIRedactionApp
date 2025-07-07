using Microsoft.AspNetCore.Mvc;
using PHIRedactionAPI.Models;
using PHIRedactionAPI.Services;
using System.Text;

namespace PHIRedactionAPI.Controllers
{
    /// <summary>
    /// Controller for handling file upload, PHI redaction, and download operations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class FilesController : ControllerBase
    {
        private readonly IPhiRedactionService _phiRedactionService;
        private readonly ILogger<FilesController> _logger;

        public FilesController(IPhiRedactionService phiRedactionService, ILogger<FilesController> logger)
        {
            _phiRedactionService = phiRedactionService;
            _logger = logger;
        }

        /// <summary>
        /// Process uploaded files to redact PHI information
        /// </summary>
        /// <param name="files">List of text files containing lab order data</param>
        /// <returns>Processing results with download links for sanitized files</returns>
        /// <response code="200">Files processed successfully</response>
        /// <response code="400">No files uploaded or invalid file format</response>
        /// <response code="500">Internal server error</response>
        [HttpPost("process")]
        [ProducesResponseType(typeof(ProcessingResult), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<ProcessingResult>> ProcessFiles(List<IFormFile> files)
        {
            try
            {
                if (files == null || files.Count == 0)
                {
                    return BadRequest("No files uploaded");
                }

                var results = new List<FileProcessingResult>();

                foreach (var file in files)
                {
                    if (file.Length == 0)
                    {
                        results.Add(new FileProcessingResult
                        {
                            FileName = file.FileName,
                            Success = false,
                            Message = "File is empty"
                        });
                        continue;
                    }

                    if (!file.FileName.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
                    {
                        results.Add(new FileProcessingResult
                        {
                            FileName = file.FileName,
                            Success = false,
                            Message = "Only .txt files are supported"
                        });
                        continue;
                    }

                    try
                    {
                        using var reader = new StreamReader(file.OpenReadStream());
                        var content = await reader.ReadToEndAsync();
                        var redactedContent = _phiRedactionService.RedactPhi(content);
                        var sanitizedFileName = GenerateSanitizedFileName(file.FileName);
                        await SaveProcessedFile(sanitizedFileName, redactedContent);

                        results.Add(new FileProcessingResult
                        {
                            FileName = file.FileName,
                            ProcessedFileName = sanitizedFileName,
                            Success = true,
                            Message = "File processed successfully",
                            DownloadUrl = $"/api/files/download/{sanitizedFileName}"
                        });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing file {FileName}", file.FileName);
                        results.Add(new FileProcessingResult
                        {
                            FileName = file.FileName,
                            Success = false,
                            Message = $"Error processing file: {ex.Message}"
                        });
                    }
                }

                return Ok(new ProcessingResult
                {
                    TotalFiles = files.Count,
                    SuccessfulFiles = results.Count(r => r.Success),
                    Results = results
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ProcessFiles");
                return StatusCode(500, "Internal server error occurred");
            }
        }

        /// <summary>
        /// Download a processed file with redacted PHI
        /// </summary>
        /// <param name="filename">Name of the processed file to download</param>
        /// <returns>File content as downloadable attachment</returns>
        /// <response code="200">File downloaded successfully</response>
        /// <response code="404">File not found</response>
        /// <response code="500">Internal server error</response>
        [HttpGet("download/{filename}")]
        [ProducesResponseType(typeof(FileResult), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> DownloadFile(string filename)
        {
            try
            {
                _logger.LogInformation("Download request for file: {FileName}", filename);

                var outputPath = Path.Combine(Directory.GetCurrentDirectory(), "ProcessedFiles");
                var filePath = Path.Combine(outputPath, filename);

                _logger.LogInformation("Looking for file at path: {FilePath}", filePath);

                if (!System.IO.File.Exists(filePath))
                {
                    _logger.LogWarning("File not found: {FilePath}", filePath);
                    return NotFound($"File not found: {filename}");
                }

                var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
                _logger.LogInformation("File found, size: {FileSize} bytes", fileBytes.Length);

                // Set proper headers for download
                Response.Headers.Add("Access-Control-Expose-Headers", "Content-Disposition");
                
                return File(fileBytes, "text/plain", filename);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading file {FileName}", filename);
                return StatusCode(500, $"Error downloading file: {ex.Message}");
            }
        }

        /// <summary>
        /// List all processed files available for download
        /// </summary>
        /// <returns>List of processed file names</returns>
        /// <response code="200">Files listed successfully</response>
        /// <response code="500">Internal server error</response>
        [HttpGet("list")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(500)]
        public IActionResult ListProcessedFiles()
        {
            try
            {
                var outputPath = Path.Combine(Directory.GetCurrentDirectory(), "ProcessedFiles");
                
                if (!Directory.Exists(outputPath))
                {
                    return Ok(new { files = new string[0] });
                }

                var files = Directory.GetFiles(outputPath)
                    .Select(f => Path.GetFileName(f))
                    .ToArray();

                return Ok(new { files });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listing files");
                return StatusCode(500, "Error listing files");
            }
        }

        private string GenerateSanitizedFileName(string originalFileName)
        {
            var nameWithoutExtension = Path.GetFileNameWithoutExtension(originalFileName);
            var extension = Path.GetExtension(originalFileName);
            return $"{nameWithoutExtension}_sanitized{extension}";
        }

        private async Task<string> SaveProcessedFile(string fileName, string content)
        {
            var outputPath = Path.Combine(Directory.GetCurrentDirectory(), "ProcessedFiles");
            
            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }

            var filePath = Path.Combine(outputPath, fileName);
            await System.IO.File.WriteAllTextAsync(filePath, content, Encoding.UTF8);
            
            _logger.LogInformation("File saved: {FilePath}", filePath);
            
            return filePath;
        }
    }
}
