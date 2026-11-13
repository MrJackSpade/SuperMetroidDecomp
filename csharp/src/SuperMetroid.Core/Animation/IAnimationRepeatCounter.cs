namespace SuperMetroid.Core.Animation;

/// <summary>The repeat counter an animation's counted phases load and count down (#1600), wherever its owner keeps it.</summary>
internal interface IAnimationRepeatCounter
{
    /// <summary>The counter's value.</summary>
    WrappingCountdown Repeats { get; set; }
}
