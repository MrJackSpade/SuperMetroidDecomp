using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Samus.Artwork;
using static SuperMetroid.Core.Samus.Projectiles.ChargeFlareAnimationDefinitions;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// One charge-flare component's ordinary animation, played forward by the shared animation player (#2114, #2396, #2553):
/// the picture it shows, how long it holds it, and the composition it shows that picture in. The Hyper Beam's flare counts
/// the component's pictures backward in an object of its own (<see cref="HyperBeamFlareCount{TSegment, TPicture}"/>), so a
/// forward play never counts backward.
/// </summary>
/// <typeparam name="TSegment">The component's runs of pictures.</typeparam>
/// <typeparam name="TPicture">The component's pictures: the main flare's or a spark ring's.</typeparam>
internal sealed class ChargeFlarePlay<TSegment, TPicture> : IChargeFlarePlay
    where TSegment : struct
    where TPicture : struct
{
    /// <summary>Sets up the component's animation cleared, on its first picture.</summary>
    /// <param name="segments">The component's runs of pictures.</param>
    /// <param name="looping">The component's animation, which loops.</param>
    /// <param name="composition">The composition each of its pictures shows in, by the way Samus faces.</param>
    internal ChargeFlarePlay(AnimationSegments<TSegment, TPicture, NoAnimationCue> segments, LoopingAnimation<TSegment> looping,
        Func<HorizontalDirection, TPicture, ProjectileSpriteId> composition)
    {
        this.segments = segments;
        Looping = looping;
        this.composition = composition;
        Player = ShowingAnimationPlayer<TSegment, TPicture, NoAnimationCue>.StartShowing(segments, Looping.Animation, ClearedPictureUpdates);
    }

    /// <summary>The component's runs of pictures.</summary>
    private readonly AnimationSegments<TSegment, TPicture, NoAnimationCue> segments;

    /// <summary>The component's animation, which loops.</summary>
    private LoopingAnimation<TSegment> Looping { get; }

    /// <summary>The composition each of the component's pictures shows in, by Samus's facing.</summary>
    private readonly Func<HorizontalDirection, TPicture, ProjectileSpriteId> composition;

    /// <summary>The play of the component's animation: the picture it shows and the updates that picture has left.</summary>
    private ShowingAnimationPlayer<TSegment, TPicture, NoAnimationCue> Player { get; }

    /// <summary>The picture the component shows: a forward play always shows one.</summary>
    internal TPicture Picture => Player.Showing;

    /// <inheritdoc/>
    public ProjectileSpriteId Composition(HorizontalDirection facing) => composition(facing, Picture);

    /// <inheritdoc/>
    public void Start() => Player.Show(Looping.Animation);

    /// <summary>
    /// The grapple beam's flare when fired: the component restarts at its animation's loop, showing the loop's first picture
    /// for that picture's own updates, and plays on through the loop.
    /// </summary>
    internal void RestartAtLoop() => Player.ShowFrom(Looping.Animation, AnimationPlace.Before(Looping.CyclePhase));

    /// <inheritdoc/>
    public void PlayForward() => Player.Step();

    /// <inheritdoc/>
    public void Clear() => Player.Show(Looping.Animation, ClearedPictureUpdates);

    /// <inheritdoc/>
    public IHyperBeamFlareCount CountBackwardFromLast() =>
        HyperBeamFlareCount<TSegment, TPicture>.FromLast(segments, Looping.Animation, composition);

    /// <inheritdoc/>
    public IHyperBeamFlareCount CountBackwardFromHere() =>
        HyperBeamFlareCount<TSegment, TPicture>.TakingOver(Player, composition);
}
