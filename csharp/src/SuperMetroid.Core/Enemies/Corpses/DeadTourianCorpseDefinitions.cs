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
            new(DeadTourianSegment.ZoomerCorpse0, 16),
            new(DeadTourianSegment.ZoomerCorpse1, 16),
            new(DeadTourianSegment.ZoomerCorpse2, 16),
        ],
        [DeadTourianCorpseSpecies.Ripper] =
        [
            new(DeadTourianSegment.RipperCorpse0, 16),
            new(DeadTourianSegment.RipperCorpse1, 16),
        ],
        [DeadTourianCorpseSpecies.Skree] =
        [
            new(DeadTourianSegment.SkreeCorpse0, 32),
            new(DeadTourianSegment.SkreeCorpse1, 32),
            new(DeadTourianSegment.SkreeCorpse2, 32),
        ],
    };
}
