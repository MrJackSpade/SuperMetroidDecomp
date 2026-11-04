using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Bosses.Draygon.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms.SpriteObjects;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.Core.World;

namespace SuperMetroid.Verification.Bosses.Draygon;

/// <summary>Checks the six Draygon burial Evir spawn and subspeed records.</summary>
internal static class DraygonBurialEvirDefinitionsVerification
{
    /// <summary>Compares native tables and follows real sprite-object allocation and 16.16 movement.</summary>
    internal static void VerifyDraygonBurialEvirDefinitions(SuperMetroidAddressSpace rom)
    {
        const int subspeedTable = 0xa5a1af;
        const int positionTable = 0xa5a1c7;
        const int angleTable = 0xa5a1df;

        for (int entry = 0; entry < 6; entry++)
        {
            DraygonBurialEvirDefinition definition =
                DraygonBurialEvirs.ForEntry(entry);
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((subspeedTable + entry * 4)), definition.SubspeedMagnitude.X.Fraction,
                $"Draygon burial Evir {entry} X subspeed");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((subspeedTable + entry * 4 + 2)), definition.SubspeedMagnitude.Y.Fraction,
                $"Draygon burial Evir {entry} Y subspeed");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((positionTable + entry * 4)), definition.Initial.X,
                $"Draygon burial Evir {entry} initial X");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((positionTable + entry * 4 + 2)), definition.Initial.Y,
                $"Draygon burial Evir {entry} initial Y");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((angleTable + entry * 4)), (ushort)definition.Angle.RawValue,
                $"Draygon burial Evir {entry} angle");
            AssertEqual(0, CartridgeImportSource.Require(rom).ReadCartridgeWord((angleTable + entry * 4 + 2)),
                $"Draygon burial Evir {entry} angle padding");
        }

        AssertThrows<InvalidDataException>(
            () => DraygonBurialEvirs.ForEntry(-1),
            "Draygon burial Evir negative entry");
        AssertThrows<InvalidDataException>(
            () => DraygonBurialEvirs.ForEntry(6),
            "Draygon burial Evir entry past table");

        var guarded = NewDraygonBurialEvirReadGuard(rom);
        var enemies = new RoomEnemySystem(new EnemySession(new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence()), definitions: CompiledRoomEnemyDefinitions.Instance);
        // Draygon dies on a white hurt frame here, so his burial Evirs, which draw in his own row, spawn in the flash.
        var draygon = new DraygonEnemyState(enemies.SlotTable.Slots[0]);
        draygon.Colours.ShowHurt(true);

        for (int entry = 5; entry >= 0; entry--)
        {
            RoomSpriteObjectKind kind = entry >= 3
                ? RoomSpriteObjectKind.DraygonIntroEvir
                : RoomSpriteObjectKind.DraygonDeathEvirFacingRight;
            enemies.DraygonBehavior.Death.SpawnDraygonDeathEvir(draygon, entry, kind);
            // The n-th Evir spawned takes the n-th slot in the pool's processing order.
            RoomSpriteObjectSlot sprite = enemies.SpriteObjects.InProcessingOrder.ElementAt(5 - entry);
            DraygonBurialEvirDefinition definition =
                DraygonBurialEvirs.ForEntry(entry);
            AssertTrue(sprite.IsActive, $"Draygon burial Evir {entry} active after spawn");
            AssertEqual(kind, sprite.Kind, $"Draygon burial Evir {entry} kind");
            AssertEqual(definition.Initial.X, sprite.Position.X.Whole,
                $"Draygon burial Evir {entry} spawned X");
            AssertEqual(definition.Initial.Y, sprite.Position.Y.Whole,
                $"Draygon burial Evir {entry} spawned Y");
            AssertEqual(SuperMetroid.Core.Enemies.SpriteGraphics.SharedIn(draygon.Colours.Sprites), sprite.Graphics,
                $"Draygon burial Evir {entry} draws in the shading his sprites show");
        }
        AssertTrue(draygon.Colours.Sprites.Shades.Contains(DraygonShades.SpriteWhite), "Draygon's sprites show the white flash");

        // His next hurt update rewrites his row; every Evir follows it that update.
        draygon.Colours.ShowHurt(false);
        draygon.ShadeEvirs(enemies.SpriteObjects);
        foreach (RoomSpriteObjectSlot evir in enemies.SpriteObjects.InProcessingOrder.Take(6))
        {
            AssertEqual(SuperMetroid.Core.Enemies.SpriteGraphics.SharedIn(draygon.Colours.Sprites), evir.Graphics,
                "A burial Evir follows Draygon's material colours after his white flash");
            AssertTrue(evir.Graphics.Shading.Shades.Contains(DraygonShades.Sprite) && !evir.Graphics.Shading.Shades.Contains(DraygonShades.SpriteWhite),
                "A burial Evir no longer shows the white flash");
        }

        enemies.DraygonBehavior.Death.MoveDraygonDeathEvirs();
        for (int entry = 5; entry >= 0; entry--)
        {
            // The n-th Evir spawned takes the n-th slot in the pool's processing order.
            RoomSpriteObjectSlot sprite = enemies.SpriteObjects.InProcessingOrder.ElementAt(5 - entry);
            DraygonBurialEvirDefinition definition =
                DraygonBurialEvirs.ForEntry(entry);
            (ushort expectedX, ushort expectedXSub) = AddDraygonBurialSubspeed(
                definition.Initial.X,
                definition.SubspeedMagnitude.X.Fraction,
                add: ((definition.Angle.RawValue + 0x0040) & 0x0080) != 0);
            (ushort expectedY, ushort expectedYSub) = AddDraygonBurialSubspeed(
                definition.Initial.Y,
                definition.SubspeedMagnitude.Y.Fraction,
                add: ((definition.Angle.RawValue + 0x0080) & 0x0080) != 0);
            AssertEqual(expectedX, sprite.Position.X.Whole,
                $"Draygon burial Evir {entry} moved X");
            AssertEqual(expectedXSub, sprite.Position.X.Fraction,
                $"Draygon burial Evir {entry} moved X fraction");
            AssertEqual(expectedY, sprite.Position.Y.Whole,
                $"Draygon burial Evir {entry} moved Y");
            AssertEqual(expectedYSub, sprite.Position.Y.Fraction,
                $"Draygon burial Evir {entry} moved Y fraction");
        }

        Console.WriteLine(
            "Draygon burial Evir definitions: all six spawn/movement records match ROM and the real allocation plus 16.16 movement paths pass with all three source tables forbidden.");
    }

    /// <summary>Models native whole/fraction addition or subtraction for one burial axis.</summary>
    private static AddDraygonBurialSubspeedResult AddDraygonBurialSubspeed(
        ushort position,
        ushort magnitude,
        bool add)
    {
        uint fixedPosition = (uint)position << 16;
        fixedPosition = add
            ? unchecked(fixedPosition + magnitude)
            : unchecked(fixedPosition - magnitude);
        return new AddDraygonBurialSubspeedResult(FixedCoordinate.WholeOf(unchecked((int)fixedPosition)), unchecked((ushort)fixedPosition));
    }

    /// <summary>Rejects gameplay reads from the three migrated bank-$A5 burial tables.</summary>
    private static CartridgeReadGuard NewDraygonBurialEvirReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(address => address is >= 0xa5a1af and < 0xa5a1f7,
                address => $"Draygon burial Evir attempted migrated definition read ${address:X6}.");
}
