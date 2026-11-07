namespace SuperMetroid.Core.Progression;

/// <summary>How many Zebetites Samus has destroyed, as the Zebetites read it when they spawn and advance it when one falls.</summary>
public interface IZebetiteProgress
{
    /// <summary>The Zebetites destroyed, 0 through <see cref="PersistentProgress.ZebetiteCount"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">A value outside that range is written.</exception>
    int ZebetitesDestroyed { get; set; }
}
