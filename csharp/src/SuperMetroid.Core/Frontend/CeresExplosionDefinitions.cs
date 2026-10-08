namespace SuperMetroid.Core.Frontend;

/// <summary>Fixed actor metadata and physical spawn schedule for the Ceres destruction blasts.</summary>
internal static class CeresExplosionDefinitions
{
    /// <summary>Bank containing the native definitions and placement tables.</summary>
    public const int NativeBank = 0x8b0000;

    /// <summary>Number of actors spawned by instruction <c>$8B:C404</c>.</summary>
    public const int InitialExplosionCount = 5;

    /// <summary>Number of cyclic offsets consumed by pre-instruction <c>$8B:C489</c>.</summary>
    public const int RepeatingExplosionCount = 8;

    /// <summary>Number of actors spawned by instruction <c>$8B:C50C</c>.</summary>
    public const int FinalExplosionCount = 4;

    /// <summary><c>$8B:CE35</c>, invisible wait before instruction <c>$C404</c>.</summary>
    public const ushort InitialWaitFrames = 0x0080;

    /// <summary><c>$8B:CE3B</c>, wait before installing repeating pre-instruction <c>$C489</c>.</summary>
    public const ushort RepeatingStartWaitFrames = 0x0050;

    /// <summary><c>$8B:CE43</c>, final spawner lifetime before instruction <c>$C50C</c>.</summary>
    public const ushort RepeatingLifetimeFrames = 0x0040;

    /// <summary><c>$8B:C4A8</c>, frames between repeating second-wave explosions.</summary>
    public const ushort RepeatingPeriodFrames = 0x000c;

    /// <summary><c>$8B:C404</c>, instruction-list callback that creates the first wave.</summary>
    public const ushort SpawnInitialWaveInstruction = 0xc404;

    /// <summary><c>$8B:C489</c>, pre-instruction that periodically creates the second wave.</summary>
    public const ushort SpawnRepeatingWavePreInstruction = 0xc489;

    /// <summary><c>$8B:C50C</c>, instruction-list callback that creates the final wave.</summary>
    public const ushort SpawnFinalWaveInstruction = 0xc50c;

    /// <summary>First host frame on which the initial wait has expired.</summary>
    public const int InitialSpawnFrame = InitialWaitFrames + 1;

    /// <summary>First host frame on which the newly installed repeating callback runs.</summary>
    public const int RepeatingFirstFrame = InitialWaitFrames + RepeatingStartWaitFrames + 2;

    /// <summary>Last host frame owned by the spawner and its final-wave instruction.</summary>
    public const int SpawnerFinalFrame =
        InitialWaitFrames + RepeatingStartWaitFrames + RepeatingLifetimeFrames + 1;

    /// <summary>$8B:C33C, CinematicFunctionTimer seed when the initial fade completes.</summary>
    internal const ushort RepeatingCountdownSeed = 1;

    /// <summary>Native spawner events, with repeating pre-instruction before final-wave instruction.</summary>
    internal static CeresExplosionSpawnEvents EventsAtFrame(int frame, bool repeatingEnabled, ref ushort countdown)
    {
        bool repeat = false;
        if (repeatingEnabled && frame >= RepeatingFirstFrame && frame <= SpawnerFinalFrame)
        {
            countdown = unchecked((ushort)(countdown - 1));
            if ((short)countdown <= 0)
            {
                repeat = true;
                countdown = RepeatingPeriodFrames;
            }
        }
        return new(frame == InitialSpawnFrame, repeat, frame == SpawnerFinalFrame);
    }
    /// <summary><c>$8B:CEBB</c>, first delayed small-explosion actor.</summary>
    /// <remarks>Native list $8B:CCDB..CCF4 displays six duration-3 small-explosion frames, then deletes. The stream is generated as explicit frame/loop/delete operations by CeresExplosionInstructionDefinitions.</remarks>
    public static CeresExplosionActorDefinition InitialActor =>
        new(0xcebb, 0xc434, 0xc582, 0xccdb);

    /// <summary><c>$8B:CEC1</c>, cyclic repeating small-explosion actor.</summary>
    /// <remarks>Native list $8B:CCF5..CD1A repeats six duration-3 frames and a duration-16 blank six times, then deletes. The stream is generated as explicit frame/loop/delete operations by CeresExplosionInstructionDefinitions.</remarks>
    public static CeresExplosionActorDefinition RepeatingActor =>
        new(0xcec1, 0xc4b9, 0xc582, 0xccf5);

    /// <summary><c>$8B:CEC7</c>, final delayed large-explosion actor.</summary>
    /// <remarks>Native list $8B:CD1B..CD38 repeats four duration-5 frames and a duration-8 blank seven times, then deletes. The stream is generated as explicit frame/loop/delete operations by CeresExplosionInstructionDefinitions.</remarks>
    public static CeresExplosionActorDefinition FinalWaveActor =>
        new(0xcec7, 0xc533, 0xc582, 0xcd1b);

    /// <summary><c>$8B:CF2D</c>, final station blast actor created by <c>$8B:C345</c>.</summary>
    /// <remarks>Native list $8B:CE1B..CE34 displays six duration-5 station-blast frames, then deletes. The stream is generated as explicit frame/loop/delete operations by CeresExplosionInstructionDefinitions.</remarks>
    public static CeresExplosionActorDefinition StationBlastActor =>
        new(0xcf2d, 0xc5a9, 0xc582, 0xce1b);

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
        return new((short)x, (short)y, (ushort)(index == 0 ? 1 : 16 * index));
    }

    /// <summary>$8B:C4EB-$C50A, the repeating burst's eight chosen screen-space positions.</summary>
    /// <remarks>
    /// Retained under #1165's drawing/choreography exception. Each ordinal creates the
    /// same small blast, then advances modulo eight even if allocation fails. These
    /// points describe the particular scattered sequence around the station, not sampled
    /// motion, actor dispatch, or hull attachment points: the offsets stay in screen space
    /// while the underlying Mode7 image changes scale. A per-index switch or fitted curve
    /// would just encode this same layout. The initial square/center layout, final X rule,
    /// instruction delays and shared actor motion are calculated separately.
    /// </remarks>
    private static readonly (short X, short Y)[] RepeatingBurstLayout =
    [
        (14, -8), (8, 12), (-16, 12), (-8, -14),
        (0, 0), (16, 14), (-12, 4), (-8, -16),
    ];

    /// <summary>Returns a chosen repeating-burst position with its common first-call delay.</summary>
    public static CeresExplosionPlacement RepeatingExplosion(int index)
    {
        if ((uint)index >= RepeatingExplosionCount)
            throw new ArgumentOutOfRangeException(nameof(index));
        var position = RepeatingBurstLayout[index];
        return new(position.X, position.Y, 1);
    }

    /// <summary>$8B:C57A-$C581, chosen heights of the four final large bursts.</summary>
    /// <remarks>
    /// Retained as the final burst's spatial choreography under #1165. These are absolute
    /// offsets from the scrolling anchor, not successive displacements or a velocity curve.
    /// The alternating upper/lower placements have individually chosen heights; turning
    /// their ordinal-to-height list into cases would merely recite the layout. Only this
    /// Y content is retained: the independent X and delay fields remain calculated.
    /// </remarks>
    private static ReadOnlySpan<short> FinalBurstHeights => [-4, 8, -10, 12];

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
        return new((short)x, (short)y, (ushort)(index == 0 ? 1 : 1 << (index + 1)));
    }
}

/// <summary>One native six-byte bank-$8B cinematic sprite-object definition.</summary>
internal readonly record struct CeresExplosionActorDefinition(
    ushort Pointer,
    ushort Initialization,
    ushort PreInstruction,
    ushort InstructionList);

/// <summary>One fixed Ceres explosion offset and initial instruction delay.</summary>
internal readonly record struct CeresExplosionPlacement(short X, short Y, ushort DelayFrames);

/// <summary>Ordered spawner events; repeating and final waves may occur on the same call.</summary>
internal readonly record struct CeresExplosionSpawnEvents(bool Initial, bool Repeating, bool Final);
