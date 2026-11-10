using SuperMetroid.Core.Fx.PaletteFx;

namespace SuperMetroid.AssetExtraction.Fx.PaletteFx;

/// <summary>
/// One of the cartridge's timed palette-effect records (#1598), numbered from zero in the order the colour effects'
/// programs show them, which is the order the cartridge lays them out: the import's key for each record's source, write
/// layout and installed colours. The game names a record only by the <see cref="PaletteFxFrame"/> that shows it.
/// </summary>
/// <param name="Record">The record's place in the cartridge's order.</param>
public readonly record struct PaletteFxRecord(int Record)
{
    /// <summary>The frame of a colour effect's animation that shows the record.</summary>
    /// <exception cref="ArgumentOutOfRangeException">No program shows the record.</exception>
    public PaletteFxFrame Frame => PaletteFxRecords.FrameOf(this);

    /// <summary>The record <paramref name="frame"/> shows.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The frame is not one its program shows.</exception>
    public static PaletteFxRecord Of(PaletteFxFrame frame) => PaletteFxRecords.Of(frame);
}
