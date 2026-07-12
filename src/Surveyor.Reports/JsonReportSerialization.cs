using System.Globalization;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace Surveyor.Reports;

internal static class JsonReportSerialization
{
    private static readonly JsonSerializerOptions StringOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    internal static byte[] Serialize(ReportJsonDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        StringBuilder builder = new();
        builder.Append("{\n");
        AppendLine(builder, 1, "\"schemaVersion\": ", Quote(ReportConstants.SchemaVersion), true);
        AppendLine(builder, 1, "\"documentKind\": ", Quote(ReportConstants.DocumentKind), true);
        AppendRun(builder, document.Run);
        AppendConfidentiality(builder, document.Confidentiality);
        AppendScreen(builder, document.Screen);
        AppendScore(builder, document.Score);
        AppendAxes(builder, document.Axes);
        AppendFindings(builder, document.Findings);
        AppendImprovementCandidates(builder, document.ImprovementCandidates);
        AppendStages(builder, document.Stages);
        AppendDiagnostics(builder, document.Diagnostics);
        AppendSerialization(builder);
        builder.Append("}\n");
        return Encoding.UTF8.GetBytes(builder.ToString());
    }

    private static void AppendRun(StringBuilder builder, ReportRunSection run)
    {
        builder.Append("  \"run\": {\n");
        AppendLine(builder, 2, "\"runId\": ", Quote(run.RunId), true);
        AppendLine(builder, 2, "\"outcome\": ", Quote(run.Outcome), true);
        AppendLine(builder, 2, "\"startedAtUtc\": ", Quote(run.StartedAtUtc), true);
        AppendLine(builder, 2, "\"completedAtUtc\": ", Quote(run.CompletedAtUtc), true);
        AppendLine(builder, 2, "\"generatedAtUtc\": ", Quote(run.GeneratedAtUtc), true);
        AppendLine(builder, 2, "\"targetSafeId\": ", Quote(run.TargetSafeId), true);
        AppendLine(builder, 2, "\"screenSelectionMetadataPresent\": ", FormatBoolean(run.ScreenSelectionMetadataPresent), false);
        builder.Append("  },\n");
    }

    private static void AppendConfidentiality(StringBuilder builder, ReportConfidentialitySection confidentiality)
    {
        builder.Append("  \"confidentiality\": {\n");
        AppendLine(builder, 2, "\"mode\": ", Quote(confidentiality.Mode), true);
        AppendLine(builder, 2, "\"policyVersion\": ", Quote(confidentiality.PolicyVersion), true);
        AppendLine(builder, 2, "\"decidedAtUtc\": ", Quote(confidentiality.DecidedAtUtc), true);
        AppendLine(builder, 2, "\"decisionSource\": ", Quote(confidentiality.DecisionSource), true);
        AppendLine(builder, 2, "\"optOutReasonCode\": ", QuoteOrNull(confidentiality.OptOutReasonCode), true);
        AppendStringArray(builder, 2, "appliedTransforms", confidentiality.AppliedTransforms, true);
        AppendLine(builder, 2, "\"handlingNoticeCode\": ", Quote(confidentiality.HandlingNoticeCode), false);
        builder.Append("  },\n");
    }

    private static void AppendScreen(StringBuilder builder, ReportScreenSection screen)
    {
        builder.Append("  \"screen\": {\n");
        AppendLine(builder, 2, "\"screenKey\": ", Quote(screen.ScreenKey), true);
        AppendLine(builder, 2, "\"keyVersion\": ", Quote(screen.KeyVersion), true);
        AppendLine(builder, 2, "\"isFallback\": ", FormatBoolean(screen.IsFallback), true);
        AppendLine(builder, 2, "\"stableAcrossExports\": ", FormatBoolean(screen.StableAcrossExports), true);
        AppendLine(builder, 2, "\"elementCount\": ", screen.ElementCount.ToString(CultureInfo.InvariantCulture), true);
        AppendLine(builder, 2, "\"unavailableElementCount\": ", screen.UnavailableElementCount.ToString(CultureInfo.InvariantCulture), true);
        AppendLine(builder, 2, "\"availabilitySummary\": ", Quote(screen.AvailabilitySummary), false);
        builder.Append("  },\n");
    }

    private static void AppendScore(StringBuilder builder, ReportScoreSection score)
    {
        builder.Append("  \"score\": {\n");
        AppendLine(builder, 2, "\"screenKey\": ", Quote(score.ScreenKey), true);
        AppendLine(builder, 2, "\"scoringConfigVersion\": ", Quote(score.ScoringConfigVersion), true);
        AppendLine(builder, 2, "\"candidateRulesVersion\": ", Quote(score.CandidateRulesVersion), true);
        AppendLine(builder, 2, "\"aggregateScoreBp\": ", score.AggregateScoreBp.ToString(CultureInfo.InvariantCulture), true);
        AppendLine(builder, 2, "\"aggregateScorePercentText\": ", Quote(score.AggregateScorePercentText), true);
        AppendLine(builder, 2, "\"testabilityClass\": ", Quote(score.TestabilityClass), true);
        AppendLine(builder, 2, "\"confidence\": ", Quote(score.Confidence), true);
        AppendLine(builder, 2, "\"priorityBasis\": ", QuoteOrNull(score.PriorityBasis), false);
        builder.Append("  },\n");
    }

    private static void AppendAxes(StringBuilder builder, IReadOnlyList<ReportAxisSection> axes)
    {
        builder.Append("  \"axes\": [\n");
        for (int index = 0; index < axes.Count; index++)
        {
            ReportAxisSection axis = axes[index];
            builder.Append("    {\n");
            AppendLine(builder, 3, "\"axis\": ", Quote(axis.Axis), true);
            AppendLine(builder, 3, "\"applicability\": ", Quote(axis.Applicability), true);
            AppendLine(builder, 3, "\"scoreBp\": ", FormatNullableInt(axis.ScoreBp), true);
            AppendLine(builder, 3, "\"scorePercentText\": ", QuoteOrNull(axis.ScorePercentText), true);
            AppendLine(builder, 3, "\"confidence\": ", Quote(axis.Confidence), true);
            AppendInlineStringArray(builder, 3, "findingIds", axis.FindingIds, true);
            AppendInlineStringArray(builder, 3, "evidenceCodes", axis.EvidenceCodes, false);
            builder.Append(index == axes.Count - 1 ? "    }\n" : "    },\n");
        }

        builder.Append("  ],\n");
    }

    private static void AppendFindings(StringBuilder builder, IReadOnlyList<ReportFindingSection> findings)
    {
        builder.Append("  \"findings\": [\n");
        for (int index = 0; index < findings.Count; index++)
        {
            ReportFindingSection finding = findings[index];
            builder.Append("    {\n");
            AppendLine(builder, 3, "\"id\": ", Quote(finding.Id), true);
            AppendLine(builder, 3, "\"code\": ", Quote(finding.Code), true);
            AppendLine(builder, 3, "\"axis\": ", Quote(finding.Axis), true);
            AppendLine(builder, 3, "\"rootCause\": ", Quote(finding.RootCause), true);
            AppendLine(builder, 3, "\"severity\": ", Quote(finding.Severity), true);
            AppendLine(builder, 3, "\"elementKey\": ", QuoteOrNull(finding.ElementKey), true);
            AppendLine(builder, 3, "\"availability\": ", QuoteOrNull(finding.Availability), true);
            AppendLine(builder, 3, "\"acquisitionConfidence\": ", QuoteOrNull(finding.AcquisitionConfidence), true);
            AppendInlineStringArray(builder, 3, "relatedFindingIds", finding.RelatedFindingIds, true);
            AppendLine(builder, 3, "\"recommendationCode\": ", Quote(finding.RecommendationCode), false);
            builder.Append(index == findings.Count - 1 ? "    }\n" : "    },\n");
        }

        builder.Append("  ],\n");
    }

    private static void AppendImprovementCandidates(StringBuilder builder, IReadOnlyList<ReportImprovementCandidateSection> candidates)
    {
        builder.Append("  \"improvementCandidates\": [\n");
        for (int index = 0; index < candidates.Count; index++)
        {
            ReportImprovementCandidateSection candidate = candidates[index];
            builder.Append("    {\n");
            AppendLine(builder, 3, "\"id\": ", Quote(candidate.Id), true);
            AppendLine(builder, 3, "\"code\": ", Quote(candidate.Code), true);
            AppendLine(builder, 3, "\"rootCause\": ", Quote(candidate.RootCause), true);
            AppendLine(builder, 3, "\"primaryAxis\": ", Quote(candidate.PrimaryAxis), true);
            AppendLine(builder, 3, "\"targetElementKey\": ", QuoteOrNull(candidate.TargetElementKey), true);
            AppendLine(builder, 3, "\"affectedElementCount\": ", candidate.AffectedElementCount.ToString(CultureInfo.InvariantCulture), true);
            AppendLine(builder, 3, "\"expectedEffect\": ", Quote(candidate.ExpectedEffect), true);
            AppendInlineStringArray(builder, 3, "sourceFindingIds", candidate.SourceFindingIds, true);
            AppendLine(builder, 3, "\"scope\": ", Quote(candidate.Scope), true);
            AppendLine(builder, 3, "\"userSuppliedPriorityBasis\": ", QuoteOrNull(candidate.UserSuppliedPriorityBasis), false);
            builder.Append(index == candidates.Count - 1 ? "    }\n" : "    },\n");
        }

        builder.Append("  ],\n");
    }

    private static void AppendStages(StringBuilder builder, IReadOnlyList<ReportStageSection> stages)
    {
        builder.Append("  \"stages\": [\n");
        for (int index = 0; index < stages.Count; index++)
        {
            ReportStageSection stage = stages[index];
            builder.Append("    {\n");
            AppendLine(builder, 3, "\"stage\": ", Quote(stage.Stage), true);
            AppendLine(builder, 3, "\"status\": ", Quote(stage.Status), true);
            AppendLine(builder, 3, "\"timeoutBudgetMs\": ", FormatNullableInt(stage.TimeoutBudgetMs), true);
            AppendInlineStringArray(builder, 3, "diagnosticCodes", stage.DiagnosticCodes, false);
            builder.Append(index == stages.Count - 1 ? "    }\n" : "    },\n");
        }

        builder.Append("  ],\n");
    }

    private static void AppendDiagnostics(StringBuilder builder, IReadOnlyList<ReportDiagnosticSection> diagnostics)
    {
        builder.Append("  \"diagnostics\": [\n");
        for (int index = 0; index < diagnostics.Count; index++)
        {
            ReportDiagnosticSection diagnostic = diagnostics[index];
            builder.Append("    {\n");
            AppendLine(builder, 3, "\"stage\": ", Quote(diagnostic.Stage), true);
            AppendLine(builder, 3, "\"severity\": ", Quote(diagnostic.Severity), true);
            AppendLine(builder, 3, "\"code\": ", Quote(diagnostic.Code), true);
            AppendLine(builder, 3, "\"status\": ", Quote(diagnostic.Status), true);
            AppendLine(builder, 3, "\"elementKey\": ", QuoteOrNull(diagnostic.ElementKey), true);
            AppendSafeArgs(builder, diagnostic.SafeArgs);
            builder.Append(index == diagnostics.Count - 1 ? "    }\n" : "    },\n");
        }

        builder.Append("  ],\n");
    }

    private static void AppendSafeArgs(StringBuilder builder, IReadOnlyList<KeyValuePair<string, string>> safeArgs)
    {
        builder.Append("      \"safeArgs\": {\n");
        for (int index = 0; index < safeArgs.Count; index++)
        {
            KeyValuePair<string, string> pair = safeArgs[index];
            AppendLine(builder, 4, Quote(pair.Key) + ": ", Quote(pair.Value), index < safeArgs.Count - 1);
        }

        builder.Append("      }\n");
    }

    private static void AppendSerialization(StringBuilder builder)
    {
        builder.Append("  \"serialization\": {\n");
        AppendLine(builder, 2, "\"schemaVersion\": ", Quote(ReportConstants.SchemaVersion), true);
        AppendLine(builder, 2, "\"serializerVersion\": ", Quote(ReportConstants.SerializerVersion), true);
        AppendLine(builder, 2, "\"timestampFormat\": ", Quote(ReportConstants.TimestampFormat), true);
        AppendLine(builder, 2, "\"encoding\": ", Quote(ReportConstants.Encoding), true);
        AppendLine(builder, 2, "\"newline\": ", Quote(ReportConstants.Newline), true);
        AppendLine(builder, 2, "\"propertyOrder\": ", Quote(ReportConstants.PropertyOrder), true);
        AppendLine(builder, 2, "\"contentHashAlgorithm\": ", Quote(ReportConstants.ContentHashAlgorithm), false);
        builder.Append("  }\n");
    }

    private static void AppendStringArray(StringBuilder builder, int indent, string propertyName, IReadOnlyList<string> values, bool trailingComma)
    {
        AppendLine(builder, indent, Quote(propertyName) + ": ", FormatInlineStringArray(values), trailingComma);
    }

    private static void AppendInlineStringArray(StringBuilder builder, int indent, string propertyName, IReadOnlyList<string> values, bool trailingComma)
    {
        AppendLine(builder, indent, Quote(propertyName) + ": ", FormatInlineStringArray(values), trailingComma);
    }

    private static void AppendLine(StringBuilder builder, int indentLevel, string propertyPrefix, string value, bool trailingComma)
    {
        builder.Append(' ', indentLevel * 2);
        builder.Append(propertyPrefix);
        builder.Append(value);
        if (trailingComma)
        {
            builder.Append(',');
        }

        builder.Append('\n');
    }

    private static string Quote(string value)
    {
        return JsonSerializer.Serialize(value, StringOptions);
    }

    private static string QuoteOrNull(string? value)
    {
        return value is null ? "null" : Quote(value);
    }

    private static string FormatBoolean(bool value)
    {
        return value ? "true" : "false";
    }

    private static string FormatNullableInt(int? value)
    {
        return value?.ToString(CultureInfo.InvariantCulture) ?? "null";
    }

    private static string FormatInlineStringArray(IReadOnlyList<string> values)
    {
        return "[" + string.Join(", ", values.Select(Quote)) + "]";
    }
}
