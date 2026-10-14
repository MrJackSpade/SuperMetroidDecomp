namespace SuperMetroid.Core.Assets;

/// <summary>Native bank-$8C stream boundaries used only by extraction and parity tests.</summary>
/// <param name="Id">Named story-page identity, independent of the editable wording.</param>
/// <param name="InstructionPointer">Sixteen-bit offset of the stock background-object list in bank $8C, not a full SNES address.</param>
/// <param name="BeginOpcode">Bank-$8B callback offset expected at the list's start, establishing that page's subtitle state.</param>
/// <param name="FinishOpcode">Bank-$8B callback offset expected before deletion, establishing the next wait or scene transition.</param>
public readonly record struct IntroNarrationNativePage(
    IntroNarrationPageId Id,
    ushort InstructionPointer,
    ushort BeginOpcode,
    ushort FinishOpcode);
