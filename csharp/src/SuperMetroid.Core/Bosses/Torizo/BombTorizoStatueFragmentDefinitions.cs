using System.Collections.Frozen;
using F = SuperMetroid.Core.Bosses.Torizo.BombTorizoStatueFragment;
using P = SuperMetroid.Core.Bosses.Torizo.TorizoProjectileSegment;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>The pieces of Bomb Torizo's breaking statue hand (#2029): each piece's animation, place and fall.</summary>
internal static class BombTorizoStatueFragmentDefinitions
{
    /// <summary>Every piece's initial fall speed, one pixel an update.</summary>
    internal static readonly SignedEightEight InitialFallSpeed = SignedEightEight.FromPixels(1);

    /// <summary>Every piece's downward acceleration.</summary>
    internal static readonly SignedEightEight Acceleration = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(16);

    /// <summary>
    /// Each piece: the animation it plays, named for the order the crumble drops it in, and its place from the hand's block
    /// in room pixels, the centre of its sixteen-pixel cell.
    /// </summary>
    internal static IReadOnlyDictionary<BombTorizoStatueFragment, BombTorizoStatueFragmentDefinition> Fragments { get; } =
        new Dictionary<BombTorizoStatueFragment, BombTorizoStatueFragmentDefinition>
        {
            [F.TopMiddle] = new(P.FirstStatueFragment, new(8, -8), InitialFallSpeed, Acceleration),
            [F.TopRight] = new(P.SecondStatueFragment, new(24, -8), InitialFallSpeed, Acceleration),
            [F.MiddleLeft] = new(P.ThirdStatueFragment, new(-8, 8), InitialFallSpeed, Acceleration),
            [F.Centre] = new(P.FourthStatueFragment, new(8, 8), InitialFallSpeed, Acceleration),
            [F.MiddleRight] = new(P.FifthStatueFragment, new(24, 8), InitialFallSpeed, Acceleration),
            [F.BottomLeft] = new(P.SixthStatueFragment, new(-8, 24), InitialFallSpeed, Acceleration),
            [F.BottomMiddle] = new(P.SeventhStatueFragment, new(8, 24), InitialFallSpeed, Acceleration),
            [F.BottomRight] = new(P.EighthStatueFragment, new(24, 24), InitialFallSpeed, Acceleration),
        }.ToFrozenDictionary();
}
