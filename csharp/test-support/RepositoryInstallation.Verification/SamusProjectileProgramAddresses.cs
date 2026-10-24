using SuperMetroid.Core.Programs;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>The installed projectile list layout, for checks that compare cursors with native instruction pointers.</summary>
internal static class SamusProjectileProgramAddresses
{
    /// <summary>The installed layout.</summary>
    private static SamusProjectileListLayout Layout => InstalledFixtures.SamusOverreads.ProjectileLists;

    /// <summary>Each step's instruction pointer, by program.</summary>
    internal static IReadOnlyDictionary<SamusProjectileProgram, ushort[]> StepAddresses => Layout.StepAddresses;

    /// <summary>The list entry of <paramref name="program"/>.</summary>
    internal static ushort Entry(SamusProjectileProgram program) => Layout.StepAddresses[program][0];

    /// <inheritdoc cref="SamusProjectileListLayout.Prefix"/>
    internal static ushort Prefix(SamusProjectileProgram program) => Layout.Prefix(program);

    /// <inheritdoc cref="SamusProjectileListLayout.Address(AnimationProgramCursor{SamusProjectileProgram})"/>
    internal static ushort Address(AnimationProgramCursor<SamusProjectileProgram> cursor) => Layout.Address(cursor);

    /// <inheritdoc cref="SamusProjectileListLayout.Address(AnimationProgramCursor{SamusProjectileProgram}?)"/>
    internal static ushort Address(AnimationProgramCursor<SamusProjectileProgram>? cursor) => Layout.Address(cursor);

    /// <inheritdoc cref="SamusProjectileListLayout.CursorAt"/>
    internal static AnimationProgramCursor<SamusProjectileProgram>? CursorAt(ushort address) => Layout.CursorAt(address);

    /// <inheritdoc cref="SamusProjectileListLayout.RecordAddress"/>
    internal static ushort RecordAddress(int record) => Layout.RecordAddress(record);
}
