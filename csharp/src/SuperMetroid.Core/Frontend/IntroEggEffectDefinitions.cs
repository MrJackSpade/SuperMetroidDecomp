namespace SuperMetroid.Core.Frontend;

/// <summary>Fixed cinematic-object definitions for egg shell fragments and slime drops.</summary>
internal static class IntroEggEffectDefinitions
{
    /// <summary>Bank containing the seven native cinematic-object definitions.</summary>
    public const int NativeDefinitionBank = 0x8b0000;

    /// <summary>Number of shell-fragment definitions at <c>$8B:CECD-$8B:CEEF</c>.</summary>
    public const int ParticleCount = 6;

    /// <summary>Number of slime drops allocated by the confused-baby pre-instruction.</summary>
    public const int SlimeDropCount = 4;

    /// <summary>
    /// Returns one of the six adjacent shell-fragment definitions. All share initializer
    /// <c>$A958</c> and pre-instruction <c>$A994</c>; each owns a distinct eight-byte list.
    /// </summary>
    public static IntroEggEffectActorDefinition Particle(int index)
    {
        if ((uint)index >= ParticleCount)
            throw new ArgumentOutOfRangeException(nameof(index));
        return new(
            unchecked((ushort)(0xcecd + index * 6)),
            0xa958,
            0xa994,
            unchecked((ushort)(0xcd39 + index * 8)));
    }

    /// <summary><c>$8B:CEF1</c>, the shared egg-slime initialization, motion, and list definition.</summary>
    public static IntroEggEffectActorDefinition SlimeDrop =>
        new(0xcef1, 0xaa9a, 0xaab3, 0xcd69);
}

/// <summary>One native six-byte shell/slime cinematic-object definition.</summary>
internal readonly record struct IntroEggEffectActorDefinition(
    ushort Pointer,
    ushort Initialization,
    ushort PreInstruction,
    ushort InstructionList);
