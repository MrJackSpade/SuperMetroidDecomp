using SuperMetroid.Core.Enemies;
using static SuperMetroid.Core.Bosses.Draygon.DraygonBurialEvirDefinitions;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>Places and moves Draygon's six death/burial Evir actors on their radial approach.</summary>
internal static class DraygonBurialEvirs
{
    /// <summary>
    /// The burial subspeeds are the absolute unit-circle components scaled by <see cref="DraygonBurialEvirDefinitions.SubspeedScale"/>
    /// and truncated, in 65536ths of a pixel; the six angles all lie in the upper half of the circle, so the vertical one is positive. The spawn positions use the same six angles on the radius508
    /// circle, floor to pixels, then wrap signed X into the coordinate range. The angle
    /// selects the movement signs; its unused zero padding is not state.
    /// </summary>
    internal static DraygonBurialEvirDefinition ForEntry(int entry)
    {
        if ((uint)entry >= EntryCount)
            throw new InvalidDataException($"Draygon burial Evir entry {entry} exceeds its six-record table.");
        int angle = FirstAngle - AngleStep * entry;
        double radians = angle * Math.PI / 128;
        double x = Math.Cos(radians), y = Math.Sin(radians);
        return new(
            new FixedVector<RoomPixels>(
                SignedSixteenSixteen.FromParts(0, (int)Math.Floor(SubspeedScale * Math.Abs(x))),
                SignedSixteenSixteen.FromParts(0, (int)Math.Floor(SubspeedScale * y))),
            Position16<RoomPixels>.Wrapped(
                (int)Math.Floor(CenterX + Radius * x),
                (int)Math.Floor(CenterY - Radius * y)),
            TrigonometryAngle.Wrapped(angle));
    }

    /// <summary>
    /// The signed per-frame displacement: X moves right when the angle plus a quarter turn lies in the second half of the
    /// turn, and Y moves down when the angle plus a half turn does; otherwise each magnitude is subtracted.
    /// </summary>
    internal static FixedVector<RoomPixels> Velocity(DraygonBurialEvirDefinition evir) => new(
        EnemyMath.CosineIsNegative(evir.Angle) ? evir.SubspeedMagnitude.X : SignedSixteenSixteen.Zero - evir.SubspeedMagnitude.X,
        EnemyMath.SineIsNegative(evir.Angle) ? evir.SubspeedMagnitude.Y : SignedSixteenSixteen.Zero - evir.SubspeedMagnitude.Y);
}
