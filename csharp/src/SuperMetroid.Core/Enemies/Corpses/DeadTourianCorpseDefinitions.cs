namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>Calculated initializer and corpse-rotting metadata for dead Tourian enemies.</summary>
internal static class DeadTourianCorpseDefinitions
{
    /// <summary>$7E:92C0 begins the five64-byte Zoomer/Ripper rotting work areas.</summary>
    private const ushort FirstWideRottingArea = 0x92c0;
    /// <summary>$7E:9140 begins the three128-byte Skree rotting work areas.</summary>
    private const ushort FirstSkreeRottingArea = 0x9140;
    /// <summary>$A9:E134 begins five18-byte Zoomer/Ripper DMA lists.</summary>
    private const ushort FirstWideTransfer = 0xe134;
    /// <summary>$A9:E18E begins three34-byte Skree DMA lists.</summary>
    private const ushort FirstSkreeTransfer = 0xe18e;

    /// <summary>The corpse pose program of the eight Zoomer, Ripper and Skree variants, in native configuration order.</summary>
    private static DeadTourianProgram CorpseProgram(int ordinal) => ordinal switch
    {
        0 => DeadTourianProgram.ZoomerCorpse0,
        1 => DeadTourianProgram.ZoomerCorpse1,
        2 => DeadTourianProgram.ZoomerCorpse2,
        3 => DeadTourianProgram.RipperCorpse0,
        4 => DeadTourianProgram.RipperCorpse1,
        5 => DeadTourianProgram.SkreeCorpse0,
        6 => DeadTourianProgram.SkreeCorpse1,
        7 => DeadTourianProgram.SkreeCorpse2,
        _ => throw new ArgumentOutOfRangeException(nameof(ordinal), ordinal, null),
    };

    /// <summary>Calculates one Zoomer, Ripper, or Skree corpse's animation, rotting, DMA, movement, and rotation identities.</summary>
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
        int rowTiles = skree ? 2 : 3;
        return new(
            CorpseProgram(ordinal),
            (ushort)((skree ? FirstSkreeRottingArea : FirstWideRottingArea)+(skree ? 128 : 64)*layout),
            (ushort)((skree ? FirstSkreeTransfer : FirstWideTransfer)+(skree ? 34 : 18)*layout),            skree ? (ushort)32 : (ushort)16,            (ushort)(32*rowTiles-12));
    }
}
