using static SuperMetroid.Core.Frontend.CeresCinematics.CeresExplosionDefinitions;

namespace SuperMetroid.Core.Frontend.CeresCinematics;

/// <summary>
/// Schedules the Ceres destruction blasts (<see cref="CeresExplosionDefinitions"/>): the spawner's waves on their host frames, and
/// each wave's actor placements.
/// </summary>
internal static class CeresExplosions
{
    /// <summary>Native spawner events, with repeating pre-instruction before final-wave instruction.</summary>
    internal static CeresExplosionSpawnEvents EventsAtFrame(int frame, bool repeatingEnabled, ref WrappingCountdown countdown)
    {
        bool repeat = false;
        if (repeatingEnabled && frame >= RepeatingFirstFrame && frame <= SpawnerFinalFrame)
        {
            CountdownStep step = countdown.Decrement();
            countdown = step.Remaining;
            if (step.IsZeroOrNegative)
            {
                repeat = true;
                countdown = WrappingCountdown.Start(RepeatingPeriodFrames);
            }
        }
        return new(frame == FrameOf(SpawnerMilestone.InitialWave), repeat,
            frame == FrameOf(SpawnerMilestone.FinalWave));
    }

    /// <summary>Spawner-schedule milestones that each fire a wave on exactly one host frame.</summary>
    private enum SpawnerMilestone
    {
        /// <summary>Instruction <c>$8B:C404</c> spawns the initial square-and-center wave.</summary>
        InitialWave,
        /// <summary>Instruction <c>$8B:C50C</c> spawns the final large-burst wave.</summary>
        FinalWave,
    }

    /// <summary>Maps the initial and final instruction milestones to their exact host-frame numbers.</summary>
    private static int FrameOf(SpawnerMilestone milestone) => milestone switch
    {
        SpawnerMilestone.InitialWave => InitialSpawnFrame,
        SpawnerMilestone.FinalWave => SpawnerFinalFrame,
        _ => throw new InvalidOperationException($"Undefined {nameof(SpawnerMilestone)} {milestone}."),
    };

    /// <summary>
    /// Returns one of the five timer/X/Y rows at <c>$8B:C46B/$C475/$C47F</c>.
    /// </summary>
    /// <remarks>
    /// Issues #625 and #992: the five instruction-delay words at
    /// $8B:C46B+2*i match pinned NTSC J/U v1.0 ROM and bank_8B.asm:
    /// 1, 16, 32, 48, 64. For bounded blast index i=0..4 this is exactly
    /// i=0 ? 1 : 16*i. $8B:C404 spawns those five indices, and initializer
    /// $8B:C434 copies the selected word into the instruction timer before
    /// the generic sprite handler runs. The first value is one, not zero:
    /// it schedules the first blast on its first eligible handler call.
    /// The selector rejects other indices rather than reading the adjacent
    /// X-offset table.
    ///
    /// Issues #625 and #993: the separate signed X-offset table at
    /// $8B:C475+2*i contains +16, -16, +16, -16, 0 for i=0..4; all five
    /// words match pinned NTSC J/U v1.0 ROM and bank_8B.asm. The outer
    /// four follow 16*(1-2*(i&amp;1)); the fifth is the center blast at zero.
    /// Initializer $8B:C434 computes Mode 7 origin X minus BG1 X, then
    /// adds this signed offset with native 16-bit wrap. The bounded
    /// parity-and-center rule expresses the geometry without extending
    /// into the adjacent Y table.
    ///
    /// Issues #625 and #994: the separate signed Y-offset table at
    /// $8B:C47F+2*i contains -16, +16, +16, -16, 0 for i=0..4; all five
    /// words match pinned NTSC J/U v1.0 ROM and bank_8B.asm. For the outer
    /// four, Y is the negative of the matching X offset when i&lt;2, then
    /// equals X when i=2..3; the fifth is the center at zero. Initializer
    /// $8B:C434 computes Mode 7 origin Y minus BG1 Y before adding this
    /// signed value with native 16-bit wrap. Together the bounded X/Y
    /// selectors describe four corners and a center without extrapolating
    /// into the following pre-instruction bytes.
    /// </remarks>
    public static CeresExplosionPlacement InitialExplosion(int index)
    {
        if ((uint)index >= InitialExplosionCount)
            throw new ArgumentOutOfRangeException(nameof(index));
        int x = index == 4 ? 0 : 16 * (1 - 2 * (index & 1));
        int y = index < 2 ? -x : x;
        return new(new((short)x, (short)y), index == 0 ? 1 : 16 * index);
    }

    /// <summary>Returns a chosen repeating-burst position with its common first-call delay.</summary>
    public static CeresExplosionPlacement RepeatingExplosion(int index)
    {
        if ((uint)index >= RepeatingExplosionCount)
            throw new ArgumentOutOfRangeException(nameof(index));
        var position = RepeatingBurstLayout[index];
        return new(new(position.X, position.Y), 1);
    }

    /// <summary>Returns one of the four timer/X/Y rows at <c>$8B:C56A/$C572/$C57A</c>.</summary>
    /// <remarks>
    /// Issues #625 and #997: the four final-wave instruction-delay words
    /// at $8B:C56A+2*i are 1, 4, 8, 16 in pinned NTSC J/U v1.0 ROM and
    /// bank_8B.asm. For bounded blast index i=0..3 this is i=0 ? 1 :
    /// 1&lt;&lt;(i+1). $8B:C50C spawns these four indices, and initializer
    /// $8B:C533 installs each selected word as the first instruction timer.
    /// Index zero retains its nonzero first-call delay. Other indices throw
    /// rather than reading the adjacent X-offset table.
    ///
    /// Issues #625 and #998: the separate signed X-offset words at
    /// $8B:C572+2*i are +8, +12, -8, -12 for i=0..3, matching pinned
    /// NTSC J/U v1.0 ROM and bank_8B.asm. Their magnitude is
    /// 8+4*(i&amp;1), positive for i&lt;2 and negative otherwise.
    /// Initializer $8B:C533 adds this X after Mode 7 origin X minus BG1 X
    /// with native 16-bit wrap. The bounded sign/magnitude rule exactly
    /// covers the four spawned actors, without reading adjacent Y words.
    ///
    /// Issues #625 and #999: the separate signed Y-offset words at
    /// $8B:C57A+2*i are -4, +8, -10, +12 for i=0..3, matching pinned
    /// NTSC J/U v1.0 ROM and bank_8B.asm. Initializer $8B:C533 adds Y
    /// after Mode 7 origin Y minus BG1 Y with native 16-bit wrap.
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
        return new(new((short)x, (short)y), index == 0 ? 1 : 1 << (index + 1));
    }
}
