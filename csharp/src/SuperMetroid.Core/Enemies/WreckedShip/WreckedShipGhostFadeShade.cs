using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.WreckedShip.Artwork;

namespace SuperMetroid.Core.Enemies.WreckedShip;

/// <summary>
/// The shade the Wrecked Ship ghost shows for its fading colours (#1512, #1552): the effect that last changed them and how
/// many of its steps did. The ghost shows it, and the import pre-colours every one the ghost passes through.
/// </summary>
internal static class WreckedShipGhostFadeShade
{
    /// <summary>The shade naming the colours <paramref name="fade"/> shows.</summary>
    internal static PictureShade Of(WreckedShipGhostFade fade) => new(EffectName(fade.ChangedBy), fade.StepsChanged);

    /// <summary>The effect name of each change.</summary>
    private static IdentityTable<WreckedShipGhostFade.Effect, string> EffectNameByChange => field ??= new(new Dictionary<WreckedShipGhostFade.Effect, string>
    {
        [WreckedShipGhostFade.Effect.Dark] = WreckedShipGhostShades.DarkEffect,
        [WreckedShipGhostFade.Effect.Brightening] = WreckedShipGhostShades.BrighteningEffect,
        [WreckedShipGhostFade.Effect.Materializing] = WreckedShipGhostShades.MaterializingEffect,
        [WreckedShipGhostFade.Effect.Vanishing] = WreckedShipGhostShades.VanishingEffect,
    });

    /// <summary>The name the import gives <paramref name="change"/>'s shades.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="change"/> is not a defined effect.</exception>
    private static string EffectName(WreckedShipGhostFade.Effect change) => EffectNameByChange[change];
}
