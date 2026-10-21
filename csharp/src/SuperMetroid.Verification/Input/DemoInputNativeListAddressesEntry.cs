using SuperMetroid.Core.Input;

namespace SuperMetroid.Verification.Input;

/// <summary>The program and entry of one demo input native list addresses entry.</summary>
/// <param name="Program">The program.</param>
/// <param name="Entry">The entry.</param>
internal readonly record struct DemoInputNativeListAddressesEntry(DemoInputProgram Program, ushort Entry);
