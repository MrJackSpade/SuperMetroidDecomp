using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies.BossBodies;

namespace SuperMetroid.Core.Gameplay;

/// <summary>
/// The body a boss shows in an accepted frame (#1352-#1356, #1466, #1617): its parts, where Kraid's, Draygon's, Crocomire's
/// and Mother Brain's lie in the room, and the pictures it shows. A body's origin is the room pixel its parts' area's
/// top-left corner lies on; the display draws each part there, clipped to the screen. Enemy AI chooses each during the main
/// loop.
/// </summary>
/// <param name="Body">The body a boss shows, or null without one.</param>
/// <param name="KraidBodyOrigin">Where Kraid's body lies in the room, or null when it is not shown.</param>
/// <param name="DraygonBody">Draygon's body position and where his body lies in the room, or null without Draygon (#1354).</param>
/// <param name="CrocomireBodyOrigin">Where Crocomire's body lies in the room, or null without Crocomire.</param>
/// <param name="MotherBrainBody">Where Mother Brain's body lies in the room and her death-flicker hiding, or null without either (#1356).</param>
/// <param name="ShownBody">
/// The body as the frame shows it, its pictures placed where they show, each for the frame its animation had selected,
/// resolved once the frame's animations have bound (#1514); null without a body.
/// </param>
public readonly record struct GameplayLatchedBossBackgrounds(
    BossBodyLayer? Body,
    Position16<RoomPixels>? KraidBodyOrigin,
    DraygonBodyPlacement? DraygonBody,
    Position16<RoomPixels>? CrocomireBodyOrigin,
    MotherBrainBodyPlacement? MotherBrainBody,
    ShownBossBody? ShownBody = null)
{
    /// <summary>
    /// Where the boss body the frame places lies in the room (#1802): Kraid's, Draygon's, Crocomire's or Mother Brain's,
    /// whichever the room's boss holds, or null where no boss places one.
    /// </summary>
    public Position16<RoomPixels>? BodyOrigin => KraidBodyOrigin ?? DraygonBody?.BodyOrigin ?? CrocomireBodyOrigin ?? MotherBrainBody?.BodyOrigin;
}
