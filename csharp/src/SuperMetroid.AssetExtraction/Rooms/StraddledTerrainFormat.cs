using System.Text.Json;
using System.Text.Json.Serialization;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Defines the installed straddled-terrain file (#1612).</summary>
internal static class StraddledTerrainFormat
{
    /// <summary>
    /// Supported straddled-terrain schema revision. Revision one holds, for every block a Wave shot's scan can land halfway
    /// through, the lead its end contributes; for every block number, the tail its start contributes; and for every lead
    /// and tail, the terrain they make (#1612).
    /// </summary>
    internal const int Version = 1;

    /// <summary>JSON filename holding the decoded straddled terrain.</summary>
    internal const string FileName = "straddled-terrain.json";

    /// <summary>Shared camel-case, case-insensitive JSON options that reject unknown fields; the file is written compactly.</summary>
    internal static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = false,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        RespectRequiredConstructorParameters = true,
    };
}
