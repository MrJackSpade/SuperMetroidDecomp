using static SuperMetroid.Core.Samus.Drawing.SamusArmCannonDefinitions;

namespace SuperMetroid.Core.Samus.Drawing;

/// <summary>Whether the selected HUD item opens the arm cannon.</summary>
internal static partial class SamusArmCannonOpening
{
    /// <summary>Whether a HUD item opens the cover.</summary>
    public static bool Opens(HudItem selectedHudItem) => OpensCover[selectedHudItem];
}
