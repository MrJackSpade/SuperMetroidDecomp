namespace SuperMetroid.Core.Runtime;

/// <summary>
/// Room-main scratch word <c>RoomMainASMVar1</c> ($07E1), shared by every room main that uses it.
/// </summary>
/// <remarks>
/// Room loading never clears this word, so a room whose code does not initialize it inherits
/// the previous owner's value. The Ceres pre-elevator hall's debris timer ($8F:E525) starts
/// from the elevator shaft's rotation index ($8F:E509/$89:AD41), and the Maridia elevatube's
/// door callbacks ($8F:E26C/$E291) leave the fraction of its tracked position stale. The
/// other room-main words ($07E3-$07E7) are written by each owner before they are read, so
/// they stay with their owners.
/// </remarks>
public sealed class RoomMainScratchState
{
    /// <summary>WRAM <c>$07E1</c>.</summary>
    public ushort Var1 { get; set; }

    /// <summary>
    /// WRAM <c>$07E1</c> read as the wrapping countdown the timed and screen-shaking room-main routines keep
    /// in it; other routines use the same word as an index or position fraction through <see cref="Var1"/>.
    /// </summary>
    public NativeCountdown Countdown
    {
        get => NativeCountdown.FromRaw(Var1);
        set => Var1 = value.RawValue;
    }
}
