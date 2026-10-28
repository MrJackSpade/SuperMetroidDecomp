namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>Initializer and corpse-rotting metadata for dead Tourian enemies.</summary>
internal static class DeadTourianCorpseDefinitions
{
    /// <summary>
    /// Each species' corpse variants: the pose program, in native configuration order; the rot table, five 64-byte
    /// Zoomer and Ripper tables from $2C0 and three 128-byte Skree tables from $140 in the rotting heap; the rotting
    /// kind; the entries per table, sixteen for the three-tile-wide Zoomer and Ripper and thirty-two for the two-tile-wide
    /// Skree; and the wrap offset, 32 per row tile less 12.
    /// </summary>
    internal static readonly Dictionary<DeadTourianCorpseSpecies, DeadTourianCorpseDefinition[]> Variants = new()
    {
        [DeadTourianCorpseSpecies.Zoomer] =
        [
            new(DeadTourianProgram.ZoomerCorpse0, 0x2c0, new RottingCorpseKind(RottingCorpseSpecies.Zoomer, 0), 16, 84),
            new(DeadTourianProgram.ZoomerCorpse1, 0x300, new RottingCorpseKind(RottingCorpseSpecies.Zoomer, 1), 16, 84),
            new(DeadTourianProgram.ZoomerCorpse2, 0x340, new RottingCorpseKind(RottingCorpseSpecies.Zoomer, 2), 16, 84),
        ],
        [DeadTourianCorpseSpecies.Ripper] =
        [
            new(DeadTourianProgram.RipperCorpse0, 0x380, new RottingCorpseKind(RottingCorpseSpecies.Ripper, 0), 16, 84),
            new(DeadTourianProgram.RipperCorpse1, 0x3c0, new RottingCorpseKind(RottingCorpseSpecies.Ripper, 1), 16, 84),
        ],
        [DeadTourianCorpseSpecies.Skree] =
        [
            new(DeadTourianProgram.SkreeCorpse0, 0x140, new RottingCorpseKind(RottingCorpseSpecies.Skree, 0), 32, 52),
            new(DeadTourianProgram.SkreeCorpse1, 0x1c0, new RottingCorpseKind(RottingCorpseSpecies.Skree, 1), 32, 52),
            new(DeadTourianProgram.SkreeCorpse2, 0x240, new RottingCorpseKind(RottingCorpseSpecies.Skree, 2), 32, 52),
        ],
    };
}
