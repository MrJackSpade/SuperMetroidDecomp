using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// The state of Fake Kraid, with its deliberately aliased bank-$7E:7800 scratch area. “Mini-Kraid” is the name used by
/// the item-drop and projectile routines; both names refer to definition <c>$E0FF</c>. The values the cartridge keeps
/// in the enemy's common record are emptied with it when the enemy is killed (<see cref="ClearWithRecord"/>).
/// </summary>
public sealed class FakeKraidEnemyState
{
    /// <summary>Where the typed animation (#641) resumes, in place of the native instruction pointer.</summary>
    internal AnimationProgramCursor<FakeKraidProgram> Program { get; set; }

    /// <summary>Signed whole-pixel walk delta.</summary>
    public short WalkDelta { get; internal set; }

    /// <summary>Signed four-pixel facing marker.</summary>
    public short FacingDelta { get; internal set; }

    /// <summary>
    /// Walk steps remaining before the next forced reversal. A step expires when its decrement reaches zero; a cleared
    /// timer wraps instead and counts down from the full word.
    /// </summary>
    public NativeCountdown WalkStepTimer { get; internal set; }

    /// <summary>Number of animation decision points before the next spit attack.</summary>
    public NativeCountdown SpitDecisionTimer { get; internal set; }

    /// <summary>
    /// Three independent spike clocks stored at scratch words three through five. Main AI
    /// visits exactly one clock per frame in the native order 0, 1, 2, 0...
    /// </summary>
    public ushort[] SpikeTimers { get; } = new ushort[3];

    /// <summary>Index 0-2 of the spike clock visited this frame (native byte offset twice this).</summary>
    public int SpikeTimerIndex { get; internal set; }


    /// <summary>Number of spike projectiles successfully allocated by this actor.</summary>
    public int SpawnedSpikeCount { get; internal set; }

    /// <summary>Number of spit projectiles successfully allocated by this actor.</summary>
    public int SpawnedSpitCount { get; internal set; }

    /// <summary>Empties the values the cartridge keeps in the enemy's common record.</summary>
    internal void ClearWithRecord()
    {
        WalkDelta = 0;
        FacingDelta = 0;
        WalkStepTimer = NativeCountdown.Start(0);
        SpitDecisionTimer = NativeCountdown.Start(0);
    }
}
