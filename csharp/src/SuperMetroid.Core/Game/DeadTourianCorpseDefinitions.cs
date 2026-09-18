namespace SuperMetroid.Core.Game;

/// <summary>Compiled initializer and corpse-rotting metadata for dead Tourian enemies.</summary>
internal static class DeadTourianCorpseDefinitions
{
    /// <summary>
    /// Dead Zoomer selectors at <c>$A9:D86A-$A9:D875</c> and configurations at
    /// <c>$A9:DD88-$A9:DDB7</c>.
    /// </summary>
    private static readonly DeadTourianCorpseDefinition[] Zoomer =
    [
        new(0xecf5, 0xdd88, 0x92c0, 0xe134, 0xe6b9, 0xe66a, 0x0010, 0xdf4f, 0xe24c, 0xdc08, 0x0054),
        new(0xecfb, 0xdd98, 0x9300, 0xe146, 0xe745, 0xe6f6, 0x0010, 0xdf6c, 0xe24c, 0xdc08, 0x0054),
        new(0xed01, 0xdda8, 0x9340, 0xe158, 0xe7d1, 0xe782, 0x0010, 0xdf89, 0xe24c, 0xdc08, 0x0054),
    ];

    /// <summary>
    /// Dead Ripper selectors at <c>$A9:D897-$A9:D89E</c> and configurations at
    /// <c>$A9:DDB8-$A9:DDD7</c>.
    /// </summary>
    private static readonly DeadTourianCorpseDefinition[] Ripper =
    [
        new(0xed07, 0xddb8, 0x9380, 0xe16a, 0xe85d, 0xe80e, 0x0010, 0xdfa6, 0xe252, 0xdc08, 0x0054),
        new(0xed0d, 0xddc8, 0x93c0, 0xe17c, 0xe8e9, 0xe89a, 0x0010, 0xdfc3, 0xe252, 0xdc08, 0x0054),
    ];

    /// <summary>
    /// Dead Skree selectors at <c>$A9:D8C0-$A9:D8CB</c> and configurations at
    /// <c>$A9:DDD8-$A9:DE07</c>.
    /// </summary>
    private static readonly DeadTourianCorpseDefinition[] Skree =
    [
        new(0xed13, 0xddd8, 0x9140, 0xe18e, 0xe95b, 0xe926, 0x0020, 0xdfe0, 0xe258, 0xdc08, 0x0034),
        new(0xed19, 0xdde8, 0x91c0, 0xe1b0, 0xe9b9, 0xe984, 0x0020, 0xe019, 0xe258, 0xdc08, 0x0034),
        new(0xed1f, 0xddf8, 0x9240, 0xe1d2, 0xea17, 0xe9e2, 0x0020, 0xe052, 0xe258, 0xdc08, 0x0034),
    ];

    /// <summary>Returns one parameter-selected native initialization record.</summary>
    internal static DeadTourianCorpseDefinition For(
        DeadTourianCorpseSpecies species,
        int variantIndex)
    {
        DeadTourianCorpseDefinition[] definitions = species switch
        {
            DeadTourianCorpseSpecies.Zoomer => Zoomer,
            DeadTourianCorpseSpecies.Ripper => Ripper,
            DeadTourianCorpseSpecies.Skree => Skree,
            _ => throw new ArgumentOutOfRangeException(nameof(species), species, null),
        };
        if ((uint)variantIndex >= definitions.Length)
            throw new ArgumentOutOfRangeException(nameof(variantIndex), variantIndex, null);
        return definitions[variantIndex];
    }
}

/// <summary>One immutable native dead-monster initialization record.</summary>
internal readonly record struct DeadTourianCorpseDefinition(
    ushort InitialInstructionPointer,
    ushort ConfigurationPointer,
    ushort RottingTablePointer,
    ushort VramTransferPointer,
    ushort CopyFunction,
    ushort MoveFunction,
    ushort EntryCount,
    ushort GraphicsInitializationFunction,
    ushort RotationTablePointer,
    ushort FinishFunction,
    ushort WrapOffset);
