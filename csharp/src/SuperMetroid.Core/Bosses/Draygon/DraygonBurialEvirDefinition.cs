using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>One compiled Draygon death/burial Evir position and movement record.</summary>
/// <param name="SubspeedMagnitude">The <c>$A5:A1AF</c> per-axis subpixel speeds: unsigned magnitudes with zero whole words.</param>
/// <param name="Initial">The <c>$A5:A1C7</c> spawn position.</param>
/// <param name="Angle">The <c>$A5:A1DF</c> angle word that selects each axis's movement sign.</param>
internal readonly record struct DraygonBurialEvirDefinition(
    FixedVector<RoomPixels> SubspeedMagnitude,
    Position16<RoomPixels> Initial,
    SnesByteAngle Angle)
{
    /// <summary>
    /// The signed per-frame displacement: X moves right when angle+$40 has bit 7 set and Y moves
    /// down when angle+$80 has bit 7 set; otherwise each magnitude is subtracted.
    /// </summary>
    internal FixedVector<RoomPixels> Velocity => new(
        EnemyMath.CosineIsNegative(Angle) ? SubspeedMagnitude.X : SnesSignedSixteenSixteen.Zero - SubspeedMagnitude.X,
        EnemyMath.SineIsNegative(Angle) ? SubspeedMagnitude.Y : SnesSignedSixteenSixteen.Zero - SubspeedMagnitude.Y);
}
