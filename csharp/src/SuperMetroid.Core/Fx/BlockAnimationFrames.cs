using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Fx;

/// <summary>
/// The animations room effects, treadmills, spikes and sand show over a room's blocks (#1406): each is an animation whose
/// frames are the pictures it shows, which asset extraction pictures and the animation selects at the update the game
/// showed them.
/// </summary>
internal static partial class BlockAnimationFrames
{
    /// <summary>
    /// The installed name of each block animation (#2511), written out so renaming an animation's identity never changes a
    /// name on disk.
    /// </summary>
    private static readonly IdentityTable<BlockAnimation, string> Keys =
        new(new Dictionary<BlockAnimation, string>
        {
            [BlockAnimation.TourianStatuePhantoon] = "TourianStatuePhantoon block animation",
            [BlockAnimation.TourianStatueRidley] = "TourianStatueRidley block animation",
            [BlockAnimation.TourianStatueKraid] = "TourianStatueKraid block animation",
            [BlockAnimation.TourianStatueDraygon] = "TourianStatueDraygon block animation",
            [BlockAnimation.HorizontalSpikes] = "HorizontalSpikes block animation",
            [BlockAnimation.MaridiaSandCeiling] = "MaridiaSandCeiling block animation",
            [BlockAnimation.MaridiaSandFalling] = "MaridiaSandFalling block animation",
            [BlockAnimation.WreckedShipTreadmillRightwards] = "WreckedShipTreadmillRightwards block animation",
            [BlockAnimation.WreckedShipTreadmillLeftwards] = "WreckedShipTreadmillLeftwards block animation",
            [BlockAnimation.Lava] = "Lava block animation",
            [BlockAnimation.Acid] = "Acid block animation",
            [BlockAnimation.Rain] = "Rain block animation",
            [BlockAnimation.Spores] = "Spores block animation",
        });

    /// <summary>The animation <paramref name="animation"/> shows.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The animation has no installed name.</exception>
    internal static ArtworkAnimation Of(BlockAnimation animation) => new(Keys[animation]);

    /// <summary>The frame of <paramref name="animation"/> named <paramref name="name"/>.</summary>
    internal static ArtworkFrame Frame(BlockAnimation animation, [InstalledKey] string name) => new(Of(animation), Ensure.NotNull(name));
}
