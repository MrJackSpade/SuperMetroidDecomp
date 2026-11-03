using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.AssetExtraction.Bosses.MotherBrain.Artwork;

/// <summary>The live Shitroid scene's selected colours: the normal pulse and the three initialization targets.</summary>
/// <param name="Normal">Uses the giant Baby Metroid's selected target inks when they reproduce every pulse frame; otherwise retains the supplied frames.</param>
/// <param name="Sidehopper">Live victim colors copied into the Sidehopper palette during encounter setup.</param>
/// <param name="Shitroid">Visible giant Baby Metroid inks, excluding its retained transparent slot.</param>
/// <param name="ShitroidTransparentSlot">
/// The Shitroid's transparent-slot colour is copied to the target palette. The stock value is an exact transparent-slot
/// compatibility payload: sprite ink 0 is skipped before color lookup. It has no visible hue to derive; independently supplied replacements remain exact.
/// </param>
/// <param name="DeadSidehopper">Victim colors selected after the giant Baby Metroid drains the Sidehopper.</param>
internal sealed record ShitroidColorSelection(ShitroidColorPulse Normal, SidehopperInitialPalette Sidehopper,
    BabyMetroidInitialPalette Shitroid, RgbColor ShitroidTransparentSlot, SidehopperCorpsePalette DeadSidehopper);
