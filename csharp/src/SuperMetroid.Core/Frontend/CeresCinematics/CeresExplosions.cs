using static SuperMetroid.Core.Frontend.CeresCinematics.CeresExplosionDefinitions;

namespace SuperMetroid.Core.Frontend.CeresCinematics;

/// <summary>
/// Schedules the Ceres destruction blasts (<see cref="CeresExplosionDefinitions"/>): the spawner's waves on their host frames, and
/// each wave's actor placements.
/// </summary>
internal static class CeresExplosions
{
    /// <summary>Spawner events, with the repeating frame behaviour before the final wave.</summary>
    internal static CeresExplosionSpawnEvents EventsAtFrame(int frame, bool repeatingEnabled, ref int countdown)
    {
        bool repeat = false;
        if (repeatingEnabled && frame >= RepeatingFirstFrame && frame <= SpawnerFinalFrame)
        {
            if (--countdown <= 0)
            {
                repeat = true;
                countdown = RepeatingPeriodFrames;
            }
        }
        return new(frame == FrameOf(SpawnerMilestone.InitialWave), repeat,
            frame == FrameOf(SpawnerMilestone.FinalWave));
    }

    /// <summary>Spawner-schedule milestones that each fire a wave on exactly one host frame.</summary>
    private enum SpawnerMilestone
    {
        /// <summary>The first wave spawns the initial square-and-center wave.</summary>
        InitialWave,
        /// <summary>The final wave spawns the large bursts.</summary>
        FinalWave,
    }

    /// <summary>Maps the initial and final cue milestones to their exact host-frame numbers.</summary>
    private static int FrameOf(SpawnerMilestone milestone) => milestone switch
    {
        SpawnerMilestone.InitialWave => InitialSpawnFrame,
        SpawnerMilestone.FinalWave => SpawnerFinalFrame,
        _ => throw new InvalidOperationException($"Undefined {nameof(SpawnerMilestone)} {milestone}."),
    };

    /// <summary>
    /// Returns one of the five timer/X/Y rows of the first wave.
    /// </summary>
    /// <remarks>
    /// Issues #625 and #992: the first wave's five delays are
    /// 1, 16, 32, 48, 64. For bounded blast index i=0..4 this is exactly
    /// i=0 ? 1 : 16*i. The first wave spawns those five indices, and each
    /// blast's initializer copies its delay into the frame hold before
    /// the generic sprite handler runs. The first value is one, not zero:
    /// it schedules the first blast on its first eligible handler call.
    /// The selector rejects other indices rather than reading the adjacent
    /// X-offset table.
    ///
    /// Issues #625 and #993: the separate signed X offsets are
    /// +16, -16, +16, -16, 0 for i=0..4, matching the
    /// authored data. The outer
    /// four follow 16*(1-2*(i&amp;1)); the fifth is the center blast at zero.
    /// Each blast starts at the floor's centre X minus its scroll X, then
    /// adds this signed offset, wrapping past 65535. The bounded
    /// parity-and-center rule expresses the geometry without extending
    /// into the adjacent Y table.
    ///
    /// Issues #625 and #994: the separate signed Y offsets are
    /// -16, +16, +16, -16, 0 for i=0..4, matching the
    /// authored data. For the outer
    /// four, Y is the negative of the matching X offset when i&lt;2, then
    /// equals X when i=2..3; the fifth is the center at zero. Initializer
    /// Each blast starts at the floor's centre Y minus its scroll Y before adding this
    /// signed value, wrapping past 65535. Together the bounded X/Y
    /// selectors describe four corners and a center without extrapolating
    /// past the table.
    /// </remarks>
    public static CeresExplosionPlacement InitialExplosion(int index)
    {
        if ((uint)index >= InitialExplosionCount)
            throw new ArgumentOutOfRangeException(nameof(index));
        int x = index == 4 ? 0 : 16 * (1 - 2 * (index & 1));
        int y = index < 2 ? -x : x;
        return new(new(CoordinateAxis.AsSigned(x), CoordinateAxis.AsSigned(y)), index == 0 ? 1 : 16 * index);
    }

    /// <summary>Returns a chosen repeating-burst position with its common first-call delay.</summary>
    public static CeresExplosionPlacement RepeatingExplosion(int index)
    {
        if ((uint)index >= RepeatingExplosionCount)
            throw new ArgumentOutOfRangeException(nameof(index));
        var position = RepeatingBurstLayout[index];
        return new(new(position.X, position.Y), 1);
    }

    /// <summary>Returns one of the four timer/X/Y rows of the final wave.</summary>
    /// <remarks>
    /// Issues #625 and #997: the four final-wave delays are
    /// 1, 4, 8, 16, matching the authored
    /// data. For bounded blast index i=0..3 this is i=0 ? 1 :
    /// 1&lt;&lt;(i+1). The final wave spawns these four indices, and each
    /// initializer installs its delay as the first frame hold.
    /// Index zero retains its nonzero first-call delay. Other indices throw
    /// rather than reading the adjacent X-offset table.
    ///
    /// Issues #625 and #998: the separate signed X offsets are
    /// +8, +12, -8, -12 for i=0..3, matching
    /// the authored data. Their magnitude is
    /// 8+4*(i&amp;1), positive for i&lt;2 and negative otherwise.
    /// Each initializer adds this X after the floor's centre X minus its scroll X
    /// wrapping past 65535. The bounded sign/magnitude rule exactly
    /// covers the four spawned actors, without reading adjacent Y values.
    ///
    /// Issues #625 and #999: the separate signed Y offsets are
    /// -4, +8, -10, +12 for i=0..3, matching
    /// the authored data. Each initializer adds Y
    /// after the floor's centre Y minus its scroll Y, wrapping past 65535.
    /// These particular vertical placements are retained spatial choreography; see
    /// FinalBurstHeights for the narrowly scoped nonsense disposition under #1165.
    /// </remarks>
    public static CeresExplosionPlacement FinalExplosion(int index)
    {
        if ((uint)index >= FinalExplosionCount)
            throw new ArgumentOutOfRangeException(nameof(index));
        int magnitude = 8 + 4 * (index & 1);
        int x = index < 2 ? magnitude : -magnitude;
        int y = FinalBurstHeights[index];
        return new(new(CoordinateAxis.AsSigned(x), CoordinateAxis.AsSigned(y)), index == 0 ? 1 : 1 << (index + 1));
    }
}
