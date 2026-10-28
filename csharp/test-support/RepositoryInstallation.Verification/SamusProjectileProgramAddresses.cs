using SuperMetroid.Core.Programs;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>The installed projectile list layout, for checks that compare cursors with native instruction pointers.</summary>
internal static class SamusProjectileProgramAddresses
{
    /// <summary>The installed layout.</summary>
    private static InstalledProjectileListPositions Layout => InstalledFixtures.SamusOverreads.ProjectileLists;

    /// <summary>Each step's instruction pointer, by program.</summary>
    internal static IReadOnlyDictionary<SamusProjectileProgram, ushort[]> StepAddresses => Layout.StepPositions;

    /// <summary>The list entry of <paramref name="program"/>.</summary>
    internal static ushort Entry(SamusProjectileProgram program) => Layout.StepPositions[program][0];

    /// <inheritdoc cref="InstalledProjectileListPositions.Prefix"/>
    internal static ushort Prefix(SamusProjectileProgram program) => Layout.Prefix(program);

    /// <inheritdoc cref="InstalledProjectileListPositions.Position(AnimationProgramCursor{SamusProjectileProgram})"/>
    internal static ushort Address(AnimationProgramCursor<SamusProjectileProgram> cursor) => Layout.Position(cursor);

    /// <inheritdoc cref="InstalledProjectileListPositions.Position(AnimationProgramCursor{SamusProjectileProgram}?)"/>
    internal static ushort Address(AnimationProgramCursor<SamusProjectileProgram>? cursor) => Layout.Position(cursor);

    /// <inheritdoc cref="InstalledProjectileListPositions.CursorAt"/>
    internal static AnimationProgramCursor<SamusProjectileProgram>? CursorAt(ushort address) => Layout.CursorAt(address);

    /// <inheritdoc cref="InstalledProjectileListPositions.RecordPosition"/>
    internal static ushort RecordAddress(int record) => Layout.RecordPosition(record);
}
