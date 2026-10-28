using SuperMetroid.Core.Enemies;
using static SuperMetroid.Core.Bosses.Draygon.DraygonBurialEvirDefinitions;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>Places and moves Draygon's six death/burial Evir actors on their radial approach.</summary>
internal static class DraygonBurialEvirs
{
    /// <summary>
    /// $A5:A1AF subspeeds are the absolute unit-circle components scaled by $FFFF
    /// and truncated. $A5:A1C7 positions use the same six angles on the radius508
    /// circle, floor to pixels, then wrap signed X into the native word. The angle
    /// at $A5:A1DF selects the movement signs; its unused zero padding is not state.
    /// </summary>
    internal static DraygonBurialEvirDefinition ForEntry(int entry)
    {
        if ((uint)entry >= EntryCount)
            throw new InvalidDataException($"Draygon burial Evir entry {entry} exceeds its six-record table.");
        int angle = FirstAngle - AngleStep * entry;
        double radians = angle * Math.PI / 128;
        double x = Math.Cos(radians), y = Math.Sin(radians);
        return new(
            FixedVector<RoomPixels>.FromWords(
                0, (ushort)Math.Floor(ushort.MaxValue * Math.Abs(x)),
                0, (ushort)Math.Floor(ushort.MaxValue * y)),
            new Position16<RoomPixels>(
                unchecked((ushort)(int)Math.Floor(CenterX + Radius * x)),
                (ushort)Math.Floor(CenterY - Radius * y)),
            new ByteAngle(unchecked((byte)((ushort)angle))));
    }

    /// <summary>
    /// The signed per-frame displacement: X moves right when angle+$40 has bit 7 set and Y moves
    /// down when angle+$80 has bit 7 set; otherwise each magnitude is subtracted.
    /// </summary>
    internal static FixedVector<RoomPixels> Velocity(DraygonBurialEvirDefinition evir) => new(
        EnemyMath.CosineIsNegative(evir.Angle) ? evir.SubspeedMagnitude.X : SignedSixteenSixteen.Zero - evir.SubspeedMagnitude.X,
        EnemyMath.SineIsNegative(evir.Angle) ? evir.SubspeedMagnitude.Y : SignedSixteenSixteen.Zero - evir.SubspeedMagnitude.Y);
}
