namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>The state of Crocomire's tongue actor.</summary>
public sealed class CrocomireTongueState
{
    /// <summary>Starts the tongue's state as part of <paramref name="crocomire"/>, the encounter its body set up.</summary>
    internal CrocomireTongueState(CrocomireEnemyState crocomire) => Crocomire = Ensure.NotNull(crocomire);

    /// <summary>The encounter the tongue is part of (#2177), which its body set up before it.</summary>
    public CrocomireEnemyState Crocomire { get; }
}
