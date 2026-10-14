namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>One native six-byte shell/slime cinematic-object definition.</summary>
internal readonly record struct IntroEggEffectActorDefinition(
    ushort PreInstruction,
    ushort InstructionList);
