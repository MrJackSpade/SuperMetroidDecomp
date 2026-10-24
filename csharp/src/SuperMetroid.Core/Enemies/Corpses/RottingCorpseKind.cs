using System.Globalization;

namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>
/// One kind of rotting corpse (#1406): a species' corpse variant, whose staged characters every corpse of the kind rots
/// together. Two corpses of one kind therefore show the same rot, whichever of them is rotting.
/// </summary>
/// <param name="Species">The corpse species.</param>
/// <param name="Variant">Which of the species' corpse variants it is.</param>
internal readonly record struct RottingCorpseKind(RottingCorpseSpecies Species, int Variant)
{
    /// <summary>The kind's name, which its sheet's rot pictures are named for.</summary>
    public string Name => Species.ToString() + Variant.ToString(CultureInfo.InvariantCulture);
}
