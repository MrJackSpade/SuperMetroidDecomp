namespace SuperMetroid.Core.Game;

/// <summary>
/// Kraid's physical foot components for the initial frame at $A7:A565 and
/// the 35 selected extended frames at $A7:8CE3-$A7:8F47. The OAM records are presentation;
/// these offsets and the shared $A7:9453 hitbox list govern collision.
/// </summary>
internal static class KraidFootCollisionDefinitions
{
    /// <summary>ROM bank containing Kraid's foot animation and collision records.</summary>
    internal const byte Bank = 0xa7;

    /// <summary>First authored Kraid-foot extended frame at $A7:8CE3.</summary>
    internal const ushort FirstFrame = 0x8ce3;

    /// <summary>Small-Kraid initial foot frame at $A7:A565; physically the first walk frame.</summary>
    internal const ushort InitialFrame = 0xa565;

    /// <summary>The shared single-rectangle hitbox list at $A7:9453.</summary>
    internal const ushort HitboxList = 0x9453;

    /// <summary>Each two-component frame occupies 18 bytes in bank $A7.</summary>
    private const int FrameByteCount = 18;

    /// <summary>Number of extended foot-animation frames addressable from <see cref="FirstFrame"/>.</summary>
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

    /// <summary>Rejects frame indices outside the compiled extended animation range.</summary>
    private static void CheckFrame(int frame)
    {
        if ((uint)frame >= FrameCount) throw new IndexOutOfRangeException();
    }

    /// <summary>Maps a native frame pointer to its two calculated foot components; the initial pose maps to frame zero.</summary>
    /// <param name="pointer">Bank-$A7 pointer supplied by the enemy animation.</param>
    /// <param name="components">Receives the indexed component sequence when the pointer is recognized, or an empty sequence otherwise.</param>
    /// <returns><see langword="true"/> when the pointer identifies the initial pose or an aligned compiled frame.</returns>
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
/// <summary>Provides bounds-checked access to the two calculated foot components of one animation frame.</summary>
/// <param name="Frame">Zero-based extended animation frame whose component offsets are evaluated on access.</param>
/// <param name="Length">Number of components exposed by this sequence; zero represents an unsuccessful pointer lookup.</param>
internal readonly record struct KraidFootComponentSequence(int Frame, int Length)
{
    /// <summary>Gets the requested foot component, evaluating its frame-relative offset and shared hitbox pointer.</summary>
    /// <param name="index">Zero for the fixed component or one for the moving component.</param>
    /// <exception cref="IndexOutOfRangeException">The index is outside this sequence's <see cref="Length"/>.</exception>
    internal KraidFootCollisionComponent this[int index]
    {
        get
        {
            if ((uint)index >= (uint)Length) throw new IndexOutOfRangeException();
            return index == 0
                ? new(KraidFootCollisionDefinitions.FirstX(Frame), KraidFootCollisionDefinitions.FirstY(Frame), KraidFootCollisionDefinitions.HitboxList)
                : new(KraidFootCollisionDefinitions.SecondX(Frame), KraidFootCollisionDefinitions.SecondY(Frame), KraidFootCollisionDefinitions.HitboxList);
        }
    }
    /// <summary>Creates a value-type enumerator over the available foot components.</summary>
    public Enumerator GetEnumerator() => new(this);

    /// <summary>Iterates a component sequence without allocating an iterator object.</summary>
    /// <param name="sequence">Sequence whose components are returned in physical component order.</param>
    internal struct Enumerator(KraidFootComponentSequence sequence)
    {
        /// <summary>Index of the next component to yield.</summary>
        private int next;

        /// <summary>Advances to the next component and reports whether the sequence has another item.</summary>
        public bool MoveNext() => next++ < sequence.Length;

        /// <summary>Gets the component most recently selected by <see cref="MoveNext"/>.</summary>
        public KraidFootCollisionComponent Current => sequence[next - 1];
    }
}

/// <summary>Direct construction of the single shared rectangle without a stored hitbox lookup.</summary>
/// <summary>Provides bounds-checked access to Kraid's shared centered foot hitbox definition.</summary>
/// <param name="Length">Number of hitbox entries exposed by this sequence.</param>
internal readonly record struct KraidFootHitboxSequence(int Length)
{
    /// <summary>Gets the centered rectangle and its touch/shot AI pointers at the requested index.</summary>
    /// <param name="index">Zero-based hitbox entry index.</param>
    /// <exception cref="IndexOutOfRangeException">The index is outside this sequence's <see cref="Length"/>.</exception>
    internal KraidFootCollisionHitbox this[int index]
    {
        get
        {
            if ((uint)index >= (uint)Length) throw new IndexOutOfRangeException();
            return new(-6, -6, 6, 6, EnemyAiCodePointers.BankA7.KraidBackgroundTouch,
                EnemyAiCodePointers.BankA7.KraidNoOpShot);
        }
    }
    /// <summary>Creates a value-type enumerator over the hitbox entries.</summary>
    public Enumerator GetEnumerator() => new(this);

    /// <summary>Iterates a hitbox sequence without allocating an iterator object.</summary>
    /// <param name="sequence">Sequence whose hitbox entries are returned.</param>
    internal struct Enumerator(KraidFootHitboxSequence sequence)
    {
        /// <summary>Index of the next hitbox entry to yield.</summary>
        private int next;

        /// <summary>Advances to the next hitbox entry and reports whether one remains.</summary>
        public bool MoveNext() => next++ < sequence.Length;

        /// <summary>Gets the hitbox entry most recently selected by <see cref="MoveNext"/>.</summary>
        public KraidFootCollisionHitbox Current => sequence[next - 1];
    }
}
/// <summary>Frame-relative physical placement of one foot component and the hitbox list used for collision.</summary>
/// <param name="X">Horizontal offset from Kraid's body origin in pixels.</param>
/// <param name="Y">Vertical offset from Kraid's body origin in pixels.</param>
/// <param name="HitboxPointer">Native bank-$A7 pointer to the component's collision definition.</param>
internal readonly record struct KraidFootCollisionComponent(short X, short Y, ushort HitboxPointer);

/// <summary>Rectangle bounds and native contact callbacks for a Kraid-foot collision entry.</summary>
/// <param name="Left">Left edge relative to the foot component origin.</param>
/// <param name="Top">Top edge relative to the foot component origin.</param>
/// <param name="Right">Right edge relative to the foot component origin.</param>
/// <param name="Bottom">Bottom edge relative to the foot component origin.</param>
/// <param name="TouchAi">Native enemy AI pointer invoked for background contact.</param>
/// <param name="ShotAi">Native enemy AI pointer invoked when the hitbox is shot.</param>
internal readonly record struct KraidFootCollisionHitbox(
    short Left, short Top, short Right, short Bottom, ushort TouchAi, ushort ShotAi);
