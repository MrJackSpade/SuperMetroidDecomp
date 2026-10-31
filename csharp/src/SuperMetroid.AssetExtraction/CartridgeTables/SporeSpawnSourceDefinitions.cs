using SuperMetroid.Core.Bosses.SporeSpawn;
using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.CartridgeTables;

/// <summary>
/// Where the cartridge keeps Spore Spawn's colour images (#1413): the import pipeline's private knowledge of the
/// cartridge, decoded once into the installed Spore Spawn colour document.
/// </summary>
[EncodingOwner("cartridge addresses of Spore Spawn's colour tables")]
internal static class SporeSpawnSourceDefinitions
{
    /// <summary>$A5:E359, the spore colour image the initializer copies to sprite palette seven.</summary>
    internal const int SporeColors = 0xa5e359;

    /// <summary>$A5:E379..$A5:E3F8, the four consecutive health-stage body images.</summary>
    internal const int HealthColors = 0xa5e379;

    /// <summary>$A5:E3F9..$A5:E4F8, the eight consecutive death-sequence body images.</summary>
    internal const int DeathSpriteColors = 0xa5e3f9;

    /// <summary>$A5:E4F9..$A5:E5D8, the seven consecutive death-sequence foreground images.</summary>
    internal const int DeathLevelColors = 0xa5e4f9;

    /// <summary>$A5:E5D9..$A5:E6B8, the seven consecutive death-sequence background images.</summary>
    internal const int DeathBackgroundColors = 0xa5e5d9;

    /// <summary>The bytes of one RGB5 colour word.</summary>
    internal const int ColorWordBytes = sizeof(ushort);

    /// <summary>
    /// The bytes of one sixteen-colour image: the stride between consecutive images, and the unit of the byte-offset
    /// operand the cartridge's palette instructions and health selector name an image by.
    /// </summary>
    internal const int ImageBytes = SporeSpawnColorDefinitions.ColorsPerFrame * ColorWordBytes;
}
