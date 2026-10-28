using static SuperMetroid.Core.Bosses.Draygon.DraygonIntroDanceDefinitions;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>Draygon's opening Evir dance: each sprite's stream latency and the movement it reads from the traced path.</summary>
internal static class DraygonIntroDance
{
    /// <summary>Returns the signed stream latency for native sprite slot 28 through 31.</summary>
    /// <remarks>
    /// Independently reviewed for #1165 against NTSC J/U v1.0 and pinned bank_A5.asm:
    /// the four reachable signed words are the arithmetic progression 128*(slot-35).
    /// The caller adds this byte-stream latency with 16-bit wrap, then tests the sign.
    /// Keep the first four native words (including the 128-frame initial delay); the
    /// unused following four words do not extend the domain. Packed motion is separate.
    /// </remarks>
    public static short MovementLatencyForSlot(int slotIndex)
    {
        int index = slotIndex - 28;
        if ((uint)index >= 4)
            throw new ArgumentOutOfRangeException(nameof(slotIndex));
        return (short)(128 * (slotIndex - 35));
    }

    /// <summary>
    /// Resolves one supported, four-byte-aligned Evir movement record from the
    /// compiled <c>$A5:CE07-$A5:DF44</c> trajectory. The native 1,232-frame owner can
    /// address these 1,104 records after applying the four sprite latencies; active
    /// sprites stop consuming movement at the first delete command. Preserve the full
    /// existing decoder domain, including its post-delete continuation.
    /// A restored index outside that domain is corrupt state, not permission to interpret
    /// adjacent executable or presentation bytes as signed movement.
    /// </summary>
    public static DraygonIntroMovement ResolveMovement(int streamOffset)
    {
        if ((streamOffset & 3) != 0 || streamOffset is < 0 or > LastMovementStreamOffset)
        {
            throw new InvalidDataException(
                $"Draygon intro movement offset ${streamOffset:X4} is outside the compiled " +
                "four-byte-aligned retail trajectory.");
        }

        if (streamOffset is FirstDeleteOffset or FirstDeleteOffset + StreamIndexAdvance)
            return new DraygonIntroMovement(Offset16<RoomPixels>.Zero, DeletesSprite: true);

        int index = streamOffset / StreamIndexAdvance;
        if (streamOffset > FirstDeleteOffset) index -= 2;
        int packed = PackedMovement[index];

        return new DraygonIntroMovement(
            new((short)((packed >> MovementXShift) - MovementBias), (short)((packed & MovementComponentMask) - MovementBias)),
            DeletesSprite: false);
    }
}
