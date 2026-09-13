using SuperMetroid.Core.Frontend;

namespace SuperMetroid.Core.Assets;

/// <summary>Reserve-strip visual identities and import-only cartridge bindings.</summary>
public static class PauseReserveTankDefinitions
{
    public const int Version = 1;
    public const string FileName = "pause-reserve-tanks.json";
    /// <summary>$82:C1D6 supplies six origins, including the trailing cap position.</summary>
    public const int AnchorCount = 6;
    public const int XPositions = PauseReserveTankRomData.XPositions;
    /// <summary>$82:C1E2 supplies Y plus one; import applies the draw routine's decrement.</summary>
    public const int YPosition = PauseReserveTankRomData.YPosition;
    /// <summary>$82:B3D9 has two identical eight-entry partial-fill tables.</summary>
    public const int PartialMaps = PauseReserveTankRomData.PartialMaps;
    /// <summary>$82:B3FC uses OBJ palette three; $82:B433 keeps that palette even as its unused timer advances.</summary>
    public const ushort PaletteBits = PauseReserveTankRomData.PaletteBits;
    /// <summary>$82:B305 full tank; $82:B396 trailing cap; $82:B37D empty tank and $82:B3D9 seven fill levels.</summary>
    public static IEnumerable<(string Name, ushort Id)> Frames()
    {
        yield return ("Full", PauseReserveTankRomData.FullMap);
        yield return ("EndCap", PauseReserveTankRomData.EndCapMap);
        yield return ("Empty", PauseReserveTankRomData.EmptyMap);
        for (int fill = 1; fill <= 7; fill++) yield return ($"Fill{fill}", (ushort)(PauseReserveTankRomData.EmptyMap + fill));
    }
    /// <summary>$82:B3D9 maps each eighth-step index to $20..$27; both native tables agree.</summary>
    public static ushort PartialMap(int index) => (uint)index < 16
        ? (ushort)(PauseReserveTankRomData.EmptyMap + index % 8) : throw new ArgumentOutOfRangeException(nameof(index));
}
