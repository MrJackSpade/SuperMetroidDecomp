using System.Globalization;

namespace SuperMetroid.AssetExtraction.Enemies.Corpses;

/// <summary>
/// One of the cartridge's dead-monster row upload lists (#1406, #1539): the list a species' corpse variant names, which the
/// extraction decodes to find the characters the corpse's rows show in. Each corpse rots its own rows in the game; the list
/// is only the import's way to find where they lie in its pictures.
/// </summary>
/// <param name="Species">The corpse species.</param>
/// <param name="Variant">Which of the species' corpse variants names the list.</param>
internal readonly record struct CorpseRowUploadList(RottingCorpseSpecies Species, int Variant)
{
    /// <summary>The list's name, for messages.</summary>
    public string Name => Species.ToString() + Variant.ToString(CultureInfo.InvariantCulture);
}
