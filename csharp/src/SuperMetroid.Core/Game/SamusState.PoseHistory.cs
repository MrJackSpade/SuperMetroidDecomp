using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

public sealed partial class SamusState
{
    private SamusPoseHistoryState? _poseHistory;

    /// <summary>
    /// Transition-owned WRAM $0A20-$0A27. Lazy construction also covers legacy
    /// debugger graphs whose constructors are bypassed and which lack these words.
    /// A legacy graph has no recoverable historical value; zero is not a claim
    /// that its actual cartridge history was zero.
    /// </summary>
    public SamusPoseHistoryState PoseHistory => _poseHistory ??= new();

    /// <summary>
    /// Commits current pose metadata through the native four-word history shift.
    /// Transition owners must call this after their final pose/metadata selection,
    /// not for intermediate assignments, rollbacks, or ordinary frame sampling.
    /// </summary>
    internal void CommitPoseHistory(ISnesAddressSpace bus)
    {
        ushort metadata = (ushort)(ReadPoseXDirection(bus) | ((byte)ReadMovementType(bus) << 8));
        PoseHistory.CommitTransition(Pose, metadata);
    }
}
