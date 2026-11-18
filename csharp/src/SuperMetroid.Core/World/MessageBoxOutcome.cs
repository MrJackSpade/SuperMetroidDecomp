using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.World;

/// <summary>
/// What waits for an open message box to close (#2486): the gunship's save answer, a save station's confirmation or its
/// completion, or a suit's transformation. One message box is open at a time, so at most one outcome waits; an ordinary
/// item or station message waits for none.
/// </summary>
internal abstract record MessageBoxOutcome
{
    /// <summary>Only the outcomes below exist.</summary>
    private MessageBoxOutcome()
    {
    }

    /// <summary>The gunship's save prompt waits for its answer: a YES saves at the gunship, and either answer lets Samus out.</summary>
    internal sealed record GunshipSaveAnswer : MessageBoxOutcome;

    /// <summary>A save station waits for the save confirmation's answer.</summary>
    /// <param name="Station">The save station whose floor trigger asked to save.</param>
    internal sealed record SaveConfirmation(SaveStationActivation Station) : MessageBoxOutcome;

    /// <summary>A save station's animation waits for its completion message to close.</summary>
    /// <param name="Station">The save station that saved.</param>
    internal sealed record SaveCompletion(SaveStationActivation Station) : MessageBoxOutcome;

    /// <summary>A suit's transformation waits for its item message to return to the item's room object.</summary>
    /// <param name="Suit">The suit picked up.</param>
    internal sealed record SuitTransformation(SamusSuitPickupKind Suit) : MessageBoxOutcome;
}
