namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// The shapes of Lower Norfair Ridley's palettes (#1413): his initial body and tail colours, and the arena reveal's
/// rows. Where each sits in the cartridge is the import's knowledge.
/// </summary>
public static class NorfairRidleyPaletteDefinitions
{
    /// <summary>Ridley's initial colours: two complete sixteen-colour sprite palettes.</summary>
    public const int InitialColorCount = 32;

    /// <summary>The arena reveal's rows, one shown every third reveal step; the reveal ends after the last.</summary>
    public const int RevealRowCount = 15;

    /// <summary>The colours of one reveal row: the arena background palette's colours one to fourteen.</summary>
    public const int RevealColorCount = 14;
}
