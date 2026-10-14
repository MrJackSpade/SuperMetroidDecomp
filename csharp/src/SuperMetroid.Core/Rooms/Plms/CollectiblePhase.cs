namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Debugger-visible phase of one translated permanent-item PLM.</summary>
public enum CollectiblePhase : byte
{
    /// <summary>A previously collected exposed or orb item awaits its empty draw and immediate slot deletion.</summary>
    CollectedEmpty,
    /// <summary>The exposed item alternates its two visible frames and accepts a touch trigger for acquisition.</summary>
    Visible,
    /// <summary>The intact Chozo orb cycles its four shell frames while waiting for a projectile trigger.</summary>
    ChozoOrb,
    /// <summary>The triggered Chozo orb runs its timed empty/burst draws before revealing the item.</summary>
    ChozoOrbBurst,
    /// <summary>An uncollected item is concealed in its saved shot block and waits for a projectile trigger.</summary>
    ShotBlock,
    /// <summary>The shot block runs three reveal frames before showing either the item or its already-collected empty space.</summary>
    ShotBlockReveal,
    /// <summary>The revealed item accepts touch acquisition during twenty-two pairs of four-frame animation draws.</summary>
    ShotBlockVisible,
    /// <summary>An uncollected revealed item runs the reverse reveal sequence and restores its concealed shot block.</summary>
    ShotBlockReconceal,
    /// <summary>A previously collected item's restored shot block waits for another projectile trigger.</summary>
    CollectedShotBlock,
    /// <summary>The collected shot block remains empty for twenty-two eight-frame intervals before respawning.</summary>
    CollectedShotBlockEmpty,
    /// <summary>The collected shot block runs three four-frame respawn draws before restoring its saved level word.</summary>
    CollectedShotBlockRespawn,
    /// <summary>Inventory and persistence have been granted; the physical slot and drawn tile remain while the synchronous pickup message runs.</summary>
    AwaitingMessage,
    /// <summary>The pickup message has returned and the suspended item list is ready for its empty draw or shot-block continuation.</summary>
    ResumeAfterMessage,
    /// <summary>The empty draw has run; the instruction list deletes the PLM next pass.</summary>
    EmptyAwaitingDelete,
}
