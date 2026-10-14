namespace SuperMetroid.Core.Game;

/// <summary>Crocomire's tongue programs.</summary>
public enum CrocomireTongueProgram
{
    /// <summary><c>InstList_CrocomireTongue_Fight</c> ($A4:BE56): four five-update frames, looping.</summary>
    Fight,
    /// <summary><c>InstList_Crocomire_Sleep</c> ($A4:BF62): a bare sleep for the hidden tongue after the bridge collapses.</summary>
    Sleep,
    /// <summary><c>InstList_CrocomireTongue_Melting</c> ($A4:BF98): five five-update frames, looping.</summary>
    Melting,
}
