namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>
/// Initializer and corpse-rotting metadata for dead Tourian enemies: each corpse's pose program and its height in pixel
/// rows, sixteen for the Zoomer and Ripper and thirty-two for the Skree.
/// </summary>
internal static class DeadTourianCorpseDefinitions
{
    /// <summary>
    /// Each species' corpses, by their place in the species' configuration order: the pose each lies in and its height. A
    /// room places a corpse by its species and that place.
    /// </summary>
    internal static readonly Dictionary<DeadTourianCorpseSpecies, DeadTourianCorpseDefinition[]> Variants = new()
    {
        [DeadTourianCorpseSpecies.Zoomer] =
        [
            new(DeadTourianProgram.ZoomerCorpse0, 16),
            new(DeadTourianProgram.ZoomerCorpse1, 16),
            new(DeadTourianProgram.ZoomerCorpse2, 16),
        ],
        [DeadTourianCorpseSpecies.Ripper] =
        [
            new(DeadTourianProgram.RipperCorpse0, 16),
            new(DeadTourianProgram.RipperCorpse1, 16),
        ],
        [DeadTourianCorpseSpecies.Skree] =
        [
            new(DeadTourianProgram.SkreeCorpse0, 32),
            new(DeadTourianProgram.SkreeCorpse1, 32),
            new(DeadTourianProgram.SkreeCorpse2, 32),
        ],
    };
}
