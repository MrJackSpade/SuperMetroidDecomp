using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Frontend;

public sealed partial class SuperMetroidGame
{
    /// <summary>Optional installed artwork catalog used to draw room X-ray reveal overlays.</summary>
    [NonSerialized] private XrayRevealVisualCatalog? xrayRevealVisuals;

    /// <summary>Attaches installed X-ray art after construction or state restoration.</summary>
    public void BindXrayRevealVisuals(XrayRevealVisualCatalog? catalog)
    {
        xrayRevealVisuals = catalog;
        if (runtime is not null) runtime.XrayRevealVisuals = catalog;
    }
}
