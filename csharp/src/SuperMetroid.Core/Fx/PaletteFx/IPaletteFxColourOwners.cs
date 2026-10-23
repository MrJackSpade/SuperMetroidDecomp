using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>The colours palette-FX objects write, by owner (#1405).</summary>
public interface IPaletteFxColourOwners
{
    /// <summary>All of <paramref name="owner"/>'s colours.</summary>
    ColourRun Colours(PaletteFxColourOwner owner);
}
