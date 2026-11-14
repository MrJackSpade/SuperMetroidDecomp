namespace SuperMetroid.Core.Saves;

/// <summary>One of the three save files the file select offers, listed top to bottom.</summary>
/// <remarks>The slots are mutually exclusive identities; <see cref="SaveSlots"/> gives their order and store numbers.</remarks>
public enum SaveSlot
{
    /// <summary>The first save file, SAMUS A.</summary>
    A,

    /// <summary>The second save file, SAMUS B.</summary>
    B,

    /// <summary>The third save file, SAMUS C.</summary>
    C,
}
