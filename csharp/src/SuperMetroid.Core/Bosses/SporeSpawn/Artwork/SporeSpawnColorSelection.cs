using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.SporeSpawn.Artwork;

/// <summary>Spore Spawn's selected colours: the spores, the health rows and the three death layers.</summary>
/// <param name="Spores">Only spore colors that differ from the healthy boss row; stock art shares that row.</param>
/// <param name="Health">Painted health endpoints and intermediate edits used by boss palette selection.</param>
/// <param name="DeathSprite">Death-sprite endpoint colors plus samples that differ from the calculated seven-step fade.</param>
/// <param name="DeathLevel">Required final BG palette 4 row and independent earlier level-art colors.</param>
/// <param name="DeathBackground">Required final BG palette 7 row and independent earlier backdrop colors.</param>
internal sealed record SporeSpawnColorSelection(Dictionary<ColourCell, RgbColor> Spores, SporeSpawnHealthRows Health,
    Dictionary<ColourCell, RgbColor> DeathSprite, Dictionary<ColourCell, RgbColor> DeathLevel,
    Dictionary<ColourCell, RgbColor> DeathBackground);
