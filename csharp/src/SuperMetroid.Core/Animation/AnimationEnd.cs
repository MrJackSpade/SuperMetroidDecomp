namespace SuperMetroid.Core.Animation;

/// <summary>How an animation ends once its last phase has played (#1600).</summary>
internal abstract record AnimationEnd
{
    /// <summary>Allows only the endings below.</summary>
    private AnimationEnd()
    {
    }

    /// <summary>The animation is over.</summary>
    internal static AnimationEnd Stops { get; } = new Stop();

    /// <summary>The animation plays on from phase <paramref name="phase"/> again.</summary>
    internal static AnimationEnd LoopsFrom(int phase) => new Loop(Ensure.AtLeastZero(phase));

    /// <summary>The animation rests on the frame it showed last until its owner plays another.</summary>
    internal static AnimationEnd Holds { get; } = new Hold();

    /// <summary>The animation is over.</summary>
    internal sealed record Stop : AnimationEnd;

    /// <summary>The animation plays on from <paramref name="Phase"/>.</summary>
    /// <param name="Phase">The phase it plays on from.</param>
    internal sealed record Loop(int Phase) : AnimationEnd;

    /// <summary>The animation rests on its last frame until its owner plays another.</summary>
    internal sealed record Hold : AnimationEnd;
}
