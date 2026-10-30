using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.AssetExtraction.CartridgeTables;

/// <summary>
/// Where the cartridge keeps the Ceres station's door visuals, escape character pages and floor animation lists
/// (#1415): the import pipeline's private knowledge of the cartridge, decoded once into the installed Ceres documents.
/// </summary>
internal static class CeresSourceDefinitions
{
    /// <summary>The bytes of one RGB5 colour word.</summary>
    internal const int ColorWordBytes = sizeof(ushort);

    /// <summary>The Ceres door actor's sources, read by its initialization at $A6:F6C5 and its animation at $A6:F850.</summary>
    internal static class Door
    {
        /// <summary>$B0:C400, the four-bit characters the door's second variant copies over the sprite characters.</summary>
        internal const int Characters = 0xb0c400;

        /// <summary>$A6:F4EE, the fifteen normal setup colours.</summary>
        internal const int NormalColors = 0xa6f4ee;

        /// <summary>$A6:F50E, the fifteen escape setup colours.</summary>
        internal const int EscapeColors = 0xa6f50e;

        /// <summary>$A6:F871, the eight glow rows of six colours each.</summary>
        internal const int AnimationColors = 0xa6f871;

        /// <summary>The bytes between consecutive glow rows: eight stored colours, of which the first six are copied.</summary>
        internal const int AnimationRowBytes = 8 * ColorWordBytes;

        /// <summary>$A6:F918, the light platform strip; the two $A6:F900 transfer records name it and the dark strip after it.</summary>
        internal const int PlatformFrames = 0xa6f918;

        /// <summary>The bytes between the light and dark platform strips: one byte per floor cell.</summary>
        internal const int PlatformFrameBytes = 4;
    }

    /// <summary>The bank-$A6 words of the Ceres escape warning's overlay pages.</summary>
    internal static class EscapeOverlay
    {
        /// <summary>$A6:C164: the English EMERGENCY title's words.</summary>
        internal static readonly SnesAddress Emergency = new(0xa6, 0xc164);

        /// <summary>$A6:C3F4: the Japanese subtitle's four row halves' words, stored one after another.</summary>
        internal static readonly SnesAddress JapaneseRows = new(0xa6, 0xc3f4);
    }

    /// <summary>
    /// The bank-$A6 floor update lists Ridley's getaway queues ($A6:ACBC selects the capsule's, $A6:AD27 the wing's):
    /// each a run of nine-byte descriptors ended by a zero byte.
    /// </summary>
    internal static class FloorAnimation
    {
        /// <summary>$A6:ACE2, $A6:ACF5 and $A6:AD08: the Baby capsule's three frames, two descriptors each.</summary>
        internal static readonly SnesAddress[] CapsuleFrames = [new(0xa6, 0xace2), new(0xa6, 0xacf5), new(0xa6, 0xad08)];

        /// <summary>$A6:AD49 and $A6:AD80: Ridley's wings' two frames, six descriptors each.</summary>
        internal static readonly SnesAddress[] WingFrames = [new(0xa6, 0xad49), new(0xa6, 0xad80)];

        /// <summary>The bytes of one descriptor: control, three-byte source, two-byte count, two-byte floor cell, increment mode.</summary>
        internal const int DescriptorBytes = 9;

        /// <summary>Where a descriptor's three-byte tile source starts within it.</summary>
        internal const int DescriptorSource = 1;

        /// <summary>Where a descriptor's two-byte tile count starts within it.</summary>
        internal const int DescriptorCount = 4;

        /// <summary>Where a descriptor's two-byte first floor cell starts within it.</summary>
        internal const int DescriptorCell = 6;
    }
}
