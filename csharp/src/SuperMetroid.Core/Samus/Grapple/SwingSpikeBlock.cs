namespace SuperMetroid.Core.Samus.Grapple;

/// <summary>The kind of spike block a grapple swing touched.</summary>
public enum SwingSpikeBlock
{
    /// <summary>Spike air, which the swing passes through.</summary>
    Plain,

    /// <summary>A solid spike block, which deals its own damage.</summary>
    Solid,
}
