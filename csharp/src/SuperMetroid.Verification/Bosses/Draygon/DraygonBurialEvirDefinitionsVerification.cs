using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Bosses.Draygon.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.EffectActors;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.Core.World;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;

namespace SuperMetroid.Verification.Bosses.Draygon;

/// <summary>Checks the six Draygon burial Evir spawn and subspeed records.</summary>
internal static class DraygonBurialEvirDefinitionsVerification
{
    /// <summary>Compares native tables and follows real sprite-object allocation and 16.16 movement.</summary>
    internal static void VerifyDraygonBurialEvirDefinitions(CartridgeImportAddressSpace rom)
    {
        const int subspeedTable = 0xa5a1af;
        const int positionTable = 0xa5a1c7;
        const int angleTable = 0xa5a1df;

        // The cartridge's three tables list the Evirs from the last spawned to the first. Its angle table signs the subspeed
        // table's magnitudes, which the port's drift takes from the shared integer sine of the heading: an axis's magnitude
        // is added when the angle, turned a quarter (X) or a half (Y), lies in the second half of the turn, and subtracted
        // otherwise.
        AssertEqual(6, DraygonBurialEvirDefinitions.InSpawnOrder.Count, "Draygon's death spawns six burial Evirs");
        for (int entry = 0; entry < 6; entry++)
        {
            DraygonBurialEvirDefinition definition = DraygonBurialEvirDefinitions.InSpawnOrder[5 - entry];
            int angle = rom.ReadCartridgeWord((angleTable + entry * 4));
            AssertEqual(new TrigonometryAngle(angle), definition.Heading, $"Draygon burial Evir {entry} heading");
            FixedVector<RoomPixels> drift = DraygonDeath.BurialDrift(definition);
            AssertEqual(NativeBurialVelocity(rom.ReadCartridgeWord((subspeedTable + entry * 4)), add: ((angle + 0x0040) & 0x0080) != 0),
                drift.X, $"Draygon burial Evir {entry} X velocity");
            AssertEqual(NativeBurialVelocity(rom.ReadCartridgeWord((subspeedTable + entry * 4 + 2)), add: ((angle + 0x0080) & 0x0080) != 0),
                drift.Y, $"Draygon burial Evir {entry} Y velocity");
            Position16<RoomPixels> start = Position16<RoomPixels>.Zero + definition.Start;
            AssertEqual(rom.ReadCartridgeWord((positionTable + entry * 4)), start.X,
                $"Draygon burial Evir {entry} initial X");
            AssertEqual(rom.ReadCartridgeWord((positionTable + entry * 4 + 2)), start.Y,
                $"Draygon burial Evir {entry} initial Y");
            AssertEqual(0, rom.ReadCartridgeWord((angleTable + entry * 4 + 2)),
                $"Draygon burial Evir {entry} angle padding");
        }

        _ = NewDraygonBurialEvirReadGuard(rom);
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        // Draygon dies on a white hurt frame here; his burial Evirs spawn in their own colours, not in his flash (#1544).
        var draygon = new DraygonEnemyState(enemies.Places.InUpdateOrder[0].BoundFor(EnemyDefinitionId.DraygonBody), enemies.Places.InUpdateOrder[1], enemies.Places.InUpdateOrder[2], enemies.Places.InUpdateOrder[3]);
        draygon.Colours.ShowHurt(DraygonBodyColours.WhiteFlash);

        for (int entry = 5; entry >= 0; entry--)
        {
            // The cartridge spawns its last three entries as the left-facing intro Evir and its first three facing right.
            RoomEffectActorKind kind = entry >= 3
                ? RoomEffectActorKind.EvirFacingLeft
                : RoomEffectActorKind.EvirFacingRight;
            DraygonBurialEvirDefinition definition = DraygonBurialEvirDefinitions.InSpawnOrder[5 - entry];
            Position16<RoomPixels> start = Position16<RoomPixels>.Zero + definition.Start;
            enemies.DraygonBehavior.Death.SpawnDraygonDeathEvir(draygon, definition);
            // The n-th Evir spawned is the n-th live sprite object.
            RoomEffectActor sprite = enemies.RoomEffectActors[5 - entry];
            AssertTrue(sprite.IsActive, $"Draygon burial Evir {entry} active after spawn");
            AssertEqual(kind, sprite.Kind, $"Draygon burial Evir {entry} kind");
            AssertEqual(start.X, sprite.Position.X.Whole,
                $"Draygon burial Evir {entry} spawned X");
            AssertEqual(start.Y, sprite.Position.Y.Whole,
                $"Draygon burial Evir {entry} spawned Y");
            AssertEqual(SuperMetroid.Core.Enemies.SpriteGraphics.SharedIn(DraygonColours.Evirs), sprite.Graphics,
                $"Draygon burial Evir {entry} draws in the Evirs' own colours");
            AssertTrue(!sprite.Graphics.Shading.Shades.Contains(DraygonShades.SpriteWhite),
                $"Draygon burial Evir {entry} does not show his white flash");
        }
        AssertTrue(draygon.Colours.Sprites.Shades.Contains(DraygonShades.SpriteWhite), "Draygon's sprites show the white flash");

        DraygonDeath.MoveDraygonDeathEvirs(draygon);
        for (int entry = 5; entry >= 0; entry--)
        {
            // The n-th Evir spawned is the n-th live sprite object.
            RoomEffectActor sprite = enemies.RoomEffectActors[5 - entry];
            int angle = rom.ReadCartridgeWord((angleTable + entry * 4));
            (ushort expectedX, ushort expectedXSub) = AddDraygonBurialSubspeed(
                rom.ReadCartridgeWord((positionTable + entry * 4)),
                rom.ReadCartridgeWord((subspeedTable + entry * 4)),
                add: ((angle + 0x0040) & 0x0080) != 0);
            (ushort expectedY, ushort expectedYSub) = AddDraygonBurialSubspeed(
                rom.ReadCartridgeWord((positionTable + entry * 4 + 2)),
                rom.ReadCartridgeWord((subspeedTable + entry * 4 + 2)),
                add: ((angle + 0x0080) & 0x0080) != 0);
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

    /// <summary>The per-update velocity the cartridge's subspeed <paramref name="magnitude"/> gives one axis, added or subtracted.</summary>
    private static SignedSixteenSixteen NativeBurialVelocity(ushort magnitude, bool add) =>
        add ? SignedSixteenSixteen.FromParts(0, magnitude) : -SignedSixteenSixteen.FromParts(0, magnitude);

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
        return new AddDraygonBurialSubspeedResult((ushort)SixteenSixteenWords.WholeOf(fixedPosition), unchecked((ushort)fixedPosition));
    }

    /// <summary>Rejects gameplay reads from the three migrated bank-$A5 burial tables.</summary>
    private static CartridgeReadGuard NewDraygonBurialEvirReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(address => address is >= 0xa5a1af and < 0xa5a1f7,
                address => $"Draygon burial Evir attempted migrated definition read ${address:X6}.");
}
