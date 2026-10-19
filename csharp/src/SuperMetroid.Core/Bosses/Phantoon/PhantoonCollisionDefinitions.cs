using SuperMetroid.Core.Bosses.Phantoon.Artwork;

namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>
/// Phantoon's three fixed hitbox lists, separate from its editable BG2 frames: the inert point carried by hidden
/// parts and tentacles, the five-rectangle full body, and the vulnerable eye alone. Every frame root has zero
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
    /// <summary>Position of the vulnerable eye among the full body's rectangles.</summary>
    private const int EyeRectangle = 1;
    /// <summary>
    /// Native vertical extents of the body, eye, and three tentacle hitboxes. Declared before the lists below, which
    /// read it during static initialization.
    /// </summary>
    private static readonly (short Top, short Bottom)[] RequiredVerticalBounds =
        [(-40, 56), (22, 39), (52, 71), (53, 70), (53, 69)];

    /// <summary>The single no-reaction point carried by hidden parts and by both tentacle components.</summary>
    private static readonly PhantoonCollisionHitbox[] PointList =
        [new(0, 0, 0, 0, PhantoonHitboxTouch.Inert, PhantoonHitboxShot.Inert)];
    /// <summary>The visible body: body, eye, left, right and center tentacle rectangles, tested in that order.</summary>
    private static readonly PhantoonCollisionHitbox[] FullBodyList =
        [BodyHitbox(0), BodyHitbox(EyeRectangle), BodyHitbox(2), BodyHitbox(3), BodyHitbox(4)];
    /// <summary>The vulnerable eye alone, repeating the full body's eye rectangle.</summary>
    private static readonly PhantoonCollisionHitbox[] EyeOnlyList = [BodyHitbox(EyeRectangle)];

    /// <summary>
    /// One of the body's rectangles. Centered intervals have left=-right-1; side tentacle intervals reflect exactly
    /// around that same pixel-centered axis, while their chosen vertical bounds remain independent.
    /// </summary>
    private static PhantoonCollisionHitbox BodyHitbox(int index)
    {
        var vertical = RequiredVerticalBounds[index];
        (short left, short right) = index switch
        {
            0 => Centered(BodyHalfWidth),
            EyeRectangle => Centered(EyeHalfWidth),
            2 => ((short)-SideTentacleOuterExtent, (short)-SideTentacleInnerExtent),
            3 => ((short)(SideTentacleInnerExtent - 1), (short)(SideTentacleOuterExtent - 1)),
            4 => Centered(CenterTentacleHalfWidth),
            _ => throw new IndexOutOfRangeException(),
        };
        return new(left, vertical.Top, right, vertical.Bottom, PhantoonHitboxTouch.Contact, PhantoonHitboxShot.Vulnerable);

        static (short Left, short Right) Centered(short halfWidth) => ((short)-halfWidth, (short)(halfWidth - 1));
    }
    /// <summary>Maps a BG2 frame to its active body/eye hitbox or inert point components.</summary>
    internal static ComponentSequence ComponentsAt(ushort pointer)
    {
        if (!PhantoonBg2FrameDefinitions.IsFrame(pointer))
            throw new InvalidDataException($"Phantoon frame $A7:{pointer:X4} has no compiled hitbox identity.");
        // Every other frame carries one inert point component.
        if (!Enum.IsDefined((PhantoonCollisionFrame)pointer))
            return new(PointList, 1);
        return (PhantoonCollisionFrame)pointer switch
        {
            PhantoonCollisionFrame.BodyFullHitbox => new(FullBodyList, 1),
            PhantoonCollisionFrame.BodyEyeHitboxOnly => new(EyeOnlyList, 1),
            PhantoonCollisionFrame.Tentacles0 or
                PhantoonCollisionFrame.Tentacles1 or
                PhantoonCollisionFrame.Tentacles2 => new(PointList, 2),
            _ => throw new InvalidOperationException($"Undefined Phantoon collision frame $A7:{pointer:X4}."),
        };
    }

    /// <summary>Native frame components use their actor origin; tentacle frames have two inert components.</summary>
    /// <param name="hitboxes">The one hitbox list every component of the frame holds.</param>
    /// <param name="count">The number of components in the frame.</param>
    internal readonly struct ComponentSequence(IReadOnlyList<PhantoonCollisionHitbox> hitboxes, int count) : IReadOnlyList<PhantoonCollisionComponent>
    {
        /// <summary>Gets the number of frame components, including both inert tentacle components.</summary>
        public int Count => count;
        /// <summary>Gets a zero-offset component holding the frame's hitbox list.</summary>
        public PhantoonCollisionComponent this[int index] => (uint)index < Count
            ? new(Offset16<RoomPixels>.Zero, hitboxes) : throw new IndexOutOfRangeException();
        /// <summary>Yields the frame's collision components in native order.</summary>
        public IEnumerator<PhantoonCollisionComponent> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return this[index];
        }
        /// <summary>Exposes the same component order to non-generic collection consumers.</summary>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
