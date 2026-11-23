using SuperMetroid.Core.Frontend.Ending;

namespace SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;

/// <summary>
/// One step of an ending typed text's source artwork (#2195): the editable composition the cartridge shows its text typed
/// as far as one more letter in, from which the import pictures that letter.
/// </summary>
/// <param name="Typed">The text and how many of its letters the step shows.</param>
/// <param name="StockPartCount">How many parts the step's stock composition holds.</param>
public readonly partial record struct EndingTypedStepDefinition(EndingTextTyping Typed, int StockPartCount)
{
    /// <summary>
    /// The composition's name in the editable source artwork, spelled from its text's key and the letters it shows, as the
    /// letter's sheet frame is (#2556).
    /// </summary>
    public string Name => $"{EndingTypedTextKeys.Installed.Of(Typed.Text)}.Letter{Typed.Letters}";
}
