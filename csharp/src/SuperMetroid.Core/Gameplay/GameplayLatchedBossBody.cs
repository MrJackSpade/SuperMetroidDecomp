using SuperMetroid.Core.Enemies.BossBodies;

namespace SuperMetroid.Core.Gameplay;

/// <summary>
/// The body a boss shows in an accepted frame (#1352-#1356, #1466, #2031): its parts, where it is placed in the room and
/// whether it shows this frame. Each of its frames lies where the import placed it against its boss (each installed frame's
/// anchor), so the display draws each part from the body's placement, clipped to the screen. Enemy AI chooses each during
/// the update; the frame boundary reads where the boss stands as the frame ends.
/// </summary>
/// <param name="Body">The body a boss shows, or null without one.</param>
/// <param name="Placement">
/// The room pixel the body's frames are placed from, or null where no boss places a body: Kraid's, Phantoon's, Crocomire's
/// and Mother Brain's position, Draygon's moved by his tail whip's shake of his body's picture, and once Mother Brain is gone
/// the room's top-left corner, where the escape warning written over the background lies.
/// </param>
/// <param name="Hidden">Whether the body shows nothing this frame: Draygon is deleted, or Mother Brain's death flicker hides hers.</param>
/// <param name="ShownBody">
/// The body as the frame shows it, its pictures placed where they show, each for the frame its animation had selected,
/// resolved once the frame's animations have bound (#1514); null without a body.
/// </param>
public readonly record struct GameplayLatchedBossBody(
    BossBodyLayer? Body,
    Position16<RoomPixels>? Placement,
    bool Hidden,
    ShownBossBody? ShownBody = null);
