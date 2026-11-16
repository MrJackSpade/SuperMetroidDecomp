namespace SuperMetroid.Core.Samus;

/// <summary>Cartridge values and helpers of <see cref="HudItems"/> that only verification reads.</summary>
internal static class HudItemsConstants
{
    /// <summary>Every HUD item, in the order the select button steps through them.</summary>
    public static IReadOnlyList<HudItem> All => HudItems.SelectionRing;
}
