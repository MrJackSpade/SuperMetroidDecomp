using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend;

public sealed partial class SuperMetroidGame
{
    [NonSerialized] private RoomCharacterAtlas? standardObjectArt;

    /// <summary>Attach installed common sprites before gameplay or after state restoration.</summary>
    public void BindStandardObjectArt(RoomCharacterAtlas? atlas)
    {
        standardObjectArt = atlas;
        if (runtime is not null) runtime.StandardObjectArt = atlas;
    }
}
