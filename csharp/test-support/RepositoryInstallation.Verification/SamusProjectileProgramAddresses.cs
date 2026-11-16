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

    /// <summary>Each place's instruction pointer, by segment: one per beat, then the segment's end.</summary>
    internal static IReadOnlyDictionary<SamusProjectileSegment, ushort[]> BeatAddresses => Layout.BeatPositions;

    /// <summary>The list entry of <paramref name="program"/>.</summary>
    internal static ushort Entry(SamusProjectileSegment program) => Layout.BeatPositions[program][0];

    /// <inheritdoc cref="SamusProjectileListLayout.Prefix"/>
    internal static ushort Prefix(SamusProjectileSegment program) => Layout.Prefix(program);

    /// <inheritdoc cref="SamusProjectileListLayout.Position(SamusProjectileSegmentBeat)"/>
    internal static ushort Address(SamusProjectileSegmentBeat place) => Layout.Position(place);

    /// <summary>The bank position of <paramref name="playback"/>, or zero for no playback.</summary>
    internal static ushort Address(AnimationPlayback<SamusProjectileSegment>? playback) => Layout.Position(playback);

    /// <summary>The playback the bank position <paramref name="address"/> names, or none for zero.</summary>
    internal static AnimationPlayback<SamusProjectileSegment>? PlaybackAt(ushort address) => Layout.PlaybackAt(address);

    /// <inheritdoc cref="SamusProjectileListLayout.RecordPosition"/>
    internal static ushort RecordAddress(SamusProjectileFrame record) => Layout.RecordPosition(record);
}
