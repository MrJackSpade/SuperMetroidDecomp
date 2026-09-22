using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend;

public sealed partial class SuperMetroidGame
{
    [NonSerialized] private RoomCharacterAtlasCatalog? roomCharacterArt;

    /// <summary>Attaches installed room artwork after construction or debugger-state restoration.</summary>
    public void BindRoomCharacterArt(RoomCharacterAtlasCatalog? catalog)
    {
        roomCharacterArt = catalog;
        if (runtime is not null) runtime.RoomCharacterArt = catalog;
    }
}
