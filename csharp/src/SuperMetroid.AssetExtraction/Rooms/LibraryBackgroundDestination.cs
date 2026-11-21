namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// Where one library-background command puts what it reads (#1414), named for what the room shows there rather than the
/// video memory the cartridge writes; extraction maps each destination to the cartridge's.
/// </summary>
public enum LibraryBackgroundDestination
{

    /// <summary>The first of the background layer's two tilemap pages; a two-page transfer fills both.</summary>
    BackgroundFirstPage = 1,

    /// <summary>The second of the background layer's two tilemap pages.</summary>
    BackgroundSecondPage = 2,

    /// <summary>The two further background pages Kraid's room shows his body on.</summary>
    KraidBackgroundPages = 3,

    /// <summary>The sprite characters the Tourian entrance statue's ghost would be drawn from.</summary>
    StatueGhostSpriteCharacters = 4,

    /// <summary>The layer-3 characters Kraid's room reads the HUD from while his body covers the standard ones.</summary>
    KraidLayer3Characters = 5,
}
