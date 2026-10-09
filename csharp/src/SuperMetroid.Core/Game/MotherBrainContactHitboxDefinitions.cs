using System.Collections;

namespace SuperMetroid.Core.Game;

/// <summary>The mutually exclusive Mother Brain component owning a Samus-contact list.</summary>
internal enum MotherBrainContactPart : byte
{
    /// <summary>Mother Brain's body component, tested at the body slot's position.</summary>
    Body,

    /// <summary>The brain component, tested independently at the head slot's position.</summary>
    Brain,

    /// <summary>A middle neck joint tested at that joint's solved position.</summary>
    Neck,
}

/// <summary>
/// One asymmetric Mother Brain contact rectangle, expressed as signed distances from its
/// component origin exactly as stored by the cartridge.
/// </summary>
/// <param name="Left">Signed horizontal extent from the origin to the rectangle's left edge.</param>
/// <param name="Top">Signed vertical extent from the origin to the rectangle's top edge.</param>
/// <param name="Right">Signed horizontal extent from the origin to the rectangle's right edge.</param>
/// <param name="Bottom">Signed vertical extent from the origin to the rectangle's bottom edge.</param>
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

    /// <summary>Returns the native ordered collision rectangles for one physical component.</summary>
    internal static MotherBrainContactHitboxes Get(MotherBrainContactPart part) =>
        part switch
        {
            MotherBrainContactPart.Body or MotherBrainContactPart.Brain or MotherBrainContactPart.Neck => new(part),
            _ => throw new ArgumentOutOfRangeException(nameof(part)),
        };
}

/// <summary>Ordered physical regions of a component; no stored rectangle sequence.</summary>
/// <param name="Part">Component whose fixed ordered contact regions this sequence exposes.</param>
internal readonly record struct MotherBrainContactHitboxes(MotherBrainContactPart Part) : IReadOnlyList<MotherBrainContactHitbox>
{
    /// <summary>Gets the number of ordered rectangles used for this component's contact test.</summary>
    public int Count => Part == MotherBrainContactPart.Neck ? 1 : 2;

    /// <summary>Gets a component's native contact rectangle by its collision-test order.</summary>
    /// <param name="region">Zero-based rectangle index within the selected component.</param>
    /// <returns>The stored rectangle at that position.</returns>
    /// <exception cref="IndexOutOfRangeException">The component has no rectangle at <paramref name="region"/>.</exception>
    public MotherBrainContactHitbox this[int region] => (Part, region) switch
    {
        // Lower body first, then the narrower upper body. Native collision stops on its first hit.
        (MotherBrainContactPart.Body, 0) => new(-32, -24, 42, 56),
        (MotherBrainContactPart.Body, 1) => new(-24, -42, 28, -25),
        // The brain's upper and lower regions meet between local Y=0 and Y=1.
        (MotherBrainContactPart.Brain, 0) => new(-24, -22, 22, 0),
        (MotherBrainContactPart.Brain, 1) => new(-22, 1, 16, 20),
        (MotherBrainContactPart.Neck, 0) => new(-8, -8, 8, 8),
        _ => throw new IndexOutOfRangeException(),
    };

    /// <summary>Iterates through the component's collision rectangles in native first-hit order.</summary>
    /// <returns>An enumerator over the one neck rectangle or two body/brain rectangles.</returns>
    public IEnumerator<MotherBrainContactHitbox> GetEnumerator()
    {
        for (int region = 0; region < Count; region++) yield return this[region];
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
