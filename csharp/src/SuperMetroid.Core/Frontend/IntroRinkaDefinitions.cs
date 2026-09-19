namespace SuperMetroid.Core.Frontend;

/// <summary>Fixed cinematic-object and physical launch definitions for the intro Rinkas.</summary>
internal static class IntroRinkaDefinitions
{
    /// <summary>Bank containing the native actor definitions and physical initializer tables.</summary>
    public const int NativeBank = 0x8b0000;

    /// <summary><c>$8B:93D9</c>, the shared cinematic-sprite no-op callback.</summary>
    public const ushort SharedNoOp = 0x93d9;

    /// <summary><c>$8B:CF21</c>, intro Rinka initialization, no-op pre-instruction, and initial list.</summary>
    public static IntroRinkaActorDefinition RinkaActor =>
        new(0xcf21, 0xb896, SharedNoOp, 0xcdeb);

    /// <summary><c>$8B:CF27</c>, Rinka-spawner no-op callbacks and initial list.</summary>
    public static IntroRinkaActorDefinition SpawnerActor =>
        new(0xcf27, SharedNoOp, SharedNoOp, 0xce0d);

    /// <summary>Number of parameter-selected Rinka initializer rows at <c>$8B:B8B5</c>.</summary>
    public const int RinkaCount = 4;

    /// <summary><c>$8B:B8B5</c>, four Rinka initial X positions.</summary>
    public const int InitialXReferenceAddress = 0x8bb8b5;

    /// <summary><c>$8B:B8BD</c>, four Rinka initial Y positions before the fixed eight-pixel subtraction.</summary>
    public const int InitialYReferenceAddress = 0x8bb8bd;

    /// <summary><c>$8B:B985</c>, four signed whole-pixel X velocity components.</summary>
    public const int XWholeVelocityReferenceAddress = 0x8bb985;

    /// <summary>
    /// Returns one complete parameter-selected physical definition. <c>$8B:B896</c>
    /// subtracts eight from the authored Y position; both movement paths use fraction
    /// <c>$8000</c>, so the signed whole component distinguishes +0.5 from -0.5 px/frame.
    /// </summary>
    public static IntroRinkaPhysicalDefinition Rinka(int parameter) => parameter switch
    {
        0 => new(0x0070, 0x0048, 0),
        1 => new(0x00c0, 0x0038, -1),
        2 => new(0x0080, 0x0030, 0),
        3 => new(0x00e8, 0x0050, -1),
        _ => throw new ArgumentOutOfRangeException(nameof(parameter)),
    };

    /// <summary><c>$8B:B8E4/$B947</c>, shared half-pixel X/Y fractional velocity.</summary>
    public const ushort HalfPixelFraction = 0x8000;
}

/// <summary>One native six-byte intro-Rinka cinematic-object definition.</summary>
internal readonly record struct IntroRinkaActorDefinition(
    ushort Pointer,
    ushort Initialization,
    ushort PreInstruction,
    ushort InstructionList);

/// <summary>One intro Rinka's final origin and signed whole-pixel X velocity component.</summary>
internal readonly record struct IntroRinkaPhysicalDefinition(
    ushort X,
    ushort Y,
    short XWholeVelocity);
