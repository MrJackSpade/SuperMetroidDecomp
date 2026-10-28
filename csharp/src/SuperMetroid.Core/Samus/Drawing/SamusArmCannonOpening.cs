using static SuperMetroid.Core.Samus.Drawing.SamusArmCannonDefinitions;

namespace SuperMetroid.Core.Samus.Drawing;

/// <summary>Whether the selected HUD item opens the arm cannon.</summary>
internal static class SamusArmCannonOpening
{
    /// <summary>Returns the native desired cover state for a HUD item.</summary>
    public static byte DesiredOpenFlag(HudItem selectedHudItem) =>
        DesiredOpenFlags.TryGetValue(selectedHudItem, out byte value) ? value : throw new ArgumentOutOfRangeException(nameof(selectedHudItem), selectedHudItem, "HUD item is outside the six-entry dispatcher.");
}
