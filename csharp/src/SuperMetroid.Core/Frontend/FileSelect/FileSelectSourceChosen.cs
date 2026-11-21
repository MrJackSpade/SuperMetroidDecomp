using SuperMetroid.Core.Saves;

namespace SuperMetroid.Core.Frontend.FileSelect;

/// <summary>A file-select copy or clear that has chosen its source slot.</summary>
/// <param name="Source">The slot copied or cleared.</param>
internal partial record FileSelectSourceChosen(SaveSlot Source) : FileSelectSlotChoice;
