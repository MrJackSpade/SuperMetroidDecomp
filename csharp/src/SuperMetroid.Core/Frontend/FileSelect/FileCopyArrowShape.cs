namespace SuperMetroid.Core.Frontend.FileSelect;

/// <summary>The file-copy arrow's shape: short between neighbouring slots or long between A and C, pointing at the destination.</summary>
public enum FileCopyArrowShape
{
    /// <summary>From a slot down to the slot just below it.</summary>
    ShortDown,

    /// <summary>From a slot up to the slot just above it.</summary>
    ShortUp,

    /// <summary>From slot A down to slot C.</summary>
    LongDown,

    /// <summary>From slot C up to slot A.</summary>
    LongUp,
}
