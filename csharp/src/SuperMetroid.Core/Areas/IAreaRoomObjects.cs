using SuperMetroid.Core.Areas.Tourian;

namespace SuperMetroid.Core.Areas;

/// <summary>
/// The area PLM families a room's PLM owner runs (#1395), which area PLM pre-instructions reach from their step context.
/// The room composition implements it.
/// </summary>
internal interface IAreaRoomObjects
{
    /// <summary>Runs Tourian room object instructions, including encounter scenery.</summary>
    TourianObjects Tourian { get; }
}
