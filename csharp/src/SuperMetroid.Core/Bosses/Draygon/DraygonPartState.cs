namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>What Draygon's tail or arms keep (#2177): the encounter they are part of, which his body set up before them.</summary>
/// <param name="draygon">The encounter the part joins as it sets up.</param>
internal sealed class DraygonPartState(DraygonEnemyState draygon)
{
    /// <summary>The encounter the part is part of: its animation's playback and the body that moves it.</summary>
    internal DraygonEnemyState Draygon { get; } = Ensure.NotNull(draygon);
}
