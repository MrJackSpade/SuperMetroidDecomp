using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.EffectActors;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Rooms.EffectActors;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks every spawned room sprite-object kind's typed program against its native bank-$B4 list.</summary>
internal static class RoomEffectActorKindsVerification
{
    /// <summary>The most updates each kind runs, well past every program's last change; a holding program stops earlier.</summary>
    private const int UpdatesPerKind = 0x8000 + 64;

    /// <summary>
    /// Runs each named kind beside the cartridge's own list, comparing occupancy, spritemap and the updates before the
    /// picture changes until the list ends or holds; a held list shows its frame for good, which the object's hold
    /// reproduces without the native list's countdown reload. A still picture holds from its first update, where the native
    /// list holds once its frame's one update has run out (#2275), and a picture the list shows in two records in a row is
    /// one frame of their summed updates.
    /// </summary>
    internal static void VerifyRoomEffectActorKinds(CartridgeImportAddressSpace rom)
    {
        int kinds = 0, updates = 0;
        foreach (RoomEffectActorKind kind in Enum.GetValues<RoomEffectActorKind>())
        {
            ushort entry = RoomEffectActorNativeLists.Entry(rom, RoomEffectActorNativeWords.Kinds.Word(kind));
            AssertEqual(entry, RoomEffectActorProgramAddresses.Entries[kind], $"room sprite object {kind} entry");
            var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
            RoomEffectActor slot = enemies.EffectActors.SpawnRoomEffectActor(new(0x1000, 0x2000), kind, SuperMetroid.Core.Enemies.SpriteGraphics.SharedIn(SuperMetroid.Core.Enemies.Drawing.EnemyShades.InProjectileLook));
            AssertEqual(kind, slot.Kind, $"room sprite object {kind} identity");
            var native = new RoomEffectActorNativeLists.Reference(rom, entry);
            for (int update = 0; update < UpdatesPerKind; update++)
            {
                AssertEqual(native.IsActive, slot.IsActive, $"room sprite object {kind} occupancy after {update} updates");
                if (!native.IsActive)
                    break;
                AssertEqual(native.Spritemap, slot.SpritemapPointer, $"room sprite object {kind} spritemap after {update} updates");
                if (slot.Holds && !native.Holds)
                {
                    // A still picture: the native list's frame runs out into its hold, showing the same picture.
                    ushort held = native.Spritemap;
                    while (!native.Holds)
                    {
                        native.Step();
                        AssertEqual(held, native.Spritemap, $"room sprite object {kind}'s still picture is the one its native list holds");
                    }
                }
                AssertEqual(native.Holds, slot.Holds, $"room sprite object {kind} hold after {update} updates");
                if (native.Holds)
                    break;
                AssertEqual(UpdatesBeforeChange(native), slot.FrameHold, $"room sprite object {kind} updates before its picture changes after {update} updates");
                native.Step();
                enemies.EffectActors.StepRoomEffectActors();
                updates++;
            }
            kinds++;
        }
        Console.WriteLine($"Room sprite objects: {kinds} spawned kinds' programs match their native bank-$B4 lists over {updates} updates.");
    }

    /// <summary>
    /// The updates before the native list's picture changes, holds or ends: its timer, and the whole timers of the records
    /// straight after it that show the same picture.
    /// </summary>
    private static int UpdatesBeforeChange(RoomEffectActorNativeLists.Reference native)
    {
        int updates = native.Timer;
        ushort spritemap = native.Spritemap;
        RoomEffectActorNativeLists.Reference ahead = native.Copy();
        for (int step = 0; step < native.Timer; step++)
            ahead.Step();
        while (ahead.IsActive && !ahead.Holds && ahead.Spritemap == spritemap)
        {
            updates += ahead.Timer;
            for (int step = ahead.Timer; step > 0; step--)
                ahead.Step();
        }
        return updates;
    }

    /// <summary>Dispatches the cartridge-backed room sprite-object verification command.</summary>
    [VerificationCommand("--room-sprite-object-definitions")]
    internal static int RoomEffectActorKindsCommand()
    {
        Suite(nameof(VerifyRoomEffectActorKinds), () => VerifyRoomEffectActorKinds(
            SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }
}
