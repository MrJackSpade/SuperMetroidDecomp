using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.FaceBlocks;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Spatial;
using SuperMetroid.Core.World;

namespace SuperMetroid.Verification.Enemies.FaceBlocks;

/// <summary>
/// A face block the checks place as its room's population places it (#2554), and the waking they observe: a woken block
/// starts its turn toward Samus, whose first step shows on its next animation step, while a resting block holds its picture.
/// </summary>
internal static class BlueBrinstarFaceBlockPlacing
{
    /// <summary>
    /// Places a face block whose population entry's words are 64 and 0, a 64-pixel trigger distance facing as the words
    /// decode, at <paramref name="at"/> in the first place, set up with Samus not yet holding the Morph Ball, and shows the
    /// picture its setup started.
    /// </summary>
    internal static RoomEnemyPlace Place(RoomEnemySystem enemies, Position16<RoomPixels> at)
    {
        RoomEnemyPlace place = enemies.Places.InUpdateOrder[0];
        var settings = (FaceBlockSpawnSettings)EnemySpawnSettingsCodec.Decode(EnemyDefinitionId.FaceBlock, PopulationWords.Parameters(64, 0));
        SamusState samus = RoomEnemyFrameFixtures.StandaloneSamus();
        _ = enemies.PlaceAs<BlueBrinstarFaceBlockEntity>(EnemyDefinitionId.FaceBlock, settings, samus, at);
        place.AdvanceAnimation(samus);
        return place;
    }

    /// <summary>Runs the block's main AI with <paramref name="samus"/> and tells whether that update woke it.</summary>
    internal static bool RunMainAiWakes(RoomEnemyPlace place, SamusState samus)
    {
        place.RunMainAi(samus);
        place.AdvanceAnimation(samus);
        return place.AnimationClock.ShowsNewFrame;
    }
}
