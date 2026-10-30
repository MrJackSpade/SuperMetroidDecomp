using static SuperMetroid.Core.Samus.Drawing.SamusArmCannonDefinitions;

namespace SuperMetroid.Core.Samus.Drawing;

/// <summary>Whether the selected HUD item opens the arm cannon.</summary>
internal static class SamusArmCannonOpening
{
    /// <summary>Whether a HUD item opens the cover.</summary>
    public static bool Opens(HudItem selectedHudItem) =>
        OpensCover.ContainsKey(selectedHudItem) ? OpensCover[selectedHudItem] : throw new ArgumentOutOfRangeException(nameof(selectedHudItem), selectedHudItem, "HUD item is outside the six-entry dispatcher.");
}
