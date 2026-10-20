namespace SuperMetroid.Core.Frontend.FileSelect;

/// <summary>A selectable file-select row: a save slot, a data-management command, or Exit.</summary>
public enum FileSelectItem
{
    /// <summary>Save slot A.</summary>
    SlotA,

    /// <summary>Save slot B.</summary>
    SlotB,

    /// <summary>Save slot C.</summary>
    SlotC,

    /// <summary>Data Copy, shown on the main page only while a save exists.</summary>
    Copy,

    /// <summary>Data Clear, shown on the main page only while a save exists.</summary>
    Clear,

    /// <summary>Exit, on the main page and on every data-management page.</summary>
    Exit,
}
