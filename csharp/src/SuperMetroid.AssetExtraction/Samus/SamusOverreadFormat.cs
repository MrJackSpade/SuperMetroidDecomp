using System.Text.Json;
using System.Text.Json.Serialization;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>Defines the installed Samus overread file and its manifest.</summary>
internal static class SamusOverreadFormat
{
    /// <summary>
    /// Supported Samus overread schema revision. Revision eight holds each over-read's decoded domain result in place of
    /// cartridge byte ranges and list positions (#1474); revision nine holds SpaceTime's overwrite as the events, bosses,
    /// orbs, items, doors, stations and maps it decides rather than table bytes (#1475); revision ten adds what the Spazer
    /// and Plasma beams' launch, the Chainsaw beam's palette and the drained Shinespark crash read past their tables (#1500);
    /// revision eleven adds the Spazer and Plasma beams' cooldowns (#1506); revision twelve names the poses by their
    /// renamed identities and holds no entry for a record no gameplay path reaches (#1517); revision thirteen adds the step
    /// the animation reads at each place of a program that lands on an instruction's operand (#1535); revision fourteen
    /// holds no Chozo orbs, which nothing in the game reads (#1553); revision fifteen holds no trail start past a placement
    /// list, which leaves no trail (#1625).
    /// </summary>
    internal const int Version = 15;
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
