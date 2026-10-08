namespace SuperMetroid.Core.Game;

/// <summary>Calculated initializer and corpse-rotting metadata for dead Tourian enemies.</summary>
internal static class DeadTourianCorpseDefinitions
{
    /// <summary>$A9:DD88 begins eight consecutive16-byte corpse configuration records.</summary>
    private const ushort FirstConfiguration = 0xdd88;
    /// <summary>$7E:92C0 begins the five64-byte Zoomer/Ripper rotting work areas.</summary>
    private const ushort FirstWideRottingArea = 0x92c0;
    /// <summary>$7E:9140 begins the three128-byte Skree rotting work areas.</summary>
    private const ushort FirstSkreeRottingArea = 0x9140;
    /// <summary>$A9:E134 begins five18-byte Zoomer/Ripper DMA lists.</summary>
    private const ushort FirstWideTransfer = 0xe134;
    /// <summary>$A9:E18E begins three34-byte Skree DMA lists.</summary>
    private const ushort FirstSkreeTransfer = 0xe18e;
    /// <summary>$A9:E66A begins five140-byte paired Zoomer/Ripper move/copy routines; copy begins79 bytes later.</summary>
    private const ushort FirstWideMove = 0xe66a;
    /// <summary>$A9:E926 begins three94-byte paired Skree move/copy routines; copy begins53 bytes later.</summary>
    private const ushort FirstSkreeMove = 0xe926;
    /// <summary>$A9:E24C/E252/E258 are the Zoomer/Ripper/Skree row-offset lists, six bytes apart because the preceding Zoomer/Ripper lists each contain three offsets.</summary>
    private const ushort FirstRotation = 0xe24c;
    /// <summary>$A9:DC08, CorpseRotEntryFinishedHook_Normal: shared completion callback.</summary>
    private const ushort Finish = 0xdc08;

    internal static DeadTourianCorpseDefinition For(DeadTourianCorpseSpecies species,int variantIndex)
    {
        int first = species switch
        {
            DeadTourianCorpseSpecies.Zoomer => 0,
            DeadTourianCorpseSpecies.Ripper => 3,
            DeadTourianCorpseSpecies.Skree => 5,
            _ => throw new ArgumentOutOfRangeException(nameof(species),species,null),
        };
        int count = species == DeadTourianCorpseSpecies.Ripper ? 2 : 3;
        if ((uint)variantIndex >= count) throw new ArgumentOutOfRangeException(nameof(variantIndex),variantIndex,null);
        int ordinal = first+variantIndex;
        bool skree = species == DeadTourianCorpseSpecies.Skree;
        int layout = skree ? variantIndex : ordinal;
        ushort move = (ushort)((skree ? FirstSkreeMove : FirstWideMove)+(skree ? 94 : 140)*layout);
        int rowTiles = skree ? 2 : 3;
        int rotationIndex = species == DeadTourianCorpseSpecies.Zoomer ? 0 : species == DeadTourianCorpseSpecies.Ripper ? 1 : 2;
        return new(
            DeadTourianCorpseInstructionProgramDefinitions.Program(ordinal),
            (ushort)(FirstConfiguration+16*ordinal),
            (ushort)((skree ? FirstSkreeRottingArea : FirstWideRottingArea)+(skree ? 128 : 64)*layout),
            (ushort)((skree ? FirstSkreeTransfer : FirstWideTransfer)+(skree ? 34 : 18)*layout),
            (ushort)(move+(skree ? 53 : 79)),move,
            skree ? (ushort)32 : (ushort)16,
            (ushort)(FirstRotation+6*rotationIndex),Finish,
            (ushort)(32*rowTiles-12));
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
    ushort RotationTablePointer,
    ushort FinishFunction,
    ushort WrapOffset);
