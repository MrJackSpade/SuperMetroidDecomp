namespace SuperMetroid.Core.Samus.Grapple;

/// <summary>When a grapple step applies its pose change.</summary>
public enum GrapplePoseChange
{
    /// <summary>During the step.</summary>
    Immediate,

    /// <summary>Later in the frame, after the grapple's own step.</summary>
    Deferred,
}
