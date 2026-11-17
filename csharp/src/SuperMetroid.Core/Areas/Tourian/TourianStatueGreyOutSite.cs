namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>
/// Where a released Tourian statue's grey-out shows (#2288): the Kraid and Draygon statues are drawn in the room's
/// background, so their grey-out recolours the room's pictures; the Ridley and Phantoon statues are the entrance statue's
/// sprites, which show their grey-out on their own draws. Its values are arbitrary.
/// </summary>
public enum TourianStatueGreyOutSite
{
    /// <summary>The statue is drawn in the room's background, whose pictures show its grey-out.</summary>
    RoomBackground,

    /// <summary>The statue is one of the entrance statue's sprites, which show its grey-out.</summary>
    EntranceStatue,
}
