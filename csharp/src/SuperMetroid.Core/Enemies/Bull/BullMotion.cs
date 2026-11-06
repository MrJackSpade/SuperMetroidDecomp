using static SuperMetroid.Core.Enemies.Bull.BullMovementDefinitions;

namespace SuperMetroid.Core.Enemies.Bull;

/// <summary>A Bull's shot-reaction angle.</summary>
public static class BullMotion
{
    /// <summary>The compass angle an immune shot from projectile direction <paramref name="direction"/> moves the Bull at.</summary>
    internal static int ShotAngle(int direction)
    {
        if ((uint)direction >= ShotAngles.Length)
            throw new InvalidDataException($"Bull immune-shot reaction received invalid projectile direction {direction}.");
        return ShotAngles[direction];
    }
}
