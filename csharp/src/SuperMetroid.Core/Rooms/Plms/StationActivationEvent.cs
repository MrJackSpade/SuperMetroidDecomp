using SuperMetroid.Core.Hud;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// A station action/message request published by a PLM handler pass for the gameplay
/// frontend to consume. Save events retain the station identity needed to return
/// confirmation or message-close results to the same resident room owner.
/// </summary>
/// <param name="Kind">The station family whose activation produced this event.</param>
/// <param name="MessageBoxIndex">The native gameplay message identity: map/refill completion, save confirmation, or save completion.</param>
/// <param name="AreaIndex">The room's area, used for map acquisition and save-station identity.</param>
/// <param name="StationIndex">The station PLM's native room argument, used with the area to identify a resident save station.</param>
/// <param name="BlockIndex">The parent station's linear foreground-block index, rather than a side-access trigger's index.</param>
public readonly record struct StationActivationEvent(
    StationKind Kind,
    GameplayMessageId MessageBoxIndex,
    AreaId AreaIndex,
    ushort StationIndex,
    int BlockIndex);
