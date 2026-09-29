using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>Native data-as-code behavior used by the SpaceTime Beam progression reset.</summary>
public sealed partial class SamusProjectileSystem
{
    /// <summary>
    /// Executes the observable tail of the malformed $90:AD16 callback. The callback
    /// starts on the second operand byte of a long store, so $C1 $7E is first decoded as
    /// an irrelevant compare. It then increments the projectile loop's X and inherited Y
    /// before conditionally falling back into LoadBeamPalette's copy loop at $90:AD12.
    /// </summary>
    /// <returns>
    /// True when the copy touched a translated persistent-progression or saved-entry byte.
    /// </returns>
    private static bool RunSpacetimePaletteCopyTail(
        ISnesAddressSpace bus,
        SamusProjectileSlot slot)
    {
        ushort inheritedY = slot.SpritemapPointer == 0
            ? unchecked((ushort)(slot.PackedType.BeamCombinationIndex *
                SamusProjectileRomData.Beams.InitialSpeedRowBytes))
            : unchecked((ushort)(slot.InstructionPointer -
                SpacetimeBeamCorruptionLayout.AnimationRecordByteCount));
        ushort x = unchecked((ushort)(slot.NativeByteIndex + sizeof(ushort)));
        ushort y = unchecked((ushort)(inheritedY + sizeof(ushort)));
        bool persistentMemoryCorrupted = false;

        while (unchecked((short)(y - SpacetimeBeamCorruptionLayout.PaletteByteCount)) < 0)
        {
            int sourcePointer =
                (bus as ISnesMutableMemory ?? throw new InvalidOperationException(
                    "SpaceTime source pointer requires WRAM.")).ReadWorkRamByte(
                    SpacetimeBeamCorruptionLayout.SourceLongPointerAddress) |
                ((ISnesMutableMemory)bus).ReadWorkRamByte(
                    SpacetimeBeamCorruptionLayout.SourceLongPointerAddress + 1) << 8 |
                ((ISnesMutableMemory)bus).ReadWorkRamByte(
                    SpacetimeBeamCorruptionLayout.SourceLongPointerAddress + 2) << 16;
            int sourceAddress = (sourcePointer + y) &
                SpacetimeBeamCorruptionLayout.CpuAddressMask;
            int destinationAddress =
                (SpacetimeBeamCorruptionLayout.SpritePaletteSixWramAddress + x) &
                SpacetimeBeamCorruptionLayout.CpuAddressMask;

            bus.WriteByte(destinationAddress, ReadSpacetimeSourceByte(bus, sourceAddress));
            bus.WriteByte(
                (destinationAddress + 1) & SpacetimeBeamCorruptionLayout.CpuAddressMask,
                ReadSpacetimeSourceByte(bus, (sourceAddress + 1) &
                    SpacetimeBeamCorruptionLayout.CpuAddressMask));
            persistentMemoryCorrupted |=
                destinationAddress <= SaveRamLayout.LoadingGameStateWramAddress + 1 &&
                destinationAddress + 1 >= SaveRamLayout.EventsWramAddress;

            x = unchecked((ushort)(x + sizeof(ushort)));
            y = unchecked((ushort)(y + sizeof(ushort)));
        }

        return persistentMemoryCorrupted;
    }

    private static byte ReadSpacetimeSourceByte(ISnesAddressSpace bus, int address) =>
        SnesDmaSourceMap.Classify(SnesAddress.FromBusAddress(address)) switch
        {
            SnesDmaSourceKind.WorkRam => (bus as ISnesMutableMemory ??
                throw new InvalidOperationException("SpaceTime source requires WRAM."))
                .ReadWorkRamByte(address),
            SnesDmaSourceKind.SaveRam => (bus as ISnesMutableMemory ??
                throw new InvalidOperationException("SpaceTime source requires SRAM."))
                .ReadSaveRamByte(address),
            SnesDmaSourceKind.Unmapped => (bus as ISnesCpuPeripheralSource ??
                throw new InvalidOperationException($"SpaceTime source ${address:X6} requires an unimplemented peripheral."))
                .ReadPeripheralByte(address),
            _ => throw new InvalidOperationException(
                $"SpaceTime source ${address:X6} requires a compiled cartridge definition."),
        };
}
