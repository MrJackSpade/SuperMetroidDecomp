using System.Text;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

/// <summary>One palette-FX family's frame and colour counts, colour-pointer function and definitions (#633).</summary>
internal readonly record struct PaletteFxFamilyShape
{
    /// <summary>Frame count.</summary>
    public required int FrameCount { get; init; }

    /// <summary>Colors per frame.</summary>
    public required int ColorsPerFrame { get; init; }

    /// <summary>Color pointer.</summary>
    public required Func<int, int, ushort> ColorPointer { get; init; }

    /// <summary>Definitions.</summary>
    public required IReadOnlyList<ushort> Definitions { get; init; }
}
