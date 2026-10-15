using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>
/// Verification read view of Draygon's wall-cannon control words (#629). Production only writes
/// them through <see cref="DraygonCannonMemory"/>; tests read the published words back here.
/// </summary>
[CpuBusBoundary("domain view: verification reads of Draygon cannon-control WRAM")]
internal readonly struct DraygonCannonVerificationMemory(ISnesMemoryBus bus)
{
    /// <summary>The control word for <paramref name="word"/>: one once that cannon is disabled.</summary>
    internal ushort ControlWord(DraygonCannonControlWord word)
    {
        return new WramWords(bus, DraygonCannonWorkRam.ControlWords)[(int)word].Read();
    }
}
