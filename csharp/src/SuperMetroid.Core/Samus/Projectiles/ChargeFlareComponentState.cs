namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// Where one charge-flare component stands in its animation (#2114): the picture it shows and the wait it holds that
/// picture for. Only the Hyper Beam's backward count takes a component past its first picture; there it shows nothing
/// and its count goes on until the flare restarts or is cleared, since coming back round to its pictures would take
/// tens of thousands of pictures and the fast sparks reaching their first end the flare long before.
/// </summary>
/// <param name="component">The component whose animation this is.</param>
internal sealed class ChargeFlareComponentState(ChargeFlareComponent component)
{
    /// <summary>The component whose animation this is.</summary>
    internal ChargeFlareComponent Component { get; } = component;

    /// <summary>
    /// The place in the component's animation of the picture it shows, or none once the Hyper Beam's count has run it
    /// past its first picture.
    /// </summary>
    [IntegerQuantity("beats into the component's animation")]
    internal int? Picture { get; private set; } = ChargeFlareAnimations.FirstPlace;

    /// <summary>The wait the component holds its picture for: the picture shows for it and one update more.</summary>
    internal WrappingCountdown Hold { get; private set; }

    /// <summary>Shows <paramref name="step"/>'s picture and holds it for the step's wait.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The step's place is none of the component's pictures.</exception>
    internal void Show(ChargeFlareStep step)
    {
        Picture = ChargeFlareAnimations.IsPicture(Component, step.Phase)
            ? step.Phase
            : throw new ArgumentOutOfRangeException(nameof(step), step, $"Charge-flare {Component} has no picture at place {step.Phase}.");
        Hold = step.Hold;
    }

    /// <summary>
    /// Runs the component past its first picture, or keeps it there, holding <paramref name="hold"/> before its count
    /// goes on: the Hyper Beam's backward count stepping back from the first picture.
    /// </summary>
    internal void RunPastFirstPicture(WrappingCountdown hold)
    {
        Picture = null;
        Hold = hold;
    }

    /// <summary>Counts one update off the wait, keeping what remains.</summary>
    internal CountdownStep CountHold()
    {
        CountdownStep step = Hold.Decrement();
        Hold = step.Remaining;
        return step;
    }

    /// <summary>Returns the component to its first picture with no wait.</summary>
    internal void Clear()
    {
        Picture = ChargeFlareAnimations.FirstPlace;
        Hold = default;
    }
}
