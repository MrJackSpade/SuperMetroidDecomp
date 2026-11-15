namespace SuperMetroid.Core.Enemies.Shaktool;

/// <summary>
/// How one of Shaktool's attack circles moves itself (#1843).
/// </summary>
internal enum ShaktoolCircleMotion
{
    /// <summary>Flies Shaktool's front circle with independent horizontal and vertical room collision.</summary>
    Leading,

    /// <summary>Flies Shaktool's middle and back circles while their owner lives.</summary>
    Trailing,
}
