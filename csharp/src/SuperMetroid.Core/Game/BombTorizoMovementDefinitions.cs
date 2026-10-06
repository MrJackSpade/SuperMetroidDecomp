namespace SuperMetroid.Core.Game;

/// <summary>One frame-relative displacement used while Bomb Torizo changes posture.</summary>
internal readonly record struct BombTorizoPostureDisplacement(short X, short Y);

/// <summary>Fixed posture and walking movement definitions for Bomb Torizo.</summary>
internal static class BombTorizoMovementDefinitions
{
    /// <summary>
    /// The sixteen X words at <c>$AA:C3EE-$AA:C40D</c> and eight wrapping Y words at
    /// <c>$AA:C40E-$AA:C41D</c>. The sitting-down copies at <c>$AA:C440-$AA:C46F</c>
    /// are byte-identical and are subtracted by the caller.
    /// </summary>
    // Left-facing per-frame displacements are authored to the posture animation (no ramp or
    // symmetry fits -9,-6,-7,5,-16,-7). Right-facing movement reflects their horizontal sign;
    // no second stock sequence is stored (see residualScalarInputsReview).
    private static readonly short[] LeftPostureX = [-9, -6, -7, 5, -16, -7, 0, 0];

    private static readonly short[] PostureY =
        [0, -6, -6, -7, 0, 0, 0, 0];

    /// <summary>
    /// The twenty velocities duplicated at <c>$AA:C4BD-$AA:C4E4</c> and
    /// <c>$AA:C532-$AA:C559</c> by the normal and faceless walking instructions.
    /// </summary>
    // Ten per-frame stride magnitudes authored to the walk animation; facing only changes sign.
    private static readonly short[] LeftWalkVelocities = [-5, 0, -5, -19, -16, -7, 0, -7, -17, -18];

    /// <summary>Returns the posture displacement selected by an even byte offset.</summary>
    internal static BombTorizoPostureDisplacement Posture(ushort tableOffset)
    {
        if ((tableOffset & 1) != 0 || tableOffset >= LeftPostureX.Length * 4)
        {
            throw new InvalidDataException(
                $"Bomb Torizo posture offset ${tableOffset:X4} exceeds " +
                "its sixteen even cartridge selections.");
        }

        int xIndex = tableOffset >> 1;
        int yIndex = (tableOffset & 0x000f) >> 1;
        short horizontal = LeftPostureX[xIndex % LeftPostureX.Length];
        return new BombTorizoPostureDisplacement(
            xIndex < LeftPostureX.Length ? horizontal : (short)-horizontal, PostureY[yIndex]);
    }

    /// <summary>Returns the walking velocity selected by an even byte offset.</summary>
    internal static ushort WalkVelocity(ushort tableOffset)
    {
        if ((tableOffset & 1) != 0 || tableOffset >= LeftWalkVelocities.Length * 4)
        {
            throw new InvalidDataException(
                $"Bomb Torizo walk offset ${tableOffset:X4} exceeds " +
                "its twenty even cartridge selections.");
        }

        int index = tableOffset >> 1;
        short velocity = LeftWalkVelocities[index % LeftWalkVelocities.Length];
        return unchecked((ushort)(index < LeftWalkVelocities.Length ? velocity : -velocity));
    }
}
