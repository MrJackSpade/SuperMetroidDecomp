using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.SpriteObjects;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Rooms.SpriteObjects;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks every spawned room sprite-object kind's typed program against its native bank-$B4 list.</summary>
internal static class RoomSpriteObjectDefinitionsVerification
{
    /// <summary>The most updates each kind runs, well past every program's last change; a holding program stops earlier.</summary>
    private const int UpdatesPerKind = 0x8000 + 64;

    /// <summary>
    /// Runs each named kind beside the cartridge's own list, comparing occupancy, record, timer and spritemap until the
    /// list ends or holds; a held list shows its frame for good, which the object's hold reproduces without the native
    /// list's countdown reload.
    /// </summary>
    internal static void VerifyRoomSpriteObjectDefinitions(CartridgeImportAddressSpace rom)
    {
        int kinds = 0, updates = 0;
        foreach (RoomSpriteObjectKind kind in Enum.GetValues<RoomSpriteObjectKind>().Distinct())
        {
            ushort entry = RoomSpriteObjectNativeLists.Entry(rom, RoomSpriteObjectNativeWords.Kinds.Word(kind));
            AssertEqual(entry, RoomSpriteObjectDefinitions.InstructionPointer(kind), $"room sprite object {kind} entry");
            var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
            RoomSpriteObject slot = enemies.SpriteObjects.SpawnRoomSpriteObject(new(0x1000, 0x2000), kind, SuperMetroid.Core.Enemies.SpriteGraphics.SharedIn(SuperMetroid.Core.Enemies.Drawing.EnemyShades.InProjectileLook));
            AssertEqual(kind, slot.Kind, $"room sprite object {kind} identity");
            var native = new RoomSpriteObjectNativeLists.Reference(rom, entry);
            for (int update = 0; update < UpdatesPerKind; update++)
            {
                AssertEqual(native.IsActive, slot.IsActive, $"room sprite object {kind} occupancy after {update} updates");
                if (!native.IsActive)
                    break;
                AssertEqual(native.Pointer, slot.InstructionPointer, $"room sprite object {kind} record after {update} updates");
                AssertEqual(native.Spritemap, slot.SpritemapPointer, $"room sprite object {kind} spritemap after {update} updates");
                AssertEqual(native.Holds, slot.Holds, $"room sprite object {kind} hold after {update} updates");
                if (native.Holds)
                    break;
                AssertEqual(native.Timer, slot.FrameHold, $"room sprite object {kind} timer after {update} updates");
                native.Step();
                enemies.SpriteObjects.StepRoomSpriteObjects();
                updates++;
            }
            kinds++;
        }
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
