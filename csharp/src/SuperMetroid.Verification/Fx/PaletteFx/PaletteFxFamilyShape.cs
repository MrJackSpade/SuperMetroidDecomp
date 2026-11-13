using SuperMetroid.Core.Fx.PaletteFx;

namespace SuperMetroid.Verification.Fx.PaletteFx;

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

    /// <summary>Whether the family plays on a frontend screen, which steps it as screens do (#1512), not in gameplay.</summary>
    public bool OnScreen { get; init; }

    /// <summary>
    /// The Torizo a belly glow family lights, which a Torizo's fight start hands the glow it starts (#1700); none for every
    /// other family, which starts as an ordinary effect.
    /// </summary>
    public ILitTorizo? LitTorizo { get; init; }
}
