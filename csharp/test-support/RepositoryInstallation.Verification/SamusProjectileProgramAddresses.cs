using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// The projectile list layout, read from the supported cartridge, for checks that compare cursors with native instruction
/// pointers. The game never sees a list position (#1474).
/// </summary>
internal static class SamusProjectileProgramAddresses
{
    /// <summary>The layout, read once from the repository's cartridge.</summary>
    private static readonly Lazy<SamusProjectileListLayout> layout = new(() => SamusProjectileListEntries.Read(
        CartridgeImportSource.Require(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")))));

    /// <summary>The layout.</summary>
    internal static SamusProjectileListLayout Layout => layout.Value;

    /// <summary>Each step's instruction pointer, by program.</summary>
    internal static IReadOnlyDictionary<SamusProjectileProgram, ushort[]> StepAddresses => Layout.StepPositions;

    /// <summary>The list entry of <paramref name="program"/>.</summary>
    internal static ushort Entry(SamusProjectileProgram program) => Layout.StepPositions[program][0];

    /// <inheritdoc cref="SamusProjectileListLayout.Prefix"/>
    internal static ushort Prefix(SamusProjectileProgram program) => Layout.Prefix(program);

    /// <inheritdoc cref="SamusProjectileListLayout.Position(AnimationProgramCursor{SamusProjectileProgram})"/>
    internal static ushort Address(AnimationProgramCursor<SamusProjectileProgram> cursor) => Layout.Position(cursor);

    /// <inheritdoc cref="SamusProjectileListLayout.Position(AnimationProgramCursor{SamusProjectileProgram}?)"/>
    internal static ushort Address(AnimationProgramCursor<SamusProjectileProgram>? cursor) => Layout.Position(cursor);

    /// <inheritdoc cref="SamusProjectileListLayout.CursorAt"/>
    internal static AnimationProgramCursor<SamusProjectileProgram>? CursorAt(ushort address) => Layout.CursorAt(address);

    /// <inheritdoc cref="SamusProjectileListLayout.RecordPosition"/>
    internal static ushort RecordAddress(int record) => Layout.RecordPosition(record);
}
