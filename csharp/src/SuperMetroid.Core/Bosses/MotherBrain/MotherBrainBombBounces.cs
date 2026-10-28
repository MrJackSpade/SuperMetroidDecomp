namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The gravity of Mother Brain's bomb at each bounce stage.</summary>
internal static class MotherBrainBombBounces
{
    /// <summary>Returns native acceleration for stages zero through eight, then zero for expiry at stage nine.</summary>
    internal static ushort YAcceleration(int stage) =>
        (uint)stage < MotherBrainBombBounceDefinitions.StageCount
            ? MotherBrainBombBounceDefinitions.YAccelerations[stage]
            : throw new ArgumentOutOfRangeException(nameof(stage));
}
