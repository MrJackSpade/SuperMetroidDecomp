namespace SuperMetroid.Core.Game;

/// <summary>Compiled collision geometry, independent of Kraid's editable head artwork.</summary>
internal static class KraidMouthHitboxDefinitions
{
    /// <summary>Aligned fixed records at $A7:9788..97C7.</summary>
    public static bool IsDefined(ushort pointer) => Enum.IsDefined((KraidMouthHitbox)pointer);

    /// <summary>
    /// $A7:9788..97C7, Hitbox_KraidMouth_0..7. The native projectile test uses
    /// left/top/bottom only; the authored right edge is retained as definition data.
    /// Entry four is unused but remains a defined all-zero rectangle.
    /// </summary>
    /// <remarks>
    /// Independently reviewed for #1165 against all 32 original words and pinned
    /// bank_A7.asm head-program selectors. These are named collision shapes selected
    /// by mouth stage and vulnerability, not a chronological numerical curve.
    /// The existing semantic cases preserve the otherwise unused right edge and
    /// zero shape. Live low-half geometry remains a separate caller-owned path.
    /// </remarks>
    public static (short Left, short Top, short Right, short Bottom) Resolve(ushort pointer) =>
        ClosedNativeWords.Decode<KraidMouthHitbox>(pointer, "Kraid mouth hitbox") switch
        {
            KraidMouthHitbox.ClosedVulnerable => (16, -120, 40, -88),
            KraidMouthHitbox.OpeningVulnerable => (16, -120, 40, -104),
            KraidMouthHitbox.WiderVulnerable => (16, -128, 40, -112),
            KraidMouthHitbox.OpenVulnerable => (16, -128, 40, -120),
            KraidMouthHitbox.EmptyShape => (0, 0, 0, 0),
            KraidMouthHitbox.OpeningInvulnerable => (6, -96, 32, -80),
            KraidMouthHitbox.WiderInvulnerable => (0, -104, 32, -80),
            KraidMouthHitbox.OpenInvulnerable => (0, -112, 32, -80),
            _ => throw new InvalidOperationException($"Undefined Kraid mouth hitbox $A7:{pointer:X4}."),
        };

    /// <summary>
    /// Resolves compiled cartridge geometry or a genuine bank-$A7 low-half live-memory,
    /// hardware, or open-bus alias. The seven upper-window bytes reachable when a low-half
    /// record crosses $7FFF are retained explicitly; unrelated cartridge addresses are not
    /// accepted as hitboxes.
    /// </summary>
    public static (short Left, short Top, short Bottom) ResolveCollision(
        ISnesAddressSpace bus,
        ushort pointer)
    {
        Ensure.NotNull(bus);
        if (IsDefined(pointer))
        {
            (short left, short top, _, short bottom) = Resolve(pointer);
            return (left, top, bottom);
        }
        if (pointer >= 0x8000)
        {
            throw new InvalidDataException(
                $"Kraid mouth hitbox $A7:{pointer:X4} is outside the compiled geometry catalog " +
                "and is not a live bank-$A7 low-half alias.");
        }

        return (
            unchecked((short)ReadLiveWord(bus, pointer)),
            unchecked((short)ReadLiveWord(bus, AddWithinBank(pointer, 2))),
            unchecked((short)ReadLiveWord(bus, AddWithinBank(pointer, 6))));
    }

    /// <summary>
    /// Exact bytes at <c>$A7:8000-$8006</c>, reachable only when a mutable low-half
    /// record beginning at <c>$A7:7FF9-$7FFF</c> crosses into the LoROM window.
    /// </summary>
    public const int LowHalfBoundaryLength = 7;

    /// <summary>Returns one of the seven bounded native instruction bytes visible to low-half aliases.</summary>
    /// <remarks>
    /// The pinned bank_A7 stubs are JSL KraidMouthHitboxBoundaryDefinitions.ResumeMainAiTarget; RTL; JSL KraidMouthHitboxBoundaryDefinitions.GrappleLatchTarget.
    /// Decode by opcode/operand role, extracting the long call target little-endian.
    /// Only the second call's low operand byte is reachable. This models bounded
    /// compatibility data without a stored byte lookup or runtime cartridge read.
    /// </remarks>
    public static byte LowHalfBoundaryByte(int index)
    {
        if ((uint)index >= LowHalfBoundaryLength) throw new IndexOutOfRangeException();
        if (index == 4) return KraidMouthHitboxBoundaryDefinitions.LongReturnOpcode;
        int callOffset = index % 5;
        if (callOffset == 0) return KraidMouthHitboxBoundaryDefinitions.LongCallOpcode;
        int target = index < 5 ? KraidMouthHitboxBoundaryDefinitions.ResumeMainAiTarget : KraidMouthHitboxBoundaryDefinitions.GrappleLatchTarget;
        return (byte)(target >> (8 * (callOffset - 1)));
    }

    /// <summary>Reads a low-half collision coordinate with the native little-endian, bank-wrapping access pattern.</summary>
    private static ushort ReadLiveWord(ISnesAddressSpace bus, ushort pointer)
    {
        byte low = ReadLiveByte(bus, pointer);
        byte high = ReadLiveByte(bus, AddWithinBank(pointer, 1));
        return (ushort)(SnesLittleEndian.Word(low, high));
    }

    /// <summary>Reads mutable bank-$A7 alias data or the seven instruction bytes reachable across the $8000 boundary.</summary>
    private static byte ReadLiveByte(ISnesAddressSpace bus, ushort pointer)
    {
        if (pointer < KraidBackgroundRomData.HeadRomWindowStart)
            return new KraidLowBankAliasMemory(bus).MouthHitboxByte(pointer);

        int boundaryIndex = pointer - 0x8000;
        if ((uint)boundaryIndex < LowHalfBoundaryLength)
            return LowHalfBoundaryByte(boundaryIndex);

        throw new InvalidDataException(
            $"Kraid low-half mouth geometry crossed into uncompiled cartridge address $A7:{pointer:X4}.");
    }

    /// <summary>Advances a native 16-bit pointer without carrying into another bank.</summary>
    private static ushort AddWithinBank(ushort pointer, int offset) =>
        unchecked((ushort)(pointer + offset));
}
