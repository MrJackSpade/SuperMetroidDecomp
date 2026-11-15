using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Fx;

/// <summary>
/// The treadmill animation a Wrecked Ship entrance door's setup starts. The door chooses the belt's direction; the
/// animation then waits for Phantoon's defeat and afterwards loops its treadmill frames (#1406, #1988).
/// </summary>
[RoomLifetime]
public sealed class WreckedShipTreadmillBlockAnimationsState
{
    /// <summary>The treadmill's loop, once a door's setup starts it.</summary>
    private readonly BlockAnimationPlayer animation = new();

    /// <summary>Whether a door's setup has started the treadmill.</summary>
    public bool IsActive => animation.IsPlaying;

    /// <summary>Starts the treadmill animation for a belt pushing <paramref name="direction"/>.</summary>
    /// <exception cref="InvalidOperationException">A door's setup has already started the treadmill.</exception>
    public void Start(WreckedShipTreadmillDirection direction) =>
        animation.Play(BlockAnimationLoops.Of(BlockAnimationLoops.Treadmill(direction)));

    /// <summary>
    /// Runs one pass: while Phantoon is not yet defeated the treadmill waits and selects nothing; afterwards its frames
    /// loop forever, each selected for the following frame boundary.
    /// </summary>
    public void Step(bool areaBossDefeated, ArtworkFrameSelections frames) =>
        animation.Step(areaBossDefeated, frames);
}
