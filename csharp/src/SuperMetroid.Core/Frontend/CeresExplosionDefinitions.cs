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

    /// <summary><c>$8B:CEBB</c>, first delayed small-explosion actor.</summary>
    public static CeresExplosionActorDefinition InitialActor =>
        new(0xcebb, 0xc434, 0xc582, 0xccdb);

    /// <summary><c>$8B:CEC1</c>, cyclic repeating small-explosion actor.</summary>
    public static CeresExplosionActorDefinition RepeatingActor =>
        new(0xcec1, 0xc4b9, 0xc582, 0xccf5);

    /// <summary><c>$8B:CEC7</c>, final delayed large-explosion actor.</summary>
    public static CeresExplosionActorDefinition FinalWaveActor =>
        new(0xcec7, 0xc533, 0xc582, 0xcd1b);

    /// <summary><c>$8B:CF2D</c>, final station blast actor created by <c>$8B:C345</c>.</summary>
    public static CeresExplosionActorDefinition StationBlastActor =>
        new(0xcf2d, 0xc5a9, 0xc582, 0xce1b);

    /// <summary><c>$8B:CF33</c>, invisible actor whose list owns the three-wave schedule.</summary>
    public static CeresExplosionActorDefinition SpawnerActor =>
        new(0xcf33, 0x93d9, 0x93d9, 0xce35);

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
    public static CeresExplosionPlacement InitialExplosion(int index) => index switch
    {
        0 => new(16, -16, 1),
        1 => new(-16, 16, 16),
        2 => new(16, 16, 32),
        3 => new(-16, -16, 48),
        4 => new(0, 0, 64),
        _ => throw new ArgumentOutOfRangeException(nameof(index)),
    };

    /// <summary>Returns one of the eight interleaved X/Y rows at <c>$8B:C4EB-$C50A</c>.</summary>
    public static CeresExplosionPlacement RepeatingExplosion(int index) => index switch
    {
        0 => new(14, -8, 1),
        1 => new(8, 12, 1),
        2 => new(-16, 12, 1),
        3 => new(-8, -14, 1),
        4 => new(0, 0, 1),
        5 => new(16, 14, 1),
        6 => new(-12, 4, 1),
        7 => new(-8, -16, 1),
        _ => throw new ArgumentOutOfRangeException(nameof(index)),
    };

    /// <summary>Returns one of the four timer/X/Y rows at <c>$8B:C56A/$C572/$C57A</c>.</summary>
    public static CeresExplosionPlacement FinalExplosion(int index) => index switch
    {
        0 => new(8, -4, 1),
        1 => new(12, 8, 4),
        2 => new(-8, -10, 8),
        3 => new(-12, 12, 16),
        _ => throw new ArgumentOutOfRangeException(nameof(index)),
    };
}

/// <summary>One native six-byte bank-$8B cinematic sprite-object definition.</summary>
internal readonly record struct CeresExplosionActorDefinition(
    ushort Pointer,
    ushort Initialization,
    ushort PreInstruction,
    ushort InstructionList);

/// <summary>One fixed Ceres explosion offset and initial instruction delay.</summary>
internal readonly record struct CeresExplosionPlacement(short X, short Y, ushort DelayFrames);
