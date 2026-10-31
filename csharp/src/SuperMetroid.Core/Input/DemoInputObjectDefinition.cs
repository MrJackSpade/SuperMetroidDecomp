namespace SuperMetroid.Core.Input;

/// <summary>A demo-input object (#1300): the pre-instruction it installs and the program it runs.</summary>
/// <param name="FrameBehavior">The routine the object installs, which its owner interprets.</param>
/// <param name="Program">The program the object starts.</param>
public readonly record struct DemoInputObjectDefinition(DemoInputFrameBehavior FrameBehavior, DemoInputProgram Program);
