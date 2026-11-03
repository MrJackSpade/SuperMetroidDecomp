namespace SuperMetroid.Core.Enemies.WreckedShip.Artwork;

/// <summary>
/// The effects the Wrecked Ship ghost's shades are named for (#1512): its darkened colours, its brightening, and its
/// fades that materialize and vanish it. Each step's shade is the effect and the steps it has taken
/// (WreckedShipGhostFade.Shade); the import pre-colours every one the ghost passes through.
/// </summary>
public static class WreckedShipGhostShades
{
    /// <summary>The ghost's colours darkened to black.</summary>
    public const string DarkEffect = "wrecked-ship-ghost-dark";

    /// <summary>The ghost's brightening from black.</summary>
    public const string BrighteningEffect = "wrecked-ship-ghost-brightening";

    /// <summary>The ghost's fade into its own colours.</summary>
    public const string MaterializingEffect = "wrecked-ship-ghost-materializing";

    /// <summary>The ghost's fade to white.</summary>
    public const string VanishingEffect = "wrecked-ship-ghost-vanishing";
}
