namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Phase of one permanent-item room object.</summary>
public enum CollectiblePhase
{
    /// <summary>A previously collected exposed or orb item awaits its empty draw and immediate removal from the room.</summary>
    CollectedEmpty,
    /// <summary>The exposed item alternates its two looks and accepts a touch trigger for acquisition.</summary>
    Visible,
    /// <summary>The intact Chozo orb pulses its shell while waiting for a projectile trigger.</summary>
    ChozoOrb,
    /// <summary>The triggered Chozo orb bursts open before revealing the item.</summary>
    ChozoOrbBurst,
    /// <summary>An uncollected item is concealed in its saved shot block and waits for a projectile trigger.</summary>
    ShotBlock,
    /// <summary>The shot block crumbles away before showing either the item or its already-collected empty space.</summary>
    ShotBlockReveal,
    /// <summary>The revealed item accepts touch acquisition while it alternates its two looks twenty-two times.</summary>
    ShotBlockVisible,
    /// <summary>An uncollected revealed item is hidden as its shot block forms again and restores its concealed shot block.</summary>
    ShotBlockReconceal,
    /// <summary>A previously collected item's restored shot block waits for another projectile trigger.</summary>
    CollectedShotBlock,
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
