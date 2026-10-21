using SuperMetroid.Core.Fx;

namespace SuperMetroid.Verification.Fx;

/// <summary>The type and native value of one representative type.</summary>
/// <param name="Type">The type.</param>
/// <param name="NativeValue">The native value.</param>
internal readonly record struct RepresentativeType(RoomFxType Type, ushort NativeValue);
