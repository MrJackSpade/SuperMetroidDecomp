using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Runtime;

public sealed partial class SuperMetroidRuntime
{
    /// <summary>Gets or binds the host-owned CRE and graphics-set character catalog used by subsequent room loads; the reference is excluded from debugger snapshots.</summary>
    [field: NonSerialized]
    public RoomCharacterAtlasCatalog? RoomCharacterArt { get; set; }
}
