namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled form of the complete 29-entry message-definition table at
/// $85:869B-$85:8748. The selected tilemap payloads remain cartridge presentation data.
/// </summary>
internal static class GameplayMessageDefinitions
{
    /// <summary>Number of native records, including both content-boundary terminators.</summary>
    public const int NativeDefinitionCount = 29;

    private static readonly GameplayMessageDefinition[] Definitions =
    [
        Small(0x877f),
        Shoot(0x87bf),
        Shoot(0x88bf),
        Shoot(0x89bf),
        Shoot(0x8abf),
        Run(0x8bbf),
        Small(0x8cbf),
        Small(0x8cff),
        Small(0x8d3f),
        Small(0x8d7f),
        Small(0x8dbf),
        Small(0x8dff),
        Run(0x8e3f),
        Small(0x8f3f),
        Small(0x8f7f),
        Small(0x8fbf),
        Small(0x8fff),
        Small(0x903f),
        Shoot(0x907f),
        Small(0x917f),
        Small(0x923f),
        Small(0x92ff),
        LargeSetupSmallDraw(0x93bf),
        Small(0x94bf),
        Small(0x94ff),
        Small(0x953f),
        Small(0x957f),
        LargeSetupSmallDraw(0x93bf),
        Small(0x94bf),
    ];

    /// <summary>Returns one one-based native record, including terminator records 27 and 29.</summary>
    public static GameplayMessageDefinition AtNativeIndex(int oneBasedIndex)
    {
        if (oneBasedIndex is < 1 or > NativeDefinitionCount)
            throw new ArgumentOutOfRangeException(nameof(oneBasedIndex));
        return Definitions[oneBasedIndex - 1];
    }

    private static GameplayMessageDefinition Small(ushort contentPointer) => new(
        GameplayMessageRomData.Routines.SetupSmall,
        GameplayMessageRomData.Routines.DrawSmallTilemap,
        contentPointer);

    private static GameplayMessageDefinition Shoot(ushort contentPointer) => new(
        GameplayMessageRomData.Routines.PatchShootButton,
        GameplayMessageRomData.Routines.DrawLargeTilemap,
        contentPointer);

    private static GameplayMessageDefinition Run(ushort contentPointer) => new(
        GameplayMessageRomData.Routines.PatchRunButton,
        GameplayMessageRomData.Routines.DrawLargeTilemap,
        contentPointer);

    private static GameplayMessageDefinition LargeSetupSmallDraw(ushort contentPointer) => new(
        GameplayMessageRomData.Routines.SetupLarge,
        GameplayMessageRomData.Routines.DrawSmallTilemap,
        contentPointer);
}

/// <summary>One native setup callback, draw callback, and presentation payload pointer.</summary>
internal readonly record struct GameplayMessageDefinition(
    ushort ModifyFunction,
    ushort DrawFunction,
    ushort ContentPointer);
