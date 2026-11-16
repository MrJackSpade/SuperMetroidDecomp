using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Grapple;

namespace SuperMetroid.Verification.Spatial;

/// <summary>
/// Confirms the #1684 typed direction buckets against the quarter-of-the-turn arithmetic they replace: Draygon's eye's
/// nearest straight direction for every bearing, and the grapple rope's mirroring for every angle.
/// </summary>
internal static class TypedDirectionBucketsVerification
{
    /// <summary>Runs every typed direction-bucket check.</summary>
    internal static void VerifyTypedDirectionBuckets()
    {
        CompassDirection[] quarters = [CompassDirection.Up, CompassDirection.Right, CompassDirection.Down, CompassDirection.Left];
        for (int units = 0; units < RotationAngle.StepsPerTurn; units++)
        {
            // The replaced arithmetic: the bearing turned on an eighth of a turn, then its quarter of the turn.
            int quarter = ((units + 32) & 255) * 4 / 256;
            AssertEqual(quarters[quarter], new Bearing(units).NearestStraightDirection(), $"bearing {units} nearest straight direction");
        }

        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            RotationAngle angle = RotationAngle.FromRaw(raw);
            // The replaced arithmetic: the quarter of the turn the angle's whole step lies in.
            int quarter = (raw >> 8) * 4 / 256;
            SpriteMirror expected = (quarter >= 2 ? SpriteMirror.Horizontal : SpriteMirror.None) |
                (quarter is 0 or 3 ? SpriteMirror.Vertical : SpriteMirror.None);
            if (SamusGrappleBeamDrawing.RopeMirror(angle) != expected)
                throw new InvalidDataException($"Grapple angle {raw} mirrors the rope {SamusGrappleBeamDrawing.RopeMirror(angle)}, not {expected}.");
        }

        Console.WriteLine("  Typed direction buckets: 256 bearings' straight directions and 65536 rope mirrors equal the quarter arithmetic.");
    }
}
