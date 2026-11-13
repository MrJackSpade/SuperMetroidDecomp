using SuperMetroid.Core.Programs;
namespace SuperMetroid.Core.Enemies.Zoa;

/// <summary>
/// The state of a Zoa, with its launch speed stage. “Shooting” is the game's name for the creature launching itself
/// horizontally; it does not create a separate enemy projectile.
/// </summary>
public sealed class ZoaEnemyState
{
    /// <summary>
    /// Whole-pixel population position captured at initialization and restored when a launch ends offscreen. Only the
    /// whole pixels are reset: the subpixel fractions accumulated in flight survive.
    /// </summary>
    public Position16<RoomPixels> SpawnPosition { get; internal set; }

    /// <summary>Element index zero through three in the program table.</summary>
    public ZoaAnimationSelector AnimationScriptTableIndex { get; internal set; }

    /// <summary>
    /// Last installed animation selector, initially zero; the animation change compares it with the requested selector so unchanged
    /// lists keep their timer and loop state.
    /// </summary>
    public ZoaAnimationSelector PreviousAnimationScriptTableIndex { get; internal set; }

    /// <summary>
    /// The current AI behaviour, initialized to wait and transitioned through rise, horizontal launch, and offscreen
    /// return to wait.
    /// </summary>
    public ZoaEnemyFunction? Function { get; internal set; }

    /// <summary>The launch's speed stage, zero (at rest) through three, advanced by the shooting animation's cues (<see cref="ZoaSpeedDefinitions.StageSpeedEighths"/>).</summary>
    public int ShotSpeedStage { get; internal set; }

    /// <summary>Where Zoa's typed animation (#641) resumes.</summary>
    internal AnimationPlayback<ZoaProgram> Playback { get; set; }
}
