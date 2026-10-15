using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Bosses.Phantoon.Artwork;

namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>
/// Phantoon's three hitbox lists at $A7:E020, $E02E, and $E06C, separate from
/// its editable BG2 frames. The 22 frame roots at $A7:DEDD-$DFFD have zero
/// component offsets; the three tentacle roots each have two components.
/// </summary>
internal static class PhantoonCollisionDefinitions
{
    /// <summary>$A7:DD95, Phantoon's active touch callback.</summary>
    internal const ushort TouchAi = 0xdd95;
    /// <summary>$A7:DD9B, Phantoon's active shot callback.</summary>
    internal const ushort ShotAi = 0xdd9b;

    /// <summary>Half width of the full body's central collision rectangle.</summary>
    private const short BodyHalfWidth = 33;
    /// <summary>Half width of the vulnerable eye's central collision rectangle.</summary>
    private const short EyeHalfWidth = 9;
    /// <summary>Half width of the centered tentacle collision rectangle.</summary>
    private const short CenterTentacleHalfWidth = 12;
    /// <summary>Near horizontal edge of each mirrored side tentacle rectangle.</summary>
    private const short SideTentacleInnerExtent = 16;
    /// <summary>Far horizontal edge of each mirrored side tentacle rectangle.</summary>
    private const short SideTentacleOuterExtent = 23;
    /// <summary>Native vertical extents of the body, eye, and three tentacle hitboxes.</summary>
    private static readonly (short Top, short Bottom)[] RequiredVerticalBounds =
        [(-40, 56), (22, 39), (52, 71), (53, 70), (53, 69)];

    /// <summary>
    /// $A7:E02E body/eye/left/right/center rectangles. Centered intervals have
    /// left=-right-1; side tentacle intervals reflect exactly around that same
    /// pixel-centered axis, while their chosen vertical bounds remain independent.
    /// </summary>
    private static PhantoonCollisionHitbox BodyHitbox(int index)
    {
        var vertical = RequiredVerticalBounds[index];
        (short left, short right) = index switch
        {
            0 => Centered(BodyHalfWidth),
            1 => Centered(EyeHalfWidth),
            2 => ((short)-SideTentacleOuterExtent, (short)-SideTentacleInnerExtent),
            3 => ((short)(SideTentacleInnerExtent - 1), (short)(SideTentacleOuterExtent - 1)),
            4 => Centered(CenterTentacleHalfWidth),
            _ => throw new IndexOutOfRangeException(),
        };
        return new(left, vertical.Top, right, vertical.Bottom, TouchAi, ShotAi);

        static (short Left, short Right) Centered(short halfWidth) => ((short)-halfWidth, (short)(halfWidth - 1));
    }
    /// <summary>Maps a BG2 frame to its active body/eye hitbox or inert point components.</summary>
    internal static ComponentSequence ComponentsAt(ushort pointer)
    {
        if (!PhantoonBg2FrameDefinitions.IsFrame(pointer))
            throw new InvalidDataException($"Phantoon frame $A7:{pointer:X4} has no compiled hitbox identity.");
        // Every other frame carries one inert point component.
        if (!Enum.IsDefined((PhantoonCollisionFrame)pointer))
            return new(PhantoonHitboxList.Point, 1);
        return (PhantoonCollisionFrame)pointer switch
        {
            PhantoonCollisionFrame.BodyFullHitbox => new(PhantoonHitboxList.FullBody, 1),
            PhantoonCollisionFrame.BodyEyeHitboxOnly => new(PhantoonHitboxList.EyeOnly, 1),
            PhantoonCollisionFrame.Tentacles0 or
                PhantoonCollisionFrame.Tentacles1 or
                PhantoonCollisionFrame.Tentacles2 => new(PhantoonHitboxList.Point, 2),
            _ => throw new InvalidOperationException($"Undefined Phantoon collision frame $A7:{pointer:X4}."),
        };
    }

    /// <summary>Gets the engine-owned rectangles and callbacks for a supported hitbox-list pointer.</summary>
    internal static HitboxSequence HitboxesAt(ushort pointer) =>
        new(ClosedNativeWords.Decode<PhantoonHitboxList>(pointer, "Phantoon hitbox list"));

    /// <summary>Native frame components use their actor origin; tentacle frames have two inert components.</summary>
    internal readonly struct ComponentSequence(PhantoonHitboxList list, int count) : IReadOnlyList<PhantoonCollisionComponent>
    {
        /// <summary>Gets the number of frame components, including both inert tentacle components.</summary>
        public int Count => count;
        /// <summary>Gets a zero-offset component bound to the frame's selected hitbox list.</summary>
        public PhantoonCollisionComponent this[int index] => (uint)index < Count
            ? new(Offset16<RoomPixels>.Zero, (ushort)list) : throw new IndexOutOfRangeException();
        /// <summary>Yields the frame's collision components in native order.</summary>
        public IEnumerator<PhantoonCollisionComponent> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return this[index];
        }
        /// <summary>Exposes the same component order to non-generic collection consumers.</summary>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }

    /// <summary>$A7:E06C repeats the full body's eye rectangle at $A7:E03C; hidden lists use a no-op point.</summary>
    internal readonly struct HitboxSequence(PhantoonHitboxList list) : IReadOnlyList<PhantoonCollisionHitbox>
    {
        /// <summary>Gets five rectangles for the full body or one for the eye and inert point lists.</summary>
        public int Count => list == PhantoonHitboxList.FullBody ? RequiredVerticalBounds.Length : 1;
        /// <summary>Gets the selected native rectangle or no-op point with its touch and shot callbacks.</summary>
        public PhantoonCollisionHitbox this[int index]
        {
            get
            {
                if ((uint)index >= Count) throw new IndexOutOfRangeException();
                if (list == PhantoonHitboxList.Point) return new(0, 0, 0, 0, EnemyAiCodePointers.BankA0.NoOp, EnemyAiCodePointers.BankA0.NoOp);
                return BodyHitbox(list == PhantoonHitboxList.EyeOnly ? 1 : index);
            }
        }
        /// <summary>Yields the selected hitbox list in native rectangle order.</summary>
        public IEnumerator<PhantoonCollisionHitbox> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return this[index];
        }
        /// <summary>Exposes the same hitbox order to non-generic collection consumers.</summary>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
