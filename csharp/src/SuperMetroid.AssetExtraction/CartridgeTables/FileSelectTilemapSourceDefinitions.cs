using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.CartridgeTables;

/// <summary>
/// Where bank $81 keeps the file-select screens' authored tilemap text streams (#1416): the import pipeline's private
/// knowledge, decoded once into the installed file-select presentation.
/// </summary>
[EncodingOwner("bank-$81 addresses of the file-select tilemap text streams")]
internal static class FileSelectTilemapSourceDefinitions
{
    /// <summary>SAMUS DATA heading at $81:B40A.</summary>
    internal const ushort SamusData = 0xb40a;

    /// <summary>The SAMUS A, B and C label streams at $81:B436, B456 and B476, one 32-byte record per slot.</summary>
    internal static readonly ushort[] SlotLabels = [0xb436, 0xb456, 0xb476];

    /// <summary>ENERGY label at $81:B496.</summary>
    internal const ushort Energy = 0xb496;

    /// <summary>TIME label at $81:B4A0.</summary>
    internal const ushort Time = 0xb4a0;

    /// <summary>Time separator at $81:B4A8.</summary>
    internal const ushort TimeColon = 0xb4a8;

    /// <summary>NO DATA slot contents at $81:B4AC.</summary>
    internal const ushort NoData = 0xb4ac;

    /// <summary>DATA COPY command at $81:B4C4.</summary>
    internal const ushort DataCopy = 0xb4c4;

    /// <summary>DATA CLEAR command at $81:B4D8.</summary>
    internal const ushort DataClear = 0xb4d8;

    /// <summary>EXIT command at $81:B4EE.</summary>
    internal const ushort Exit = 0xb4ee;

    /// <summary>DATA COPY MODE heading at $81:B4F8.</summary>
    internal const ushort DataCopyMode = 0xb4f8;

    /// <summary>DATA CLEAR MODE heading at $81:B534.</summary>
    internal const ushort DataClearMode = 0xb534;

    /// <summary>COPY WHICH DATA prompt at $81:B574.</summary>
    internal const ushort CopyWhichData = 0xb574;

    /// <summary>COPY SAMUS TO WHERE prompt at $81:B596.</summary>
    internal const ushort CopySamusToWhere = 0xb596;

    /// <summary>COPY SAMUS TO SAMUS confirmation at $81:B5C8.</summary>
    internal const ushort CopySamusToSamus = 0xb5c8;

    /// <summary>IS THIS OK prompt at $81:B602.</summary>
    internal const ushort IsThisOkay = 0xb602;

    /// <summary>YES confirmation choice at $81:B61A.</summary>
    internal const ushort Yes = 0xb61a;

    /// <summary>NO confirmation choice at $81:B62A.</summary>
    internal const ushort No = 0xb62a;

    /// <summary>COPY COMPLETED result at $81:B63A.</summary>
    internal const ushort CopyCompleted = 0xb63a;

    /// <summary>CLEAR WHICH DATA prompt at $81:B65A.</summary>
    internal const ushort ClearWhichData = 0xb65a;

    /// <summary>CLEAR SAMUS confirmation at $81:B69A.</summary>
    internal const ushort ClearSamus = 0xb69a;

    /// <summary>DATA CLEARED result at $81:B6DA.</summary>
    internal const ushort DataCleared = 0xb6da;
}
