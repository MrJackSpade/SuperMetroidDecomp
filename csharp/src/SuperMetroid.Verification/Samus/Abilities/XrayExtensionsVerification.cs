using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus.Abilities;
namespace SuperMetroid.Verification.Samus.Abilities;

/// <summary>Checks X-ray reveal traversal through signed extension blocks and terminal block types.</summary>
internal static class XrayExtensionsVerification
{
    /// <summary>The block the fixture's installed one-block reveals show, distinct from the negative-exit blank.</summary>
    private const int RevealedBlock = 500;

    /// <summary>Checks axis routing, byte arithmetic, blank exits, unsupported reveal types, and cycle detection.</summary>
    internal static void VerifyXrayExtensions()
    {
        RoomLevelData Create() => new RoomLevelData(new Extent16<RoomBlocks>(8, 8), new RoomLevelPlanes(Enumerable.Repeat(RoomTerrainBlock.Of(0x000, BlockMirroring.None, RoomCollisionType.SolidBlock), 64).ToArray(), new RoomBlockBehavior[64], new RoomBlockAppearance[64]), RoomLevelData.NothingBeyondGrid());
        void Set(RoomLevelData level, int index, RoomCollisionType type, int behavior)
        {
            level.SetObjectTerrain(index, RoomTerrainBlock.Of(0, BlockMirroring.None, type));
            level.SetObjectBehavior(index, RoomBlockBehaviorCodec.Decode(behavior));
        }
        // A terminal special-air block shows its installed reveal block (#1596); only a negative-coordinate exit shows the
        // extension's own blank.
        var visuals = XrayRevealFixtures.Complete(shape => XrayRevealFixtures.Corners(shape, RevealedBlock, 0, 0, 0));
        var level = Create();
        Set(level, 18, RoomCollisionType.HorizontalExtension, 0xff);
        Set(level, 17, RoomCollisionType.SpecialAir, 0x46);
        AssertEqual(RevealedBlock, XrayRevealExtensions.Resolve(level, Position16<RoomBlocks>.Of(2, 2), visuals),
            "initial negative horizontal BTS reaches the scroll trigger's installed reveal");
        Set(level, 17, RoomCollisionType.ShootableBlock, 0);
        AssertTrue(XrayRevealExtensions.Resolve(level, Position16<RoomBlocks>.Of(2, 2), visuals) is null,
            "X-ray extension does not inherit a shot-block reveal like a collision extension");
        Set(level, 18, RoomCollisionType.VerticalExtension, 1);
        Set(level, 26, RoomCollisionType.HorizontalExtension, 1);
        Set(level, 34, RoomCollisionType.SpecialAir, 0x46);
        AssertEqual(RevealedBlock, XrayRevealExtensions.Resolve(level, Position16<RoomBlocks>.Of(2, 2), visuals),
            "positive horizontal BTS reached vertically retains the vertical loop");
        Set(level, 26, RoomCollisionType.HorizontalExtension, 0xff);
        Set(level, 25, RoomCollisionType.SpecialAir, 0x46);
        AssertEqual(RevealedBlock, XrayRevealExtensions.Resolve(level, Position16<RoomBlocks>.Of(2, 2), visuals),
            "negative horizontal BTS switches from Y to X");
        Set(level, 26, RoomCollisionType.VerticalExtension, 0xfe);
        Set(level, 10, RoomCollisionType.SpecialAir, 0x46);
        AssertEqual(RevealedBlock, XrayRevealExtensions.Resolve(level, Position16<RoomBlocks>.Of(2, 2), visuals),
            "subsequent vertical BTS stays unsigned and hardware multiply uses low Y byte");
        Set(level, 0, RoomCollisionType.HorizontalExtension, 0xff);
        AssertEqual(XrayRevealTableWords.BlankMetatile, XrayRevealExtensions.Resolve(level, Position16<RoomBlocks>.Zero, visuals),
            "negative coordinate exit emits native blank");
        Set(level, 0, RoomCollisionType.HorizontalExtension, 0);
        AssertTrue(XrayRevealExtensions.Resolve(level, Position16<RoomBlocks>.Zero, visuals) is null, "zero BTS retains original art");
        Set(level, 0, RoomCollisionType.HorizontalExtension, 1);
        Set(level, 1, RoomCollisionType.HorizontalExtension, 0xff);
        bool cycleRejected = false;
        try { XrayRevealExtensions.Resolve(level, Position16<RoomBlocks>.Zero, visuals); }
        catch (InvalidDataException e) when (e.Message.Contains("Cyclic")) { cycleRejected = true; }
        AssertTrue(cycleRejected, "cyclic authored extensions report an error instead of hanging");
        Console.WriteLine("  X-ray extensions: signed entry, asymmetric axis routing, byte multiplication, terminal filtering and cycle detection agree.");
    }
}
