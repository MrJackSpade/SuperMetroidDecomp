namespace SuperMetroid.Core.Assets;

/// <summary>
/// Cartridge identities are ordered by the six consecutive bank-$8B instruction
/// lists at $ECED, $ECF5, $ECFD, $ED05, $ED0D, and $ED15.
/// </summary>
public static class EndingCloudSpriteDefinitions
{
    /// <summary>$8C:B6F3, EndingCutsceneBottomCloudsPattern; four16-part records
    /// precede the right and left32-part records.</summary>
    private const ushort FirstRecord = 0xb6f3;
    /// <summary>Four horizontal cloud records plus two side-cloud records.</summary>
    private const int FrameCount = 6;
    /// <summary>Native storage order, which differs from cinematic actor/list order.</summary>
    private enum Record
    {
        /// <summary>First horizontal 16-part native record; selected for the actor's lower pattern.</summary>
        BottomPattern,
        /// <summary>Second horizontal 16-part native record; selected for the actor's upper pattern.</summary>
        TopPattern,
        /// <summary>Third horizontal 16-part native record; selected for the actor's lower edge.</summary>
        BottomEdge,
        /// <summary>Fourth horizontal 16-part native record; selected for the actor's upper edge.</summary>
        TopEdge,
        /// <summary>First 32-part side-cloud record in the cartridge's storage order.</summary>
        Right,
        /// <summary>Second 32-part side-cloud record in the cartridge's storage order.</summary>
        Left,
    }
    /// <summary>Cloud roles in the order used by the ending actors.</summary>
    internal enum Role
    {
        /// <summary>Upper cloud pattern requested first by the ending actor lists.</summary>
        UpperPattern,
        /// <summary>Upper cloud edge paired with the first pattern.</summary>
        UpperEdge,
        /// <summary>Lower cloud edge drawn before its lower pattern.</summary>
        LowerEdge,
        /// <summary>Lower cloud pattern selected after the lower edge.</summary>
        LowerPattern,
        /// <summary>Right-hand side cloud, drawn from the first 32-part native record.</summary>
        Right,
        /// <summary>Left-hand side cloud, drawn from the second 32-part native record.</summary>
        Left,
    }

    /// <summary>Gets the ordered upper, lower, right, and left cloud roles used by the ending actors.</summary>
    public static IReadOnlyList<EndingCloudSpriteFrameDefinition> Frames { get; } = new FrameView();

    /// <summary>Maps an actor-list role to the corresponding native cloud record.</summary>
    private static EndingCloudSpriteFrameDefinition Get(int index) => (Role)index switch
    {
        Role.UpperPattern => Define("scene-b-upper-a", Record.TopPattern),
        Role.UpperEdge => Define("scene-b-upper-b", Record.TopEdge),
        Role.LowerEdge => Define("scene-b-lower-a", Record.BottomEdge),
        Role.LowerPattern => Define("scene-b-lower-b", Record.BottomPattern),
        Role.Right => Define("scene-a-right", Record.Right),
        Role.Left => Define("scene-a-left", Record.Left),
        _ => throw new ArgumentOutOfRangeException(nameof(index)),
    };

    /// <summary>Computes a cloud record pointer from preceding 16- and 32-part native records.</summary>
    private static EndingCloudSpriteFrameDefinition Define(string name, Record record)
    {
        int index = (int)record;
        int horizontalRecords = Math.Min(index, 4);
        int sideRecords = Math.Max(index - 4, 0);
        int offset = horizontalRecords * (2 + 16 * 5) + sideRecords * (2 + 32 * 5);
        return new(name, (ushort)(FirstRecord + offset), index < 4 ? 16 : 32);
    }

    /// <summary>Calculated view of the six cloud records in actor order.</summary>
    private sealed class FrameView : IReadOnlyList<EndingCloudSpriteFrameDefinition>
    {
        /// <summary>Four horizontal and two side-cloud actor roles.</summary>
        public int Count => FrameCount;
        /// <summary>Selects one cloud identity in cinematic actor order.</summary>
        public EndingCloudSpriteFrameDefinition this[int index] => Get(index);
        /// <summary>Enumerates all calculated cloud identities without caching an array.</summary>
        public IEnumerator<EndingCloudSpriteFrameDefinition> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return Get(index);
        }
        /// <summary>Uses the typed cloud enumeration for nongeneric callers.</summary>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }

}
