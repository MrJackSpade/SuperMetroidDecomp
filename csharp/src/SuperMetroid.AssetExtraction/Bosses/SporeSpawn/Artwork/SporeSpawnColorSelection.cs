using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Bosses.SporeSpawn.Artwork;

/// <summary>Spore Spawn's selected colours: the spores, the health bands and the three death layers.</summary>
/// <param name="Spores">Only spore colors that differ from the healthy band; stock art shares that band.</param>
/// <param name="Health">Painted health endpoints and intermediate edits used by boss palette selection.</param>
/// <param name="DeathSprite">Death-sprite endpoint colors plus samples that differ from the calculated seven-step fade.</param>
/// <param name="DeathLevel">Required final BG palette 4 fade step and independent earlier level-art colors.</param>
/// <param name="DeathBackground">Required final BG palette 7 fade step and independent earlier backdrop colors.</param>
internal sealed record SporeSpawnColorSelection(Dictionary<PaintColour, RgbColor> Spores, SporeSpawnHealthBands Health,
    Dictionary<PaintColour, RgbColor> DeathSprite, Dictionary<PaintColour, RgbColor> DeathLevel,
    Dictionary<PaintColour, RgbColor> DeathBackground);
