using SuperMetroid.Core.Enemies.BossBodies;

namespace SuperMetroid.Core.Gameplay;

/// <summary>
/// The body a boss shows in an accepted frame (#1352-#1356, #1466, #2031): its parts and where it is placed in the room;
/// whether the frame hides it is the latch's own fact (<see cref="OrdinaryRoomFrameLatch.BossBodyHidden"/>). Each of its frames lies where the import placed it against its boss (each installed frame's
/// anchor), so the display draws each part from the body's placement, clipped to the screen. Enemy AI chooses each during
/// the update; the frame boundary reads where the boss stands as the frame ends. The frame shows it as its own layer
/// (<see cref="Rendering.BossBodyView"/>, #2573).
/// </summary>
/// <param name="Body">The body the boss shows.</param>
/// <param name="Placement">
/// The room pixel the body's frames are placed from, or null where no boss places a body: Kraid's, Phantoon's, Crocomire's
/// and Mother Brain's position, Draygon's moved by his tail whip's shake of his body's picture, and once Mother Brain is gone
/// the room's top-left corner, where the escape warning written over the background lies.
/// </param>
/// <param name="Shown">The body's parts' pictures as the frame shows them (#2573), resolved once as the frame boundary latches it.</param>
public sealed record GameplayLatchedBossBody(
    BossBodyLayer Body,
    Position16<RoomPixels>? Placement,
    ShownBossBody Shown)
{
    /// <summary>
    /// The body <paramref name="display"/> shows as the frame boundary reads it (#2560), or null where the room shows no
    /// body: no boss attached one, or its body is not revealed.
    /// </summary>
    /// <param name="display">The boss body the room shows, or null where no boss attached one.</param>
    internal static GameplayLatchedBossBody? Of(IBossBodyDisplay? display) =>
        display?.Showing is { } showing ? new(showing.Layer, showing.Placement, showing.Layer.Show()) : null;
}
