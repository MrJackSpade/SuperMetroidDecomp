using SuperMetroid.Core.Rom;
using System.Reflection;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms.SpriteObjects;
using SuperMetroid.Core.Enemies.Drawing;

/// <summary>Checks the six Draygon burial Evir spawn and subspeed records.</summary>
internal static class DraygonBurialEvirDefinitionsVerification
{
    /// <summary>Compares native tables and follows real sprite-object allocation and 16.16 movement.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Draygon burial Evir definitions")]
    internal static void VerifyDraygonBurialEvirDefinitions(SuperMetroidAddressSpace rom)
    {
        const int subspeedTable = 0xa5a1af;
        const int positionTable = 0xa5a1c7;
        const int angleTable = 0xa5a1df;
        const int spriteSlotCount = 32;
        BindingFlags instanceFlags = BindingFlags.Instance | BindingFlags.NonPublic;

        for (int entry = 0; entry < 6; entry++)
        {
            DraygonBurialEvirDefinition definition =
                DraygonBurialEvirDefinitions.ForEntry(entry);
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((subspeedTable + entry * 4)), definition.SubspeedMagnitude.X.Fraction,
                $"Draygon burial Evir {entry} X subspeed");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((subspeedTable + entry * 4 + 2)), definition.SubspeedMagnitude.Y.Fraction,
                $"Draygon burial Evir {entry} Y subspeed");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((positionTable + entry * 4)), definition.Initial.X,
                $"Draygon burial Evir {entry} initial X");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((positionTable + entry * 4 + 2)), definition.Initial.Y,
                $"Draygon burial Evir {entry} initial Y");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((angleTable + entry * 4)), definition.Angle,
                $"Draygon burial Evir {entry} angle");
            AssertEqual(0, CartridgeImportSource.Require(rom).ReadCartridgeWord((angleTable + entry * 4 + 2)),
                $"Draygon burial Evir {entry} angle padding");
        }

        AssertThrows<InvalidDataException>(
            () => DraygonBurialEvirDefinitions.ForEntry(-1),
            "Draygon burial Evir negative entry");
        AssertThrows<InvalidDataException>(
            () => DraygonBurialEvirDefinitions.ForEntry(6),
            "Draygon burial Evir entry past table");

        var guarded = NewDraygonBurialEvirReadGuard(rom);
        var enemies = new RoomEnemySystem();
        enemies.BindFixtureBus(guarded);

        for (int entry = 5; entry >= 0; entry--)
        {
            RoomSpriteObjectKind kind = entry >= 3
                ? RoomSpriteObjectKind.DraygonIntroEvir
                : RoomSpriteObjectKind.DraygonDeathEvirFacingRight;
            enemies.DraygonBehavior.SpawnDraygonDeathEvir(entry, kind);
            RoomSpriteObjectSlot sprite = enemies.RoomSpriteObjects[
                spriteSlotCount - 1 - (5 - entry)];
            DraygonBurialEvirDefinition definition =
                DraygonBurialEvirDefinitions.ForEntry(entry);
            AssertTrue(sprite.IsActive, $"Draygon burial Evir {entry} active after spawn");
            AssertEqual(kind, sprite.Kind, $"Draygon burial Evir {entry} kind");
            AssertEqual(definition.Initial.X, sprite.Position.X.Whole,
                $"Draygon burial Evir {entry} spawned X");
            AssertEqual(definition.Initial.Y, sprite.Position.Y.Whole,
                $"Draygon burial Evir {entry} spawned Y");
            AssertEqual(EnemyPaletteBits.Palette7, sprite.GraphicsIndex,
                $"Draygon burial Evir {entry} palette");
        }

        enemies.DraygonBehavior.MoveDraygonDeathEvirs();
        for (int entry = 5; entry >= 0; entry--)
        {
            RoomSpriteObjectSlot sprite = enemies.RoomSpriteObjects[
                spriteSlotCount - 1 - (5 - entry)];
            DraygonBurialEvirDefinition definition =
                DraygonBurialEvirDefinitions.ForEntry(entry);
            (ushort expectedX, ushort expectedXSub) = AddDraygonBurialSubspeed(
                definition.Initial.X,
                definition.SubspeedMagnitude.X.Fraction,
                add: ((definition.Angle + 0x0040) & 0x0080) != 0);
            (ushort expectedY, ushort expectedYSub) = AddDraygonBurialSubspeed(
                definition.Initial.Y,
                definition.SubspeedMagnitude.Y.Fraction,
                add: ((definition.Angle + 0x0080) & 0x0080) != 0);
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
    private static (ushort Position, ushort Subposition) AddDraygonBurialSubspeed(
        ushort position,
        ushort magnitude,
        bool add)
    {
        uint fixedPosition = (uint)position << 16;
        fixedPosition = add
            ? unchecked(fixedPosition + magnitude)
            : unchecked(fixedPosition - magnitude);
        return (SnesFixedPosition.WholeOf(unchecked((int)fixedPosition)), unchecked((ushort)fixedPosition));
    }

    /// <summary>Rejects gameplay reads from the three migrated bank-$A5 burial tables.</summary>
    private static CartridgeReadGuard NewDraygonBurialEvirReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(address => address is >= 0xa5a1af and < 0xa5a1f7,
                address => $"Draygon burial Evir attempted migrated definition read ${address:X6}.");
}
