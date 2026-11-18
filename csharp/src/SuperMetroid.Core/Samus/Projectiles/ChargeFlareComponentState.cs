using SuperMetroid.Core.Animation;
using static SuperMetroid.Core.Samus.Projectiles.ChargeFlareAnimationDefinitions;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// One charge-flare component's animation (#2114, #2396), played by the shared animation player: the picture it shows and
/// how long it holds it. The ordinary flare plays the component's animation forward; the Hyper Beam's flare plays its
/// pictures backward from wherever the component stands, each for <see cref="HyperBeamPictureUpdates"/>, and once that
/// count steps back past the first picture the component shows nothing until the flare restarts or is cleared.
/// </summary>
internal sealed class ChargeFlareComponentState
{
    /// <summary>Sets up <paramref name="component"/>'s animation cleared, on its first picture.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="component"/> has no animation.</exception>
    internal ChargeFlareComponentState(ChargeFlareComponent component)
    {
        Component = component;
        animation = Animations.TryGetValue(component, out Animation<ChargeFlareSegment>? played)
            ? played
            : throw new ArgumentOutOfRangeException(nameof(component), component, $"Undefined {nameof(ChargeFlareComponent)}.");
        player = AnimationPlayer<ChargeFlareSegment, ChargeFlarePicture, NoAnimationCue>.StartShowing(Segments, animation, ClearedPictureUpdates);
    }

    /// <summary>The component whose animation this is.</summary>
    internal ChargeFlareComponent Component { get; }

    /// <summary>The component's animation.</summary>
    private readonly Animation<ChargeFlareSegment> animation;

    /// <summary>The play of the component's animation: the picture it shows and the updates that picture has left.</summary>
    private readonly AnimationPlayer<ChargeFlareSegment, ChargeFlarePicture, NoAnimationCue> player;

    /// <summary>Whether the Hyper Beam's backward count plays the component, rather than the ordinary flare.</summary>
    internal bool PlaysBackward { get; private set; }

    /// <summary>The picture the component shows, or none once the Hyper Beam's count has stepped back past its first.</summary>
    internal ChargeFlarePicture? Picture => player.Shown;

    /// <summary>The ordinary flare's first update: the component shows its first picture for that picture's own updates.</summary>
    internal void Start()
    {
        PlaysBackward = false;
        player.Show(animation);
    }

    /// <summary>
    /// The grapple beam's flare when fired: the component restarts at its animation's loop, showing the loop's first picture
    /// for that picture's own updates, and plays on through the loop.
    /// </summary>
    /// <exception cref="InvalidOperationException">The component's animation has no loop.</exception>
    internal void RestartAtLoop()
    {
        PlaysBackward = false;
        player.ShowFrom(animation, AnimationPlace.Before(animation.CyclePhase));
    }

    /// <summary>
    /// The Hyper Beam's shot: the component shows its last picture for <see cref="HyperBeamPictureUpdates"/>, to count its
    /// pictures backward from there.
    /// </summary>
    internal void StartBackward()
    {
        PlaysBackward = true;
        player.ShowLastFrame(animation, HyperBeamPictureUpdates);
    }

    /// <summary>Plays the ordinary flare one update on: once the picture's updates run out, the next picture shows.</summary>
    /// <exception cref="InvalidOperationException">
    /// The Hyper Beam's count plays the component: the ordinary flare never plays again once Samus has the Hyper Beam.
    /// </exception>
    internal void PlayForward()
    {
        if (PlaysBackward)
            throw new InvalidOperationException($"Charge-flare {Component} is counting the Hyper Beam's flare backward and plays no further forward.");
        player.Step();
    }

    /// <summary>
    /// Counts the Hyper Beam's flare one update back: once the picture's updates run out, the picture before it shows for
    /// <see cref="HyperBeamPictureUpdates"/>, the first picture for <see cref="HyperBeamFirstPictureUpdates"/>, and past the
    /// first the component shows nothing.
    /// </summary>
    /// <returns>Whether the update reached the component's first picture.</returns>
    internal bool PlayBackward()
    {
        if (!PlaysBackward)
        {
            // The Baby's gift can catch the ordinary flare mid-charge, and the backward count takes the component over
            // where it stands. The ordinary flare's wait runs out on the update after it reaches zero, the backward
            // count's on the update it reaches zero, so the picture keeps one update less of the wait it had left, and at
            // least one.
            PlaysBackward = true;
            player.HoldShownFrameFor(Math.Max(player.UpdatesLeft - 1, 1));
        }
        return player.StepBackward(HyperBeamPictureUpdates, HyperBeamFirstPictureUpdates);
    }

    /// <summary>Returns the component to its first picture, its wait spent.</summary>
    internal void Clear()
    {
        PlaysBackward = false;
        player.Show(animation, ClearedPictureUpdates);
    }
}
