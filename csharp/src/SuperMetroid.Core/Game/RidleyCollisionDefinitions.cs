namespace SuperMetroid.Core.Game;

/// <summary>Engine-owned offset and hitbox-list identity of one Ridley body component.</summary>
internal readonly record struct RidleyCollisionComponent(short X, short Y, ushort HitboxPointer);

/// <summary>Engine-owned rectangle and callbacks; never derived from editable OAM.</summary>
internal readonly record struct RidleyCollisionHitbox(
    short Left, short Top, short Right, short Bottom, ushort TouchAi, ushort ShotAi);

/// <summary>
/// All eleven bank-$A6 extended body frames selected by Ceres and Lower Norfair
/// Ridley's instruction programs, and their seventeen distinct hitbox lists.
/// The original $A0:9A5A/$9B7F collision walker consumes these offsets and
/// rectangles independently of the displayed sprite composition.
/// </summary>
internal static class RidleyCollisionDefinitions
{
    /// <summary>$A6, shared native bank for both Ceres and Lower Norfair Ridley body maps.</summary>
    internal const byte Bank = 0xa6;
    private const ushort Touch = EnemyAiCodePointers.BankA6.RidleyExtendedTouch;
    private const ushort Shot = EnemyAiCodePointers.BankA6.RidleyShot;

    /// <summary>$A6:DE7A, CheckIfRidleyIsOffScreen: signed world coordinates and the
    /// camera-relative origin plus 32 must be inside 320 by 288 pixels.</summary>
    internal static bool IsOutsideInteractionWindow(ushort x, ushort y, ushort cameraX, ushort cameraY)
    {
        short relativeX = unchecked((short)(x + 32 - cameraX));
        short relativeY = unchecked((short)(y + 32 - cameraY));
        return unchecked((short)x) < 0 || unchecked((short)y) < 0 ||
            relativeX < 0 || relativeY < 0 ||
            unchecked((short)(relativeX - 320)) >= 0 ||
            unchecked((short)(relativeY - 288)) >= 0;
    }

    private enum BodyFrame : ushort
    {
        /// <summary>$A6:E983, ExtendedSpritemap_Ridley_FacingLeft.</summary>
        Left = 0xe983,
        /// <summary>$A6:E9A5, ExtendedSpritemap_Ridley_FacingRight.</summary>
        Right = 0xe9a5,
        /// <summary>$A6:E9C7, ExtendedSpritemap_Ridley_FacingLeft_MouthHalfOpen.</summary>
        LeftMouthHalfOpen = 0xe9c7,
        /// <summary>$A6:E9E9, ExtendedSpritemap_Ridley_FacingLeft_MouthOpen.</summary>
        LeftMouthOpen = 0xe9e9,
        /// <summary>$A6:EA0B, ExtendedSpritemap_Ridley_FacingRight_MouthHalfOpen.</summary>
        RightMouthHalfOpen = 0xea0b,
        /// <summary>$A6:EA2D, ExtendedSpritemap_Ridley_FacingRight_MouthOpen.</summary>
        RightMouthOpen = 0xea2d,
        /// <summary>$A6:EA4F, ExtendedSpritemap_Ridley_FacingLeft_LegsHalfExtended.</summary>
        LeftLegsHalfExtended = 0xea4f,
        /// <summary>$A6:EA71, ExtendedSpritemap_Ridley_FacingLeft_LegsExtended.</summary>
        LeftLegsExtended = 0xea71,
        /// <summary>$A6:EA93, ExtendedSpritemap_Ridley_FacingRight_LegsHalfExtended.</summary>
        RightLegsHalfExtended = 0xea93,
        /// <summary>$A6:EAB5, ExtendedSpritemap_Ridley_FacingRight_LegsExtended.</summary>
        RightLegsExtended = 0xeab5,
        /// <summary>$A6:EAD7, ExtendedSpritemap_Ridley_FacingForward.</summary>
        Forward = 0xead7,
    }
    /// <summary>$A6:E983..EAD6: each side-facing frame has a two-byte count and four eight-byte component records.</summary>
    private const int SideFrameBytes = 2 + 4 * 8;
    /// <summary>$A6:EB2F, Hitbox_Ridley_FacingLeft_LegsNotExtended; the two extended variants follow as one-rectangle lists.</summary>
    private const ushort LeftLegs = 0xeb2f;
    /// <summary>$A6:EB59, Hitbox_Ridley_FacingLeft_Hand.</summary>
    private const ushort LeftHand = 0xeb59;
    /// <summary>$A6:EB67, Hitbox_Ridley_FacingLeft_Torso.</summary>
    private const ushort LeftBody = 0xeb67;
    /// <summary>$A6:EAE1, Hitbox_Ridley_FacingLeft_MouthClosed; the two mouth variants follow as two-rectangle lists.</summary>
    private const ushort LeftHead = 0xeae1;
    /// <summary>$A6:EB91, Hitbox_Ridley_FacingForward.</summary>
    private const ushort ForwardHitbox = 0xeb91;
    /// <summary>$A6:EBAB..EC3E right-facing lists repeat the left-facing identities at $CA-byte displacement.</summary>
    private const int RightHitboxDisplacement = 0xebab - LeftHead;
    /// <summary>$A6:EADB, the forward frame's single component Y offset, authored for its drawing.</summary>
    private const short ForwardYOffset = -6;

    private readonly record struct ComponentOffset(short X, short Y);
    // The four left-facing component origins (legs, hand, torso, head) are authored placements
    // for the drawing; right-facing frames negate X.
    private static readonly ComponentOffset[] LeftBase =
        [new(15, 22), new(-8, 7), new(16, 0), new(-3, -24)];

    internal readonly record struct ComponentSequence(ushort Frame)
    {
        internal int Length => Frame == (ushort)BodyFrame.Forward ? 1 : 4;
        internal RidleyCollisionComponent this[int index] => (uint)index < Length
            ? ComponentAt((BodyFrame)Frame, index)
            : throw new IndexOutOfRangeException();
        public Enumerator GetEnumerator() => new(this);
        internal struct Enumerator(ComponentSequence sequence)
        {
            private int index = -1;
            public readonly RidleyCollisionComponent Current => sequence[index];
            public bool MoveNext() => ++index < sequence.Length;
        }
    }

    private static RidleyCollisionComponent ComponentAt(BodyFrame frame, int component)
    {
        if (frame == BodyFrame.Forward) return new(0, ForwardYOffset, ForwardHitbox);
        bool right = frame is BodyFrame.Right or BodyFrame.RightMouthHalfOpen or BodyFrame.RightMouthOpen
            or BodyFrame.RightLegsHalfExtended or BodyFrame.RightLegsExtended;
        ushort hitbox = component switch
        {
            0 => (ushort)(LeftLegs + (frame switch
            {
                BodyFrame.LeftLegsHalfExtended or BodyFrame.RightLegsHalfExtended => 1,
                BodyFrame.LeftLegsExtended or BodyFrame.RightLegsExtended => 2,
                _ => 0,
            }) * (2 + 12)),
            1 => LeftHand,
            2 => LeftBody,
            _ => (ushort)(LeftHead + (frame switch
            {
                BodyFrame.LeftMouthHalfOpen or BodyFrame.RightMouthHalfOpen => 1,
                BodyFrame.LeftMouthOpen or BodyFrame.RightMouthOpen => 2,
                _ => 0,
            }) * (2 + 2 * 12)),
        };
        ComponentOffset origin = LeftBase[component];
        return new((short)(right ? -origin.X : origin.X), origin.Y,
            (ushort)(hitbox + (right ? RightHitboxDisplacement : 0)));
    }

    /// <summary>
    /// Two contiguous list runs: the eight left-facing lists from $A6:EAE1 (three mouth states,
    /// three leg states, hand and torso), then from $A6:EB91 the forward list followed by the
    /// eight right-facing lists in the same order. Rectangles are fitted per facing to the
    /// drawings, not mirrored.
    /// </summary>
    private static readonly CollisionRecordRuns<RidleyCollisionHitbox> Lists = CollisionRecordRuns<RidleyCollisionHitbox>.HitboxLists(
        new(LeftHead,
        [
            [new(-12, -26, 11, 13, Touch, Shot), new(-24, 3, -13, 21, Touch, Shot)],
            [new(-41, -19, -21, -9, Touch, Shot), new(-20, -29, 11, 5, Touch, Shot)],
            [new(-37, -40, -14, -31, Touch, Shot), new(-25, -31, 9, 6, Touch, Shot)],
            [new(-15, -10, 7, 2, Touch, Shot)],
            [new(-17, -9, 6, 15, Touch, Shot)],
            [new(-14, -1, 10, 23, Touch, Shot)],
            [new(-15, -2, -1, 8, Touch, Shot)],
            [new(-16, -20, 12, 21, Touch, Shot)],
        ]),
        new(ForwardHitbox,
        [
            [new(-16, -32, 16, 34, Touch, Shot), new(-8, -45, 8, -33, Touch, Shot)],
            [new(-12, -25, 11, 13, Touch, Shot), new(12, 5, 24, 20, Touch, Shot)],
            [new(-13, -29, 20, 5, Touch, Shot), new(21, -18, 39, -8, Touch, Shot)],
            [new(-10, -31, 25, 8, Touch, Shot), new(13, -42, 35, -32, Touch, Shot)],
            [new(-10, -10, 17, 2, Touch, Shot)],
            [new(-9, -8, 17, 15, Touch, Shot)],
            [new(-11, -8, 14, 23, Touch, Shot)],
            [new(1, -2, 14, 9, Touch, Shot)],
            [new(-13, -22, 14, 21, Touch, Shot)],
        ]));
    internal static bool HasFrame(ushort frame) => frame >= (ushort)BodyFrame.Left
        && frame <= (ushort)BodyFrame.Forward && (frame - (ushort)BodyFrame.Left) % SideFrameBytes == 0;

    internal static ComponentSequence ComponentsAt(ushort frame) => HasFrame(frame)
        ? new(frame)
        : throw new InvalidDataException($"Ridley frame $A6:{frame:X4} has no compiled collision.");
    internal static ReadOnlySpan<RidleyCollisionHitbox> HitboxesAt(ushort list) =>
        Lists.TryGet(list, out RidleyCollisionHitbox[] hitboxes)
            ? hitboxes
            : throw new InvalidDataException($"Ridley hitbox list $A6:{list:X4} is not compiled.");
}
