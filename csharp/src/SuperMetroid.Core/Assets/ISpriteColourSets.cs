namespace SuperMetroid.Core.Assets;

/// <summary>Sprite colour sets a room's graphics set loads its enemies' colours into (#1405): gameplay's, or a screen's.</summary>
public interface ISpriteColourSets
{
    /// <summary>Set <paramref name="set"/>, zero through seven.</summary>
    ColourRun Set(int set);
}
