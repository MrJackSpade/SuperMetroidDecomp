using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Samus.Artwork;
using static SuperMetroid.Core.Samus.Projectiles.ChargeFlareAnimationDefinitions;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// The Hyper Beam's flare counting one component's pictures backward (#2114, #2396, #2553), each for
/// <see cref="HyperBeamPictureUpdates"/>: from the last picture at the Hyper Beam's shot, or from where the ordinary flare
/// stood when the Baby's gift took it over. Once the count steps back past the first picture the component shows nothing
/// until the flare restarts or is cleared, which ends the count.
/// </summary>
/// <typeparam name="TSegment">The component's runs of pictures.</typeparam>
/// <typeparam name="TPicture">The component's pictures.</typeparam>
internal sealed partial class HyperBeamFlareCount<TSegment, TPicture> : IHyperBeamFlareCount
    where TSegment : struct
    where TPicture : struct
{
    /// <summary>Counts with <paramref name="player"/>, showing its pictures in <paramref name="composition"/>.</summary>
    private HyperBeamFlareCount(AnimationPlayer<TSegment, TPicture, NoAnimationCue> player, Func<HorizontalDirection, TPicture, ProjectileSpriteId> composition)
    {
        Player = player;
        this.composition = composition;
    }

    /// <summary>The play counted backward: the picture it shows and the updates that picture has left.</summary>
    private AnimationPlayer<TSegment, TPicture, NoAnimationCue> Player { get; }

    /// <summary>The composition each of the component's pictures shows in, by Samus's facing.</summary>
    private readonly Func<HorizontalDirection, TPicture, ProjectileSpriteId> composition;

    /// <summary>The Hyper Beam's shot: the count shows the animation's last picture for <see cref="HyperBeamPictureUpdates"/>.</summary>
    internal static HyperBeamFlareCount<TSegment, TPicture> FromLast(AnimationSegments<TSegment, TPicture, NoAnimationCue> segments,
        Animation<TSegment> animation, Func<HorizontalDirection, TPicture, ProjectileSpriteId> composition)
    {
        var player = AnimationPlayer<TSegment, TPicture, NoAnimationCue>.StartShowing(segments, animation, ClearedPictureUpdates);
        player.ShowLastFrame(animation, HyperBeamPictureUpdates);
        return new(player, composition);
    }

    /// <summary>
    /// The Baby's gift catching the ordinary flare mid-charge: the count takes the component over where <paramref name="forward"/>
    /// stands. The ordinary flare's wait runs out on the update after it reaches zero, the backward count's on the update it
    /// reaches zero, so the picture keeps one update less of the wait it had left, and at least one.
    /// </summary>
    internal static HyperBeamFlareCount<TSegment, TPicture> TakingOver(AnimationPlayer<TSegment, TPicture, NoAnimationCue> forward,
        Func<HorizontalDirection, TPicture, ProjectileSpriteId> composition)
    {
        AnimationPlayer<TSegment, TPicture, NoAnimationCue> player = forward.Copy();
        player.HoldShownFrameFor(Math.Max(forward.UpdatesLeft - 1, 1));
        return new(player, composition);
    }

    /// <inheritdoc/>
    public ProjectileSpriteId? Composition(HorizontalDirection facing) => Player.Shown is { } shown ? composition(facing, shown) : null;

    /// <inheritdoc/>
    public bool PlayBackward() => Player.StepBackward(HyperBeamPictureUpdates, HyperBeamFirstPictureUpdates);
}
