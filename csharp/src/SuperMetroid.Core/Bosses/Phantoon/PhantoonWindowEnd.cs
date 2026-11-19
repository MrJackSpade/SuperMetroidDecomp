namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>
/// What follows Phantoon's vulnerable window when it closes (#1933, #2562): his eye closing, unless a shot has hit him in
/// the window, which has him swoop at Samus instead. Each window starts closing his eye; the first hit chooses the swoop.
/// </summary>
public enum PhantoonWindowEnd
{
    /// <summary>His eye closes as the window ends.</summary>
    EyeCloses,
    /// <summary>A shot hit him in the window: he swoops at Samus as it ends.</summary>
    Swoop,
}
