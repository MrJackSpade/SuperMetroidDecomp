using SuperMetroid.Core.Saves;

namespace SuperMetroid.Core.Frontend.FileSelect;

/// <summary>
/// The slots a file-select copy or clear has chosen so far (#1467): none yet, a source, or a copy's source and target.
/// The pages after a choice read the slots it made; a page that names a slot not yet chosen is a defect.
/// </summary>
internal abstract partial record FileSelectSlotChoice
{
    /// <summary>A copy or clear that has chosen no slot yet.</summary>
    internal static FileSelectSlotChoice Unchosen { get; } = new FileSelectNothingChosen();

    /// <summary>The source slot the copy or clear chose.</summary>
    /// <exception cref="InvalidOperationException">No source slot has been chosen.</exception>
    internal SaveSlot ChosenSource => this is FileSelectSourceChosen chosen
        ? chosen.Source
        : throw new InvalidOperationException("File select names no source slot yet.");

    /// <summary>The target slot the copy chose.</summary>
    /// <exception cref="InvalidOperationException">No copy target has been chosen.</exception>
    internal SaveSlot ChosenTarget => this is FileSelectCopyChosen chosen
        ? chosen.Target
        : throw new InvalidOperationException("File select names no copy target yet.");

    /// <summary>A copy or clear that has chosen no slot yet.</summary>
    private sealed partial record FileSelectNothingChosen : FileSelectSlotChoice;
}
