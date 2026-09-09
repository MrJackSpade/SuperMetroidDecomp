namespace SuperMetroid.Core.Frontend;

/// <summary>Bank-$8B reward gesture constructors and their jump handoffs.</summary>
internal static class EndingRewardActorDefinitions
{
    /// <summary>$8B:EF33/EF39 are the suitless upper/lower hair-release actors spawned by E342.</summary>
    public const ushort SuitlessUpper = 0xef33, SuitlessLower = 0xef39;
    /// <summary>$8B:EF57/EF5D are the suited body and moving thumbs-up arm.</summary>
    public const ushort SuitedBody = 0xef57, SuitedArm = 0xef5d;
    /// <summary>$8B:EF63/EF69 select helmeted/helmetless gesture heads.</summary>
    public const ushort HelmetedHead = 0xef63, HelmetlessHead = 0xef69;
    /// <summary>$8B:F143 initializes suitless actors at (120,136), OBJ palette five.</summary>
    public const ushort InitializeSuitless = 0xf143;
    /// <summary>$8B:F156 initializes suited body/arm at (120,152), OBJ palette six.</summary>
    public const ushort InitializeSuitedBody = 0xf156;
    /// <summary>$8B:F169 initializes the helmeted head at (124,108), OBJ palette six.</summary>
    public const ushort InitializeHelmetedHead = 0xf169;
    /// <summary>$8B:F17C initializes the helmetless head at (121,107), OBJ palette five.</summary>
    public const ushort InitializeHelmetlessHead = 0xf17c;
    /// <summary>$8B:F51D requests the suitless jumping actor after letting down her hair.</summary>
    public const ushort SpawnSuitlessJump = 0xf51d;
    /// <summary>$8B:F554 requests the suited jumping body/head after the thumbs-up gesture.</summary>
    public const ushort SpawnSuitedJump = 0xf554;

    /// <summary>Literal position/palette writes made by the four native reward initialization routines.</summary>
    public static (int X, int Y, int Palette) GetInitialization(ushort address) => address switch
    {
        InitializeSuitless => (120, 136, 0x0a00),
        InitializeSuitedBody => (120, 152, 0x0c00),
        InitializeHelmetedHead => (124, 108, 0x0c00),
        InitializeHelmetlessHead => (121, 107, 0x0a00),
        _ => throw new InvalidDataException($"Unsupported reward initialization $8B:{address:X4}.")
    };
}
