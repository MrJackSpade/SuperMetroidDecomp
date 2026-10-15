namespace SuperMetroid.Core.Input;

/// <summary>A bank-$91 demo-input object (#1300): the pre-instruction it installs and the program it runs.</summary>
/// <param name="PreInstruction">The object's native pre-instruction identity, which its owner interprets.</param>
/// <param name="Program">The program the object starts.</param>
public readonly record struct DemoInputObjectDefinition(ushort PreInstruction, DemoInputProgram Program);
