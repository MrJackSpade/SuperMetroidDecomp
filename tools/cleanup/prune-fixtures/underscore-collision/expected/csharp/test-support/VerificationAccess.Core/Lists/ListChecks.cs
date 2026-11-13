using SuperMetroid.AssetExtraction.Lists;

namespace SuperMetroid.Verification.Lists;

/// <summary>The racks the checks compare.</summary>
internal static class ListChecks
{
    /// <summary>Twice the places of a rack of four.</summary>
    internal static int Doubled => new ListRack(new RackPlaces(4)).Doubled;
}
