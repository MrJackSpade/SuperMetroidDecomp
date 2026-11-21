namespace SuperMetroid.Core.Samus;

/// <summary>What each movement mode lets run (<see cref="SamusMovementModes"/>).</summary>
internal static partial class SamusMovementModeDefinitions
{
    /// <summary>The movement modes that admit no pose update: dying, a suit pickup, and a script's lock.</summary>
    internal static readonly SamusMovementMode[] WithoutPoseUpdate = [SamusMovementMode.Fatal, SamusMovementMode.SuitPickup, SamusMovementMode.ScriptLocked];
}
