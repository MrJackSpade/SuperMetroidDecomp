namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>The eight Samus Eater plant draws (#1994): each plant idle and in the three looks of its chew.</summary>
internal enum SamusEaterDraw
{
    /// <summary>The floor plant idle, its mouth hidden.</summary>
    FloorIdle,
    /// <summary>The floor plant chewing, its teeth peeking out.</summary>
    FloorTeethPeeking,
    /// <summary>The floor plant chewing, its teeth bared.</summary>
    FloorTeethBared,
    /// <summary>The floor plant biting, its fangs out.</summary>
    FloorFangsOut,
    /// <summary>The ceiling plant idle, its mouth hidden.</summary>
    CeilingIdle,
    /// <summary>The ceiling plant chewing, its teeth peeking out.</summary>
    CeilingTeethPeeking,
    /// <summary>The ceiling plant chewing, its teeth bared.</summary>
    CeilingTeethBared,
    /// <summary>The ceiling plant biting, its fangs out.</summary>
    CeilingFangsOut,
}
