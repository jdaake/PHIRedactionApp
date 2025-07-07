namespace PHIRedactionAPI.Models
{
    /// <summary>
    /// Result of processing multiple files for PHI redaction
    /// </summary>
    public class ProcessingResult
    {
        /// <summary>
        /// Total number of files submitted for processing
        /// </summary>
        public int TotalFiles { get; set; }
        
        /// <summary>
        /// Number of files successfully processed
        /// </summary>
        public int SuccessfulFiles { get; set; }
        
        /// <summary>
        /// Detailed results for each file processed
        /// </summary>
        public List<FileProcessingResult> Results { get; set; } = new();
    }

    /// <summary>
    /// Result of processing a single file for PHI redaction
    /// </summary>
    public class FileProcessingResult
    {
        /// <summary>
        /// Original filename of the uploaded file
        /// </summary>
        public string FileName { get; set; } = string.Empty;
        
        /// <summary>
        /// Filename of the processed file with redacted PHI
        /// </summary>
        public string ProcessedFileName { get; set; } = string.Empty;
        
        /// <summary>
        /// Whether the file was successfully processed
        /// </summary>
        public bool Success { get; set; }
        
        /// <summary>
        /// Status message describing the processing result
        /// </summary>
        public string Message { get; set; } = string.Empty;
        
        /// <summary>
        /// URL to download the processed file (if successful)
        /// </summary>
        public string DownloadUrl { get; set; } = string.Empty;
    }
}
