using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// Debugger-facing projection of Fake Kraid's ordinary enemy words and its deliberately
/// aliased bank-$7E:7800 scratch area. “Mini-Kraid” is the name used by the item-drop and
/// projectile routines; both names refer to definition <c>$E0FF</c>.
/// </summary>
public sealed class FakeKraidEnemyState
{
    /// <summary>Physical enemy slot backing Fake Kraid's movement and attack timers.</summary>
    private readonly RoomEnemySlot _slot;

    /// <summary>Projects Mini-Kraid's ordinary enemy variables from its occupied slot.</summary>
    internal FakeKraidEnemyState(RoomEnemySlot slot) => _slot = slot;

    /// <summary>Where the typed animation (#641) resumes, in place of the native instruction pointer.</summary>
    internal AnimationProgramCursor<FakeKraidProgram> Program { get; set; }

    /// <summary>Signed whole-pixel walk delta in native variable B.</summary>
    public short WalkDelta
    {
        get => unchecked((short)_slot.VariableB);
        internal set => _slot.VariableB = unchecked((ushort)value);
    }

    /// <summary>Signed four-pixel facing marker in native variable C.</summary>
    public short FacingDelta
    {
        get => unchecked((short)_slot.VariableC);
        internal set => _slot.VariableC = unchecked((ushort)value);
    }

    /// <summary>Frames/steps remaining before the next forced walk reversal.</summary>
    public ushort WalkStepTimer
    {
        get => _slot.VariableD;
        internal set => _slot.VariableD = value;
    }

    /// <summary>Number of animation decision points before the next spit attack.</summary>
    public NativeCountdown SpitDecisionTimer
    {
        get => NativeCountdown.FromRaw(_slot.VariableE);
        internal set => _slot.VariableE = value.RawValue;
    }

    /// <summary>
    /// Three independent spike clocks stored at scratch words three through five. Main AI
    /// visits exactly one clock per frame in the native order 0, 1, 2, 0...
    /// </summary>
    public ushort[] SpikeTimers { get; } = new ushort[3];

    /// <summary>Byte offset zero/two/four selecting the spike clock visited this frame.</summary>
    public ushort SpikeTimerByteOffset { get; internal set; }

    /// <summary>Most recently selected spike row, or <c>null</c> before the first spawn.</summary>
    public int? LastSpikeRow { get; internal set; }

    /// <summary>Number of spike projectiles successfully allocated by this actor.</summary>
    public int SpawnedSpikeCount { get; internal set; }

    /// <summary>Number of spit projectiles successfully allocated by this actor.</summary>
    public int SpawnedSpitCount { get; internal set; }
}
