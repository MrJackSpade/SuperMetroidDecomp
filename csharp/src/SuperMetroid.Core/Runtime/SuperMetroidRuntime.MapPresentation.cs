using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Runtime;

public sealed partial class SuperMetroidRuntime
{
    /// <summary>External map content supplied by the host; never serialized into a debugger snapshot.</summary>
    [field: NonSerialized]
    public AreaMapPresentationCatalog? MapPresentation { get; set; }
}
