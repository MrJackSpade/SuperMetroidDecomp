using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Game;

/// <summary>Engine-owned component offset and bank-$A7 hitbox-list identity.</summary>
internal readonly record struct PhantoonCollisionComponent(short X, short Y, ushort HitboxPointer);

/// <summary>Engine-owned rectangle and touch/shot callbacks for Phantoon.</summary>
internal readonly record struct PhantoonCollisionHitbox(
    short Left, short Top, short Right, short Bottom, ushort TouchAi, ushort ShotAi);

/// <summary>
/// Phantoon's three hitbox lists at $A7:E020, $E02E, and $E06C, separate from
/// its editable BG2 frames. The 22 frame roots at $A7:DEDD-$DFFD have zero
/// component offsets; the three tentacle roots each have two components.
/// </summary>
internal static class PhantoonCollisionDefinitions
{
    /// <summary>$A7:E020, the one-point no-op hitbox used by hidden parts.</summary>
    internal const ushort PointList = 0xe020;
    /// <summary>$A7:E02E, Phantoon's five-rectangle full body.</summary>
    internal const ushort FullBodyList = 0xe02e;
    /// <summary>$A7:E06C, the single vulnerable eye rectangle.</summary>
    internal const ushort EyeOnlyList = 0xe06c;
    /// <summary>$A7:DD95, Phantoon's active touch callback.</summary>
    internal const ushort TouchAi = 0xdd95;
    /// <summary>$A7:DD9B, Phantoon's active shot callback.</summary>
    internal const ushort ShotAi = 0xdd9b;

    // REQUIRED: independent full-body silhouette bounds. Sharing the eye rectangle
    // and active callbacks does not resolve these five chosen collision shapes.
    private static readonly (short Left, short Top, short Right, short Bottom)[] RequiredBodyBounds =
    [
        (-33, -40, 32, 56),
        (-9, 22, 8, 39),
        (-23, 52, -16, 71),
        (15, 53, 22, 70),
        (-12, 53, 11, 69),
    ];

    internal static ComponentSequence ComponentsAt(ushort pointer)
    {
        if (!PhantoonBg2FrameDefinitions.IsFrame(pointer))
            throw new InvalidDataException($"Phantoon frame $A7:{pointer:X4} has no compiled hitbox identity.");
        return pointer switch
        {
            PhantoonBg2FrameDefinitions.BodyFullHitbox => new(FullBodyList, 1),
            PhantoonBg2FrameDefinitions.BodyEyeHitboxOnly => new(EyeOnlyList, 1),
            PhantoonBg2FrameDefinitions.Tentacles0 or
                PhantoonBg2FrameDefinitions.Tentacles1 or
                PhantoonBg2FrameDefinitions.Tentacles2 => new(PointList, 2),
            _ => new(PointList, 1),
        };
    }

    internal static HitboxSequence HitboxesAt(ushort pointer) => pointer switch
    {
        PointList or FullBodyList or EyeOnlyList => new(pointer),
        _ => throw new InvalidDataException($"Phantoon hitbox list $A7:{pointer:X4} is not compiled."),
    };

    /// <summary>Native frame components use their actor origin; tentacle frames have two inert components.</summary>
    internal readonly struct ComponentSequence(ushort list, int count) : IReadOnlyList<PhantoonCollisionComponent>
    {
        public int Count => count;
        public int Length => Count;
        public PhantoonCollisionComponent this[int index] => (uint)index < Count
            ? new(0, 0, list) : throw new IndexOutOfRangeException();
        public IEnumerator<PhantoonCollisionComponent> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return this[index];
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }

    /// <summary>$A7:E06C repeats the full body's eye rectangle at $A7:E03C; hidden lists use a no-op point.</summary>
    internal readonly struct HitboxSequence(ushort list) : IReadOnlyList<PhantoonCollisionHitbox>
    {
        public int Count => list == FullBodyList ? RequiredBodyBounds.Length : 1;
        public int Length => Count;
        public PhantoonCollisionHitbox this[int index]
        {
            get
            {
                if ((uint)index >= Count) throw new IndexOutOfRangeException();
                if (list == PointList) return new(0, 0, 0, 0, EnemyAiCodePointers.BankA0.NoOp, EnemyAiCodePointers.BankA0.NoOp);
                var bounds = RequiredBodyBounds[list == EyeOnlyList ? 1 : index];
                return new(bounds.Left, bounds.Top, bounds.Right, bounds.Bottom, TouchAi, ShotAi);
            }
        }
        public IEnumerator<PhantoonCollisionHitbox> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return this[index];
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
