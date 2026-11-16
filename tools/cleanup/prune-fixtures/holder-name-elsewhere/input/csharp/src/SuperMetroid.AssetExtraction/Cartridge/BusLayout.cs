namespace SuperMetroid.AssetExtraction.Cartridge;

/// <summary>The bus's layout.</summary>
internal static class BusLayout
{
    /// <summary>The name of the bank size, which production reads.</summary>
    internal const string BankBytes = "bank";

    /// <summary>The name of the first work memory bank, which only checks read.</summary>
    internal const string FirstWorkBank = "work";
}