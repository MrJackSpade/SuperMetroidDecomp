using SuperMetroid.AssetExtraction.Samus;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// Verification's native view of a trail side: the bank-$90 address of the step its cursor names, or zero for a cleared
/// side, as native trail RAM holds it.
/// </summary>
internal static class SamusProjectileTrailSideNative
{
    /// <summary>The cursor native trail RAM's cleared zero names: none.</summary>
    private const ushort Cleared = 0;

    /// <summary>The cursor whose step is stored at bank-$90 address <paramref name="address"/>.</summary>
    internal static ProjectileTrailCursor CursorAt(ushort address)
    {
        foreach ((ProjectileTrailProgram program, ProjectileTrailStep[] steps) in ProjectileTrailProgramDefinitions.Programs)
            for (int step = 0; step < steps.Length; step++)
                if (ProjectileTrailSources.Address(new ProjectileTrailCursor(program, step)) == address)
                    return new ProjectileTrailCursor(program, step);
        throw new ArgumentOutOfRangeException(nameof(address), address, "No trail step is stored there.");
    }

    /// <summary>A side's native cursor.</summary>
    extension(SamusProjectileTrailSide side)
    {
        /// <summary>The bank-$90 address of the step the side runs next, as native trail RAM holds it.</summary>
        internal int InstructionPointer
        {
            get => side.Cursor is { } cursor ? ProjectileTrailSources.Address(cursor) : Cleared;
            set => side.Cursor = value == Cleared ? null : CursorAt(checked((ushort)value));
        }
    }
}
