namespace SuperMetroid.Core.Assets;

/// <summary>Independent native color resources selected by the ending coroutine.</summary>
public enum EndingPaletteId
{
    /// <summary>$8C:EDE9, Palettes_CloudSpritesInZebesExplosionScene: 256-color image installed for the initial escape view and its atmospheric cloud sprites.</summary>
    Escape,
    /// <summary>$8C:E7E9, Palettes_PostCredits: 256-color reward-scene image; post-credits restoration preserves CGRAM colors 0 through 3.</summary>
    PostCredits,
    /// <summary>$8C:E9E9, Palettes_Credits: 256-color resource whose first 128 colors supply the scrolling credits backgrounds and text.</summary>
    Credits,
    /// <summary>$8C:EBE9, Palettes_ZebesExplosionScene: 256-color resource whose BG and OBJ halves are transferred separately during destruction and flyaway scenes.</summary>
    Explosion,
    /// <summary>$8B:DE43, .greyGunshipPalette: 16 BG colors restored at CGRAM index 80 for the final gunship view before the operation text.</summary>
    FinalGunship,
    /// <summary>$8C:EFE9, Palettes_EndingSuperMetroidIconFadingToGrey_Sprite_0: 16 colors initially loaded into OBJ palette 7 for the assembling logo.</summary>
    LogoInitial,
    /// <summary>$8B:E5E7 palette-pointer pairs consumed by E58A: sixteen successive BG/OBJ palette pairs, flattened as [step][BG then OBJ][16 colors].</summary>
    LogoCrossfade,
}
