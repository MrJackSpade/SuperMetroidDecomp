using SuperMetroid.Core.Frontend.Pause;

namespace SuperMetroid.Verification.Frontend.Pause;

/// <summary>The first and second of one verify pause category item counts case.</summary>
/// <param name="First">The first.</param>
/// <param name="Second">The second.</param>
internal readonly record struct VerifyPauseCategoryItemCountsCase(PauseEquipmentCategory First, int Second);
