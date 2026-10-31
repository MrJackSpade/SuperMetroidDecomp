using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms.SpriteObjects;
using SuperMetroid.Core.Assets;
namespace SuperMetroid.Verification.Rooms.SpriteObjects;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks every spawned room sprite-object kind's typed program against its native bank-$B4 list.</summary>
internal static class RoomSpriteObjectDefinitionsVerification
{
    /// <summary>The updates each kind runs: past a held frame's first $7FFF reload.</summary>
    private const int UpdatesPerKind = 0x8000 + 64;

    /// <summary>Runs each named kind beside the cartridge's own list, comparing occupancy, record, timer and spritemap.</summary>
    internal static void VerifyRoomSpriteObjectDefinitions(SuperMetroidAddressSpace rom)
    {
        int kinds = 0, updates = 0;
        foreach (RoomSpriteObjectKind kind in Enum.GetValues<RoomSpriteObjectKind>().Where(kind => kind != RoomSpriteObjectKind.None).Distinct())
        {
            ushort entry = RoomSpriteObjectNativeLists.Entry(rom, (ushort)kind);
            AssertEqual(entry, RoomSpriteObjectDefinitions.InstructionPointer(kind), $"room sprite object {kind} entry");
            var enemies = new RoomEnemySystem(new EnemySession(new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence()), definitions: CompiledRoomEnemyDefinitions.Instance);
            RoomSpriteObjectSlot slot = enemies.SpriteObjects.SpawnRoomSpriteObject(new(0x1000, 0x2000), kind, SuperMetroid.Core.Assets.SpriteGraphics.CharacterZeroIn(SpriteColouring.InFixtureSet(3)))
                ?? throw new InvalidOperationException($"Room sprite object {kind} was not allocated.");
            AssertEqual(kind, slot.Kind, $"room sprite object {kind} identity");
            var native = new RoomSpriteObjectNativeLists.Reference(rom, entry);
            for (int update = 0; update < UpdatesPerKind; update++)
            {
                AssertEqual(native.IsActive, slot.IsActive, $"room sprite object {kind} occupancy after {update} updates");
                if (!native.IsActive)
                    break;
                AssertEqual(native.Pointer, slot.InstructionPointer, $"room sprite object {kind} record after {update} updates");
                AssertEqual(native.Timer, slot.InstructionTimer, $"room sprite object {kind} timer after {update} updates");
                AssertEqual(native.Spritemap, slot.SpritemapPointer, $"room sprite object {kind} spritemap after {update} updates");
                native.Step();
                enemies.SpriteObjects.StepRoomSpriteObjects();
                updates++;
            }
            kinds++;
        }
        AssertThrows<KeyNotFoundException>(() => _ = RoomSpriteObjectDefinitions.Programs[RoomSpriteObjectKind.None],
            "room sprite object none selector");
        Console.WriteLine($"Room sprite objects: {kinds} spawned kinds' programs match their native bank-$B4 lists over {updates} updates.");
    }

    /// <summary>Dispatches the cartridge-backed room sprite-object verification command.</summary>
    [VerificationCommand("--room-sprite-object-definitions")]
    internal static int RoomSpriteObjectDefinitionsCommand()
    {
        Suite(nameof(VerifyRoomSpriteObjectDefinitions), () => VerifyRoomSpriteObjectDefinitions(
            SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }
}
