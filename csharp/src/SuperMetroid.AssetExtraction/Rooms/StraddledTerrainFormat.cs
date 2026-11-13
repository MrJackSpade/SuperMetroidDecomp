using System.Text.Json;
using System.Text.Json.Serialization;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Defines the installed straddled-terrain file (#1612).</summary>
internal static class StraddledTerrainFormat
{
    /// <summary>
    /// Supported straddled-terrain schema revision. Revision one holds, for every block a Wave shot's scan can land halfway
    /// through, the lead its end contributes; for every block number, the tail its start contributes; and for every lead
    /// and tail, the terrain they make (#1612). Revision two lists the classes of blocks that share a lead or a tail by
    /// their collision kind, mirroring and block numbers, and the terrain each lead class makes with each tail class, in
    /// place of leads and tails numbered by the level word's stored halves (#1709). Revision three names unrevealed air
    /// (#1722).
    /// </summary>
    internal const int Version = 3;

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
