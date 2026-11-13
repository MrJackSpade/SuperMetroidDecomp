using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.Crocomire.Artwork;

/// <summary>
/// Crocomire's skeleton look (#1406, #1538): how long his death spends baring the skeleton behind the wall before it shows,
/// and the look it shows.
/// </summary>
internal static class CrocomireSkeletonLookDefinitions
{
    /// <summary>
    /// The updates his death spends baring the skeleton while it waits behind the wall: his frames show the skeleton look
    /// from the last of them on, as the skeleton is complete on the sixth update of its wait.
    /// </summary>
    internal const int BaringDuration = 6;

    /// <summary>His look once his death has bared the skeleton, which his skeleton's frames and the spike wall's pieces show.</summary>
    internal static EncounterLook SkeletonLook { get; } = new(EncounterArtwork.Crocomire, "Skeleton");
}
