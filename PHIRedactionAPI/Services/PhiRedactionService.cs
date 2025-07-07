using System.Text.RegularExpressions;

namespace PHIRedactionAPI.Services
{
    public interface IPhiRedactionService
    {
        string RedactPhi(string content);
    }

    public class PhiRedactionService : IPhiRedactionService
    {
        private readonly ILogger<PhiRedactionService> _logger;
        private const string REDACTED_PLACEHOLDER = "[REDACTED]";
        private readonly Dictionary<string, Regex> _phiPatterns;

        public PhiRedactionService(ILogger<PhiRedactionService> logger)
        {
            _logger = logger;
            _phiPatterns = InitializePhiPatterns();
        }

        public string RedactPhi(string content)
        {
            if (string.IsNullOrEmpty(content))
                return content;

            var redactedContent = content;

            foreach (var pattern in _phiPatterns)
            {
                redactedContent = pattern.Value.Replace(redactedContent, match =>
                {
                    _logger.LogDebug("Redacting {PatternType}: {Match}", pattern.Key, match.Value);
                    return ReplaceWithRedacted(match, pattern.Key);
                });
            }

            return redactedContent;
        }

        private Dictionary<string, Regex> InitializePhiPatterns()
        {
            return new Dictionary<string, Regex>
            {
                ["PatientName"] = new Regex(@"(?i)Patient\s+Name\s*:\s*([A-Za-z\s]+?)(?=\n|$)", RegexOptions.Compiled),
                ["DateOfBirth"] = new Regex(@"(?i)Date\s+of\s+Birth\s*:\s*(\d{1,2}[-/]\d{1,2}[-/]\d{2,4})", RegexOptions.Compiled),
                ["SSN"] = new Regex(@"(?i)Social\s+Security\s+Number\s*:\s*(\d{3}-\d{2}-\d{4})", RegexOptions.Compiled),
                ["Address"] = new Regex(@"(?i)Address\s*:\s*([^\n]+(?:\n[^\n:]*(?![A-Za-z]+\s*:))*)", RegexOptions.Compiled | RegexOptions.Multiline),
                ["PhoneNumber"] = new Regex(@"(?i)Phone\s+Number\s*:\s*(\(?\d{3}\)?[-.\s]?\d{3}[-.\s]?\d{4})", RegexOptions.Compiled),
                ["Email"] = new Regex(@"(?i)Email\s*:\s*([a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,})", RegexOptions.Compiled),
                ["MedicalRecordNumber"] = new Regex(@"(?i)Medical\s+Record\s+Number\s*:\s*(MRN-\d+)", RegexOptions.Compiled),
                ["StandaloneSSN"] = new Regex(@"\b\d{3}-\d{2}-\d{4}\b", RegexOptions.Compiled),
                ["StandalonePhone"] = new Regex(@"\b\(?\d{3}\)?[-.\s]?\d{3}[-.\s]?\d{4}\b", RegexOptions.Compiled),
                ["StandaloneEmail"] = new Regex(@"\b[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}\b", RegexOptions.Compiled),
                ["AccountNumber"] = new Regex(@"\b(?:Account|Acct)\.?\s*#?\s*(\d{6,})\b", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                ["ZipCode"] = new Regex(@"\b\d{5}(-\d{4})?\b", RegexOptions.Compiled),
                ["StandaloneDate"] = new Regex(@"\b\d{1,2}[-/]\d{1,2}[-/]\d{2,4}\b", RegexOptions.Compiled),
                ["IDNumber"] = new Regex(@"\b(?:ID|Patient\s+ID)\s*:?\s*(\d{6,})\b", RegexOptions.Compiled | RegexOptions.IgnoreCase)
            };
        }

        private string ReplaceWithRedacted(Match match, string patternType)
        {
            if (patternType.Contains("Patient") || patternType.Contains("Date") || 
                patternType.Contains("SSN") || patternType.Contains("Address") || 
                patternType.Contains("Phone") || patternType.Contains("Email") || 
                patternType.Contains("Medical"))
            {
                var parts = match.Value.Split(':', 2);
                if (parts.Length == 2)
                {
                    return $"{parts[0].Trim()}: {REDACTED_PLACEHOLDER}";
                }
            }

            return REDACTED_PLACEHOLDER;
        }
    }
}
