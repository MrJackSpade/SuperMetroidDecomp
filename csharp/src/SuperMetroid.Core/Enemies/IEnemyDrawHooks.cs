using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Enemies;

/// <summary>An enemy whose drawing adds to the common writer's.</summary>
internal interface IEnemyDrawHooks : IEnemyEntity
{
    /// <summary>The palette bits the common writer uses for the occupant, when its AI owns them; otherwise null.</summary>
    ushort? CommonDrawPaletteIndex();

    /// <summary>Draws supplemental sprites before the common writer emits the occupant's body.</summary>
    /// <param name="oam">The OAM being built.</param>
    /// <param name="camera">The camera position.</param>
    void DrawBeforeBody(OamBuffer oam, Position16<RoomPixels> camera);
}
