using System.Text.Json;
using System.Text.Json.Serialization;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>Defines the installed Samus overread file and its manifest.</summary>
internal static class SamusOverreadFormat
{
    /// <summary>
    /// Supported Samus overread schema revision. Revision eight holds each over-read's decoded domain result in place of
    /// cartridge byte ranges and list positions (#1474); revision nine holds SpaceTime's overwrite as the events, bosses,
    /// orbs, items, doors, stations and maps it decides rather than table bytes (#1475).
    /// </summary>
    internal const int Version = 9;
    /// <summary>JSON filename holding the decoded over-read results.</summary>
    internal const string FileName = "samus-overreads.json";
    /// <summary>Manifest filename recording the overread file's cartridge identity and digest.</summary>
    internal const string ManifestFileName = "samus-overreads-manifest.json";
    /// <summary>Shared camel-case, case-insensitive JSON options that reject unknown fields; the file is written compactly.</summary>
    internal static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = false,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
    };
}
