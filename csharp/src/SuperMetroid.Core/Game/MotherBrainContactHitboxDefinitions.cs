namespace SuperMetroid.Core.Game;

/// <summary>The mutually exclusive Mother Brain component owning a Samus-contact list.</summary>
internal enum MotherBrainContactPart : byte
{
    Body,
    Brain,
    Neck,
}

/// <summary>
/// One asymmetric Mother Brain contact rectangle, expressed as signed distances from its
/// component origin exactly as stored by the cartridge.
/// </summary>
internal readonly record struct MotherBrainContactHitbox(
    short Left,
    short Top,
    short Right,
    short Bottom);

/// <summary>
/// Compiled physical hitboxes for Mother Brain's body, brain, and three tested neck joints.
/// These definitions remain application-owned when visual spritemaps become editable.
/// </summary>
internal static class MotherBrainContactHitboxDefinitions
{
    /// <summary>First byte of the contiguous count-prefixed lists at <c>$A9:B427</c>.</summary>
    internal const int SourceAddress = 0xa9b427;

    /// <summary>Complete byte length through the neck list's final bottom extent.</summary>
    internal const int SourceByteLength = 46;

    /// <summary><c>$A9:B427</c>, two rectangles attached to the standing body.</summary>
    internal const int BodySourceAddress = 0xa9b427;

    /// <summary><c>$A9:B439</c>, two rectangles attached to the independently moving brain.</summary>
    internal const int BrainSourceAddress = 0xa9b439;

    /// <summary><c>$A9:B44B</c>, one rectangle reused by neck joints one through three.</summary>
    internal const int NeckSourceAddress = 0xa9b44b;

    private static readonly MotherBrainContactHitbox[] Body =
    [
        new(-32, -24, 42, 56),
        new(-24, -42, 28, -25),
    ];

    private static readonly MotherBrainContactHitbox[] Brain =
    [
        new(-24, -22, 22, 0),
        new(-22, 1, 16, 20),
    ];

    private static readonly MotherBrainContactHitbox[] Neck =
    [
        new(-8, -8, 8, 8),
    ];

    /// <summary>Returns the native ordered collision rectangles for one physical component.</summary>
    internal static ReadOnlySpan<MotherBrainContactHitbox> Get(MotherBrainContactPart part) =>
        part switch
        {
            MotherBrainContactPart.Body => Body,
            MotherBrainContactPart.Brain => Brain,
            MotherBrainContactPart.Neck => Neck,
            _ => throw new ArgumentOutOfRangeException(nameof(part)),
        };

    /// <summary>Returns the pinned list address used only for parity diagnostics.</summary>
    internal static int GetSourceAddress(MotherBrainContactPart part) =>
        part switch
        {
            MotherBrainContactPart.Body => BodySourceAddress,
            MotherBrainContactPart.Brain => BrainSourceAddress,
            MotherBrainContactPart.Neck => NeckSourceAddress,
            _ => throw new ArgumentOutOfRangeException(nameof(part)),
        };
}
