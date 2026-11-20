using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Fx.ColourEffects;

namespace SuperMetroid.Core.Fx;

/// <summary>
/// The frames a colour effect plays (#2558): an animation of its own, or, for the heat shimmer, its suit's heat pulse, in
/// whose frames it shows and which its start chooses, with no animation of its own.
/// </summary>
internal abstract record ColourEffectFrames
{
    /// <summary>Only the two kinds below exist.</summary>
    private ColourEffectFrames()
    {
    }

    /// <summary>The heat shimmer's: the pulse of the suit Samus wears, which its start chooses.</summary>
    internal static ColourEffectFrames SuitHeatPulse { get; } = new PlaysSuitHeatPulse();

    /// <summary>The segments the effect's own animation plays, in the order its phases play them; none for a suit's pulse.</summary>
    internal abstract IEnumerable<ColourEffectSegment> OwnSegments { get; }

    /// <summary>An effect's own animation, as its definition names it.</summary>
    /// <param name="animation">The animation the effect plays in its own frames.</param>
    public static implicit operator ColourEffectFrames(Animation<ColourEffectSegment> animation) => new OwnAnimation(animation);

    /// <summary>The effect plays <paramref name="Animation"/>, in its own frames.</summary>
    /// <param name="Animation">The animation the effect plays.</param>
    internal sealed record OwnAnimation(Animation<ColourEffectSegment> Animation) : ColourEffectFrames
    {
        /// <inheritdoc/>
        internal override IEnumerable<ColourEffectSegment> OwnSegments => Animation.Phases.Select(phase => phase.Segment);
    }

    /// <summary>The effect plays the pulse of the suit Samus wears, which its start chooses.</summary>
    private sealed record PlaysSuitHeatPulse : ColourEffectFrames
    {
        /// <inheritdoc/>
        internal override IEnumerable<ColourEffectSegment> OwnSegments => [];
    }
}
