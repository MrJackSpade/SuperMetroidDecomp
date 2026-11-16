using System.Text.Json;
using System.Text.Json.Serialization;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>Defines the installed enemy overread file (#1500).</summary>
internal static class EnemyOverreadFormat
{
    /// <summary>Supported enemy overread schema revision; revision one holds Magdollite's apex growth and the falling spark's unauthored launch.</summary>
    internal const int Version = 1;
    /// <summary>JSON filename holding the decoded over-read results.</summary>
    internal const string FileName = "enemy-overreads.json";
    /// <summary>Shared camel-case, case-insensitive JSON options that reject unknown fields; the file is written compactly.</summary>
    internal static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = false,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
    };
}
