using System.Text.Json;
using System.Text.Json.Serialization;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>Defines the installed Samus overread file.</summary>
internal static class SamusOverreadFormat
{
    /// <summary>
    /// Supported Samus overread schema revision. Revision eight holds each over-read's decoded domain result in place of
    /// cartridge byte ranges and list positions (#1474); revision nine holds SpaceTime's overwrite as the events, bosses,
    /// orbs, items, doors, stations and maps it decides rather than table bytes (#1475); revision ten adds what the Spazer
    /// and Plasma beams' launch, the Chainsaw beam's palette and the drained Shinespark crash read past their tables (#1500);
    /// revision eleven adds the Spazer and Plasma beams' cooldowns (#1506); revision twelve names the poses by their
    /// renamed identities and holds no entry for a record no gameplay path reaches (#1517); revision thirteen adds the step
    /// the animation reads at each place of a program that lands on a value one of its cues carries (#1535); revision fourteen
    /// holds no Chozo orbs, which nothing in the game reads (#1553); revision fifteen holds no trail start past a placement
    /// list, which leaves no trail (#1625); revision sixteen holds no waits below the charge flare's animations, which no
    /// runtime reaches (#1624); revision seventeen keys the steps a program's places read on a cue's value as cue-value steps
    /// (#1624); revision eighteen names the story events, item locations, doors and used stations SpaceTime's overwrite
    /// decides, the Zebetite count it leaves and the startup route it saves, in place of the save image's flag numbering
    /// and the saved loading-state word (#1709); revision nineteen holds neither the Speed Booster's overrun colours nor the
    /// Chainsaw beam's colours, which are import data (#1693); revision twenty names each animation position's animation
    /// under <c>animation</c> (#1624).
    /// </summary>
    internal const int Version = 20;
    /// <summary>JSON filename holding the decoded over-read results.</summary>
    internal const string FileName = "samus-overreads.json";
    /// <summary>Shared camel-case, case-insensitive JSON options that reject unknown fields; the file is written compactly.</summary>
    internal static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = false,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
    };
}
