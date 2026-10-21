using SuperMetroid.Core.Enemies.Shaktool;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The enemy PLM families a room's PLM owner runs (#1395), which enemy PLM pre-instructions reach from their step context.
/// The room composition implements it.
/// </summary>
internal interface IEnemyRoomPlms
{
    /// <summary>Coordinates Shaktool's tunnel digging and block replacement PLMs.</summary>
    ShaktoolPlms Shaktool { get; }
}
