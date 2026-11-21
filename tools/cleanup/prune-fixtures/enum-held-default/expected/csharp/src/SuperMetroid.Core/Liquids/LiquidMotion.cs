namespace SuperMetroid.Core.Liquids;

/// <summary>Where a room's liquid's motion stands: awaiting its first pass, as every new motion does, or moving.</summary>
public readonly record struct LiquidMotion
{
    /// <summary>The motion's stage, which a new motion holds at its default.</summary>
    private readonly LiquidMotionStage stage;

    /// <summary>A motion at <paramref name="stage"/>.</summary>
    private LiquidMotion(LiquidMotionStage stage) => this.stage = stage;

    /// <summary>A liquid whose motion has begun.</summary>
    public static LiquidMotion Moving { get; } = new(LiquidMotionStage.Moving);

    /// <summary>Whether the motion has begun.</summary>
    public bool IsMoving => stage is LiquidMotionStage.Moving;

    /// <summary>The stages of a liquid's motion.</summary>
    private enum LiquidMotionStage
    {
        /// <summary>The next pass only begins the motion: the stage every new motion holds, which no code names.</summary>
        AwaitingFirstPass,

        /// <summary>Each pass moves the liquid.</summary>
        Moving,
    }
}
