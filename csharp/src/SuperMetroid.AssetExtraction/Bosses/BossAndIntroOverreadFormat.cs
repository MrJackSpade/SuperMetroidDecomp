using System.Text.Json;
using System.Text.Json.Serialization;

namespace SuperMetroid.AssetExtraction.Bosses;

/// <summary>Defines the installed boss and intro overread file.</summary>
internal static class BossAndIntroOverreadFormat
{
    /// <summary>
    /// Supported schema revision. Revision one held Kraid's, Crocomire's and the egg shell's decoded reads (#1500); revision
    /// two names Crocomire's for the volley's last shot (#2464); revision three states each egg-shell fall step as one amount of
    /// pixels (#2555).
    /// </summary>
    internal const int Version = 3;
    /// <summary>JSON filename holding the decoded over-read results.</summary>
    internal const string FileName = "boss-intro-overreads.json";
    /// <summary>Shared camel-case, case-insensitive JSON options that reject unknown fields; the file is written compactly.</summary>
    internal static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = false,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        RespectNullableAnnotations = true,
        RespectRequiredConstructorParameters = true,
    };
}
