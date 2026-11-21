using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Samus.Abilities;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// The Power Bomb blast's oval stages (#2529), played by the shared animation player: each stage shows its authored ovals'
/// horizontal radii in pixels, one each update, and rests on its last, the update after which the blast moves on.
/// </summary>
internal static partial class PowerBombOvalAnimations
{
    /// <summary>The updates each authored oval shows for.</summary>
    private const int OvalUpdates = 1;

    /// <summary>The oval stages' ovals, by horizontal radius in pixels.</summary>
    internal static AnimationSegments<PowerBombOvalSegment, int, NoAnimationCue> Segments { get; } = new(
        new Dictionary<PowerBombOvalSegment, AnimationBeat<int, NoAnimationCue>[]>
        {
            [PowerBombOvalSegment.PreExplosionYellow] = Ovals(PowerBombExplosionDefinitions.YellowOvalRadii),
            [PowerBombOvalSegment.ExplosionWhite] = Ovals(PowerBombExplosionDefinitions.WhiteOvalRadii),
        });

    /// <summary>The yellow pre-explosion's ovals, played once, resting on the last.</summary>
    internal static Animation<PowerBombOvalSegment> PreExplosionYellow { get; } =
        new([Segments.Whole(PowerBombOvalSegment.PreExplosionYellow)], AnimationEnd.Holds);

    /// <summary>The white explosion's ovals, played once, resting on the last.</summary>
    internal static Animation<PowerBombOvalSegment> ExplosionWhite { get; } =
        new([Segments.Whole(PowerBombOvalSegment.ExplosionWhite)], AnimationEnd.Holds);

    /// <summary>The beats of a stage showing <paramref name="radii"/> in turn, one an update, holding the last.</summary>
    private static AnimationBeat<int, NoAnimationCue>[] Ovals(IReadOnlyList<int> radii) =>
    [
        .. radii.SkipLast(1).Select(radius => new AnimationBeat<int, NoAnimationCue>.Frame(radius, OvalUpdates)),
        new AnimationBeat<int, NoAnimationCue>.HeldFrame(radii[^1]),
    ];
}
