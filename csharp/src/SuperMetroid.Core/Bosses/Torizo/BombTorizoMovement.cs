using static SuperMetroid.Core.Bosses.Torizo.BombTorizoMovementDefinitions;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Bomb Torizo's posture displacements and walking velocities, selected by native byte offset and facing.</summary>
internal static class BombTorizoMovement
{
    /// <summary>Returns the frame-relative posture displacement selected by an even byte offset.</summary>
    internal static Offset16<RoomPixels> Posture(int tableOffset)
    {
        if (tableOffset < 0 || (tableOffset & 1) != 0 || tableOffset >= LeftPostureX.Length * 4)
        {
            throw new InvalidDataException(
                $"Bomb Torizo posture offset ${tableOffset:X4} exceeds " +
                "its sixteen even cartridge selections.");
        }

        int xIndex = tableOffset >> 1;
        int yIndex = xIndex % PostureY.Length;
        int horizontal = LeftPostureX[xIndex % LeftPostureX.Length];
        return new Offset16<RoomPixels>(
            (short)(xIndex < LeftPostureX.Length ? horizontal : -horizontal), PostureY[yIndex]);
    }

    /// <summary>Returns the walking velocity selected by an even byte offset.</summary>
    internal static ushort WalkVelocity(int tableOffset)
    {
        if (tableOffset < 0 || (tableOffset & 1) != 0 || tableOffset >= LeftWalkVelocities.Length * 4)
        {
            throw new InvalidDataException(
                $"Bomb Torizo walk offset ${tableOffset:X4} exceeds " +
                "its twenty even cartridge selections.");
        }

        int index = tableOffset >> 1;
        int velocity = LeftWalkVelocities[index % LeftWalkVelocities.Length];
        return unchecked((ushort)(index < LeftWalkVelocities.Length ? velocity : -velocity));
    }
}
