using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Colours;

/// <summary>Which suit Samus shows.</summary>
public static class SamusSuits
{
    /// <summary>
    /// The suit whose colours Samus shows with <paramref name="equippedItems"/>. Gravity wins when both suits are
    /// equipped; this is an ordinary three-way choice, not another flag set.
    /// </summary>
    public static SamusSuit ShownSuit(this SamusEquipmentFlags equippedItems)
    {
        if (equippedItems.HasAny(SamusEquipmentFlags.GravitySuit))
            return SamusSuit.Gravity;
        return equippedItems.HasAny(SamusEquipmentFlags.VariaSuit) ? SamusSuit.Varia : SamusSuit.Power;
    }

}
