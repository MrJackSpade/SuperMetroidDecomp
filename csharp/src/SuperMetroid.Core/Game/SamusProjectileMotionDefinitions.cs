using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>Compiled NTSC beam speeds and beam/missile acceleration definitions.</summary>
internal static class SamusProjectileMotionDefinitions
{
    /// <summary>$90:C2D1 BeamSpeeds_Horizontal_Vertical: all twelve authored rows use 4 px/frame.</summary>
    private const ushort CardinalSpeed = 0x0400;
    /// <summary>$90:C2D3 BeamSpeeds_Diagonal: all twelve authored rows use 683/256 px/frame.</summary>
    private const ushort DiagonalSpeed = 0x02ab;
    /// <summary>$90:C301 MissileInitializedBitset: adjacent word reached by invalid beam combinations.</summary>
    private const ushort MissileInitialized = 0x0100;
    /// <summary>$90:C303 MissileAccelerations: ten signed X/Y pairs, in direction order.</summary>
    private static ReadOnlySpan<short> Missile =>
        [0, -64, 54, -54, 64, 0, 54, 54, 0, 64, 0, 64, -54, 54, -64, 0, -54, -54, 0, -64];
    /// <summary>$90:C32B SuperMissileAccelerations: ten signed X/Y pairs.</summary>
    private static ReadOnlySpan<short> SuperMissile =>
        [0, -256, 182, -182, 256, 0, 182, 182, 0, 256, 0, 256, -182, 182, -256, 0, -182, -182, 0, -256];
    /// <summary>$90:C353/C367 ProjectileAccelerations.X/Y: ten X words followed by ten Y words.</summary>
    private static ReadOnlySpan<short> Beam =>
        [0, 16, 16, 16, 0, 0, -16, -16, -16, 0, -16, -16, 0, 16, 16, 16, 16, 0, -16, -16];

    internal static ushort ReadWord(ISnesAddressSpace bus, int address)
    {
        int offset = address - SamusProjectileRomData.Beams.HorizontalVerticalSpeeds;
        // Native data starts at an odd address. Only aligned words in that layout
        // belong to this catalog; preserve all other reads through the address space.
        if (offset >= 0 && (offset & 1) == 0)
        {
            // $90:C2D1 BeamSpeeds: twelve identical cardinal/diagonal pairs.
            // Resolve physical address before row ownership: combinations C..F
            // deliberately overread into ignition and missile acceleration data.
            if (offset < 12 * 4) return (offset & 2) == 0 ? CardinalSpeed : DiagonalSpeed;
            if (address == SamusProjectileRomData.NonBeam.MissileAccelerations - 2) return MissileInitialized;
            int index = (address - SamusProjectileRomData.NonBeam.MissileAccelerations) / 2;
            if (address >= SamusProjectileRomData.NonBeam.MissileAccelerations && index < Missile.Length)
                return unchecked((ushort)Missile[index]);
            index = (address - SamusProjectileRomData.NonBeam.SuperMissileAccelerations) / 2;
            if (address >= SamusProjectileRomData.NonBeam.SuperMissileAccelerations && index < SuperMissile.Length)
                return unchecked((ushort)SuperMissile[index]);
            index = (address - SamusProjectileRomData.Beams.XAccelerations) / 2;
            if (address >= SamusProjectileRomData.Beams.XAccelerations && index < Beam.Length)
                return unchecked((ushort)Beam[index]);
        }
        return (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);
    }
}
