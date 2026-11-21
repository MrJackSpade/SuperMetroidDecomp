using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.EffectActors;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.Core.World;
using SuperMetroid.Core.Escape;
using SuperMetroid.AssetExtraction.Audio;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;

namespace SuperMetroid.Verification.Areas.Escape;

/// <summary>Checks the eight drawn Zebes escape explosions, the kinds a draw leaves to the explosion's place, and production spawning.</summary>
internal static class ZebesEscapeExplosionDefinitionsVerification
{
    /// <summary>The cartridge's escape explosion sprite bytes, in draw order.</summary>
    private const int SpriteTable = 0x8fc1d6;

    /// <summary>The cartridge's escape explosion sound bytes, in draw order.</summary>
    private const int SoundTable = 0x8fc1de;

    /// <summary>
    /// Compares the drawn explosions with bank $8F, and the kinds left open with the entries the game's carried-over index
    /// selects: zero indoors, and outdoors the explosion block's column doubled, of which the sprite read keeps the low three
    /// bits. Then checks every draw spawning through production with each kind left open, with table reads forbidden.
    /// </summary>
    internal static void VerifyZebesEscapeExplosionDefinitions(CartridgeImportAddressSpace rom)
    {
        for (int index = 0; index < 8; index++)
        {
            ZebesEscapeExplosionDefinition definition = NamedBy(index);
            AssertEqual(rom.ReadCartridgeByte(SpriteTable + index), RoomEffectActorNativeWords.Kinds.Word(definition.SpriteKind),
                $"Zebes escape explosion sprite {index}");
            AssertEqual(SoundEffectCommands.DecodeOptional(SoundEffectLibrary.Enemies, rom.ReadCartridgeByte(SoundTable + index)), definition.SoundEffect,
                $"Zebes escape explosion sound {index}");
        }

        AssertEqual(rom.ReadCartridgeByte(SpriteTable), RoomEffectActorNativeWords.Kinds.Word(ZebesEscapeExplosionDefinitions.IndoorKindLeftOpen),
            "indoor kind left open");
        for (int column = 0; column < 64; column++)
        {
            AssertEqual(rom.ReadCartridgeByte(SpriteTable + ((2 * column) & 7)),
                RoomEffectActorNativeWords.Kinds.Word(EscapeExplosionSite.Outdoors.KindLeftOpen(Position16<RoomBlocks>.Of(column, column / 3))),
                $"outdoor kind left open over column {column}");
        }

        var guarded = NewZebesEscapeExplosionReadGuard(rom);
        RoomEffectActorKind[] kindsLeftOpen = [ZebesEscapeExplosionDefinitions.IndoorKindLeftOpen, .. ZebesEscapeExplosionDefinitions.OutdoorKindsLeftOpenByColumnPhase];
        for (ushort random = 0; random < 16; random++)
        foreach (RoomEffectActorKind kindLeftOpen in kindsLeftOpen)
        {
            var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
            enemies.EscapeExplosionsBehavior.SpawnEscapeExplosion(new(0x1234, 0x5678), random, kindLeftOpen);

            ZebesEscapeExplosionDefinition? drawn = random < 8 ? NamedBy(random) : null;
            AssertTrue(random < 8 || ZebesEscapeExplosionDefinitions.Draw.Choose(random) is ZebesEscapeExplosionDraw.LeftOpen,
                $"draw {random} leaves the kind open");
            RoomEffectActor sprite = enemies.RoomEffectActors.Single(slot => slot.IsActive);
            AssertTrue(sprite.Shows(drawn?.SpriteKind ?? kindLeftOpen),
                $"production escape sprite draw {random}, kind left open {kindLeftOpen}");
            AssertEqual(0x1234, sprite.Position.X.Whole,
                $"production escape X draw {random}, kind left open {kindLeftOpen}");
            AssertEqual(0x5678, sprite.Position.Y.Whole,
                $"production escape Y draw {random}, kind left open {kindLeftOpen}");

            AssertEqual(drawn?.SoundEffect is null ? 0 : 1, enemies.Audio.SoundRequests.Count,
                $"production escape sound count draw {random}, kind left open {kindLeftOpen}");
            if (drawn?.SoundEffect is { } sound)
            {
                AssertEqual(new SoundRequest(sound, SoundRequestPriority.Normal), enemies.Audio.SoundRequests.Single(),
                    $"production escape sound draw {random}, kind left open {kindLeftOpen}");
            }
        }

        Console.WriteLine(
            "Zebes escape explosion definitions: sixteen native bytes, the kinds left open indoors and over 64 columns, and all 80 draw/kind-left-open spawns pass with source tables forbidden.");
    }

    /// <summary>The explosion the draw's choice <paramref name="choice"/>, one of the first eight, names.</summary>
    private static ZebesEscapeExplosionDefinition NamedBy(int choice) =>
        ZebesEscapeExplosionDefinitions.Draw.Choose(choice) is ZebesEscapeExplosionDraw.Named { Explosion: var explosion }
            ? explosion
            : throw new InvalidOperationException($"Draw {choice} names no explosion.");

    /// <summary>Rejects runtime reads of the migrated sprite and sound bytes at $8F:C1D6-C1E5.</summary>
    private static CartridgeReadGuard NewZebesEscapeExplosionReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source).Forbid(address => address is >= SpriteTable and < SoundTable + 8,
            address => $"Zebes escape explosion attempted migrated table read ${address:X6}.");
}
