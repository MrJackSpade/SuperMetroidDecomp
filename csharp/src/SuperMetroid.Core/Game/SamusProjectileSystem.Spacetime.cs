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
            ? unchecked((ushort)(slot.PackedType.BeamCombination.TableIndex *
                SamusProjectileRomData.Beams.InitialSpeedRowBytes))
            : unchecked((ushort)(slot.InstructionPointer -
                SpacetimeBeamCorruptionLayout.AnimationRecordByteCount));
        ushort x = unchecked((ushort)(slot.NativeByteIndex + sizeof(ushort)));
        ushort y = unchecked((ushort)(inheritedY + sizeof(ushort)));
        bool persistentMemoryCorrupted = false;

        while (unchecked((short)(y - SpacetimeBeamCorruptionLayout.PaletteByteCount)) < 0)
        {
            persistentMemoryCorrupted |= new SpacetimeBeamCorruptionMemory(
                SnesMemoryBusRequirement.Require(bus, "SpaceTime source pointer requires WRAM.")).CopyPaletteWord(x, y);
            x = unchecked((ushort)(x + sizeof(ushort)));
            y = unchecked((ushort)(y + sizeof(ushort)));
        }

        return persistentMemoryCorrupted;
    }
}
