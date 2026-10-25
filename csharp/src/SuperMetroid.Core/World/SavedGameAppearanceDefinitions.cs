using SuperMetroid.Core.Fx.PaletteFx;

namespace SuperMetroid.Core.World;

/// <summary>How Samus appears when a saved game loads at a station: her suit's loading glow and how long the appearance lasts.</summary>
internal static class SavedGameAppearanceDefinitions
{
    /// <summary>The loading glow Samus shows in the Power Suit.</summary>
    public const PaletteFxObject PowerSuitGlow = PaletteFxObject.SamusLoadingPowerSuit;

    /// <summary>The loading glow Samus shows in the Varia Suit.</summary>
    public const PaletteFxObject VariaSuitGlow = PaletteFxObject.SamusLoadingVariaSuit;

    /// <summary>The loading glow Samus shows in the Gravity Suit, which takes precedence over the Varia Suit.</summary>
    public const PaletteFxObject GravitySuitGlow = PaletteFxObject.SamusLoadingGravitySuit;

    /// <summary>Frames the saved-game appearance lasts before Samus takes ordinary input.</summary>
    public const int FrameCount = 360;
}
