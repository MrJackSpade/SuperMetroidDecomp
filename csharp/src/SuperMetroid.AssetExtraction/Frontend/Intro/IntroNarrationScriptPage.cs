using SuperMetroid.Core.Frontend.Intro.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Intro;

/// <summary>Where one narration page's stock script lies in the cartridge and the callbacks that bound it; extraction-only.</summary>
/// <param name="Id">Named story-page identity, independent of the editable wording.</param>
/// <param name="InstructionPointer">Offset of the page's stock background-object list within the narration script bank.</param>
/// <param name="BeginOpcode">Callback offset expected at the list's start, establishing that page's subtitle state.</param>
/// <param name="FinishOpcode">Callback offset expected before deletion, establishing the next wait or scene transition.</param>
/// <param name="EndsWithBlinkingCaret">
/// Whether the page's text ends by setting the caret blinking and holding the finished page, as the last page does.
/// </param>
internal readonly partial record struct IntroNarrationScriptPage(
    IntroNarrationPageId Id,
    int InstructionPointer,
    int BeginOpcode,
    int FinishOpcode,
    bool EndsWithBlinkingCaret = false);
