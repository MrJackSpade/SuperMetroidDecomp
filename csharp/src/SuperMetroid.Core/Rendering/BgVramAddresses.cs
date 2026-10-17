using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rendering;

/// <summary>Where a background layer's tilemap and character data start in VRAM, as word addresses (#633).</summary>
public readonly record struct BgVramAddresses
{
    /// <summary>Validated tilemap base word.</summary>
    private readonly int tilemapBaseWord;

    /// <summary>Validated character base word.</summary>
    private readonly int characterBaseWord;

    /// <summary>Tilemap base VRAM word address, 0 through <see cref="VramWordRange.MaximumWord"/>.</summary>
    public required int TilemapBaseWord
    {
        get => tilemapBaseWord;
        init => tilemapBaseWord = VramWordRange.Require(value);
    }

    /// <summary>Character base VRAM word address, 0 through <see cref="VramWordRange.MaximumWord"/>.</summary>
    public required int CharacterBaseWord
    {
        get => characterBaseWord;
        init => characterBaseWord = VramWordRange.Require(value);
    }
}
