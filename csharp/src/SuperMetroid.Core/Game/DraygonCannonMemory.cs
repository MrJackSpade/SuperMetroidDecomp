using SuperMetroid.Core.Hardware;

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
    public static readonly CpuBusCell ControlWords = CpuBusCell.WramWords(nameof(ControlWords), 0x7e8802, 5,
        "cannon disabled flags (u16[] little-endian)");
}

/// <summary>Typed view of Draygon's wall-cannon control words (#629).</summary>
[CpuBusBoundary("domain view: Draygon cannon-control WRAM")]
public readonly struct DraygonCannonMemory(ISnesMemoryBus bus)
{
    /// <summary>Stores the disabled flag (one) in <paramref name="word"/>, low byte first.</summary>
    public void Disable(DraygonCannonControlWord word)
    {
        int index = ((ushort)word - SnesAddress.FromBusAddress(DraygonCannonWorkRam.ControlWords.Address).Offset) / sizeof(ushort);
        new WramWords(bus, DraygonCannonWorkRam.ControlWords)[index].Write(1);
    }
}
