using SuperMetroid.Core.Frontend.Ending;

namespace SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;

/// <summary>
/// One step of an ending typed text's source artwork (#2195): the editable composition the cartridge shows its text typed
/// as far as one more letter in, from which the import pictures that letter.
/// </summary>
/// <param name="Name">The composition's name in the editable source artwork.</param>
/// <param name="Typed">The text and how many of its letters the step shows.</param>
/// <param name="StockPartCount">How many parts the step's stock composition holds.</param>
public readonly record struct EndingTypedStepDefinition(string Name, EndingTextTyping Typed, int StockPartCount);
