using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Enemies;

/// <summary>The persistent event bits and one area's boss bits, as a room's PLMs and enemies read and write them.</summary>
/// <param name="system">The session's progression owner.</param>
/// <param name="area">The room's area, whose boss bits these address.</param>
internal sealed class RoomAreaProgress(SharedGameState system, AreaId area) : IGlobalEventFlags, IAreaBossFlags
{
    /// <summary>Checks a persistent event bit from the system owner.</summary>
    public bool Has(EventNumber eventNumber) => system.HasEvent(eventNumber);

    /// <summary>Publishes an event bit through the system owner.</summary>
    public void Set(EventNumber eventNumber) => system.SetEvent(eventNumber);

    /// <summary>Clears an event bit through the system owner.</summary>
    public void Clear(EventNumber eventNumber) => system.ClearEvent(eventNumber);

    /// <summary>Checks whether any requested boss bit is set for this room's area.</summary>
    public bool IsDefeated(BossBits bits) => system.HasAnyBossBits(area, bits);

    /// <summary>Sets boss bits in this room's area record.</summary>
    public void SetDefeated(BossBits bits) => system.SetBossBits(area, bits);
}
