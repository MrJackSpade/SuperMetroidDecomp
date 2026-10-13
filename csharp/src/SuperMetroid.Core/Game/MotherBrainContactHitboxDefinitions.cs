using System.Collections;

namespace SuperMetroid.Core.Game;

/// <summary>The mutually exclusive Mother Brain component owning a Samus-contact list.</summary>
internal enum MotherBrainContactPart : byte
{
    /// <summary>Lower and upper body regions, tested in that native order.</summary>
    Body,
    /// <summary>Upper and lower brain regions meeting between local Y zero and one.</summary>
    Brain,
    /// <summary>Single centered contact region for a tested neck joint.</summary>
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

    /// <summary>Returns the native ordered collision rectangles for one physical component.</summary>
    internal static MotherBrainContactHitboxes Get(MotherBrainContactPart part) =>
        part switch
        {
            MotherBrainContactPart.Body or MotherBrainContactPart.Brain or MotherBrainContactPart.Neck => new(part),
            _ => throw new ArgumentOutOfRangeException(nameof(part)),
        };
}

/// <summary>Ordered physical regions of a component; no stored rectangle sequence.</summary>
internal readonly record struct MotherBrainContactHitboxes(MotherBrainContactPart Part) : IReadOnlyList<MotherBrainContactHitbox>
{
    /// <summary>Body and brain each have two ordered regions; the neck has one.</summary>
    public int Count => Part == MotherBrainContactPart.Neck ? 1 : 2;

    /// <summary>Returns a component's signed local rectangle, preserving the native first-hit test order.</summary>
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

    /// <summary>Yields the physical regions in the same order as native collision tests.</summary>
    public IEnumerator<MotherBrainContactHitbox> GetEnumerator()
    {
        for (int region = 0; region < Count; region++) yield return this[region];
    }

    /// <summary>Preserves native collision order for non-generic collection consumers.</summary>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
