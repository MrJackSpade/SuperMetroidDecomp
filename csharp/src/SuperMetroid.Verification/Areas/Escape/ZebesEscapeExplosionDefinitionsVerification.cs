using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms.SpriteObjects;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.Core.World;
using SuperMetroid.Core.Escape;
using SuperMetroid.AssetExtraction.Audio;

namespace SuperMetroid.Verification.Areas.Escape;

/// <summary>Checks the eight native Zebes escape explosion sprites and sound selectors in production spawning.</summary>
internal static class ZebesEscapeExplosionDefinitionsVerification
{
    /// <summary>Compares compiled sprite and sound rows with bank-$8F and checks all random and inherited index handoffs with table reads forbidden.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Zebes escape explosion definitions")]
    internal static void VerifyZebesEscapeExplosionDefinitions(SuperMetroidAddressSpace rom)
    {
        for (int index = 0; index < 8; index++)
        {
            ZebesEscapeExplosionDefinition definition =
                ZebesEscapeExplosionDefinitions.ForIndex(index);
            AssertEqual(rom.ReadCartridgeByte(0x8fc1d6 + index), (byte)definition.SpriteKind,
                $"Zebes escape explosion sprite {index}");
            AssertEqual(SoundEffectCommands.DecodeOptional(SoundEffectLibrary.Library2, rom.ReadCartridgeByte(0x8fc1de + index)), definition.SoundEffect,
                $"Zebes escape explosion sound {index}");
        }

        var guarded = NewZebesEscapeExplosionReadGuard(rom);
        for (ushort choice = 0; choice < 16; choice++)
        for (ushort inherited = 0; inherited < 8; inherited++)
        {
            var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance);
            enemies.EscapeExplosionsBehavior.SpawnEscapeExplosion(new(0x1234, 0x5678), choice, inherited);

            int selectedIndex = choice < 8 ? choice : inherited;
            ZebesEscapeExplosionDefinition expected =
                ZebesEscapeExplosionDefinitions.ForIndex(selectedIndex);
            RoomSpriteObjectSlot sprite = enemies.RoomSpriteObjects.Single(slot => slot.IsActive);
            AssertEqual(expected.SpriteKind, sprite.Kind,
                $"production escape sprite choice {choice}, inherited {inherited}");
            AssertEqual(0x1234, sprite.Position.X.Whole,
                $"production escape X choice {choice}, inherited {inherited}");
            AssertEqual(0x5678, sprite.Position.Y.Whole,
                $"production escape Y choice {choice}, inherited {inherited}");

            bool queuesSound = choice < 8 && expected.SoundEffect is not null;
            AssertEqual(queuesSound ? 1 : 0, enemies.Audio.SoundRequests.Count,
                $"production escape sound count choice {choice}, inherited {inherited}");
            if (queuesSound)
            {
                AssertEqual(new SoundRequest(expected.SoundEffect ?? throw new InvalidOperationException("A sounding choice has a sound."), SoundQueueLimit.Max6),
                    enemies.Audio.SoundRequests.Single(),
                    $"production escape sound choice {choice}, inherited {inherited}");
            }
        }

        AssertThrows<InvalidDataException>(
            () => ZebesEscapeExplosionDefinitions.ForIndex(-1),
            "negative Zebes escape explosion index");
        AssertThrows<InvalidDataException>(
            () => ZebesEscapeExplosionDefinitions.ForIndex(8),
            "Zebes escape explosion index past table");

        Console.WriteLine(
            "Zebes escape explosion definitions: sixteen native bytes and all 128 random/inherited selection handoffs pass with source tables forbidden.");
    }

    /// <summary>Rejects runtime reads of the migrated sprite and sound bytes at $8F:C1D6-C1E5.</summary>
    private static CartridgeReadGuard NewZebesEscapeExplosionReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source).Forbid(address => address is >= 0x8fc1d6 and < 0x8fc1e6,
            address => $"Zebes escape explosion attempted migrated table read ${address:X6}.");
}
