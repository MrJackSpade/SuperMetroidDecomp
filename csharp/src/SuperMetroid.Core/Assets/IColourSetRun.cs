namespace SuperMetroid.Core.Assets;

/// <summary>A run of one of gameplay's colour sets, background or sprite (#1405), named by set, colour and count.</summary>
public interface IColourSetRun
{
    /// <summary>These colours of <paramref name="colours"/>.</summary>
    ColourRun In(GameplayColours colours);
}
