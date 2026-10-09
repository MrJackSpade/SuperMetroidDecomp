using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Runtime;

public sealed partial class SuperMetroidRuntime
{
    /// <summary>Installed character art is host-owned and rebound after debugger-state restoration.</summary>
    [NonSerialized] private RoomCharacterAtlasCatalog? roomCharacterArt;

    /// <summary>Gets or binds the host-owned CRE and graphics-set character catalog used by subsequent room loads; the reference is excluded from debugger snapshots.</summary>
    public RoomCharacterAtlasCatalog? RoomCharacterArt
    {
        get => roomCharacterArt;
        set => roomCharacterArt = value;
    }
}
