namespace SuperMetroid.Core.Game;

/// <summary>Draygon's wall-cannon control words in WRAM (#629).</summary>
[CpuBusBoundary("catalog: Draygon cannon-control WRAM")]
public static class DraygonCannonWorkRam
{
    /// <summary>
    /// <c>$7E:8802-$7E:880B</c>: the five cannon-control words, in <see cref="DraygonCannonControlWord"/>
    /// order. The $84:DC67/$DCA7 cannon instructions write one through <c>PLM_Vars</c> when a wall
    /// cannon is destroyed; bank $A5 reads them before firing.
    /// </summary>
    public static readonly CpuBusCell ControlWords = CpuBusCell.WramWords(nameof(ControlWords), DraygonCannonWorkRamAddresses.ControlWordsStart, 5,
        "cannon disabled flags (u16[] little-endian)");
}
