namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>
/// Initializer and corpse-rotting metadata for dead Tourian enemies: each corpse's pose program, its rotting kind, and
/// its height in pixel rows, sixteen for the Zoomer and Ripper and thirty-two for the Skree.
/// </summary>
internal static class DeadTourianCorpseDefinitions
{
    /// <summary>
    /// Each species' corpses, by their place in the species' configuration order: the pose each lies in and the rot it
    /// shows. A room places a corpse by its species and that place.
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
