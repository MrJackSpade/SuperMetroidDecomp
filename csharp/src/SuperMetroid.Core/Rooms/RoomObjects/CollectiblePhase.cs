namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Phase of one permanent-item room object.</summary>
/// <remarks>
/// VisibleTouched, ChozoOrbShot, ShotBlockShot, ShotBlockVisibleTouched and CollectedShotBlockShot are states of its course, not requests another owner stores for it (#2562): a cue, a touch or a shot ends
/// the state before, and the game acts on it one update later, the state naming where it stands until then; the update
/// that acts on it moves it on, as every other state's does. The update between is the game's.
/// </remarks>
public enum CollectiblePhase
{
    /// <summary>A previously collected exposed or orb item awaits its empty draw and immediate removal from the room.</summary>
    CollectedEmpty,
    /// <summary>The exposed item alternates its two looks and accepts a touch trigger for acquisition.</summary>
    Visible,
    /// <summary>Samus touched the exposed item; its next update collects it (#2545).</summary>
    VisibleTouched,
    /// <summary>The intact Chozo orb pulses its shell while waiting for a projectile trigger.</summary>
    ChozoOrb,
    /// <summary>A shot, bomb or grapple reached the intact orb; its next update bursts it open (#2545).</summary>
    ChozoOrbShot,
    /// <summary>The triggered Chozo orb bursts open before revealing the item.</summary>
    ChozoOrbBurst,
    /// <summary>An uncollected item is concealed in its saved shot block and waits for a projectile trigger.</summary>
    ShotBlock,
    /// <summary>A shot, bomb or grapple reached the concealing block; its next update crumbles it away (#2545).</summary>
    ShotBlockShot,
    /// <summary>The shot block crumbles away before showing the item it conceals.</summary>
    ShotBlockReveal,
    /// <summary>A previously collected item's shot block crumbles away before showing its empty space (#2482).</summary>
    CollectedShotBlockReveal,
    /// <summary>The revealed item accepts touch acquisition while it alternates its two looks twenty-two times.</summary>
    ShotBlockVisible,
    /// <summary>Samus touched the revealed item; its next update collects it (#2545).</summary>
    ShotBlockVisibleTouched,
    /// <summary>An uncollected revealed item is hidden as its shot block forms again and restores its concealed shot block.</summary>
    ShotBlockReconceal,
    /// <summary>A previously collected item's restored shot block waits for another projectile trigger.</summary>
    CollectedShotBlock,
    /// <summary>A shot, bomb or grapple reached the collected item's restored block; its next update crumbles it away (#2545).</summary>
    CollectedShotBlockShot,
    /// <summary>The collected shot block remains empty for twenty-two eight-frame intervals before respawning.</summary>
    CollectedShotBlockEmpty,
    /// <summary>The collected shot block grows back before restoring its saved block.</summary>
    CollectedShotBlockRespawn,
    /// <summary>The item has been granted and recorded collected; it keeps its place and its block while its pickup message shows.</summary>
    AwaitingMessage,
    /// <summary>The pickup message has closed: the item next shows its empty block, or leaves its shot block empty.</summary>
    ResumeAfterMessage,
    /// <summary>The empty draw has run; the sequence deletes the room object next pass.</summary>
    EmptyAwaitingDelete,
}
