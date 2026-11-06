namespace SuperMetroid.Core.Samus.Movement;

/// <summary>The five square slope shapes: which of a block's four quarters are solid before the block's mirroring.</summary>
public enum SquareSlopeShape
{
    /// <summary>The bottom half: both lower quarters.</summary>
    BottomHalf,

    /// <summary>The right half: both right quarters.</summary>
    RightHalf,

    /// <summary>The bottom-right quarter alone.</summary>
    BottomRightQuarter,

    /// <summary>Every quarter but the top-left one.</summary>
    AllButTopLeft,

    /// <summary>The whole block.</summary>
    Full,
}
