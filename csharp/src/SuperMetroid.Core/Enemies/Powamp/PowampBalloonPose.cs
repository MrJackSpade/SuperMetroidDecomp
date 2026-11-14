namespace SuperMetroid.Core.Enemies.Powamp;

/// <summary>How far a Powamp's balloon is inflated: each pose has its own picture and sits at its own height above the body.</summary>
public enum PowampBalloonPose
{
    /// <summary>Deflated, sitting lowest.</summary>
    Deflated,
    /// <summary>Halfway inflated.</summary>
    Halfway,
    /// <summary>Fully inflated, sitting highest.</summary>
    Full,
}
