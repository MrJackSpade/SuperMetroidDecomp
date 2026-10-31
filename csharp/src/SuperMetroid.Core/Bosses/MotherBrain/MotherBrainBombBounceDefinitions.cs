namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// $86:C550-C563, the Mother Brain bomb's per-bounce Y acceleration selected by its bounce stage.
/// Stage zero uses the ordinary fall gravity of 7; each later bounce's gravity grows by sixteen
/// times the bounce number, capped at 64 per bounce (16, 32, 64, 112, 176, 240, 304, 368); the
/// stage after the eighth bounce is the zero expiry sentinel.
/// </summary>
internal static class MotherBrainBombBounceDefinitions
{
    /// <summary>Stages zero through nine, including the expiry sentinel.</summary>
    internal const int StageCount = 10;
    /// <summary>The ordinary fall gravity, also used before the first bounce.</summary>
    internal const int FallAcceleration = 7;

    /// <summary>The native acceleration of stages zero through eight, then zero for expiry at stage nine.</summary>
    internal static readonly ushort[] YAccelerations = [FallAcceleration, 16, 32, 64, 112, 176, 240, 304, 368, 0];
}
