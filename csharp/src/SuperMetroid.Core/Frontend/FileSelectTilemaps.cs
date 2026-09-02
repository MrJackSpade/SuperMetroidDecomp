namespace SuperMetroid.Core.Frontend;

/// <summary>Bank-$81 source pointers for cartridge-authored file-select tilemaps.</summary>
internal static class FileSelectTilemaps
{
    /// <summary>SAMUS DATA heading at $81:B40A.</summary>
    public const ushort SamusData = 0xb40a;

    /// <summary>SAMUS A slot label at $81:B436.</summary>
    public const ushort SamusA = 0xb436;

    /// <summary>SAMUS B slot label at $81:B456.</summary>
    public const ushort SamusB = 0xb456;

    /// <summary>SAMUS C slot label at $81:B476.</summary>
    public const ushort SamusC = 0xb476;

    /// <summary>ENERGY label at $81:B496.</summary>
    public const ushort Energy = 0xb496;

    /// <summary>TIME label at $81:B4A0.</summary>
    public const ushort Time = 0xb4a0;

    /// <summary>Time separator at $81:B4A8.</summary>
    public const ushort TimeColon = 0xb4a8;

    /// <summary>NO DATA slot contents at $81:B4AC.</summary>
    public const ushort NoData = 0xb4ac;

    /// <summary>DATA COPY command at $81:B4C4.</summary>
    public const ushort DataCopy = 0xb4c4;

    /// <summary>DATA CLEAR command at $81:B4D8.</summary>
    public const ushort DataClear = 0xb4d8;

    /// <summary>EXIT command at $81:B4EE.</summary>
    public const ushort Exit = 0xb4ee;

    /// <summary>DATA COPY MODE heading at $81:B4F8.</summary>
    public const ushort DataCopyMode = 0xb4f8;

    /// <summary>DATA CLEAR MODE heading at $81:B534.</summary>
    public const ushort DataClearMode = 0xb534;

    /// <summary>COPY WHICH DATA prompt at $81:B574.</summary>
    public const ushort CopyWhichData = 0xb574;

    /// <summary>COPY SAMUS TO WHERE prompt at $81:B596.</summary>
    public const ushort CopySamusToWhere = 0xb596;

    /// <summary>COPY SAMUS TO SAMUS confirmation at $81:B5C8.</summary>
    public const ushort CopySamusToSamus = 0xb5c8;

    /// <summary>IS THIS OK prompt at $81:B602.</summary>
    public const ushort IsThisOkay = 0xb602;

    /// <summary>YES confirmation choice at $81:B61A.</summary>
    public const ushort Yes = 0xb61a;

    /// <summary>NO confirmation choice at $81:B62A.</summary>
    public const ushort No = 0xb62a;

    /// <summary>COPY COMPLETED result at $81:B63A.</summary>
    public const ushort CopyCompleted = 0xb63a;

    /// <summary>CLEAR WHICH DATA prompt at $81:B65A.</summary>
    public const ushort ClearWhichData = 0xb65a;

    /// <summary>CLEAR SAMUS confirmation at $81:B69A.</summary>
    public const ushort ClearSamus = 0xb69a;

    /// <summary>DATA CLEARED result at $81:B6DA.</summary>
    public const ushort DataCleared = 0xb6da;
}
