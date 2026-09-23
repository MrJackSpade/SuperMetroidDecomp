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
    /// <remarks>
    /// Issues #625 and #981: for fragment i=0..5, the definition starts at
    /// $8B:CECD+6*i and contains initializer $A958, pre-instruction $A994,
    /// and instruction-list pointer $CD39+8*i, in that order. All 18 words
    /// at $8B:CECD..CEF0 match pinned NTSC J/U v1.0 ROM and bank_8B.asm.
    /// Each referenced eight-byte list starts with duration one and loops
    /// to itself. The egg spawn opcode creates exactly indices 0..5; the
    /// method rejects negative and one-past-end indices. The shared callback
    /// pair and two strides are an exact smaller representation of these
    /// six definitions. Actor verification completes all six fragments with
    /// physical definition reads forbidden.
    ///
    /// Issues #625 and #982: the six selected lists are a separate logical
    /// table at $8B:CD39..CD68. For i=0..5, list $CD39+8*i contains duration
    /// one, bank-$8C spritemap pointer $8F7E+7*i, Goto opcode $94BC, and its
    /// own list address. All 24 words match pinned NTSC J/U v1.0 ROM and
    /// bank_8B.asm; bank_8C.asm and ROM confirm one sprite entry at each
    /// selected pointer. IntroDiscoverySprite reads the frame, then follows
    /// the self-loop, so the bounded strides preserve every selected list.
    /// </remarks>
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
