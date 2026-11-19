using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// The Baby Metroid Ceres Ridley carries (#2228): an actor of its own, with its own place in the room, its fall and its
/// cutscene animation on its own clock, which its draw advances. Ceres Ridley's encounter holds it for the room.
/// </summary>
public sealed class CeresBaby
{
    /// <summary>Puts the Baby in Ridley's arms at <paramref name="position"/>, its animation waiting to show its first pose.</summary>
    /// <param name="position">Where the Baby starts: Ridley's arms.</param>
    internal CeresBaby(Position16<RoomPixels> position)
    {
        Position = FixedPosition<RoomPixels>.FromWhole(position);
        Clock.Restart();
    }

    /// <summary>Gets the Baby's behavior selecting hand attachment, falling setup, falling motion, or idle.</summary>
    public CeresBabyAction Action { get; internal set; } = CeresBabyAction.CarriedInArms;

    /// <summary>
    /// Gets the Baby's room position. Its vertical fraction of a pixel is the one its fall left and
    /// survives the hand attachments, which replace only the whole pixels.
    /// </summary>
    public FixedPosition<RoomPixels> Position { get; internal set; }

    /// <summary>
    /// Gets the Baby's falling speed: zero until it is dropped, then faster by a thirty-second of a pixel an update each
    /// falling update until it lands.
    /// </summary>
    public SignedEightEight VerticalVelocity { get; internal set; }

    /// <summary>The Baby's playing cutscene animation, standing after its current pose; owned separately from Ridley's body.</summary>
    internal AnimationPlayback<CeresBabySegment> Playback { get; set; } =
        new(CeresBabyAnimations.StartingWith(CeresBabySegment.IdleSquish), AnimationPlace.StartOfAnimation);

    /// <summary>
    /// The Baby's animation clock: how long the pose it shows holds before its animation takes its next step. Each draw
    /// update counts one off it; its first draw takes the first step.
    /// </summary>
    internal EnemyAnimationClock Clock { get; } = new();

    /// <summary>The pose the Baby's animation shows, or null before its first draw has shown one.</summary>
    internal CeresBabyPose? ShownPose { get; set; }
}
