using SuperMetroid.Verification.Bosses.MotherBrain;
using SuperMetroid.Core.Areas.Ceres;

namespace SuperMetroid.Verification.Areas.Ceres;

/// <summary>The native bank-$86 list each typed Ceres elevator arrival program replaces, for the #641 ROM oracle.</summary>
internal static class CeresElevatorArrivalNativeListAddresses
{
    /// <summary>Each program's native list: the pad's at $86:A28D, the platform's at $A299 and the shared delete at $A28B.</summary>
    internal static readonly NativeProgramEntry<CeresElevatorArrivalProgram>[] Entries =
    [
        new NativeProgramEntry<CeresElevatorArrivalProgram>(CeresElevatorArrivalProgram.Pad, 0xa28d),
        new NativeProgramEntry<CeresElevatorArrivalProgram>(CeresElevatorArrivalProgram.Platform, 0xa299),
        new NativeProgramEntry<CeresElevatorArrivalProgram>(CeresElevatorArrivalProgram.Delete, 0xa28b),
    ];
}
