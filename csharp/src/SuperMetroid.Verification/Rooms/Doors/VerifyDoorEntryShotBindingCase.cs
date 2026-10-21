using SuperMetroid.Core.Input;

namespace SuperMetroid.Verification.Rooms.Doors;

/// <summary>The held and cancels of one verify door entry shot binding case.</summary>
/// <param name="Held">The held.</param>
/// <param name="Cancels">The cancels.</param>
internal readonly record struct VerifyDoorEntryShotBindingCase(SnesButton Held, bool Cancels);
