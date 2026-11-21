using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Samus.Artwork;

namespace SuperMetroid.Core.Samus.Drawing;

/// <summary>
/// One of Samus's particles: what it shows, where, and its animation (#2388), played once through the shared animation
/// player after any hidden wait.
/// </summary>
public sealed class SamusParticle
{
    /// <summary>Starts a particle at <paramref name="from"/> in its animation.</summary>
    /// <param name="kind">What it shows.</param>
    /// <param name="from">
    /// Where in its animation it starts: its start, or, for a particle restored partway through and shown at once, before
    /// the picture it stood on.
    /// </param>
    /// <param name="start">Whether it shows at once or after a hidden wait.</param>
    /// <param name="position">Where it starts, in whole room pixels.</param>
    /// <param name="drift">How far it moves on each update it is shown.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// The start neither shows at once nor waits hidden, or a hidden particle starts anywhere but at its first picture.
    /// </exception>
    internal SamusParticle(
        SamusParticleKind kind,
        AnimationPlace from,
        SamusParticleStart start,
        Position16<RoomPixels> position,
        Offset16<RoomPixels> drift)
    {
        Ensure.NotNull(start);
        Kind = kind;
        Animation<SamusParticleKind> animation = SamusParticleAnimations.Animations[kind];
        Animation = AnimationPlayer<SamusParticleKind, SamusAtmosphereFrame, NoAnimationCue>.Starting(SamusParticleAnimations.Segments, animation);
        switch (start)
        {
            case SamusParticleStart.ShownAtOnce shown:
                Animation.ShowAt(animation, from, shown.StepUpdates);
                break;
            case SamusParticleStart.HiddenFirst hidden:
                // Every producer starts a hidden particle on its first picture, which shows for its own updates once the wait ends.
                if (from != AnimationPlace.StartOfAnimation)
                    throw new ArgumentOutOfRangeException(nameof(from), from, "A hidden particle starts on its first picture.");
                Hidden = UpdateCountdown.For(hidden.Updates);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(start), start, "A particle either shows at once or starts hidden.");
        }
        Position = position;
        Drift = drift;
    }

    /// <summary>What the particle shows.</summary>
    public SamusParticleKind Kind { get; }

    /// <summary>The particle's animation: nothing shown while it waits hidden, then its pictures in turn.</summary>
    private AnimationPlayer<SamusParticleKind, SamusAtmosphereFrame, NoAnimationCue> Animation { get; }

    /// <summary>The picture it shows, or null while it waits hidden.</summary>
    public SamusAtmosphereFrame? Shown => IsHidden ? null : Animation.Shown;

    /// <summary>The wait before it shows, counting the update it first shows on; over once shown.</summary>
    private UpdateCountdown Hidden { get; set; } = UpdateCountdown.Ended();

    /// <summary>
    /// Where it is, in whole room pixels: the camera is subtracted only when drawing, a diving splash follows the live water surface and lava spray and dust drift on each update they are shown.
    /// </summary>
    public Position16<RoomPixels> Position { get; private set; }

    /// <summary>How far it moves on each update it is shown.</summary>
    public Offset16<RoomPixels> Drift { get; }

    /// <summary>Whether it waits hidden.</summary>
    public bool IsHidden => Hidden.IsRunning;

    /// <summary>Whether its animation has run out; a finished particle is no longer shown.</summary>
    public bool IsFinished => Animation.HasStopped;

    /// <summary>
    /// The updates a particle of <paramref name="kind"/> starting at <paramref name="from"/> as <paramref name="start"/>
    /// says is shown or waits until it runs out, counting the update it runs out on: its own play, update by update.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The start or the place is not one a particle starts with.</exception>
    internal static int UpdatesUntilFinished(SamusParticleKind kind, AnimationPlace from, SamusParticleStart start)
    {
        var particle = new SamusParticle(kind, from, start, default, Offset16<RoomPixels>.Zero);
        var shown = new ElapsedUpdates();
        do
        {
            particle.Advance();
            shown.Count();
        }
        while (!particle.IsFinished);
        return shown.Age;
    }

    /// <summary>
    /// Runs one update: a hidden particle counts down its wait and, on its last update, shows its first picture for that
    /// picture's updates; a shown particle plays its animation on. A particle shown on this update drifts.
    /// </summary>
    internal void Advance()
    {
        if (IsFinished)
            throw new InvalidOperationException("A finished particle is not advanced.");
        if (Hidden.IsRunning && !Hidden.Count())
            return;
        Animation.Step();
        if (IsFinished)
            return;
        Position += Drift;
    }

    /// <summary>Holds the particle on the line <paramref name="surfaceLine"/>, as a diving splash follows the water's surface.</summary>
    internal void HoldOnLine(int surfaceLine) => Position = Position.WithY(surfaceLine);
}
