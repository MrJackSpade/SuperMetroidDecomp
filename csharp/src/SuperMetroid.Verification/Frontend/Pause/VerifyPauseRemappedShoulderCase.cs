using SuperMetroid.Core.Input;

namespace SuperMetroid.Verification.Frontend.Pause;

/// <summary>The held and opens equipment of one verify pause remapped shoulder case.</summary>
/// <param name="Held">The held.</param>
/// <param name="OpensEquipment">The opens equipment.</param>
internal readonly record struct VerifyPauseRemappedShoulderCase(SnesButton Held, bool OpensEquipment);
