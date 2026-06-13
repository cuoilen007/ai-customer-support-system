using AI.CustomerSupport.API.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace AI.CustomerSupport.API.Data
{
    public static class LegacyTrainingDataImporter
    {
        private const string LegacySource = "LegacyCsv";

        public static async Task<int> ImportAsync(
            AppDbContext context,
            string contentRootPath,
            ILogger logger)
        {
            var csvPath = Path.GetFullPath(
                Path.Combine(
                    contentRootPath,
                    "..",
                    "..",
                    "AI-Service",
                    "training",
                    "tickets.csv"));

            if (!File.Exists(csvPath))
            {
                logger.LogInformation(
                    "Legacy training CSV was not found at {CsvPath}. Skipping import.",
                    csvPath);
                return 0;
            }

            var rows = await File.ReadAllLinesAsync(csvPath);

            if (rows.Length <= 1)
            {
                return 0;
            }

            var existingReferenceList = await context.ModelTrainingExamples
                .Where(x => x.Source == LegacySource)
                .Select(x => x.SourceReference)
                .ToListAsync();
            var existingReferences = new HashSet<string>(existingReferenceList);

            var additions = new List<ModelTrainingExample>();

            foreach (var rawLine in rows.Skip(1))
            {
                if (string.IsNullOrWhiteSpace(rawLine))
                {
                    continue;
                }

                var parsed = ParseCsvLine(rawLine);

                if (parsed == null)
                {
                    continue;
                }

                var sourceReference = BuildLegacySourceReference(
                    parsed.Value.Text,
                    parsed.Value.Category);

                if (existingReferences.Contains(sourceReference))
                {
                    continue;
                }

                additions.Add(new ModelTrainingExample
                {
                    ChatEvaluationId = null,
                    Input = parsed.Value.Text,
                    ExpectedOutput = string.Empty,
                    OriginalAnswer = string.Empty,
                    Category = parsed.Value.Category,
                    Intent = "LegacySeed",
                    PrimarySourceId = "tickets.csv",
                    PrimarySourceType = "legacy_csv",
                    Status = "Ready",
                    IsActive = true,
                    Source = LegacySource,
                    SourceReference = sourceReference,
                    ImportedAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });

                existingReferences.Add(sourceReference);
            }

            if (additions.Count == 0)
            {
                return 0;
            }

            context.ModelTrainingExamples.AddRange(additions);
            await context.SaveChangesAsync();

            logger.LogInformation(
                "Imported {Count} legacy training rows from {CsvPath}.",
                additions.Count,
                csvPath);

            return additions.Count;
        }

        private static (string Text, string Category)? ParseCsvLine(string line)
        {
            var trimmed = line.Trim();
            var separatorIndex = trimmed.LastIndexOf(',');

            if (separatorIndex <= 0 || separatorIndex >= trimmed.Length - 1)
            {
                return null;
            }

            var text = trimmed[..separatorIndex].Trim().Trim('"');
            var category = trimmed[(separatorIndex + 1)..].Trim().Trim('"');

            if (string.IsNullOrWhiteSpace(text) || string.IsNullOrWhiteSpace(category))
            {
                return null;
            }

            return (text, category);
        }

        private static string BuildLegacySourceReference(string text, string category)
        {
            var normalized = $"{text.Trim()}|{category.Trim()}";
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(normalized));
            return Convert.ToHexString(bytes)[..24];
        }
    }
}
