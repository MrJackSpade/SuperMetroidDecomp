using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rendering;

/// <summary>Where a background layer's tilemap and character data start in VRAM, as word addresses (#633).</summary>
public readonly record struct BgVramAddresses
{
    /// <summary>Tilemap base VRAM word address, 0 through <c>VramWordRange.MaximumWord</c>.</summary>
    public required int TilemapBaseWord
    {
        get;
        init => field = VramWordRange.Require(value);
    }

    /// <summary>Character base VRAM word address, 0 through <c>VramWordRange.MaximumWord</c>.</summary>
    public required int CharacterBaseWord
    {
        get;
        init => field = VramWordRange.Require(value);
    }
}
