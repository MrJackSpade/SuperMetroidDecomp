using System.Text.Json;

namespace SuperMetroid.Core.Samus;

/// <summary>Defines the installed Samus overread file and its manifest.</summary>
public static class SamusOverreadFormat
{
    /// <summary>Supported Samus overread schema revision.</summary>
    public const int Version = 6;
    /// <summary>JSON filename holding the overread byte ranges.</summary>
    public const string FileName = "samus-overreads.json";
    /// <summary>Manifest filename recording the overread file's cartridge identity and digest.</summary>
    public const string ManifestFileName = "samus-overreads-manifest.json";
    /// <summary>Shared camel-case, case-insensitive JSON options that reject unknown fields.</summary>
    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        UnmappedMemberHandling = System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow,
    };
}
