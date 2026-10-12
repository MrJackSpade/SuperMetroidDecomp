using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rendering;

/// <summary>Where a background layer's tilemap and character data start in VRAM, as word addresses (#633).</summary>
public readonly record struct BgVramAddresses
{
    /// <summary>Tilemap base word.</summary>
    public required ushort TilemapBaseWord { get; init; }

    /// <summary>Character base word.</summary>
    public required ushort CharacterBaseWord { get; init; }
}
