using System.Collections.Frozen;
using F = SuperMetroid.Core.Bosses.Torizo.BombTorizoStatueFragment;
using P = SuperMetroid.Core.Bosses.Torizo.TorizoProjectileSegment;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>The pieces of Bomb Torizo's breaking statue hand (#2029): each piece's animation, place and fall.</summary>
internal static partial class BombTorizoStatueFragmentDefinitions
{
    /// <summary>Every piece's initial fall speed, one pixel an update.</summary>
    internal static readonly SignedEightEight InitialFallSpeed = SignedEightEight.FromPixels(1);

    /// <summary>Every piece's downward acceleration.</summary>
    internal static readonly SignedEightEight Acceleration = SignedEightEight.FromPixelFraction(1, 16);

    /// <summary>The updates every piece falls once it cracks off, after which it is deleted.</summary>
    internal const int FallUpdates = 112;

    /// <summary>
    /// Each piece: the animation it plays, named for the piece (#2240), and its place from the hand's block
    /// in room pixels, the centre of its sixteen-pixel cell.
    /// </summary>
    internal static IReadOnlyDictionary<BombTorizoStatueFragment, BombTorizoStatueFragmentDefinition> Fragments { get; } =
        new Dictionary<BombTorizoStatueFragment, BombTorizoStatueFragmentDefinition>
        {
            [F.TopMiddle] = new(P.StatueFragmentTopMiddle, new(8, -8)),
            [F.TopRight] = new(P.StatueFragmentTopRight, new(24, -8)),
            [F.MiddleLeft] = new(P.StatueFragmentMiddleLeft, new(-8, 8)),
            [F.Centre] = new(P.StatueFragmentCentre, new(8, 8)),
            [F.MiddleRight] = new(P.StatueFragmentMiddleRight, new(24, 8)),
            [F.BottomLeft] = new(P.StatueFragmentBottomLeft, new(-8, 24)),
            [F.BottomMiddle] = new(P.StatueFragmentBottomMiddle, new(8, 24)),
            [F.BottomRight] = new(P.StatueFragmentBottomRight, new(24, 24)),
        }.ToFrozenDictionary();
}
