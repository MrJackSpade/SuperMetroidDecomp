using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Runtime;

namespace SuperMetroid.Verification.Bosses;

/// <summary>What a captured frame shows of a boss body on BG2 (#1403).</summary>
internal static class BossBodyDisplay
{
    /// <summary>
    /// Whether <paramref name="drawn"/> shows the body the accepted NMI latched on BG2 at the map size its registers select:
    /// the picture of each cell the NMI resolved, in screen order.
    /// </summary>
    internal static bool ShowsLatchedBody(GameplaySceneLayer drawn, SuperMetroidRuntime runtime)
    {
        if (drawn.Rooms.BossBg2 is not { } shown || shown.Size != drawn.View.BackgroundMapSize)
            return false;
        System.Collections.Immutable.ImmutableArray<BgTilePicture?> latched = runtime.Display.Latched.Bosses.BodyPictures;
        return !latched.IsDefault && shown.Pictures.Select(picture => (BgTilePicture?)picture).SequenceEqual(latched.Take(shown.Pictures.Length));
    }
}
