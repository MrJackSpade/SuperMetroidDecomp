using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// The one-frame movement selector stored in Mochtroid variable F. Main AI clears this word
/// before dispatch, so touch collision must republish <see cref="TouchingSamus"/> every frame
/// for the creature to remain latched on.
/// </summary>
public enum MochtroidMovementMode : ushort
{
    /// <summary>Uses proportional free flight toward Samus.</summary>
    NotTouchingSamus = 0,
    /// <summary>Moves one pixel per axis toward Samus while contact is republished.</summary>
    TouchingSamus = 1,
    /// <summary>Runs the shipped but unreferenced four-direction shake routine.</summary>
    Shaking = 2,
}

/// <summary>
/// Named view of the common enemy words and the three parallel extension-array words used by
/// $A3:A77D-$A9A8. Exposing these values makes the attachment clock and 16.16 steering easy to
/// inspect in C# without pretending the native arrays were part of the common $40-byte slot.
/// </summary>
public sealed class MochtroidEnemyState
{
    /// <summary>Live common enemy slot backing the velocity, shake timer, and one-frame movement selector.</summary>
    private readonly RoomEnemySlot _slot;

    /// <summary>Projects the Mochtroid's named variables over its physical enemy slot.</summary>
    internal MochtroidEnemyState(RoomEnemySlot slot) => _slot = slot;

    /// <summary>
    /// Signed 16.16 velocity: X fraction and whole words in variables A and B, Y fraction and
    /// whole words in variables C and D.
    /// </summary>
    public FixedVector<RoomPixels> Velocity
    {
        get => FixedVector<RoomPixels>.FromWords(_slot.VariableB, _slot.VariableA, _slot.VariableD, _slot.VariableC);
        internal set
        {
            _slot.VariableA = value.X.Fraction;
            _slot.VariableB = value.X.WholeWord;
            _slot.VariableC = value.Y.Fraction;
            _slot.VariableD = value.Y.WholeWord;
        }
    }

    /// <summary>Variable E; used only by the shipped-but-unreferenced shake mode.</summary>
    public NativeCountdown ShakeTimer
    {
        get => NativeCountdown.FromRaw(_slot.VariableE);
        internal set => _slot.VariableE = value.RawValue;
    }

    /// <summary>Gets the one-update movement selector cleared before each main-AI dispatch.</summary>
    public MochtroidMovementMode MovementMode
    {
        get => (MochtroidMovementMode)_slot.VariableF;
        internal set => _slot.VariableF = (ushort)value;
    }

    /// <summary>$7E:7802 parallel word used by the instruction-list change detector.</summary>
    public ushort InstalledInstructionList { get; internal set; }

    /// <summary>$7E:8800 parallel word; contact damage occurs when this reaches 80.</summary>
    public ushort AttachmentDamageTimer { get; internal set; }
}
