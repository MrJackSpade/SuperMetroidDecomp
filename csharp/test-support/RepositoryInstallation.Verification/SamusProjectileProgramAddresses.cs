using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// The projectile list layout, read from the supported cartridge, for checks that compare playbacks with native instruction
/// pointers. The game never sees a list position (#1474).
/// </summary>
internal static class SamusProjectileProgramAddresses
{
    /// <summary>The layout, read once from the repository's cartridge.</summary>
    private static readonly Lazy<SamusProjectileListLayout> layout = new(() => SamusProjectileListEntries.Read(
        CartridgeImportSource.Require(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")))));

    /// <summary>The layout.</summary>
    internal static SamusProjectileListLayout Layout => layout.Value;

    /// <summary>Each list place's instruction pointer, by segment: one per timed record, then the list's end.</summary>
    internal static IReadOnlyDictionary<SamusProjectileSegment, ushort[]> ListPlaceAddresses => Layout.ListPlacePositions;

    /// <summary>The list entry of <paramref name="program"/>.</summary>
    internal static ushort Entry(SamusProjectileSegment program) => Layout.ListPlacePositions[program][0];

    /// <summary>
    /// Whether the timed record at <paramref name="record"/> belongs to a list repeating one still picture, which a
    /// projectile holds once it shows it (#2347).
    /// </summary>
    internal static bool ShowsStillPicture(ushort record) =>
        Layout.TryPlaceAt(record, out SamusProjectileSegmentBeat place)
            ? SamusProjectileAnimations.Segments.Beats(place.Segment) is [AnimationBeat<SamusProjectileFrameId, NoAnimationCue>.HeldFrame]
            : throw new InvalidDataException($"$93:{record:X4} is not a Samus projectile list position.");

    /// <summary>The end of the list storing the timed record at <paramref name="record"/>: where a held picture's place stands.</summary>
    internal static ushort ListEndOf(ushort record) =>
        Layout.TryPlaceAt(record, out SamusProjectileSegmentBeat place)
            ? Layout.ListPlacePositions[place.Segment][^1]
            : throw new InvalidDataException($"$93:{record:X4} is not a Samus projectile list position.");

    /// <summary>Whether <paramref name="animation"/> holds a still picture in any of its phases (#2347), which no bomb does.</summary>
    internal static bool HoldsStillPicture(Animation<SamusProjectileSegment> animation) =>
        animation.Phases.Any(phase => SamusProjectileAnimations.Segments.Beats(phase.Segment)
            .Any(beat => beat is AnimationBeat<SamusProjectileFrameId, NoAnimationCue>.HeldFrame));

    /// <inheritdoc cref="SamusProjectileListLayoutAccess2.extension(SamusProjectileListLayout).Prefix"/>
    internal static ushort Prefix(SamusProjectileSegment program) => Layout.Prefix(program);

    /// <inheritdoc cref="SamusProjectileListLayout.Position(SamusProjectileSegmentBeat)"/>
    internal static ushort Address(SamusProjectileSegmentBeat place) => Layout.Position(place);

    /// <summary>The bank position of <paramref name="playback"/>, or zero for no playback.</summary>
    internal static ushort Address(AnimationPlayback<SamusProjectileSegment>? playback) => Layout.Position(playback);

    /// <summary>The playback the bank position <paramref name="address"/> names, or none for zero.</summary>
    internal static AnimationPlayback<SamusProjectileSegment>? PlaybackAt(ushort address) => Layout.PlaybackAt(address);

    /// <summary>
    /// A projectile animation standing at the bank position <paramref name="address"/>, as a native slot whose pointer names
    /// it: it shows nothing new until its <paramref name="updates"/>th update plays on from there (#2458).
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The address names no playback.</exception>
    internal static AnimationPlayer<SamusProjectileSegment, SamusProjectileFrameId, NoAnimationCue> PlayerAt(ushort address, int updates = 1)
    {
        AnimationPlayback<SamusProjectileSegment> playback = PlaybackAt(address)
            ?? throw new ArgumentOutOfRangeException(nameof(address), address, "The address names no projectile animation.");
        var player = AnimationPlayer<SamusProjectileSegment, SamusProjectileFrameId, NoAnimationCue>.StartingAfter(
            SamusProjectileAnimations.Segments, playback.Animation, updates);
        player.GoOnFrom(playback);
        return player;
    }

    /// <summary>
    /// A projectile animation standing at the bank position <paramref name="address"/>, as a native slot whose pointer names
    /// it (#2458): just after showing the record before it in its list, which it shows for <paramref name="updates"/>
    /// updates, or, at a list's start, having shown nothing yet.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The address names no playback.</exception>
    internal static AnimationPlayer<SamusProjectileSegment, SamusProjectileFrameId, NoAnimationCue> StandingAt(ushort address, int updates = 1)
    {
        AnimationPlayback<SamusProjectileSegment> playback = PlaybackAt(address)
            ?? throw new ArgumentOutOfRangeException(nameof(address), address, "The address names no projectile animation.");
        return playback.Place.Beat >= 1 ? ShowingBeatBefore(playback, updates) : PlayerAt(address, updates);
    }

    /// <summary>
    /// A projectile animation standing at <paramref name="playback"/> just after showing the beat before it, which it shows
    /// for <paramref name="updates"/> updates (#2458).
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The playback has no beat before it.</exception>
    internal static AnimationPlayer<SamusProjectileSegment, SamusProjectileFrameId, NoAnimationCue> ShowingBeatBefore(
        AnimationPlayback<SamusProjectileSegment> playback, int updates = 1)
    {
        AnimationPlace place = playback.Place;
        if (place.Beat < 1)
            throw new ArgumentOutOfRangeException(nameof(playback), playback, "The playback has shown no beat.");
        var player = AnimationPlayer<SamusProjectileSegment, SamusProjectileFrameId, NoAnimationCue>.Starting(
            SamusProjectileAnimations.Segments, playback.Animation);
        player.ShowAt(playback.Animation, place with { Beat = place.Beat - 1 }, updates);
        return player;
    }

    /// <inheritdoc cref="SamusProjectileListLayout.RecordPosition"/>
    internal static ushort RecordAddress(SamusProjectileFrameId record) => Layout.RecordPosition(record);

    /// <inheritdoc cref="SamusProjectileListLayout.RecordPositions"/>
    internal static IReadOnlyList<int> RecordAddresses(SamusProjectileFrameId record) => Layout.RecordPositions(record);
}
