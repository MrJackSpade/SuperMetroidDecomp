namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>
/// The dead Zoomer, Ripper and Skree corpses a Tourian room places (#1810): each named for how the corpse lies, with its
/// pose animation and its height in pixel rows, sixteen for the Zoomer and Ripper and thirty-two for the Skree. Corpses
/// that lie the same way are one corpse.
/// </summary>
internal static class DeadTourianCorpseDefinitions
{
    /// <summary>A dead Zoomer lying on its back.</summary>
    internal static readonly DeadTourianCorpseDefinition ZoomerOnItsBack =
        new(DeadTourianCorpseSpecies.Zoomer, DeadTourianSegment.ZoomerCorpseOnItsBack, 16);

    /// <summary>A dead Zoomer lying the right way up.</summary>
    internal static readonly DeadTourianCorpseDefinition ZoomerUpright =
        new(DeadTourianCorpseSpecies.Zoomer, DeadTourianSegment.ZoomerCorpseUpright, 16);

    /// <summary>A dead Ripper lying level.</summary>
    internal static readonly DeadTourianCorpseDefinition RipperLevel =
        new(DeadTourianCorpseSpecies.Ripper, DeadTourianSegment.RipperCorpseLevel, 16);

    /// <summary>A dead Ripper lying tilted, nose down.</summary>
    internal static readonly DeadTourianCorpseDefinition RipperTilted =
        new(DeadTourianCorpseSpecies.Ripper, DeadTourianSegment.RipperCorpseTilted, 16);

    /// <summary>A dead Skree, its one pose.</summary>
    internal static readonly DeadTourianCorpseDefinition Skree =
        new(DeadTourianCorpseSpecies.Skree, DeadTourianSegment.SkreeCorpse, 32);
}
