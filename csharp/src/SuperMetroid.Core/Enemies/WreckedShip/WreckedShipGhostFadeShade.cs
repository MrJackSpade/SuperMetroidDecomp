using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.WreckedShip.Artwork;

namespace SuperMetroid.Core.Enemies.WreckedShip;

/// <summary>
/// The shade the Wrecked Ship ghost shows for its fading colours (#1512, #1552): the effect that last changed them and how
/// many of its steps did. The ghost's slot shows it, and the import pre-colours every one the ghost passes through.
/// </summary>
internal static class WreckedShipGhostFadeShade
{
    /// <summary>The shade naming the colours <paramref name="fade"/> shows.</summary>
    internal static PictureShade Of(WreckedShipGhostFade fade) => new(EffectName(fade.ChangedBy), fade.StepsChanged);

    /// <summary>The name the import gives <paramref name="change"/>'s shades.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="change"/> is not a defined effect.</exception>
    private static string EffectName(WreckedShipGhostFade.Effect change) => change switch
    {
        WreckedShipGhostFade.Effect.Dark => WreckedShipGhostShades.DarkEffect,
        WreckedShipGhostFade.Effect.Brightening => WreckedShipGhostShades.BrighteningEffect,
        WreckedShipGhostFade.Effect.Materializing => WreckedShipGhostShades.MaterializingEffect,
        WreckedShipGhostFade.Effect.Vanishing => WreckedShipGhostShades.VanishingEffect,
        _ => throw new ArgumentOutOfRangeException(nameof(change), change, "The Wrecked Ship ghost's colours change by a defined effect."),
    };
}
