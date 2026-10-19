namespace SuperMetroid.Core.Bosses.Ridley;

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

    /// <summary>The 32-pixel margin $A6:DE7A adds on each axis before the camera-relative test.</summary>
    private static readonly Offset16<RoomPixels> InteractionWindowMargin = new(32, 32);

    /// <summary>$A6:DE7A, CheckIfRidleyIsOffScreen: signed world coordinates and the
    /// camera-relative origin plus 32 must be inside 320 by 288 pixels.</summary>
    internal static bool IsOutsideInteractionWindow(Position16<RoomPixels> position, Position16<RoomPixels> camera)
    {
        Offset16<ScreenPixels> relative = (position + InteractionWindowMargin).RelativeToCamera(camera);
        short relativeX = relative.X;
        short relativeY = relative.Y;
        return unchecked((short)position.X) < 0 || unchecked((short)position.Y) < 0 ||
            relativeX < 0 || relativeY < 0 ||
            unchecked((short)(relativeX - 320)) >= 0 ||
            unchecked((short)(relativeY - 288)) >= 0;
    }

    /// <summary>Native extended body maps for both Ridley encounters, selected by facing, mouth, and leg pose.</summary>
    internal enum BodyFrame : ushort
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
    internal const int SideFrameBytes = 2 + 4 * 8;
    /// <summary>$A6:EADB, the forward frame's single component Y offset, authored for its drawing.</summary>
    private const short ForwardYOffset = -6;

    /// <summary>Hitbox_Ridley_FacingLeft_MouthClosed: the closed-mouth head of left-facing Ridley.</summary>
    private static readonly RidleyCollisionHitbox[] LeftHeadMouthClosed =
        [new(-12, -26, 11, 13), new(-24, 3, -13, 21)];
    /// <summary>The half-open-mouth head of left-facing Ridley.</summary>
    private static readonly RidleyCollisionHitbox[] LeftHeadMouthHalfOpen =
        [new(-41, -19, -21, -9), new(-20, -29, 11, 5)];
    /// <summary>The open-mouth head of left-facing Ridley.</summary>
    private static readonly RidleyCollisionHitbox[] LeftHeadMouthOpen =
        [new(-37, -40, -14, -31), new(-25, -31, 9, 6)];
    /// <summary>Hitbox_Ridley_FacingLeft_LegsNotExtended: left-facing standing legs.</summary>
    private static readonly RidleyCollisionHitbox[] LeftLegsNotExtended = [new(-15, -10, 7, 2)];
    /// <summary>Left-facing legs half extended.</summary>
    private static readonly RidleyCollisionHitbox[] LeftLegsHalfExtended = [new(-17, -9, 6, 15)];
    /// <summary>Left-facing legs extended.</summary>
    private static readonly RidleyCollisionHitbox[] LeftLegsExtended = [new(-14, -1, 10, 23)];
    /// <summary>Hitbox_Ridley_FacingLeft_Hand.</summary>
    private static readonly RidleyCollisionHitbox[] LeftHand = [new(-15, -2, -1, 8)];
    /// <summary>Hitbox_Ridley_FacingLeft_Torso.</summary>
    private static readonly RidleyCollisionHitbox[] LeftTorso = [new(-16, -20, 12, 21)];
    /// <summary>Hitbox_Ridley_FacingForward: the whole body of the front-facing turn pose.</summary>
    private static readonly RidleyCollisionHitbox[] Forward =
        [new(-16, -32, 16, 34), new(-8, -45, 8, -33)];
    /// <summary>The closed-mouth head of right-facing Ridley, fitted to the right-facing drawing rather than mirrored.</summary>
    private static readonly RidleyCollisionHitbox[] RightHeadMouthClosed =
        [new(-12, -25, 11, 13), new(12, 5, 24, 20)];
    /// <summary>The half-open-mouth head of right-facing Ridley.</summary>
    private static readonly RidleyCollisionHitbox[] RightHeadMouthHalfOpen =
        [new(-13, -29, 20, 5), new(21, -18, 39, -8)];
    /// <summary>The open-mouth head of right-facing Ridley.</summary>
    private static readonly RidleyCollisionHitbox[] RightHeadMouthOpen =
        [new(-10, -31, 25, 8), new(13, -42, 35, -32)];
    /// <summary>Right-facing standing legs.</summary>
    private static readonly RidleyCollisionHitbox[] RightLegsNotExtended = [new(-10, -10, 17, 2)];
    /// <summary>Right-facing legs half extended.</summary>
    private static readonly RidleyCollisionHitbox[] RightLegsHalfExtended = [new(-9, -8, 17, 15)];
    /// <summary>Right-facing legs extended.</summary>
    private static readonly RidleyCollisionHitbox[] RightLegsExtended = [new(-11, -8, 14, 23)];
    /// <summary>Right-facing hand.</summary>
    private static readonly RidleyCollisionHitbox[] RightHand = [new(1, -2, 14, 9)];
    /// <summary>Right-facing torso.</summary>
    private static readonly RidleyCollisionHitbox[] RightTorso = [new(-13, -22, 14, 21)];

    /// <summary>Authored origins for left-facing legs, hand, torso, and head; right-facing frames negate X.</summary>
    private static readonly Offset16<RoomPixels>[] LeftBase =
        [new(15, 22), new(-8, 7), new(16, 0), new(-3, -24)];

    /// <summary>Exposes the one forward or four side-facing collision components of a body frame without allocating a list.</summary>
    internal readonly record struct ComponentSequence(ushort Frame)
    {
        /// <summary>Forward Ridley has one body component; side-facing poses have four.</summary>
        internal int Length => Frame == (ushort)BodyFrame.Forward ? 1 : 4;
        /// <summary>Selects a component's authored offset and pose-specific hitbox list.</summary>
        internal RidleyCollisionComponent this[int index] => (uint)index < Length
            ? ComponentAt((BodyFrame)Frame, index)
            : throw new IndexOutOfRangeException();
        /// <summary>Enumerates component positions without materializing a frame array.</summary>
        public Enumerator GetEnumerator() => new(this);
        /// <summary>Walks the calculated collision components of one extended body frame.</summary>
        internal struct Enumerator(ComponentSequence sequence)
        {
            /// <summary>Current component index; starts before the first component.</summary>
            private int index = -1;
            /// <summary>Collision component at the current iterator position.</summary>
            public readonly RidleyCollisionComponent Current => sequence[index];
            /// <summary>Advances through the one or four components of this body frame.</summary>
            public bool MoveNext() => ++index < sequence.Length;
        }
    }

    /// <summary>Selects a component's offset and hitbox list from Ridley's pose, reflecting offsets but keeping the authored right-facing rectangles.</summary>
    private static RidleyCollisionComponent ComponentAt(BodyFrame frame, int component)
    {
        if (frame == BodyFrame.Forward) return new(new(0, ForwardYOffset), Forward);
        bool right = frame is BodyFrame.Right or BodyFrame.RightMouthHalfOpen or BodyFrame.RightMouthOpen
            or BodyFrame.RightLegsHalfExtended or BodyFrame.RightLegsExtended;
        RidleyCollisionHitbox[] hitboxes = component switch
        {
            0 => frame switch
            {
                BodyFrame.LeftLegsHalfExtended => LeftLegsHalfExtended,
                BodyFrame.RightLegsHalfExtended => RightLegsHalfExtended,
                BodyFrame.LeftLegsExtended => LeftLegsExtended,
                BodyFrame.RightLegsExtended => RightLegsExtended,
                BodyFrame.Left or BodyFrame.LeftMouthHalfOpen or BodyFrame.LeftMouthOpen => LeftLegsNotExtended,
                BodyFrame.Right or BodyFrame.RightMouthHalfOpen or BodyFrame.RightMouthOpen => RightLegsNotExtended,
                BodyFrame.Forward => throw new InvalidOperationException("Forward Ridley has a single body component."),
                _ => throw new InvalidOperationException($"Undefined BodyFrame {frame}."),
            },
            1 => right ? RightHand : LeftHand,
            2 => right ? RightTorso : LeftTorso,
            _ => frame switch
            {
                BodyFrame.LeftMouthHalfOpen => LeftHeadMouthHalfOpen,
                BodyFrame.RightMouthHalfOpen => RightHeadMouthHalfOpen,
                BodyFrame.LeftMouthOpen => LeftHeadMouthOpen,
                BodyFrame.RightMouthOpen => RightHeadMouthOpen,
                BodyFrame.Left or BodyFrame.LeftLegsHalfExtended or BodyFrame.LeftLegsExtended => LeftHeadMouthClosed,
                BodyFrame.Right or BodyFrame.RightLegsHalfExtended or BodyFrame.RightLegsExtended => RightHeadMouthClosed,
                BodyFrame.Forward => throw new InvalidOperationException("Forward Ridley has a single body component."),
                _ => throw new InvalidOperationException($"Undefined BodyFrame {frame}."),
            },
        };
        Offset16<RoomPixels> origin = LeftBase[component];
        return new(right ? origin.MirroredX() : origin, hitboxes);
    }

    /// <summary>Accepts the eleven contiguous native extended body frame addresses at the side-frame stride.</summary>
    internal static bool HasFrame(ushort frame) => frame >= (ushort)BodyFrame.Left
        && frame <= (ushort)BodyFrame.Forward && (frame - (ushort)BodyFrame.Left) % SideFrameBytes == 0;

    /// <summary>Gets a validated body's computed collision components for the native collision walker.</summary>
    internal static ComponentSequence ComponentsAt(ushort frame) => HasFrame(frame)
        ? new(frame)
        : throw new InvalidDataException($"Ridley frame $A6:{frame:X4} has no compiled collision.");
}
