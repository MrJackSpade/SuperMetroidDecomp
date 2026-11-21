using SuperMetroid.Core.Saves;

namespace SuperMetroid.Core.Frontend.FileSelect;

/// <summary>A file-select copy that has chosen its source and target slots.</summary>
/// <param name="Source">The slot copied.</param>
/// <param name="Target">The slot copied over.</param>
internal sealed partial record FileSelectCopyChosen(SaveSlot Source, SaveSlot Target) : FileSelectSourceChosen(Source);
