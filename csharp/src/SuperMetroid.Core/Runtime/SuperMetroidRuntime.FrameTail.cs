using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Runtime;

public sealed partial class SuperMetroidRuntime
{
    /// <summary>
    /// Gameplay-frame state the remainder of state eight reads after <c>PLM_Handler</c>.
    /// The bank-$85 message routine runs inside that handler, so a PLM that opens a box
    /// suspends the native frame; the port parks this state until the box closes.
    /// </summary>
    /// <param name="DeathOwnsSamus">Whether the death sequence owns Samus for the suspended frame.</param>
    /// <param name="PreviousCameraPoint">The camera target captured before the remaining frame steps.</param>
    /// <param name="StationaryScriptControlLocked">Whether stationary scripted control remains locked.</param>
    /// <param name="SuitOwnsSamus">Whether a suit-pickup sequence owns Samus for the suspended frame.</param>
    internal readonly record struct SuspendedGameplayFrameTail(
        bool DeathOwnsSamus,
        SamusCameraPoint PreviousCameraPoint,
        bool StationaryScriptControlLocked,
        bool SuitOwnsSamus);

    /// <summary>Retains the remainder of a gameplay frame suspended by a message box.</summary>
    private SuspendedGameplayFrameTail? _suspendedFrameTail;
}
