namespace SuperMetroid.Core.Samus.Grapple;

/// <summary>When a grapple step applies its pose change.</summary>
public enum GrapplePoseChange
{
    /// <summary>During the step.</summary>
    Immediate,

    /// <summary>At the caller's later point in the frame.</summary>
    Deferred,
}
