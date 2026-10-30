using SuperMetroid.Core.Frontend.FileSelect.Artwork;

namespace SuperMetroid.Core.Frontend.FileSelect;

/// <summary>A slot letter a copy or clear prompt names (#1467): where the page shows it, and whose letter it is.</summary>
/// <param name="Anchor">The prompt anchor the letter is shown at.</param>
/// <param name="Slot">The save slot whose letter is shown.</param>
internal readonly record struct FileSelectPromptLetter(FileSelectAnchor Anchor, int Slot);
