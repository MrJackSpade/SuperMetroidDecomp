namespace SuperMetroid.Core.World;

/// <summary>Where the Tourian statues stand in their descent after the room loads with them raised.</summary>
internal enum TourianStatueDescentPhase
{
    /// <summary>The statues wait until all four have turned grey and no release is still playing.</summary>
    AwaitingReleases,

    /// <summary>The ground shakes while the statues hold raised for the authored pre-descent hold; its end raises the descent dust.</summary>
    Holding,

    /// <summary>The ground shakes while the statues lower a quarter pixel per frame toward the authored distance.</summary>
    Lowering,
}
