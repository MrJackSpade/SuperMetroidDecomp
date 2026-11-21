using SuperMetroid.Core;
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
    /// under <c>animation</c> (#1624); revision twenty-one names each animation program's successor under <c>animationSuccessors</c> (#1758);
    /// revision twenty-two holds named outcomes in place of the programs' layout: the cue-value steps by the frame's name, what
    /// each reachable frame past an animation reads, and where each named place lands in every other animation (#2048); revision twenty-three names the special attack animations
    /// for the move (#2122); revision twenty-four keys every animation's outcomes by the animation's own name, and a read
    /// past an animation names the later animation rather than a pose kept to name it (#2048); revision twenty-five names the
    /// projectile animations for the travel they serve and holds the shinespark echo's still picture as one place (#2347);
    /// revision twenty-six names the frame each frame past a segment's cue goes on to, and holds the knockback's unnamed
    /// slow recoil as frames past the knockback's cue (#2395); revision twenty-seven states the unauthored beams' launch
    /// speeds in pixels an update (#2555); revision twenty-eight keys SpaceTime's overwrites by the shot place's name, not
    /// by a list position (#2565).
    /// </summary>
    internal const int Version = 28;

    /// <summary>The installed name of Samus's shot place <paramref name="place"/>, which keys what SpaceTime's copy overwrites from it.</summary>
    internal static string ShotPlaceName(int place) =>
        string.Create(System.Globalization.CultureInfo.InvariantCulture, $"place-{Ensure.AtLeastZero(place)}");
    /// <summary>JSON filename holding the decoded over-read results.</summary>
    internal const string FileName = "samus-overreads.json";
    /// <summary>Shared camel-case, case-insensitive JSON options that reject unknown fields; the file is written compactly.</summary>
    internal static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = false,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        RespectNullableAnnotations = true,
        TypeInfoResolver = InstalledDocumentElements.Resolver,
        RespectRequiredConstructorParameters = true,
    };
}
