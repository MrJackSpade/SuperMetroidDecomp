namespace SuperMetroid.Core.Frontend.Title;

/// <summary>A pose of the Baby Metroid lying on the title's turning floor, which its pulse animation shows in turn.</summary>
public enum TitleBabyPose
{
    /// <summary>Closed pose at the start of the pulse.</summary>
    Closed,

    /// <summary>Intermediate pose shown on both sides of the pulse.</summary>
    Middle,

    /// <summary>Open pose at the pulse's peak.</summary>
    Open,
}
