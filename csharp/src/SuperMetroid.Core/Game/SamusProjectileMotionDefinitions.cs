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
    internal static ushort ReadWord(int address)
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
            if (address >= SamusProjectileRomData.NonBeam.MissileAccelerations && index < 20)
                return Acceleration(index / 2, (index & 1) != 0, 64, 54);
            index = (address - SamusProjectileRomData.NonBeam.SuperMissileAccelerations) / 2;
            if (address >= SamusProjectileRomData.NonBeam.SuperMissileAccelerations && index < 20)
                return Acceleration(index / 2, (index & 1) != 0, 256, 182);
            index = (address - SamusProjectileRomData.Beams.XAccelerations) / 2;
            if (address >= SamusProjectileRomData.Beams.XAccelerations && index < 20)
                return Acceleration(index % 10, index >= 10, 16, 16);
        }
        throw new InvalidDataException(
            $"Projectile motion word ${address:X6} is outside the compiled definitions.");
    }
    /// <summary>
    /// $90:C303/C32B/C353: ten facing-dependent directions cover eight compass
    /// octants, with duplicated down and up directions. Project each acceleration
    /// magnitude onto the selected signed axis; diagonal magnitudes remain the
    /// native missile54 / super182, while beam acceleration uses16 on either axis.
    /// </summary>
    private static ushort Acceleration(int direction, bool yAxis, int cardinal, int diagonal)
    {
        int octant = (direction - (direction >= 5 ? 1 : 0)) & 7;
        int sign = yAxis
            ? octant is 0 or 1 or 7 ? -1 : octant is 3 or 4 or 5 ? 1 : 0
            : octant is >= 1 and <= 3 ? 1 : octant is >= 5 and <= 7 ? -1 : 0;
        return unchecked((ushort)(sign * ((octant & 1) == 0 ? cardinal : diagonal)));
    }
}
