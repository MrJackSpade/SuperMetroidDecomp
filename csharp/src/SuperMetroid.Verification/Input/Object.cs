using SuperMetroid.Core.Input;

namespace SuperMetroid.Verification.Input;

/// <summary>The header of one native object, the pre-instruction word its typed owner first watches with, and its program.</summary>
/// <param name="Header">The header.</param>
/// <param name="FrameBehavior">The native pre-instruction word the typed owner's first watch stands for.</param>
/// <param name="Program">The program the object starts.</param>
internal readonly record struct Object(ushort Header, ushort FrameBehavior, DemoInputProgram Program);
