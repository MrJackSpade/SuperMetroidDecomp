namespace SuperMetroid.Core.Frontend.Ending.Artwork;

/// <summary>Independent native color resources selected by the ending coroutine.</summary>
public enum EndingPaletteId
{
    /// <summary>256-color image installed for the initial escape view and its atmospheric cloud sprites.</summary>
    Escape,
    /// <summary>256-color reward-scene image; post-credits restoration preserves CGRAM colors 0 through 3.</summary>
    PostCredits,
    /// <summary>256-color resource whose first 128 colors supply the scrolling credits backgrounds and text.</summary>
    Credits,
    /// <summary>256-color resource whose BG and OBJ halves are transferred separately during destruction and flyaway scenes.</summary>
    Explosion,
    /// <summary>16 BG colors restored at CGRAM index 80 for the final gunship view before the operation text.</summary>
    FinalGunship,
    /// <summary>16 colors initially loaded into OBJ palette 7 for the assembling logo.</summary>
    LogoInitial,
    /// <summary>Sixteen successive BG/OBJ palette pairs of the logo crossfade, flattened as [step][BG then OBJ][16 colors].</summary>
    LogoCrossfade,
}
