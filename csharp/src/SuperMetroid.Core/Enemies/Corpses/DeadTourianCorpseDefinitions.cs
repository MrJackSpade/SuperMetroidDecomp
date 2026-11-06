namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>
/// Initializer and corpse-rotting metadata for dead Tourian enemies: each corpse's pose program, its rotting kind, and
/// its height in pixel rows, sixteen for the Zoomer and Ripper and thirty-two for the Skree.
/// </summary>
internal static class DeadTourianCorpseDefinitions
{
    /// <summary>The first dead Zoomer.</summary>
    internal static readonly DeadTourianCorpseDefinition ZoomerCorpse0 =
        new(DeadTourianProgram.ZoomerCorpse0, new RottingCorpseKind(RottingCorpseSpecies.Zoomer, 0), 16);

    /// <summary>The second dead Zoomer.</summary>
    internal static readonly DeadTourianCorpseDefinition ZoomerCorpse1 =
        new(DeadTourianProgram.ZoomerCorpse1, new RottingCorpseKind(RottingCorpseSpecies.Zoomer, 1), 16);

    /// <summary>The third dead Zoomer.</summary>
    internal static readonly DeadTourianCorpseDefinition ZoomerCorpse2 =
        new(DeadTourianProgram.ZoomerCorpse2, new RottingCorpseKind(RottingCorpseSpecies.Zoomer, 2), 16);

    /// <summary>The first dead Ripper.</summary>
    internal static readonly DeadTourianCorpseDefinition RipperCorpse0 =
        new(DeadTourianProgram.RipperCorpse0, new RottingCorpseKind(RottingCorpseSpecies.Ripper, 0), 16);

    /// <summary>The second dead Ripper.</summary>
    internal static readonly DeadTourianCorpseDefinition RipperCorpse1 =
        new(DeadTourianProgram.RipperCorpse1, new RottingCorpseKind(RottingCorpseSpecies.Ripper, 1), 16);

    /// <summary>The first dead Skree.</summary>
    internal static readonly DeadTourianCorpseDefinition SkreeCorpse0 =
        new(DeadTourianProgram.SkreeCorpse0, new RottingCorpseKind(RottingCorpseSpecies.Skree, 0), 32);

    /// <summary>The second dead Skree.</summary>
    internal static readonly DeadTourianCorpseDefinition SkreeCorpse1 =
        new(DeadTourianProgram.SkreeCorpse1, new RottingCorpseKind(RottingCorpseSpecies.Skree, 1), 32);

    /// <summary>The third dead Skree.</summary>
    internal static readonly DeadTourianCorpseDefinition SkreeCorpse2 =
        new(DeadTourianProgram.SkreeCorpse2, new RottingCorpseKind(RottingCorpseSpecies.Skree, 2), 32);

    /// <summary>Each species' corpses, in configuration order.</summary>
    internal static readonly Dictionary<DeadTourianCorpseSpecies, DeadTourianCorpseDefinition[]> Variants = new()
    {
        [DeadTourianCorpseSpecies.Zoomer] = [ZoomerCorpse0, ZoomerCorpse1, ZoomerCorpse2],
        [DeadTourianCorpseSpecies.Ripper] = [RipperCorpse0, RipperCorpse1],
        [DeadTourianCorpseSpecies.Skree] = [SkreeCorpse0, SkreeCorpse1, SkreeCorpse2],
    };
}
