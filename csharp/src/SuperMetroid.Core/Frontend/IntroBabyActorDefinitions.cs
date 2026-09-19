namespace SuperMetroid.Core.Frontend;

/// <summary>Fixed cinematic-object definitions for the SR388 egg and baby-Metroid scenes.</summary>
internal static class IntroBabyActorDefinitions
{
    /// <summary>Bank containing the native actor definitions and initializer routines.</summary>
    public const int NativeBank = 0x8b0000;

    /// <summary><c>$8B:BA5E</c>, initial confused-baby pre-instruction.</summary>
    public const ushort ConfusedBabyInitialPreInstruction = 0xba5e;

    /// <summary><c>$8B:CE5B/$A8D5</c>, the intact Metroid egg at (112,155).</summary>
    public static IntroBabyActorDefinition Egg =>
        new(0xce5b, 0xa8d5, 0xa8e8, 0xcb33, 0x0070, 0x009b, 0x0e00);

    /// <summary><c>$8B:CE61/$AD55</c>, the delivered baby Metroid at (84,139).</summary>
    public static IntroBabyActorDefinition DeliveredBaby =>
        new(0xce61, 0xad55, 0xad68, 0xcb9f, 0x0054, 0x008b, 0x0c00);

    /// <summary><c>$8B:CE67/$AD93</c>, the examined baby Metroid at (112,111).</summary>
    public static IntroBabyActorDefinition ExaminedBaby =>
        new(0xce67, 0xad93, 0xada6, 0xcbcd, 0x0070, 0x006f, 0x0c00);

    /// <summary><c>$8B:CE79/$BA4B</c>, the newly hatched confused baby at (112,155).</summary>
    public static IntroBabyActorDefinition ConfusedBaby =>
        new(0xce79, 0xba4b, ConfusedBabyInitialPreInstruction, 0xcc2b, 0x0070, 0x009b, 0x0e00);
}

/// <summary>
/// One native six-byte cinematic-object definition together with the fixed position and
/// OBJ attributes written by its initialization callback.
/// </summary>
internal readonly record struct IntroBabyActorDefinition(
    ushort Pointer,
    ushort Initialization,
    ushort PreInstruction,
    ushort InstructionList,
    ushort X,
    ushort Y,
    ushort PaletteBits);
