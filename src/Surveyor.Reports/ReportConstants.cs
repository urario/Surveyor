namespace Surveyor.Reports;

internal static class ReportConstants
{
    internal const string SchemaVersion = "surveyor.report.v1";
    internal const string DocumentKind = "SurveyorAnalysisReport";
    internal const string SerializerVersion = "deterministic-json-v1";
    internal const string TimestampFormat = "yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'";
    internal const string Encoding = "utf-8-no-bom";
    internal const string Newline = "lf";
    internal const string PropertyOrder = "explicit-v1";
    internal const string ContentHashAlgorithm = "SHA-256";
}
