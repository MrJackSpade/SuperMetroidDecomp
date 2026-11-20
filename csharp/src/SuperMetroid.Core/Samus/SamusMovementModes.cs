namespace SuperMetroid.Core.Samus;

/// <summary>Reads of the movement modes' definitions (<see cref="SamusMovementModeDefinitions"/>).</summary>
internal static class SamusMovementModes
{
    /// <summary>What a movement mode lets run.</summary>
    extension(SamusMovementMode mode)
    {
        /// <summary>Whether the mode admits the pose update after movement.</summary>
        internal bool AdmitsPoseUpdate => !SamusMovementModeDefinitions.WithoutPoseUpdate.Contains(mode);
    }
}
