using SuperMetroid.Core.Input;

namespace SuperMetroid.Verification.Input;

/// <summary>The header and definition of one object.</summary>
/// <param name="Header">The header.</param>
/// <param name="Definition">The definition.</param>
internal readonly record struct Object(ushort Header, DemoInputObjectDefinition Definition);
