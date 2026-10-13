namespace SuperMetroid.Core.Game;

/// <summary>
/// Kraid's physical foot components for the initial frame at $A7:A565 and
/// the 35 selected extended frames at $A7:8CE3-$A7:8F47. The OAM records are presentation;
/// these offsets and the shared $A7:9453 hitbox list govern collision.
/// </summary>
internal static class KraidFootCollisionDefinitions
{
    /// <summary>Bank containing the Kraid foot's extended frame and shared hitbox records.</summary>
    internal const byte Bank = 0xa7;

    /// <summary>First authored Kraid-foot extended frame at $A7:8CE3.</summary>
    internal const ushort FirstFrame = 0x8ce3;

    /// <summary>Small-Kraid initial foot frame at $A7:A565; physically the first walk frame.</summary>
    internal const ushort InitialFrame = 0xa565;

    /// <summary>The shared single-rectangle hitbox list at $A7:9453.</summary>
    internal const ushort HitboxList = 0x9453;

    /// <summary>Each two-component frame occupies 18 bytes in bank $A7.</summary>
    private const int FrameByteCount = 18;

    /// <summary>Number of authored extended foot frames in the walk sequence.</summary>
    internal const int FrameCount = 35;

    /// <summary>The fixed component moves two pixels left per frame, then retraces.</summary>
    internal static short FirstX(int frame)
    {
        CheckFrame(frame);
        return (short)(8 - 2 * Math.Min(frame, 34 - frame));
    }

    /// <summary>The fixed component rises, turns at native pose A/B, then descends and holds.</summary>
    internal static short FirstY(int frame)
    {
        CheckFrame(frame);
        return (short)(frame < 10 ? 40 - frame : frame == 10 ? 29 : frame < 18 ? 28 + 2 * (frame - 11) : 40);
    }

    /// <summary>Moving-foot horizontal placement within native poses0,1,2,3 and the return sequence.</summary>
    /// <remarks>Independently reviewed for #1165 against bank_A7.asm. Pose changes
    /// occur at frames5,10,11,21,23,31; frame17 reverses travel. Each pose's
    /// placement advances by two pixels. Pose2 at frame10 is the raised transition.</remarks>
    internal static short SecondX(int frame)
    {
        CheckFrame(frame);
        return (short)(frame switch
        {
            < 5 => -2 * frame,
            < 10 => 5 - 2 * (frame - 5),
            10 => 12,
            < 18 => 26 - 2 * (frame - 11),
            < 21 => 16 + 2 * (frame - 18),
            < 23 => 6 + 2 * (frame - 21),
            < 31 => -7 + 2 * (frame - 23),
            _ => -6 + 2 * (frame - 31),
        });
    }

    /// <summary>Moving-foot vertical placement: rise, raised pose2, descent, then pose-specific holds.</summary>
    internal static short SecondY(int frame)
    {
        CheckFrame(frame);
        return (short)(frame switch
        {
            < 5 => -frame,
            < 10 => -15 - (frame - 5),
            10 => -26,
            < 18 => -23 + 2 * (frame - 11),
            < 21 => -10,
            < 23 => -15,
            < 31 => -10,
            _ => 0,
        });
    }

    /// <summary>Rejects an extended foot-frame index outside the authored walk sequence.</summary>
    private static void CheckFrame(int frame)
    {
        if ((uint)frame >= FrameCount) throw new IndexOutOfRangeException();
    }

    /// <summary>Maps an initial or 18-byte extended frame pointer to its two collision components.</summary>
    internal static bool TryGetComponents(ushort pointer,
        out KraidFootComponentSequence components)
    {
        if (pointer == InitialFrame)
        {
            components = new(0, 2);
            return true;
        }

        int distance = pointer - FirstFrame;
        if (distance < 0 || distance % FrameByteCount != 0 ||
            distance / FrameByteCount >= FrameCount)
        {
            components = default;
            return false;
        }

        components = new(distance / FrameByteCount, 2);
        return true;
    }

    /// <summary>$A7:9453 contains one centered six-pixel-radius rectangle with background touch/no-op shot.</summary>
    internal static KraidFootHitboxSequence HitboxesAt(ushort pointer) =>
        pointer == HitboxList ? new(1) : throw new InvalidDataException(
            $"Kraid-foot hitbox list $A7:{pointer:X4} is not compiled.");
}

/// <summary>Two physical components calculated on access; an unsuccessful lookup returns an empty sequence.</summary>
internal readonly record struct KraidFootComponentSequence(int Frame, int Length)
{
    /// <summary>Calculates the fixed or moving component's offset with their shared hitbox list.</summary>
    internal KraidFootCollisionComponent this[int index]
    {
        get
        {
            if ((uint)index >= (uint)Length) throw new IndexOutOfRangeException();
            return index == 0
                ? new(new(KraidFootCollisionDefinitions.FirstX(Frame), KraidFootCollisionDefinitions.FirstY(Frame)), KraidFootCollisionDefinitions.HitboxList)
                : new(new(KraidFootCollisionDefinitions.SecondX(Frame), KraidFootCollisionDefinitions.SecondY(Frame)), KraidFootCollisionDefinitions.HitboxList);
        }
    }
    /// <summary>Enumerates the calculated components without a stored frame array.</summary>
    public Enumerator GetEnumerator() => new(this);
    /// <summary>Cursor over Kraid foot physical components.</summary>
    internal struct Enumerator(KraidFootComponentSequence sequence)
    {
        /// <summary>Index of the next component in the two-part frame.</summary>
        private int next;
        /// <summary>Advances through the frame's bounded component sequence.</summary>
        public bool MoveNext() => next++ < sequence.Length;
        /// <summary>Most recently selected fixed or moving foot component.</summary>
        public KraidFootCollisionComponent Current => sequence[next - 1];
    }
}

/// <summary>Direct construction of the single shared rectangle without a stored hitbox lookup.</summary>
internal readonly record struct KraidFootHitboxSequence(int Length)
{
    /// <summary>Constructs the shared centered rectangle and its background-touch/no-op-shot callbacks.</summary>
    internal KraidFootCollisionHitbox this[int index]
    {
        get
        {
            if ((uint)index >= (uint)Length) throw new IndexOutOfRangeException();
            return new(-6, -6, 6, 6, EnemyAiCodePointers.BankA7.KraidBackgroundTouch,
                EnemyAiCodePointers.BankA7.KraidNoOpShot);
        }
    }
    /// <summary>Enumerates the one shared Kraid foot hitbox.</summary>
    public Enumerator GetEnumerator() => new(this);
    /// <summary>Cursor over the shared physical rectangle.</summary>
    internal struct Enumerator(KraidFootHitboxSequence sequence)
    {
        /// <summary>Index of the next rectangle to expose.</summary>
        private int next;
        /// <summary>Advances once for the one-rectangle hitbox list.</summary>
        public bool MoveNext() => next++ < sequence.Length;
        /// <summary>The one shared rectangle after the cursor advances.</summary>
        public KraidFootCollisionHitbox Current => sequence[next - 1];
    }
}
/// <summary>One foot part's room offset and shared collision-hitbox pointer.</summary>
internal readonly record struct KraidFootCollisionComponent(Offset16<RoomPixels> Offset, ushort HitboxPointer);

/// <summary>One rectangle's signed extents and touch/shot AI addresses.</summary>
internal readonly record struct KraidFootCollisionHitbox(
    short Left, short Top, short Right, short Bottom, ushort TouchAi, ushort ShotAi);
