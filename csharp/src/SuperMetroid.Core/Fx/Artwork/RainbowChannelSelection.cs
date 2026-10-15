namespace SuperMetroid.Core.Fx.Artwork;

/// <summary>Which colour channels a shared rainbow body palette varies, and the red and green additions (#633).</summary>
internal sealed record RainbowChannelSelection
{
    /// <summary>Red.</summary>
    public required bool Red { get; init; }

    /// <summary>Green.</summary>
    public required bool Green { get; init; }

    /// <summary>Blue.</summary>
    public required bool Blue { get; init; }

    /// <summary>Red addition.</summary>
    public required int RedAddition { get; init; }

    /// <summary>Green addition.</summary>
    public required int GreenAddition { get; init; }
}
