using System.Collections;

namespace SuperMetroid.Core.Bosses.MotherBrain;

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
        _ => throw new ArgumentOutOfRangeException(nameof(region)),
    };

    /// <summary>Yields the physical regions in the same order as native collision tests.</summary>
    public IEnumerator<MotherBrainContactHitbox> GetEnumerator()
    {
        for (int region = 0; region < Count; region++) yield return this[region];
    }

    /// <summary>Preserves native collision order for non-generic collection consumers.</summary>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
