using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Hud.Artwork;

/// <summary>
/// What each item icon stands for (#1908): the selectable item whose selection shades it. The installation names each icon
/// by its installed key (<see cref="HudIconKeys.Installed"/>), and the installed layout places it by that key.
/// </summary>
internal static class HudIconDefinitions
{
    /// <summary>The selectable item each icon shows; the beam selection has no icon.</summary>
    internal static IReadOnlyDictionary<HudIcon, HudItem> Items { get; } = new Dictionary<HudIcon, HudItem>
    {
        [HudIcon.Missile] = HudItem.Missile,
        [HudIcon.SuperMissile] = HudItem.SuperMissile,
        [HudIcon.PowerBomb] = HudItem.PowerBomb,
        [HudIcon.Grapple] = HudItem.Grapple,
        [HudIcon.XRay] = HudItem.XRay,
    };
}
