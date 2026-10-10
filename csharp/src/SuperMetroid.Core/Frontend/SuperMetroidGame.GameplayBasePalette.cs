using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend;

public sealed partial class SuperMetroidGame
{
    /// <summary>Optional installed base-palette data passed into gameplay runtime construction and rebound after state loading.</summary>
    [NonSerialized] private GameplayBasePaletteCatalog? gameplayBasePalettes;

    /// <summary>Attach installed palette data before game setup, or rebind after state load.</summary>
    public void BindGameplayBasePalettes(GameplayBasePaletteCatalog? catalog)
    {
        gameplayBasePalettes = catalog;
        if (runtime is not null) runtime.InitialPaletteArt = catalog;
    }
}
