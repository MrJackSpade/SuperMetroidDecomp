using SuperMetroid.AssetExtraction.Lists;

namespace SuperMetroid.Verification.Lists;

/// <summary>The slots the checks compare.</summary>
internal static class ListChecks
{
    /// <summary>The free slots of four places, one of them taken, and the slots held by none.</summary>
    internal static int Compared
    {
        get
        {
            ListPlaces places = new(4);
            places.Take();
            ListSlots slots = new(places);
            return slots.Free + ListSlots.HeldBy(slots) + ListSlots.HeldBy(null);
        }
    }
}
