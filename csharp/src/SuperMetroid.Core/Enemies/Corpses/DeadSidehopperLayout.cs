namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>Which of the two Tourian Sidehopper placements a dead Sidehopper is (#1519).</summary>
public enum DeadSidehopperLayout
{
    /// <summary>The living Sidehopper the baby Metroid drains, which waits for the camera before it hops in.</summary>
    ShitroidVictim,

    /// <summary>A Sidehopper already dead when the room loads, which rots when Samus touches it.</summary>
    AlreadyDead,
}
