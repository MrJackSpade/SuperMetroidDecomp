using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Runtime;

public sealed partial class SuperMetroidRuntime
{
    /// <summary>
    /// Gameplay-frame state the remainder of state eight reads after <c>PLM_Handler</c>.
    /// The bank-$85 message routine runs inside that handler, so a PLM that opens a box
    /// suspends the native frame; the port parks this state until the box closes.
    /// </summary>
    internal readonly record struct SuspendedGameplayFrameTail(
        bool DeathOwnsSamus,
        SamusCameraPoint PreviousCameraPoint,
        bool StationaryScriptControlLocked,
        bool SuitOwnsSamus);

    private SuspendedGameplayFrameTail? _suspendedFrameTail;
}
