namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>Initializer and corpse-rotting metadata for dead Tourian enemies.</summary>
internal static class DeadTourianCorpseDefinitions
{
    /// <summary>
    /// Each species' corpse variants: the pose program, in native configuration order; the rotting kind; and the corpse's
    /// height in pixel rows, sixteen for the Zoomer and Ripper and thirty-two for the Skree.
    /// </summary>
    internal static readonly Dictionary<DeadTourianCorpseSpecies, DeadTourianCorpseDefinition[]> Variants = new()
    {
        [DeadTourianCorpseSpecies.Zoomer] =
        [
            new(DeadTourianProgram.ZoomerCorpse0, new RottingCorpseKind(RottingCorpseSpecies.Zoomer, 0), 16),
            new(DeadTourianProgram.ZoomerCorpse1, new RottingCorpseKind(RottingCorpseSpecies.Zoomer, 1), 16),
            new(DeadTourianProgram.ZoomerCorpse2, new RottingCorpseKind(RottingCorpseSpecies.Zoomer, 2), 16),
        ],
        [DeadTourianCorpseSpecies.Ripper] =
        [
            new(DeadTourianProgram.RipperCorpse0, new RottingCorpseKind(RottingCorpseSpecies.Ripper, 0), 16),
            new(DeadTourianProgram.RipperCorpse1, new RottingCorpseKind(RottingCorpseSpecies.Ripper, 1), 16),
        ],
        [DeadTourianCorpseSpecies.Skree] =
        [
            new(DeadTourianProgram.SkreeCorpse0, new RottingCorpseKind(RottingCorpseSpecies.Skree, 0), 32),
            new(DeadTourianProgram.SkreeCorpse1, new RottingCorpseKind(RottingCorpseSpecies.Skree, 1), 32),
            new(DeadTourianProgram.SkreeCorpse2, new RottingCorpseKind(RottingCorpseSpecies.Skree, 2), 32),
        ],
    };
}
