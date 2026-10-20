namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>
/// The twelve bank-$A5 Spore Spawn extended OAM frames at $EE65-$EF61 and
/// their fixed gameplay hitboxes. Version 6 mislabeled their visual keys as
/// Draygon, but collision always follows the actual Spore Spawn program.
/// </summary>
internal static class SporeSpawnCollisionDefinitions
{
    /// <summary>Spore Spawn's contact reaction, on the active shell and vulnerable point.</summary>
    private const SporeSpawnHitboxTouch Touch = SporeSpawnHitboxTouch.Contact;
    /// <summary>Spore Spawn's shot reaction, on the exposed vulnerable point.</summary>
    private const SporeSpawnHitboxShot Shot = SporeSpawnHitboxShot.Vulnerable;
    /// <summary>The inactive head's touch reaction before Spore Spawn opens.</summary>
    private const SporeSpawnHitboxTouch NoOp = SporeSpawnHitboxTouch.Inert;
    /// <summary>
    /// Samus projectile family bits 8-10: the vulnerable core accepts any projectile with one set, besides a charged
    /// beam (<c>SporeSpawn_Shot</c>).
    /// </summary>
    internal const ushort AcceptedFamilyBits = 0x0700;
    /// <summary>The dud shot reaction of shell rectangles and the inactive trailing point.</summary>
    private const SporeSpawnHitboxShot Dud = SporeSpawnHitboxShot.Dud;

    /// <summary>The two single-head root frames that precede the opening sequence.</summary>
    private enum HeadRootFrame : ushort
    {
        /// <summary>$A5:EE65, ExtendedSpritemap_SporeSpawn_Dead: one inactive head component.</summary>
        Dead = 0xee65,
        /// <summary>$A5:EE6F, ExtendedSpritemap_SporeSpawn_Closed_Closing_Opening_0: one closed head.</summary>
        Closed = 0xee6f,
    }
    /// <summary>$A5:EE79, ExtendedSpritemap_SporeSpawn_Closed_Closing_Opening_1: first of seven opening roots.</summary>
    private const ushort FirstOpeningFrame = 0xee79;
    /// <summary>$A5:EF3D, ExtendedSpritemap_SporeSpawn_FullyOpen_0: first of three open oscillation roots.</summary>
    private const ushort FirstFullyOpenFrame = 0xef3d;

    /// <summary>Builds the trailing vulnerable rectangle with active or dud shot response.</summary>
    private static SporeSpawnCollisionHitbox InnerPoint(SporeSpawnHitboxShot shot) => new(-15, -24, 14, 23, Touch, shot);

    /// <summary>Head rectangles shared by the closed and open lists, which differ only in touch AI.</summary>
    private static SporeSpawnCollisionHitbox[] Head(SporeSpawnHitboxTouch touch) =>
        [new(-41, -30, 41, 30, touch, Dud), new(-16, -45, 15, -30, touch, Dud)];

    /// <summary>Two closed-head shell rectangles without an active touch response.</summary>
    private static readonly SporeSpawnCollisionHitbox[] ClosedHeadList = Head(NoOp);
    /// <summary>Two open-head shell rectangles with active touch response.</summary>
    private static readonly SporeSpawnCollisionHitbox[] OpenHeadList = Head(Touch);
    /// <summary>Wider head rectangles used by the first opening pose.</summary>
    private static readonly SporeSpawnCollisionHitbox[] ExtendedHeadList =
        [new(-44, -35, 43, 33, Touch, Dud), new(-16, -49, 15, -35, Touch, Dud)];
    /// <summary>
    /// The six moving heads' upper and lower shell bands, inner point and crown, indexed by opening step. Each step widens
    /// the vertical shell spacing and raises the crown; the last head's inner point extends one pixel farther at both
    /// vertical ends, and the fully open frames hold it.
    /// </summary>
    private static readonly SporeSpawnCollisionHitbox[][] MovingHeadLists =
    [
        [new(-45, -38, 44, -9, Touch, Dud), new(-45, 8, 44, 35, Touch, Dud), InnerPoint(Shot), new(-16, -54, 16, -22, Touch, Dud)],
        [new(-43, -44, 42, -13, Touch, Dud), new(-44, 12, 42, 42, Touch, Dud), InnerPoint(Shot), new(-16, -58, 16, -42, Touch, Dud)],
        [new(-45, -47, 44, -17, Touch, Dud), new(-44, 16, 43, 46, Touch, Dud), InnerPoint(Shot), new(-16, -62, 16, -45, Touch, Dud)],
        [new(-44, -50, 45, -21, Touch, Dud), new(-43, 20, 43, 50, Touch, Dud), InnerPoint(Shot), new(-16, -64, 16, -48, Touch, Dud)],
        [new(-44, -53, 44, -23, Touch, Dud), new(-44, 22, 43, 52, Touch, Dud), InnerPoint(Shot), new(-16, -68, 16, -48, Touch, Dud)],
        [new(-44, -55, 43, -25, Touch, Dud), new(-45, 24, 43, 55, Touch, Dud), new(-15, -25, 14, 24, Touch, Shot), new(-16, -69, 16, -48, Touch, Dud)],
    ];
    /// <summary>Single trailing point that accepts the Spore Spawn shot callback.</summary>
    private static readonly SporeSpawnCollisionHitbox[] ShotPointList = [InnerPoint(Shot)];
    /// <summary>Single trailing point that rejects shots through the dud callback.</summary>
    private static readonly SporeSpawnCollisionHitbox[] DudPointList = [InnerPoint(Dud)];

    /// <summary>Checks seven fixed-stride opening frame roots in bank $A5.</summary>
    private static bool IsOpeningFrame(ushort pointer) =>
        pointer >= FirstOpeningFrame && pointer <= FirstOpeningFrame + 6 * 18 &&
        (pointer - FirstOpeningFrame) % 18 == 0;

    /// <summary>Checks the three fixed-stride fully open oscillation frame roots.</summary>
    private static bool IsFullyOpenFrame(ushort pointer) =>
        pointer >= FirstFullyOpenFrame && pointer <= FirstFullyOpenFrame + 2 * 18 &&
        (pointer - FirstFullyOpenFrame) % 18 == 0;

    /// <summary>Checks whether a pointer belongs to an installed Spore Spawn collision frame.</summary>
    internal static bool IsFrame(ushort pointer) =>
        Enum.IsDefined((HeadRootFrame)pointer) || IsOpeningFrame(pointer) || IsFullyOpenFrame(pointer);

    /// <summary>The opening poses' head lists, in pose order; the moving heads follow the wider first pose.</summary>
    private static readonly SporeSpawnCollisionHitbox[][] OpeningHeadLists = [ExtendedHeadList, .. MovingHeadLists];

    /// <summary>The dead root's one inactive head component.</summary>
    private static readonly SporeSpawnCollisionComponent[] DeadComponents = [new(Offset16<RoomPixels>.Zero, ClosedHeadList)];
    /// <summary>The closed root's one head component.</summary>
    private static readonly SporeSpawnCollisionComponent[] ClosedComponents = [new(Offset16<RoomPixels>.Zero, OpenHeadList)];
    /// <summary>Each opening root's head and inner point, indexed by opening step; the inner point oscillates B, C, D, C.</summary>
    private static readonly SporeSpawnCollisionComponent[][] OpeningComponents =
        [.. Enumerable.Range(0, OpeningHeadLists.Length).Select(opening => HeadAndPoint(OpeningHeadLists[opening], opening % 4))];
    /// <summary>Each fully open root's final head and inner point, indexed by oscillation phase B, C, D.</summary>
    private static readonly SporeSpawnCollisionComponent[][] FullyOpenComponents =
        [.. Enumerable.Range(0, 3).Select(phase => HeadAndPoint(MovingHeadLists[^1], phase))];

    /// <summary>A zero-offset head followed by its zero-offset inner point at <paramref name="phase"/>.</summary>
    private static SporeSpawnCollisionComponent[] HeadAndPoint(SporeSpawnCollisionHitbox[] head, int phase) =>
        [new(Offset16<RoomPixels>.Zero, head), new(Offset16<RoomPixels>.Zero, InnerPointForPhase(phase))];

    /// <summary>
    /// $A5:EE65..EEE5 and EF3D..EF61: dead/closed roots contain one head;
    /// opening roots advance the head geometry while the inner point oscillates B,C,D,C.
    /// Fully-open roots hold the final head and select B,C,D. All component offsets are zero.
    /// The intervening unused single-component roots are outside the installed frame domain.
    /// </summary>
    internal static ReadOnlySpan<SporeSpawnCollisionComponent> ComponentsAt(ushort pointer)
    {
        if (Enum.IsDefined((HeadRootFrame)pointer))
        {
            var root = (HeadRootFrame)pointer;
            return root switch
            {
                HeadRootFrame.Dead => DeadComponents,
                HeadRootFrame.Closed => ClosedComponents,
                _ => throw new InvalidOperationException($"Undefined {nameof(HeadRootFrame)} {(int)root}."),
            };
        }
        if (IsFullyOpenFrame(pointer))
            return FullyOpenComponents[(pointer - FirstFullyOpenFrame) / 18];
        if (IsOpeningFrame(pointer))
            return OpeningComponents[(pointer - FirstOpeningFrame) / 18];
        throw new InvalidDataException(
            $"Spore Spawn frame $A5:{pointer:X4} has no compiled collision identity.");
    }

    /// <summary>Selects the active or dud trailing-point hitboxes by opening phase; the mirrored point shares the active list.</summary>
    private static SporeSpawnCollisionHitbox[] InnerPointForPhase(int phase) => phase switch
    {
        2 => DudPointList,
        _ => ShotPointList,
    };
}
