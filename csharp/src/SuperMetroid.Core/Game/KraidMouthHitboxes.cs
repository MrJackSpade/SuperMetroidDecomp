using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>Compiled collision geometry, independent of Kraid's editable head artwork.</summary>
internal static class KraidMouthHitboxes
{
    /// <summary>$A7:9788, Hitbox_KraidMouth_0, closed-mouth vulnerable shape.</summary>
    public const ushort ClosedVulnerable = 0x9788;
    /// <summary>$A7:9790, Hitbox_KraidMouth_1, first opening-stage vulnerable shape.</summary>
    public const ushort OpeningVulnerable = 0x9790;
    /// <summary>$A7:9798, Hitbox_KraidMouth_2, second opening-stage vulnerable shape.</summary>
    public const ushort WiderVulnerable = 0x9798;
    /// <summary>$A7:97A0, Hitbox_KraidMouth_3, fully open vulnerable shape.</summary>
    public const ushort OpenVulnerable = 0x97a0;
    /// <summary>$A7:97A8, Hitbox_KraidMouth_4, unused zero rectangle.</summary>
    public const ushort EmptyShape = 0x97a8;
    /// <summary>$A7:97B0, Hitbox_KraidMouth_5, first opening-stage invulnerable shape.</summary>
    public const ushort OpeningInvulnerable = 0x97b0;
    /// <summary>$A7:97B8, Hitbox_KraidMouth_6, second opening-stage invulnerable shape.</summary>
    public const ushort WiderInvulnerable = 0x97b8;
    /// <summary>$A7:97C0, Hitbox_KraidMouth_7, fully open invulnerable shape.</summary>
    public const ushort OpenInvulnerable = 0x97c0;
    /// <summary>Aligned fixed records at $A7:9788..97C7.</summary>
    public static bool IsDefined(ushort pointer) => pointer >= ClosedVulnerable && pointer <= OpenInvulnerable && ((pointer - ClosedVulnerable) & 7) == 0;

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
    public static (short Left, short Top, short Right, short Bottom) Resolve(ushort pointer) => pointer switch
    {
        ClosedVulnerable => (16, -120, 40, -88),
        OpeningVulnerable => (16, -120, 40, -104),
        WiderVulnerable => (16, -128, 40, -112),
        OpenVulnerable => (16, -128, 40, -120),
        EmptyShape => (0, 0, 0, 0),
        OpeningInvulnerable => (6, -96, 32, -80),
        WiderInvulnerable => (0, -104, 32, -80),
        OpenInvulnerable => (0, -112, 32, -80),
        _ => throw new InvalidDataException($"Undefined Kraid mouth hitbox $A7:{pointer:X4}."),
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
        ArgumentNullException.ThrowIfNull(bus);
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

    private static ushort ReadLiveWord(ISnesAddressSpace bus, ushort pointer)
    {
        byte low = ReadLiveByte(bus, pointer);
        byte high = ReadLiveByte(bus, AddWithinBank(pointer, 1));
        return (ushort)(low | high << 8);
    }

    private static byte ReadLiveByte(ISnesAddressSpace bus, ushort pointer)
    {
        if (pointer < 0x8000)
        {
            int address = KraidBackgroundRomData.NativeBank | pointer;
            return SnesDmaSourceMap.Classify(SnesAddress.FromBusAddress(address)) switch
            {
                SnesDmaSourceKind.WorkRam => (bus as ISnesMutableMemory ??
                    throw new InvalidOperationException("Kraid mouth alias requires WRAM."))
                    .ReadWorkRamByte(address),
                SnesDmaSourceKind.SaveRam => (bus as ISnesMutableMemory ??
                    throw new InvalidOperationException("Kraid mouth alias requires SRAM."))
                    .ReadSaveRamByte(address),
                SnesDmaSourceKind.Unmapped => (bus as ISnesCpuPeripheralSource ??
                    throw new InvalidOperationException(
                        $"CPU read ${address >> 16:X2}:{address & 0xffff:X4} is outside the runtime address map."))
                    .ReadPeripheralByte(address),
                _ => throw new InvalidDataException(
                    $"Kraid mouth alias ${address:X6} requires a compiled cartridge definition."),
            };
        }

        int boundaryIndex = pointer - 0x8000;
        if ((uint)boundaryIndex < LowHalfBoundaryLength)
            return LowHalfBoundaryByte(boundaryIndex);

        throw new InvalidDataException(
            $"Kraid low-half mouth geometry crossed into uncompiled cartridge address $A7:{pointer:X4}.");
    }

    private static ushort AddWithinBank(ushort pointer, int offset) =>
        unchecked((ushort)(pointer + offset));
}
