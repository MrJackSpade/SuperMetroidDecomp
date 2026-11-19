using SuperMetroid.Core.Gameplay;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Runtime;

namespace SuperMetroid.Verification.Bosses;

/// <summary>What a captured frame shows of a boss body on BG2 (#1403, #1466).</summary>
internal static class BossBodyDisplay
{
    /// <summary>Whether <paramref name="drawn"/> shows the body the accepted frame latched: the parts it resolved, as it resolved them.</summary>
    internal static bool ShowsLatchedBody(GameplaySceneLayer drawn, SuperMetroidRuntime runtime) =>
        drawn.Rooms.Background is SuperMetroid.Core.Gameplay.BossBodyBackground { Body: var shown } &&
        runtime.Display.LatchedOrdinaryRoom.Rooms.Background is SuperMetroid.Core.Gameplay.BossBodyBackground { Body: var latched } && ReferenceEquals(shown, latched);
}
