using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// A station action/message request published by a room-object handler pass for the gameplay
/// frontend to consume. Save events retain the station identity needed to return
/// confirmation or message-close results to the same resident room owner.
/// </summary>
/// <param name="Kind">The station family whose activation produced this event.</param>
/// <param name="MessageBoxIndex">The gameplay message identity: map/refill completion, save confirmation, or save completion.</param>
/// <param name="AreaIndex">The room's area, used for map acquisition and save-station identity.</param>
/// <param name="SaveStation">The load station a save station saves at, which identifies the resident save station; null for
/// a map or refill station.</param>
/// <param name="Block">The parent station's room block, rather than a side-access trigger's.</param>
public readonly record struct StationActivationEvent(
    StationKind Kind,
    GameplayMessageId MessageBoxIndex,
    AreaId AreaIndex,
    LoadStation? SaveStation,
    Position16<RoomBlocks> Block)
{
    /// <summary>The load station the save station saves at, which identifies the resident station a save returns to.</summary>
    /// <exception cref="InvalidOperationException">The station is not a save station.</exception>
    public LoadStation Station => SaveStation ??
        throw new InvalidOperationException($"{Kind} station at block {Block} saves at no load station.");
}
