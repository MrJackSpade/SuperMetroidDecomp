using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Runtime;

public sealed partial class SuperMetroidRuntime
{
    /// <summary>Installed character art is host-owned and rebound after debugger-state restoration.</summary>
    [NonSerialized] private RoomCharacterAtlasCatalog? roomCharacterArt;

    public RoomCharacterAtlasCatalog? RoomCharacterArt
    {
        get => roomCharacterArt;
        set => roomCharacterArt = value;
    }
}
