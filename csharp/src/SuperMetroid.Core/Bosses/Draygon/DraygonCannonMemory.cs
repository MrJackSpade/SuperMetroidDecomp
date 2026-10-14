namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>Typed view of Draygon's wall-cannon control words (#629).</summary>
[CpuBusBoundary("domain view: Draygon cannon-control WRAM")]
public readonly struct DraygonCannonMemory(ISnesMemoryBus bus)
{
    /// <summary>Stores the disabled flag (one) in <paramref name="word"/>, low byte first.</summary>
    public void Disable(DraygonCannonControlWord word)
    {
        new WramWords(bus, DraygonCannonWorkRam.ControlWords)[(int)word].Write(1);
    }
}
