namespace SuperMetroid.Core.Samus.Drawing;

/// <summary>One of Samus's particles: what it shows, where, on which step of its animation, and for how much longer.</summary>
public sealed class SamusParticle
{
    /// <summary>Starts a particle.</summary>
    /// <param name="kind">What it shows.</param>
    /// <param name="step">The animation step it starts on.</param>
    /// <param name="start">Whether it shows at once or after a hidden wait.</param>
    /// <param name="position">Where it starts, in whole room pixels.</param>
    /// <param name="drift">How far it moves on each update it is shown.</param>
    internal SamusParticle(
        SamusParticleKind kind,
        int step,
        SamusParticleStart start,
        Position16<RoomPixels> position,
        Offset16<RoomPixels> drift)
    {
        if ((start.StepUpdates > 0) == (start.HiddenUpdates > 0))
            throw new ArgumentOutOfRangeException(nameof(start), start, "A particle either shows at once or starts hidden.");
        if (step < 0 || step >= SamusParticleAnimations.StepCount(kind))
            throw new ArgumentOutOfRangeException(nameof(step), step, $"Particle kind {kind} has no step {step}.");
        Kind = kind;
        AnimationStep = step;
        StepUpdatesLeft = start.StepUpdates;
        HiddenUpdatesLeft = start.HiddenUpdates;
        Position = position;
        Drift = drift;
    }

    /// <summary>What the particle shows.</summary>
    public SamusParticleKind Kind { get; }

    /// <summary>The zero-based step of its animation it shows.</summary>
    public int AnimationStep { get; private set; }

    /// <summary>The updates its step still shows, counting the next; zero while it is hidden or finished.</summary>
    public int StepUpdatesLeft { get; private set; }

    /// <summary>The updates it still waits before it shows, counting the update it first shows on; zero once shown.</summary>
    public int HiddenUpdatesLeft { get; private set; }

    /// <summary>
    /// Where it is, in whole room pixels: the camera is subtracted only when drawing, motion wraps at sixteen bits, a
    /// diving splash follows the live water surface and lava spray and dust drift on each update they are shown.
    /// </summary>
    public Position16<RoomPixels> Position { get; private set; }

    /// <summary>How far it moves on each update it is shown.</summary>
    public Offset16<RoomPixels> Drift { get; }

    /// <summary>Whether it waits hidden.</summary>
    public bool IsHidden => HiddenUpdatesLeft > 0;

    /// <summary>Whether its animation has run out; a finished particle is no longer shown.</summary>
    public bool IsFinished { get; private set; }

    /// <summary>
    /// Runs one update: a hidden particle counts down its wait and, on its last update, takes its step's duration and
    /// shows; a shown particle counts down its step, and a step that ends moves to the next step, which lasts as long as the
    /// step that ended. A particle shown on this update drifts.
    /// </summary>
    internal void Advance()
    {
        if (IsFinished)
            throw new InvalidOperationException("A finished particle is not advanced.");
        if (HiddenUpdatesLeft > 0)
        {
            HiddenUpdatesLeft--;
            if (HiddenUpdatesLeft > 0)
                return;
            StepUpdatesLeft = SamusParticleAnimations.StepUpdates(Kind, AnimationStep);
        }
        else
        {
            StepUpdatesLeft--;
            if (StepUpdatesLeft == 0)
            {
                StepUpdatesLeft = SamusParticleAnimations.StepUpdates(Kind, AnimationStep);
                AnimationStep++;
                if (AnimationStep >= SamusParticleAnimations.StepCount(Kind))
                {
                    IsFinished = true;
                    StepUpdatesLeft = 0;
                    return;
                }
            }
        }
        Position += Drift;
    }

    /// <summary>Holds the particle on the line <paramref name="surfaceLine"/>, as a diving splash follows the water's surface.</summary>
    internal void HoldOnLine(int surfaceLine) => Position = Position.WithY(surfaceLine);
}
