using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// Runs the Samus projectiles' typed programs (#1301) as the bank-$93 instruction handlers run their lists: each timed
/// record installs its duration, spritemap and radii.
/// </summary>
internal static class SamusProjectilePrograms
{
    /// <summary>
    /// Runs from <paramref name="cursor"/> to the next timed record and returns it with its duration, leaving the cursor
    /// past it; returns null when the list deletes the projectile.
    /// </summary>
    internal static SamusProjectileFrame? RunToNextRecord(ref AnimationProgramCursor<SamusProjectileProgram> cursor, out int duration)
    {
        WrappingCountdown counter = default;
        AnimationProgramAdvance<SamusProjectileFrame> advance =
            SamusProjectileAnimations.Programs.Advance(ref cursor, ref counter, static (_, _) => null);
        switch (advance)
        {
            case AnimationProgramAdvance<SamusProjectileFrame>.Show shown:
                duration = shown.Duration;
                return shown.Frame;
            case AnimationProgramAdvance<SamusProjectileFrame>.Stop:
                duration = 0;
                return null;
            default:
                throw new InvalidOperationException($"Samus projectile program {cursor.Program} ended an advance with {advance}.");
        }
    }

    /// <summary>The cursor at the start of <paramref name="program"/>; no program is a projectile with no list.</summary>
    internal static AnimationProgramCursor<SamusProjectileProgram>? Start(SamusProjectileProgram? program) =>
        program is { } selected ? AnimationProgramCursor<SamusProjectileProgram>.Start(selected) : null;

    /// <summary>
    /// The hitbox a freshly selected list declares through its first record, as the initializers' reads of the list's
    /// radius bytes see it; a projectile with no list (the safe left-facing Murder Beam) reads zero.
    /// </summary>
    internal static Extent16<RoomPixels> InitialRadius(SamusProjectileProgram? program)
    {
        if (program is not { } selected)
            return Extent16<RoomPixels>.Zero;
        var first = SamusProjectileAnimations.Programs.Steps(selected)[0];
        if (first is not AnimationProgramStep<SamusProjectileProgram, SamusProjectileFrame, NoAnimationCue>.ShowStep show)
            throw new InvalidDataException($"Samus projectile program {selected} does not open with a timed record.");
        SamusProjectileRecord record = SamusProjectileRecordDefinitions.Get(show.Frame);
        return record.Radius;
    }

    /// <summary>
    /// The word just before the projectile's next list position, which $93:81D8 reads as the trail's animation frame: the
    /// animation frame of the record last installed, or for a freshly installed list the word preceding it.
    /// </summary>
    /// <exception cref="InvalidOperationException">The cursor stands at a list's start and no overread catalog is installed.</exception>
    internal static ushort PrecedingAnimationFrame(AnimationProgramCursor<SamusProjectileProgram> cursor, InstalledSamusOverreads? overreads)
    {
        if (cursor.Position == 0)
            return (overreads ?? throw new InvalidOperationException(
                $"A trail spawned at the start of {cursor.Program}'s list needs the installed Samus overread catalog.")).ProjectileLists.Prefix(cursor.Program);
        var previous = SamusProjectileAnimations.Programs.Steps(cursor.Program)[cursor.Position - 1];
        if (previous is not AnimationProgramStep<SamusProjectileProgram, SamusProjectileFrame, NoAnimationCue>.ShowStep show)
            throw new InvalidOperationException($"Samus projectile program {cursor.Program} continues past a non-record step.");
        return checked((ushort)SamusProjectileRecordDefinitions.Get(show.Frame).AnimationFrame);
    }
}
