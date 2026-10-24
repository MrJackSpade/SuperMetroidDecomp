namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>Calculated initializer and corpse-rotting metadata for dead Tourian enemies.</summary>
internal static class DeadTourianCorpseDefinitions
{
    /// <summary>The five 64-byte Zoomer and Ripper rot tables begin this far into the rotting heap.</summary>
    private const int FirstWideRotTable = 0x2c0;
    /// <summary>The three 128-byte Skree rot tables begin this far into the rotting heap.</summary>
    private const int FirstSkreeRotTable = 0x140;

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

    /// <summary>Calculates one Zoomer, Ripper, or Skree corpse's animation, rotting, kind, movement, and rotation identities.</summary>
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
        RottingCorpseSpecies kind = species switch
        {
            DeadTourianCorpseSpecies.Zoomer => RottingCorpseSpecies.Zoomer,
            DeadTourianCorpseSpecies.Ripper => RottingCorpseSpecies.Ripper,
            DeadTourianCorpseSpecies.Skree => RottingCorpseSpecies.Skree,
            _ => throw new ArgumentOutOfRangeException(nameof(species), species, null),
        };
        return new(
            CorpseProgram(ordinal),
            (skree ? FirstSkreeRotTable : FirstWideRotTable) + (skree ? 128 : 64) * layout,
            new RottingCorpseKind(kind, variantIndex),
            skree ? (ushort)32 : (ushort)16,
            (ushort)(32 * rowTiles - 12));
    }
}
