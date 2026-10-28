using SuperMetroid.Core.Assets;
using SuperMetroid.Verification.Samus.Abilities;
using SuperMetroid.Core.Samus.Drawing;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Rooms;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Enemies.Elevator;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
namespace SuperMetroid.Verification.Samus.Movement;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.World;

/// <summary>Checks Samus's block collision, grounded physics, reversal, Moonwalk and wall-contact routes.</summary>
internal static class SamusTerrainMovementVerification
{

/// <summary>Slope, block, grounded, reversal, moonwalk, and wall-contact verification.</summary>
internal static void VerifySamusSlopePhysics()
{

    // Shape $12 selects multiplier-table word index 2*$12+1 = $25. Retail ROM stores
    // $00C0 there, causing a grounded 1.0 displacement to become 0.C000.
    AssertEqual(
        0x0000c000,
        SamusSlopePhysics.ScaleGroundedHorizontalDisplacement(0x12, 0x00010000, verticalSpeed: 0),
        "BTS $12 grounded positive slope scaling");
    AssertEqual(
        unchecked((int)0xffff4000),
        SamusSlopePhysics.ScaleGroundedHorizontalDisplacement(0x12, -0x00010000, verticalSpeed: 0),
        "BTS $12 grounded negative slope scaling");

    // Ceiling bit $80 and any nonzero Y speed are independent native early returns.
    AssertEqual(
        0x00010000,
        SamusSlopePhysics.ScaleGroundedHorizontalDisplacement(0x92, 0x00010000, verticalSpeed: 0),
        "ceiling slope does not scale horizontal speed");
    AssertEqual(
        0x00010000,
        SamusSlopePhysics.ScaleGroundedHorizontalDisplacement(0x12, 0x00010000, verticalSpeed: 1),
        "airborne Samus does not receive grounded slope scaling");

    // Authored shape $12 runs from height 16 to 1. BTS bit $40 mirrors X=0 to
    // sample 15; use the actual compiled geometry rather than replacing ROM samples.
    AssertEqual(16, SamusSlopePhysics.ReadAlignmentHeight(0x12, 0), "slope unmirrored height sample");
    AssertEqual(1, SamusSlopePhysics.ReadAlignmentHeight(0x52, 0), "slope mirrored height sample");

    // A type-1/BTS-$12 block occupies (0,1). At center Y=21 with radius 5, Samus's bottom
    // is Y=25 (low nibble 9). Height 8 yields correction 8-9-1 = -2, so $94:87F4 moves
    // her center upward to 19 and marks slope adjustment for the later grounding branch.
    RoomLevelData level = CreateRoom(new(2, 2),
        [0x0000, 0x0000, 0x1000, 0x0000],
        [0x00, 0x00, 0x12, 0x00]);
    SlopeAlignmentResult aligned = SamusSlopePhysics.AlignYPosition(
        level,
        position: new(8, 21),
        yRadius: 5);
    AssertEqual(19, aligned.Position.Y, "non-square floor slope whole-pixel Y correction");
    AssertTrue(aligned.Adjusted, "non-square floor slope sets adjusted flag");

    SlopeAlignmentResult disabled = SamusSlopePhysics.AlignYPosition(
        level,
        position: new(8, 21),
        yRadius: 5,
        horizontalSlopeCollisionEnabled: false);
    AssertEqual(21, disabled.Position.Y, "disabled horizontal slope collision preserves Y");
    AssertTrue(!disabled.Adjusted, "disabled horizontal slope collision preserves adjusted flag");

    Console.WriteLine("  Samus slopes: compiled native multiplier, mirrored height samples, and non-square Y alignment agree.");
}

/// <summary>
/// Exercises the native block-span scans and supported type-0/type-1/type-8 reactions
/// surrounding the isolated slope primitives.
/// </summary>
internal static void VerifySamusBlockCollision()
{
    var bus = new TestAddressSpace();

    const int width = 4;
    const int height = 4;
    var foreground = new ushort[width * height];
    var behavior = new byte[foreground.Length];

    // Row one contains a non-square slope in column one and an ordinary type-8 solid in
    // column two. Every other cell is dispatcher type zero air.
    foreground[1 * width + 1] = 0x1000;
    behavior[1 * width + 1] = 0x12;
    foreground[1 * width + 2] = 0x8000;
    RoomLevelData level = CreateRoom(new(width, height), foreground, behavior);

    // At Y=21/radius5 the body penetrates shape-$12's height-8 sample by two pixels.
    // Horizontal type-1 processing first scales 1.0 to 0.C000, then $94:87F4 raises Y.
    var slopeBody = new SamusKinematicsState
    {
        Position = FixedPosition<RoomPixels>.FromWhole(new(24, 21)),
        Radius = new(5, 5),
    };
    BlockMoveResult slopeMove = SamusBlockCollision.MoveHorizontal(new SamusTerrain { Level = level }, slopeBody, null, new HorizontalBlockMove { Displacement = 0x00010000 });
    AssertEqual(0x0000c000, slopeMove.AcceptedDisplacement, "horizontal scan applies BTS $12 multiplier");
    AssertEqual(24, slopeBody.Position.X.Whole, "subpixel slope move preserves whole X");
    AssertEqual(0xc000, slopeBody.Position.X.Fraction, "subpixel slope move updates X fraction");
    AssertEqual(19, slopeBody.Position.Y.Whole, "post-horizontal scan aligns non-square floor Y");
    AssertTrue(slopeBody.Position.Y.Whole != 21, "horizontal scan applies slope adjustment to Y");

    // Resting at center Y=19 puts the bottom at 23. The native +1.0 grounding probe
    // targets bottom 24; height 8 yields correction -1 and clips the accepted move to zero.
    var groundedBody = new SamusKinematicsState
    {
        Position = FixedPosition<RoomPixels>.FromWhole(new(24, 19)),
        Radius = new(5, 5),
    };
    BlockMoveResult groundProbe = SamusBlockCollision.MoveVertical(new SamusTerrain { Level = level }, groundedBody, null, new VerticalBlockMove { Displacement = 0x00010000, ScanLeftToRight = true });
    AssertTrue(groundProbe.Collided, "vertical non-square grounding probe collides");
    AssertEqual(0, groundProbe.AcceptedDisplacement, "vertical non-square grounding probe clips to zero");
    AssertEqual(19, groundedBody.Position.Y.Whole, "grounding collision preserves resting center Y");
    AssertEqual(0x12, level.GetCollisionBlockAtPixel(new(24, 24)).Bts.Value,
        "grounding collision probe's target block carries slope BTS");

    // Moving two pixels right from X=26 would enter the type-8 block at X=32. The solid
    // formula permits one pixel, writes subposition $FFFF, and stops center X at 27.FFFF.
    var wallBody = new SamusKinematicsState
    {
        Position = FixedPosition<RoomPixels>.FromWhole(new(26, 24)),
        Radius = new(5, 5),
        HorizontalSlopeCollisionEnable = 0,
    };
    BlockMoveResult wallMove = SamusBlockCollision.MoveHorizontal(new SamusTerrain { Level = level }, wallBody, null, new HorizontalBlockMove { Displacement = 0x00020000 });
    AssertTrue(wallMove.Collided, "horizontal type-8 solid collision flag");
    AssertEqual(0x00010000, wallMove.AcceptedDisplacement, "horizontal type-8 solid clipped amount");
    AssertEqual(27, wallBody.Position.X.Whole, "horizontal type-8 solid whole X");
    AssertEqual(0xffff, wallBody.Position.X.Fraction, "horizontal type-8 solid right-wall fraction");

    // An air-only downward move preserves all 16.16 bits and has no collision record.
    var airBody = new SamusKinematicsState
    {
        // X=56 with radius five spans pixels 51..60, entirely inside air column three.
        Position = new(new(56, 0), new(19, 0x4000)),
        Radius = new(5, 5),
    };
    BlockMoveResult airMove = SamusBlockCollision.MoveVertical(new SamusTerrain { Level = level }, airBody, null, new VerticalBlockMove { Displacement = 0x00008000, ScanLeftToRight = false });
    AssertTrue(!airMove.Collided, "vertical type-0 air has no collision");
    AssertEqual(19, airBody.Position.Y.Whole, "vertical air fractional move whole Y");
    AssertEqual(0xc000, airBody.Position.Y.Fraction, "vertical air fractional move subposition");

    // Shape four has all four $94:8E54 quadrant bytes set. Unlike an ordinary solid, its
    // clipping boundary is the leading 8-pixel half, although this fixture reaches the
    // block's first half so the permitted amount is one whole pixel.
    var squareForeground = new ushort[width * height];
    var squareBehavior = new byte[squareForeground.Length];
    squareForeground[1 * width + 2] = 0x1000;
    squareBehavior[1 * width + 2] = 0x04;
    squareForeground[2 * width + 1] = 0x1000;
    squareBehavior[2 * width + 1] = 0x04;
    RoomLevelData squareLevel = CreateRoom(new(width, height), squareForeground, squareBehavior);

    var squareWallBody = new SamusKinematicsState
    {
        Position = FixedPosition<RoomPixels>.FromWhole(new(26, 24)),
        Radius = new(5, 5),
        HorizontalSlopeCollisionEnable = 0,
    };
    BlockMoveResult squareWall = SamusBlockCollision.MoveHorizontal(new SamusTerrain { Level = squareLevel }, squareWallBody, null, new HorizontalBlockMove { Displacement = 0x00020000 });
    AssertTrue(squareWall.Collided, "horizontal fully-solid square slope collision");
    AssertEqual(0x00010000, squareWall.AcceptedDisplacement, "horizontal square-slope 8-pixel clipping");
    AssertEqual(0xffff, squareWallBody.Position.X.Fraction, "horizontal square slope writes right-wall fraction");

    var squareFloorBody = new SamusKinematicsState
    {
        Position = FixedPosition<RoomPixels>.FromWhole(new(16, 26)),
        Radius = new(5, 5),
    };
    BlockMoveResult squareFloor = SamusBlockCollision.MoveVertical(new SamusTerrain { Level = squareLevel }, squareFloorBody, null, new VerticalBlockMove { Displacement = 0x00020000, ScanLeftToRight = true });
    AssertTrue(squareFloor.Collided, "vertical fully-solid square slope collision");
    AssertEqual(0x00010000, squareFloor.AcceptedDisplacement, "vertical square-slope 8-pixel clipping");
    AssertTrue(squareFloorBody.PositionAdjustedBySlope, "downward square slope sets adjusted flag");
    AssertEqual(0xffff, squareFloorBody.Position.Y.Fraction, "downward square slope writes floor fraction");

    // `$94:938B` masks the BTS index, resolves the room's compiled door list, then inspects
    // the destination. Crateria room $00/$08 contains both cases: entry zero is a normal
    // room door, while entry two is the shared `$83:88FC` elevator pseudo-door whose zero
    // destination retains ordinary solid clipping.
    const ushort doorListPointer = 0x94f3;
    const ushort normalDoorPointer = 0x8a4e;

    var doorForeground = new ushort[width * height];
    var doorBehavior = new byte[doorForeground.Length];
    doorForeground[1 * width + 2] = 0x9000;
    RoomLevelData doorLevel = new RoomLevelData(new Extent16<RoomBlocks>(width, height), new RoomLevelPlanes(doorForeground, doorBehavior, new ushort[doorForeground.Length]), new BackgroundTile[4], doors: RoomDoorListDefinitions.ForListPointer(doorListPointer));
    var doorBody = new SamusKinematicsState
    {
        Position = FixedPosition<RoomPixels>.FromWhole(new(26, 24)),
        Radius = new(5, 5),
        HorizontalSlopeCollisionEnable = 0,
    };
    BlockMoveResult normalDoorMove = SamusBlockCollision.MoveHorizontal(new SamusTerrain { Level = doorLevel }, doorBody, null, new HorizontalBlockMove { Displacement = 0x00020000 });
    AssertTrue(!normalDoorMove.Collided, "normal type-$9 door returns carry clear");
    AssertEqual(0x00020000, normalDoorMove.AcceptedDisplacement,
        "normal type-$9 door accepts full horizontal displacement");
    AssertEqual(normalDoorPointer, (doorLevel.PendingDoorTransition ?? throw new InvalidOperationException("Verification requires doorLevel.PendingDoorTransition.")).Pointer,
        "normal type-$9 door publishes exact bank-$83 pointer");
    AssertEqual(RoomHeaderDefinitions.ForgottenHighwayElbow, doorLevel.PendingDoorTransition.DestinationRoom,
        "normal type-$9 door preserves destination room header");
    AssertEqual(normalDoorPointer, (doorLevel.ConsumePendingDoorTransition() ?? throw new InvalidOperationException("Verification requires doorLevel.ConsumePendingDoorTransition().")).Pointer,
        "door transition publication is consumed exactly once");
    AssertTrue(doorLevel.PendingDoorTransition is null,
        "consumed door transition does not leak into later movement");

    doorBehavior[1 * width + 2] = 2;
    RoomLevelData elevatorDoorLevel = new RoomLevelData(new Extent16<RoomBlocks>(width, height), new RoomLevelPlanes(doorForeground, doorBehavior, new ushort[doorForeground.Length]), new BackgroundTile[4], doors: RoomDoorListDefinitions.ForListPointer(doorListPointer));
    doorBody.Position = doorBody.Position.WithWholeX(26).WithFractionX(0);
    BlockMoveResult elevatorDoorMove = SamusBlockCollision.MoveHorizontal(new SamusTerrain { Level = elevatorDoorLevel }, doorBody, null, new HorizontalBlockMove { Displacement = 0x00020000 });
    AssertTrue(elevatorDoorMove.Collided, "elevator pseudo-door remains solid");
    AssertEqual(0x00010000, elevatorDoorMove.AcceptedDisplacement,
        "elevator pseudo-door uses ordinary solid clipping");
    AssertTrue(elevatorDoorLevel.PendingDoorTransition is null,
        "elevator pseudo-door does not publish normal room transition");

    // The room loader fills all unused level-data allocation words with `$8000` before
    // decompression. Put Samus one pixel inside the logical right and bottom boundaries;
    // a two-pixel request samples the first prefilled word and must clip like type-8 solid.
    var rightEdgeBody = new SamusKinematicsState
    {
        Position = FixedPosition<RoomPixels>.FromWhole(new(58, 24)),
        Radius = new(5, 5),
        HorizontalSlopeCollisionEnable = 0,
    };
    BlockMoveResult rightEdgeMove = SamusBlockCollision.MoveHorizontal(new SamusTerrain { Level = level }, rightEdgeBody, null, new HorizontalBlockMove { Displacement = 0x00020000 });
    AssertTrue(rightEdgeMove.Collided, "prefilled right room edge is solid");
    AssertEqual(0x00010000, rightEdgeMove.AcceptedDisplacement,
        "prefilled right room edge clips at logical boundary");
    AssertEqual(0x8000, level.GetCollisionBlockOrPrefilledSolid(new(4, 1)).PackedWord.Raw,
        "prefilled right room edge probe reads native $8000 word");

    var bottomEdgeBody = new SamusKinematicsState
    {
        Position = FixedPosition<RoomPixels>.FromWhole(new(56, 58)),
        Radius = new(5, 5),
    };
    BlockMoveResult bottomEdgeMove = SamusBlockCollision.MoveVertical(new SamusTerrain { Level = level }, bottomEdgeBody, null, new VerticalBlockMove { Displacement = 0x00020000, ScanLeftToRight = true });
    AssertTrue(bottomEdgeMove.Collided, "prefilled bottom room edge is solid");
    AssertEqual(0x00010000, bottomEdgeMove.AcceptedDisplacement,
        "prefilled bottom room edge clips at logical boundary");
    AssertEqual(0x8000, level.GetCollisionBlockOrPrefilledSolid(new(3, 4)).PackedWord.Raw,
        "prefilled bottom room edge probe reads native $8000 word");

    // Grapple endpoint/body collision now consumes this same room-owned seam instead of
    // maintaining a second interpretation of the native prefill. Check coordinate and
    // already-linear forms because bank-$94 uses both during extension redispatch.
    RoomCollisionBlock coordinatePrefill =
        level.GetCollisionBlockOrPrefilledSolid(new(level.Size.X, 0));
    AssertEqual(-1, coordinatePrefill.Index,
        "coordinate prefill is outside the logical room plane");
    AssertEqual(0x8000, coordinatePrefill.PackedWord.Raw,
        "coordinate prefill exposes native solid word");
    RoomCollisionBlock linearPrefill =
        level.GetCollisionBlockByIndexOrPrefilledSolid(-1);
    AssertEqual(0x8000, linearPrefill.PackedWord.Raw,
        "linear extension prefill exposes native solid word");

    // Exercise every formerly open body-dispatch entry through the public movement seam.
    // Types 2/3/4/6/7 are non-solid for Samus's body.  Spike type A and special type B
    // retain their independent solid topology. Their authored damage/PLM producer seams do
    // not alter the clipping result asserted here and are deliberately not claimed by this
    // kinematics-only fixture.
    foreach ((ushort collisionWord, bool expectedCollision) in new[]
    {
        new SamusTerrainMovementVerifySamusBlockCollisionCase2(0x2000, false),
        new SamusTerrainMovementVerifySamusBlockCollisionCase2(0x3000, false),
        new SamusTerrainMovementVerifySamusBlockCollisionCase2(0x4000, false),
        new SamusTerrainMovementVerifySamusBlockCollisionCase2(0x6000, false),
        new SamusTerrainMovementVerifySamusBlockCollisionCase2(0x7000, false),
        new SamusTerrainMovementVerifySamusBlockCollisionCase2(0xa000, true),
        new SamusTerrainMovementVerifySamusBlockCollisionCase2(0xb000, true),
    })
    {
        var dispatcherWords = new ushort[width * height];
        dispatcherWords[1 * width + 2] = collisionWord;
        RoomLevelData dispatcherLevel = CreateRoom(new(width, height),
            dispatcherWords,
            new byte[dispatcherWords.Length]);
        var dispatcherBody = new SamusKinematicsState
        {
            Position = FixedPosition<RoomPixels>.FromWhole(new(26, 24)),
            Radius = new(5, 5),
            HorizontalSlopeCollisionEnable = 0,
        };
        BlockMoveResult dispatcherMove = SamusBlockCollision.MoveHorizontal(new SamusTerrain { Level = dispatcherLevel }, dispatcherBody, null, new HorizontalBlockMove { Displacement = 0x00020000 });

        AssertEqual(expectedCollision, dispatcherMove.Collided,
            $"body dispatcher type ${(collisionWord >> 12):X1} collision topology");
        AssertEqual(expectedCollision ? 0x00010000 : 0x00020000,
            dispatcherMove.AcceptedDisplacement,
            $"body dispatcher type ${(collisionWord >> 12):X1} accepted displacement");
    }

    // Unlike the ownerless topology fixtures above, a live Samus-owned kinematics child
    // must publish the exact `$94:8E83/$8ECF/$8F0A` spike damage before clipping. Exercise
    // every damaging table entry plus the harmless entry two with an identical floor hit.
    WritePoseDefinitionByte(bus, (int)SamusPoseId.FacingRightNormalPose, 0, (byte)SamusFacingDirection.Right);
    foreach ((byte spikeBehavior, ushort expectedDamage) in new SamusTerrainMovementVerifySamusBlockCollisionCase[]
    {
        new SamusTerrainMovementVerifySamusBlockCollisionCase((byte)SpikeBlockBehavior.Heavy,
            SpikeHazardDefinitions.HeavySpikeDamage),
        new SamusTerrainMovementVerifySamusBlockCollisionCase((byte)SpikeBlockBehavior.Light,
            SpikeHazardDefinitions.LightSpikeDamage),
        new SamusTerrainMovementVerifySamusBlockCollisionCase((byte)SpikeBlockBehavior.AlternateLight,
            SpikeHazardDefinitions.LightSpikeDamage),
        new SamusTerrainMovementVerifySamusBlockCollisionCase(2, 0),
    })
    {
        var spikeWords = new ushort[width * height];
        var spikeBehaviors = new byte[spikeWords.Length];
        spikeWords[2 * width + 1] = 0xa000;
        spikeBehaviors[2 * width + 1] = spikeBehavior;
        RoomLevelData spikeLevel = CreateRoom(new(width, height), spikeWords, spikeBehaviors);
        var spikeSamus = new SamusState
        {
            Pose = SamusPoseId.FacingRightNormalPose,
            Position = FixedPosition<RoomPixels>.FromWhole(new(24, 26)),
        };
        spikeSamus.Kinematics.Radius = new(5, 5);
        SamusTerrainHazardCollision.PrepareFrame(
            spikeLevel,
            spikeSamus,
            AreaId.Norfair,
            areaBossDefeated: false);

        BlockMoveResult spikeFloor = SamusBlockCollision.MoveVertical(new SamusTerrain { Level = spikeLevel }, spikeSamus.Kinematics, spikeSamus, new VerticalBlockMove { Displacement = 2 << 16, ScanLeftToRight = true });
        AssertTrue(spikeFloor.Collided,
            $"spike-block BTS ${spikeBehavior:X2} retains solid collision");
        AssertEqual(expectedDamage, spikeSamus.LiquidPhysics.PeriodicDamage,
            $"spike-block BTS ${spikeBehavior:X2} publishes cartridge damage");
        AssertEqual(expectedDamage == 0 ? 0 : SpikeHazardDefinitions.InvincibilityFrames,
            spikeSamus.InvincibilityTimer.RawValue,
            $"spike-block BTS ${spikeBehavior:X2} publishes invincibility timer");
        AssertEqual(expectedDamage == 0 ? 0 : SpikeHazardDefinitions.KnockbackFrames,
            spikeSamus.KnockbackTimer.RawValue,
            $"spike-block BTS ${spikeBehavior:X2} publishes knockback timer");
        AssertEqual(KnockbackHorizontalDirection.Left, spikeSamus.KnockbackXDirection,
            $"right-facing spike-block BTS ${spikeBehavior:X2} knocks left");
    }

    // BTS-zero Wrecked Ship spikes are the one room-state-dependent table entry. They are
    // harmless before the area boss bit and become 60-damage spikes after it is set.
    foreach ((bool bossDefeated, ushort expectedDamage) in new[]
    {
        new VerifySamusBlockCollisionCase(false, 0),
        new VerifySamusBlockCollisionCase(true, SpikeHazardDefinitions.HeavySpikeDamage),
    })
    {
        var spikeWords = new ushort[width * height];
        var spikeBehaviors = new byte[spikeWords.Length];
        spikeWords[2 * width + 1] = 0xa000;
        spikeBehaviors[2 * width + 1] = (byte)SpikeBlockBehavior.Heavy;
        RoomLevelData wreckedShipSpikes = CreateRoom(new(width, height), spikeWords, spikeBehaviors);
        var spikeSamus = new SamusState
        {
            Pose = SamusPoseId.FacingRightNormalPose,
            Position = FixedPosition<RoomPixels>.FromWhole(new(24, 26)),
        };
        spikeSamus.Kinematics.Radius = new(5, 5);
        SamusTerrainHazardCollision.PrepareFrame(
            wreckedShipSpikes,
            spikeSamus,
            AreaId.WreckedShip,
            bossDefeated);
        _ = SamusBlockCollision.MoveVertical(new SamusTerrain { Level = wreckedShipSpikes }, spikeSamus.Kinematics, spikeSamus, new VerticalBlockMove { Displacement = 2 << 16, ScanLeftToRight = true });
        AssertEqual(expectedDamage, spikeSamus.LiquidPhysics.PeriodicDamage,
            $"Wrecked Ship BTS-zero spike boss gate {bossDefeated}");
    }

    // `$94:9B60` visits bottom, center, and top only when they occupy distinct block rows.
    // Put the damaging spike-air tile at each point in turn and assert the same side effects.
    foreach (int spikeRow in new[] { 0, 1, 2 })
    {
        var spikeAirWords = new ushort[width * height];
        var spikeAirBehaviors = new byte[spikeAirWords.Length];
        spikeAirWords[spikeRow * width + 1] = 0x2000;
        spikeAirBehaviors[spikeRow * width + 1] =
            SpikeHazardDefinitions.DamagingSpikeAirBehavior;
        RoomLevelData spikeAirLevel = CreateRoom(new(width, height),
            spikeAirWords,
            spikeAirBehaviors);
        var spikeAirSamus = new SamusState
        {
            Pose = SamusPoseId.FacingRightNormalPose,
            Position = FixedPosition<RoomPixels>.FromWhole(new(24, 24)),
        };
        spikeAirSamus.Kinematics.Radius = spikeAirSamus.Kinematics.Radius.WithY(17);
        SamusTerrainHazardCollision.PrepareFrame(
            spikeAirLevel,
            spikeAirSamus,
            AreaId.Norfair,
            areaBossDefeated: false);
        AssertEqual(SpikeHazardDefinitions.LightSpikeDamage,
            spikeAirSamus.LiquidPhysics.PeriodicDamage,
            $"inside-block spike-air damages at sampled row {spikeRow}");
        AssertEqual(SpikeHazardDefinitions.InvincibilityFrames,
            spikeAirSamus.InvincibilityTimer.RawValue,
            $"inside-block spike-air starts invincibility at sampled row {spikeRow}");
    }

    var contactSafeWords = new ushort[width * height];
    var contactSafeBehaviors = new byte[contactSafeWords.Length];
    contactSafeWords[1 * width + 1] = 0x2000;
    contactSafeBehaviors[1 * width + 1] =
        SpikeHazardDefinitions.DamagingSpikeAirBehavior;
    RoomLevelData contactSafeSpikeAir = CreateRoom(new(width, height),
        contactSafeWords,
        contactSafeBehaviors);
    var contactSafeSamus = new SamusState
    {
        Pose = SamusPoseId.FacingRightNormalPose,
        Position = FixedPosition<RoomPixels>.FromWhole(new(24, 24)),
    };
    contactSafeSamus.Kinematics.Radius = contactSafeSamus.Kinematics.Radius.WithY(5);
    contactSafeSamus.HorizontalSpeed.ContactDamageIndex = SamusContactDamageIndex.SpeedBooster;
    SamusTerrainHazardCollision.PrepareFrame(
        contactSafeSpikeAir,
        contactSafeSamus,
        AreaId.Norfair,
        areaBossDefeated: false);
    AssertEqual(0, contactSafeSamus.LiquidPhysics.PeriodicDamage,
        "active contact-damage body suppresses spike-air damage");

    Console.WriteLine("  Samus blocks: all sixteen dispatchers, spans, slopes, doors, and ordinary spike damage agree.");
}

/// <summary>
/// Verifies the bank-$90 ordering around the already-isolated speed and bank-$94 scans:
/// running accelerates, moves horizontally, then probes down; standing probes and clears.
/// </summary>
internal static void VerifySamusGroundedMovement()
{
    var bus = new TestAddressSpace();

    SeedRunningNormalAirSpeedRecord(bus);

    // Shape $12 scales grounded horizontal displacement by $00C0/256 = 3/4 and exposes an
    // eight-pixel surface at X nibble eight. Row one is an uninterrupted two-block floor so
    // the radius scan can visit either column without introducing another dispatcher type.
    RoomLevelData level = CreateRoom(new(2, 3),
        [0, 0, 0x1000, 0x1000, 0, 0],
        [0, 0, 0x12, 0x12, 0, 0]);

    var running = new SamusState { Pose = SamusPoseId.MovingRightNormalPose };
    running.Kinematics.Position = running.Kinematics.Position.WithWhole(new(24, 19));
    running.Kinematics.Radius = new(5, 5);
    GroundedMovementResult first = SamusGroundedMovement.StepRunning(

        level,
        running,
        frameCounter: 0);

    AssertEqual(0x0000, running.HorizontalSpeed.BaseSpeed.Whole, "running first acceleration whole speed");
    AssertEqual(0x3000, running.HorizontalSpeed.BaseSpeed.Fraction, "running first acceleration fractional speed");
    AssertEqual(0x00002400, first.Horizontal.AcceptedDisplacement, "running slope-scaled horizontal amount");
    AssertEqual(0x2400, running.Kinematics.Position.X.Fraction, "running horizontal amount reaches X subposition");
    AssertTrue(first.Vertical.Collided, "running total-speed-plus-one grounding probe collides");
    AssertEqual(0, first.Vertical.AcceptedDisplacement, "running grounding probe clips at surface");

    // A second frame proves that acceleration state persists and that odd-NMI scan order
    // produces the same single-surface result in this deliberately symmetric fixture.
    GroundedMovementResult second = SamusGroundedMovement.StepRunning(

        level,
        running,
        frameCounter: 1);
    AssertEqual(0x6000, running.HorizontalSpeed.BaseSpeed.Fraction, "running second acceleration accumulates");
    AssertEqual(0x00004800, second.Horizontal.AcceptedDisplacement, "second running slope multiplier");
    AssertEqual(0x6c00, running.Kinematics.Position.X.Fraction, "second running displacement accumulates");
    AssertTrue(second.Vertical.Collided, "odd-frame grounding scan collides");

    // Momentum routine one selects acceleration mode two after Right is released. The next
    // $90:9A7E pass subtracts $0000.8000; from $0000.6000 this underflows the signed high
    // word and therefore clears both base halves and restores mode zero.
    running.HorizontalSpeed.AccelerationMode = SamusAccelerationMode.Decelerating;
    GroundedMovementResult decelerated = SamusGroundedMovement.StepRunning(

        level,
        running,
        frameCounter: 0);
    AssertEqual(0, running.HorizontalSpeed.BaseSpeed.Whole, "running deceleration underflow clears whole speed");
    AssertEqual(0, running.HorizontalSpeed.BaseSpeed.Fraction, "running deceleration underflow clears fraction");
    AssertEqual(SamusAccelerationMode.Accelerating, running.HorizontalSpeed.AccelerationMode, "running deceleration underflow restores acceleration mode");
    AssertEqual(0, decelerated.Horizontal.AcceptedDisplacement, "cleared deceleration frame has no X displacement");

    // Standing executes its zero-base MoveX/grounding calls before clearing momentum. Base
    // speed itself is not included in Move_NoBaseSpeed_X, so the body remains on the same X.
    var standing = new SamusState { Pose = SamusPoseId.FacingRightNormalPose };
    standing.Kinematics.Position = standing.Kinematics.Position.WithWhole(new(24, 19));
    standing.Kinematics.Radius = new(5, 5);
    standing.HorizontalSpeed.BaseSpeed = new UnsignedSixteenSixteen(2, 0xc000);
    GroundedMovementResult idle = SamusGroundedMovement.StepStanding(

        level,
        standing,
        frameCounter: 0,
        controllerInput: 0);
    AssertEqual(0, idle.Horizontal.AcceptedDisplacement, "standing no-base horizontal amount");
    AssertTrue(idle.Vertical.Collided, "standing one-pixel grounding probe collides");
    AssertEqual(0, standing.HorizontalSpeed.BaseSpeed.Whole, "standing clears base speed whole");
    AssertEqual(0, standing.HorizontalSpeed.BaseSpeed.Fraction, "standing clears base speed fraction");

    // The front-view branch returns before every ordinary standing movement call. Seed
    // deliberately stale motion/collision values to prove the sole native write is the
    // vertical-result clear and that no host convenience cleanup leaked into this path.
    var forward = new SamusState
    {
        Pose = SamusPoseId.ForwardFacingPowerSuitPose,
        Position = FixedPosition<RoomPixels>.FromWhole(new(0x1234, 0x5678)),
        SolidVerticalCollisionResult = SamusVerticalCollisionResult.HitCeiling,
    };
    forward.HorizontalSpeed.BaseSpeed = new UnsignedSixteenSixteen(2, 0x3456);
    forward.HorizontalSpeed.ExtraRunSpeed = forward.HorizontalSpeed.ExtraRunSpeed.WithWhole(1);
    BlockMoveResult? stationaryForward = SamusGroundedMovement.StepFacingForward(

        level,
        forward,
        frameCounter: 0);
    AssertTrue(stationaryForward is null, "zero elevator status skips the vertical scan");
    AssertEqual(SamusVerticalCollisionResult.None, forward.SolidVerticalCollisionResult, "forward movement clears vertical collision result");
    AssertEqual(0x1234, forward.Position.X.Whole, "forward movement does not scan or move X");
    AssertEqual(0x5678, forward.Position.Y.Whole, "stationary forward movement does not move Y");
    AssertEqual(2, forward.HorizontalSpeed.BaseSpeed.Whole, "forward dispatcher retains stale base speed");
    AssertEqual(0x3456, forward.HorizontalSpeed.BaseSpeed.Fraction, "forward dispatcher retains stale base subspeed");
    AssertEqual(1, forward.HorizontalSpeed.ExtraRunSpeed.Whole, "forward dispatcher retains stale extra speed");

    // `$90:A392` consumes actor-owned elevator status but performs Samus's movement itself.
    // Put a valid solid-enemy candidate exactly one pixel below an all-air room body: the
    // ordinary MoveVertical wrapper would stop on it, whereas `$94:9763` must deliberately
    // ignore it and accept the complete 1.0000 downward displacement.
    RoomLevelData elevatorLevel = CreateEmptyRoom(new(8, 8));
    forward.Position = forward.Position.WithWhole(new(100, 100));
    forward.Kinematics.Position = forward.Kinematics.Position.WithFractionX(0x1234).WithFractionY(0x5678);
    forward.Kinematics.Radius = new(5, 10);
    forward.Kinematics.InteractiveEnemies =
    [
        new SolidEnemyCollisionBody(
            Slot: 7, Index: 0x01c0,
            Position: new(100, 116),
            Radius: new(5, 5),
            Solid: true),
    ];
    forward.SolidVerticalCollisionResult = SamusVerticalCollisionResult.HitCeiling;
    BlockMoveResult? elevatorMove = SamusGroundedMovement.StepFacingForward(

        elevatorLevel,
        forward,
        frameCounter: 1,
        elevatorIsMoving: true);
    AssertTrue(elevatorMove is { Collided: false }, "elevator block scan accepts clear air");
    BlockMoveResult acceptedElevatorMove = elevatorMove ?? throw new InvalidOperationException(
        "A nonzero elevator status must execute the one-pixel vertical scan.");
    AssertTrue(acceptedElevatorMove.EnemyCollision is null,
        "elevator `$94:9763` path skips a colliding solid enemy");
    AssertEqual(0x00010000, acceptedElevatorMove.AcceptedDisplacement,
        "elevator requests exact one-pixel downward displacement");
    AssertEqual(101, forward.Position.Y.Whole, "elevator moves forward-facing Samus down one pixel");
    AssertEqual(0x5678, forward.Kinematics.Position.Y.Fraction,
        "whole-pixel elevator motion preserves Y fraction");
    AssertEqual(SamusVerticalCollisionResult.None, forward.SolidVerticalCollisionResult,
        "elevator path clears vertical collision result after movement");

    // Every nonzero status executes `$90:A392`'s scan; destination shaft scroll PLMs rely
    // on that collision pass while the actor pins Samus. The collapsed host transition must
    // suppress only the redundant pseudo-door publication during statuses two/three.
    AssertTrue(
        RoomMainRoutines.ShouldPublishFacingForwardElevatorDoorSideEffects(
            ElevatorActorStatus.Departing),
        "departing elevator publishes forward pseudo-door contact");
    AssertTrue(
        !RoomMainRoutines.ShouldPublishFacingForwardElevatorDoorSideEffects(
            ElevatorActorStatus.Inactive),
        "inactive elevator suppresses pseudo-door contact");
    AssertTrue(
        !RoomMainRoutines.ShouldPublishFacingForwardElevatorDoorSideEffects(
            ElevatorActorStatus.BeginArrivalReturn),
        "arrival setup suppresses repeated pseudo-door contact");
    AssertTrue(
        !RoomMainRoutines.ShouldPublishFacingForwardElevatorDoorSideEffects(
            ElevatorActorStatus.ReturningToRest),
        "arrival return suppresses repeated pseudo-door contact");

    Console.WriteLine("  Samus movement: forward/elevator, standing, and running speed/X/slope/grounding order agree.");
}

/// <summary>
/// Locks down the newly translated leftward and grounded-reversal routes independently of
/// WinForms: literal pose bytes, command $F8 timing, mode-one direction inversion, and the
/// mode-clear boundary are all observable here under a normal debugger.
/// </summary>
internal static void VerifySamusGroundedReversal()
{
    var bus = new TestAddressSpace();

    // Running and turning resolve their compiled native rows independently. Both have
    // 0.8000 deceleration, while only running has the 2.C000 maximum.

    // A broad row of ordinary type-$8 solids removes slope scaling from this test. Center
    // Y=11 with radius 5 rests exactly on the row-one top edge at physical Y=16.
    const int width = 8;
    var foreground = new ushort[width * 3];
    for (int x = 0; x < width; x++)
        foreground[width + x] = 0x8000;
    RoomLevelData level = CreateRoom(new(width, 3), foreground, new byte[foreground.Length]);

    var movingLeft = new SamusState { Pose = SamusPoseId.MovingLeftNormalPose };
    movingLeft.Kinematics.Position = movingLeft.Kinematics.Position.WithWhole(new(64, 11));
    movingLeft.Kinematics.Radius = new(5, 5);
    GroundedMovementResult leftFrame = SamusGroundedMovement.StepRunning(

        level,
        movingLeft,
        frameCounter: 0);
    AssertEqual(-0x00003000, leftFrame.Horizontal.AcceptedDisplacement, "running-left signed displacement");
    AssertEqual(63, movingLeft.Position.X.Whole, "running-left borrows into whole X");
    AssertEqual(0xd000, movingLeft.Kinematics.Position.X.Fraction, "running-left fractional X");
    AssertTrue(leftFrame.Vertical.Collided, "running-left grounding probe collides");

    // `$90:8EA9` does not reserve acceleration mode one for turn poses. If a transition
    // leaves it installed after the body has already become `$09/$0A`, the visible pose's
    // literal direction is inverted for that frame. Start above the deceleration quantum so
    // `$90:9B0A` does not clear the mode before the direction selector observes it.
    var reversedRunningRight = new SamusState { Pose = SamusPoseId.MovingRightNormalPose };
    reversedRunningRight.Kinematics.Position = reversedRunningRight.Kinematics.Position.WithWhole(new(64, 11));
    reversedRunningRight.Kinematics.Radius = new(5, 5);
    reversedRunningRight.HorizontalSpeed.BaseSpeed = reversedRunningRight.HorizontalSpeed.BaseSpeed.WithWhole(1);
    reversedRunningRight.HorizontalSpeed.AccelerationMode = SamusAccelerationMode.Turning;
    GroundedMovementResult reversedRightFrame = SamusGroundedMovement.StepRunning(

        level,
        reversedRunningRight,
        frameCounter: 0);
    AssertEqual(-0x00008000, reversedRightFrame.Horizontal.AcceptedDisplacement,
        "mode-one running-right carries leftward momentum");

    var reversedRunningLeft = new SamusState { Pose = SamusPoseId.MovingLeftNormalPose };
    reversedRunningLeft.Kinematics.Position = reversedRunningLeft.Kinematics.Position.WithWhole(new(64, 11));
    reversedRunningLeft.Kinematics.Radius = new(5, 5);
    reversedRunningLeft.HorizontalSpeed.BaseSpeed = reversedRunningLeft.HorizontalSpeed.BaseSpeed.WithWhole(1);
    reversedRunningLeft.HorizontalSpeed.AccelerationMode = SamusAccelerationMode.Turning;
    GroundedMovementResult reversedLeftFrame = SamusGroundedMovement.StepRunning(

        level,
        reversedRunningLeft,
        frameCounter: 0);
    AssertEqual(0x00008000, reversedLeftFrame.Horizontal.AcceptedDisplacement,
        "mode-one running-left carries rightward momentum");

    // The production movement port now also validates the pose's literal dispatcher byte,
    // so seed the two ordinary turn definitions before isolating their displacement.
    WritePoseDefinition(bus, (int)SamusPoseId.TurningRightToLeftPose, [0x04, 0x0e, 0xff, 0xfb, 0x06, 0x00, 0x15, 0x00]);
    WritePoseDefinition(bus, (int)SamusPoseId.TurningLeftToRightPose, [0x08, 0x0e, 0xff, 0xfb, 0x06, 0x00, 0x15, 0x00]);

    // Pose $25 is already facing left ($04), but mode one makes $90:8EA9 choose the
    // opposite/right helper while its $0E speed record decelerates old momentum.
    var turnTowardLeft = new SamusState { Pose = SamusPoseId.TurningRightToLeftPose };
    turnTowardLeft.Kinematics.Position = turnTowardLeft.Kinematics.Position.WithWhole(new(64, 11));
    turnTowardLeft.Kinematics.Radius = new(5, 5);
    turnTowardLeft.HorizontalSpeed.BaseSpeed = turnTowardLeft.HorizontalSpeed.BaseSpeed.WithWhole(1);
    turnTowardLeft.HorizontalSpeed.AccelerationMode = SamusAccelerationMode.Turning;
    GroundedMovementResult carriedRight = SamusGroundedMovement.StepTurningOnGround(

        level,
        turnTowardLeft,
        frameCounter: 0);
    AssertEqual(0x8000, turnTowardLeft.HorizontalSpeed.BaseSpeed.Fraction, "left-turn native deceleration amount");
    AssertEqual(0x00008000, carriedRight.Horizontal.AcceptedDisplacement, "left-turn carries rightward momentum");

    // Pose $26 is the mirror: it displays a right-facing turn but carries old momentum to
    // the left. This is not a host sign choice; it is the other branch of $90:8EA9.
    var turnTowardRight = new SamusState { Pose = SamusPoseId.TurningLeftToRightPose };
    turnTowardRight.Kinematics.Position = turnTowardRight.Kinematics.Position.WithWhole(new(64, 11));
    turnTowardRight.Kinematics.Radius = new(5, 5);
    turnTowardRight.HorizontalSpeed.BaseSpeed = turnTowardRight.HorizontalSpeed.BaseSpeed.WithWhole(1);
    turnTowardRight.HorizontalSpeed.AccelerationMode = SamusAccelerationMode.Turning;
    GroundedMovementResult carriedLeft = SamusGroundedMovement.StepTurningOnGround(

        level,
        turnTowardRight,
        frameCounter: 1);
    AssertEqual(-0x00008000, carriedLeft.Horizontal.AcceptedDisplacement, "right-turn carries leftward momentum");

    // Underflow clears speed and mode inside $90:9A7E before direction dispatch. The last
    // turn frame consequently requests exactly zero rather than crossing into new motion.
    var exhaustedTurn = new SamusState { Pose = SamusPoseId.TurningRightToLeftPose };
    exhaustedTurn.Kinematics.Position = exhaustedTurn.Kinematics.Position.WithWhole(new(64, 11));
    exhaustedTurn.Kinematics.Radius = new(5, 5);
    exhaustedTurn.HorizontalSpeed.BaseSpeed = exhaustedTurn.HorizontalSpeed.BaseSpeed.WithFraction(0x2000);
    exhaustedTurn.HorizontalSpeed.AccelerationMode = SamusAccelerationMode.Turning;
    GroundedMovementResult stopped = SamusGroundedMovement.StepTurningOnGround(

        level,
        exhaustedTurn,
        frameCounter: 0);
    AssertEqual(0, exhaustedTurn.HorizontalSpeed.BaseSpeed.Fraction, "turn underflow clears speed");
    AssertEqual(SamusAccelerationMode.Accelerating, exhaustedTurn.HorizontalSpeed.AccelerationMode, "turn underflow clears mode one");
    AssertEqual(0, stopped.Horizontal.AcceptedDisplacement, "turn underflow does not reverse early");

    // Seed the exact retail pose metadata and delay bytecode needed by $09->$25->$02.
    // The compact standing/running streams only provide valid frame-zero delays because
    // this test's subject is the turn stream itself.
    WritePoseDefinition(bus, (int)SamusPoseId.FacingLeftNormalPose, [0x04, 0x00, 0xff, 0x07, 0x06, 0x00, 0x15, 0x00]);
    WritePoseDefinition(bus, (int)SamusPoseId.MovingRightNormalPose, [0x08, 0x01, 0x01, 0x02, 0x06, 0x00, 0x15, 0x00]);
    WritePoseDefinition(bus, (int)SamusPoseId.MovingLeftNormalPose, [0x04, 0x01, 0x02, 0x07, 0x06, 0x00, 0x15, 0x00]);
    WritePoseDefinition(bus, (int)SamusPoseId.TurningRightToLeftPose, [0x04, 0x0e, 0xff, 0xfb, 0x06, 0x00, 0x15, 0x00]);
    WritePoseDefinition(bus, (int)SamusPoseId.TurningLeftToRightPose, [0x08, 0x0e, 0xff, 0xfb, 0x06, 0x00, 0x15, 0x00]);
    WritePoseAnimationDelays(bus, SamusPoseId.FacingLeftNormalPose, 0xc100, [0x0a]);
    WritePoseAnimationDelays(bus, SamusPoseId.MovingRightNormalPose, 0xc110, [0x02]);
    WritePoseAnimationDelays(bus, SamusPoseId.MovingLeftNormalPose, 0xc120, [0x02]);
    WritePoseAnimationDelays(bus, SamusPoseId.TurningRightToLeftPose, 0xc130, [0x02, 0x02, 0x02, 0xf8, 0x02]);
    WritePoseAnimationDelays(bus, SamusPoseId.TurningLeftToRightPose, 0xc140, [0x02, 0x02, 0x02, 0xf8, 0x01]);

    // `$91:F8D3` explicitly compares the OLD pose against `$00/$9B` before it reads any
    // shot-direction metadata. Both front-view bodies must therefore retain the generic
    // direction selected by their own transition tables. Cover both targets and both suit
    // variants; this prevents a future shortcut from silently assigning front-view Samus
    // an invented left or right facing.
    var forwardToLeft = new SamusState { Pose = SamusPoseId.ForwardFacingPowerSuitPose };
    forwardToLeft.HorizontalSpeed.BaseSpeed = forwardToLeft.HorizontalSpeed.BaseSpeed.WithFraction(0x8000);
    forwardToLeft.HorizontalSpeed.ExtraRunSpeed = forwardToLeft.HorizontalSpeed.ExtraRunSpeed.WithFraction(0x4000);
    SamusGroundedPoseTransitions.ApplyGroundedTurn(forwardToLeft, SamusPoseId.TurningRightToLeftPose);
    AssertEqual(SamusPoseId.TurningRightToLeftPose, forwardToLeft.Pose,
        "power-suit forward view retains generic left-turn target");
    AssertEqual(0xc000, forwardToLeft.HorizontalSpeed.BaseSpeed.Fraction,
        "forward left turn still folds extra run speed");
    AssertEqual(SamusAccelerationMode.Turning, forwardToLeft.HorizontalSpeed.AccelerationMode,
        "forward left turn selects mode one");

    var forwardToRight = new SamusState { Pose = SamusPoseId.ForwardFacingSuitedPose };
    SamusGroundedPoseTransitions.ApplyGroundedTurn(forwardToRight, SamusPoseId.TurningLeftToRightPose);
    AssertEqual(SamusPoseId.TurningLeftToRightPose, forwardToRight.Pose,
        "suited forward view retains generic right-turn target");
    AssertEqual(SamusAccelerationMode.Turning, forwardToRight.HorizontalSpeed.AccelerationMode,
        "forward right turn selects mode one");

    var animatedTurn = new SamusState { Pose = SamusPoseId.MovingRightNormalPose };
    animatedTurn.HorizontalSpeed.BaseSpeed = animatedTurn.HorizontalSpeed.BaseSpeed.WithFraction(0x8000);
    animatedTurn.HorizontalSpeed.ExtraRunSpeed = animatedTurn.HorizontalSpeed.ExtraRunSpeed.WithFraction(0x4000);
    SamusGroundedPoseTransitions.ApplyGroundedTurn(animatedTurn, SamusPoseId.TurningRightToLeftPose);
    AssertEqual(SamusPoseId.TurningRightToLeftPose, animatedTurn.Pose, "input reversal installs pose $25");
    AssertEqual(0xc000, animatedTurn.HorizontalSpeed.BaseSpeed.Fraction, "turn setup folds extra into base speed");
    AssertEqual(0, animatedTurn.HorizontalSpeed.ExtraRunSpeed.Fraction, "turn setup consumes extra speed");
    AssertEqual(SamusAccelerationMode.Turning, animatedTurn.HorizontalSpeed.AccelerationMode, "turn setup selects mode one");

    // Three two-tick art frames lead to byte index three, command $F8. Its operand $02 is
    // pending until the transition seam; applying it initializes pose $02 frame zero.
    for (int tick = 0; tick < 6; tick++)
        SamusPresentation.AnimateNoFx(animatedTurn, new SamusAnimationStep { Random = new SharedRandom() });
    AssertEqual(SamusPoseId.FacingLeftNormalPose, (animatedTurn.PendingTransitionalPose ?? throw new InvalidOperationException("Verification requires animatedTurn.PendingTransitionalPose.")), "turn $F8 publishes left-standing pose");
    AssertTrue(SamusPoseTransitions.ApplyPendingVerifiedAnimationTransition(animatedTurn), "turn animation transition applies");
    AssertEqual(SamusPoseId.FacingLeftNormalPose, animatedTurn.Pose, "turn animation ends facing left");
    AssertEqual(0, animatedTurn.AnimationFrame, "turn completion resets animation frame");
    AssertEqual(10, animatedTurn.AnimationFrameTimer.RawValue, "left-standing delay initializes from ROM stream");

    SamusGroundedPoseTransitions.ApplyStandingLeftToRunningLeft(animatedTurn);
    AssertEqual(SamusPoseId.MovingLeftNormalPose, animatedTurn.Pose, "held left starts ordinary left run");
    SamusGroundedPoseTransitions.ApplyRunningLeftToStandingLeft(animatedTurn);
    AssertEqual(SamusPoseId.FacingLeftNormalPose, animatedTurn.Pose, "left run no-button fallback stands left");

    // The retail initializer does not blindly accept the generic `$25/$26` produced by
    // the input transition table. It reads byte three of the previous pose definition and
    // indexes `$91:F9C2`, preserving straight-up, diagonal-up, or diagonal-down aim. Seed
    // the six source definitions literally so this test will fail if either their native
    // metadata or the selector mapping is accidentally changed.
    AimedSource[] aimedSources =
    [
        new AimedSource(SamusPoseId.StandingAimUpRightPose, [0x08, 0x00, 0x01, 0x00, 0x06, 0x00, 0x15, 0x00]),
        new AimedSource(SamusPoseId.StandingAimUpLeftPose, [0x04, 0x00, 0x02, 0x09, 0x06, 0x00, 0x15, 0x00]),
        new AimedSource(SamusPoseId.StandingAimDiagonalUpRightPose, [0x08, 0x00, 0x01, 0x01, 0x06, 0x00, 0x15, 0x00]),
        new AimedSource(SamusPoseId.StandingAimDiagonalUpLeftPose, [0x04, 0x00, 0x02, 0x08, 0x06, 0x00, 0x15, 0x00]),
        new AimedSource(SamusPoseId.StandingAimDiagonalDownRightPose, [0x08, 0x00, 0x01, 0x03, 0x06, 0x00, 0x15, 0x00]),
        new AimedSource(SamusPoseId.StandingAimDiagonalDownLeftPose, [0x04, 0x00, 0x02, 0x06, 0x06, 0x00, 0x15, 0x00]),
    ];
    foreach ((SamusPoseId sourcePose, byte[] definition) in aimedSources)
    {
        WritePoseDefinition(bus, (int)sourcePose, definition);

        // Frame-zero delay belongs to the compiled native animation table. Do not
        // install a synthetic pointer or delay: those bus writes no longer affect
        // production pose transitions and would disguise a stale expectation.
    }

    // Each turn definition also comes directly from bank $91. `$FA` and `$FC` are the
    // aimed-turn shot-direction markers; they are intentionally preserved here rather
    // than normalized to ordinary direction bytes.
    AimedTurn[] aimedTurns =
    [
        new AimedTurn(SamusPoseId.TurningRightToLeftAimUpPose, [0x04, 0x0e, 0xff, 0xfa, 0x06, 0x00, 0x15, 0x00]),
        new AimedTurn(SamusPoseId.TurningLeftToRightAimUpPose, [0x08, 0x0e, 0xff, 0xfa, 0x06, 0x00, 0x15, 0x00]),
        new AimedTurn(SamusPoseId.TurningRightToLeftAimDiagonalDownPose, [0x04, 0x0e, 0xff, 0xfc, 0x06, 0x00, 0x15, 0x00]),
        new AimedTurn(SamusPoseId.TurningLeftToRightAimDiagonalDownPose, [0x08, 0x0e, 0xff, 0xfc, 0x06, 0x00, 0x15, 0x00]),
        new AimedTurn(SamusPoseId.TurningRightToLeftAimDiagonalUpPose, [0x04, 0x0e, 0xff, 0xfa, 0x06, 0x00, 0x15, 0x00]),
        new AimedTurn(SamusPoseId.TurningLeftToRightAimDiagonalUpPose, [0x08, 0x0e, 0xff, 0xfa, 0x06, 0x00, 0x15, 0x00]),
    ];
    foreach ((SamusPoseId turnPose, byte[] definition) in aimedTurns)
        WritePoseDefinition(bus, (int)turnPose, definition);

    // NTSC uses three two-tick art frames for every aimed grounded turn. `$F8` then
    // installs the corresponding opposite-facing standing-aim pose shown in this table.
    // The pinned cartridge gives straight-up poses a $02 frame-zero delay at
    // $91:B222 and diagonal poses a $10 delay at $91:B2B4.
    AimedTurnCase[] aimedTurnCases =
    [
        new AimedTurnCase(SamusPoseId.StandingAimUpRightPose, SamusPoseId.TurningRightToLeftPose,
            SamusPoseId.TurningRightToLeftAimUpPose, SamusPoseId.StandingAimUpLeftPose, 2),
        new AimedTurnCase(SamusPoseId.StandingAimUpLeftPose, SamusPoseId.TurningLeftToRightPose,
            SamusPoseId.TurningLeftToRightAimUpPose, SamusPoseId.StandingAimUpRightPose, 2),
        new AimedTurnCase(SamusPoseId.StandingAimDiagonalUpRightPose, SamusPoseId.TurningRightToLeftPose,
            SamusPoseId.TurningRightToLeftAimDiagonalUpPose, SamusPoseId.StandingAimDiagonalUpLeftPose, 16),
        new AimedTurnCase(SamusPoseId.StandingAimDiagonalUpLeftPose, SamusPoseId.TurningLeftToRightPose,
            SamusPoseId.TurningLeftToRightAimDiagonalUpPose, SamusPoseId.StandingAimDiagonalUpRightPose, 16),
        new AimedTurnCase(SamusPoseId.StandingAimDiagonalDownRightPose, SamusPoseId.TurningRightToLeftPose,
            SamusPoseId.TurningRightToLeftAimDiagonalDownPose, SamusPoseId.StandingAimDiagonalDownLeftPose, 16),
        new AimedTurnCase(SamusPoseId.StandingAimDiagonalDownLeftPose, SamusPoseId.TurningLeftToRightPose,
            SamusPoseId.TurningLeftToRightAimDiagonalDownPose, SamusPoseId.StandingAimDiagonalDownRightPose, 16),
    ];

    // Real standing/turn poses have radius 21, unlike the compact radius-five fixture used
    // above to isolate ordinary displacement arithmetic. Place their center at Y=27 over
    // a row-three floor: top Y=6 remains inside the room and bottom Y=48 touches that floor.
    var aimedForeground = new ushort[width * 5];
    for (int x = 0; x < width; x++)
        aimedForeground[width * 3 + x] = 0x8000;
    RoomLevelData aimedLevel = CreateRoom(new(width, 5), aimedForeground, new byte[aimedForeground.Length]);

    for (int caseIndex = 0; caseIndex < aimedTurnCases.Length; caseIndex++)
    {
        var testCase = aimedTurnCases[caseIndex];

        // Give every turn its own bytecode location. This catches swapped destinations
        // independently rather than allowing two cases to share a forgiving stream.
        ushort turnStreamAddress = (ushort)(0xc300 + caseIndex * 0x10);
        WritePoseAnimationDelays(bus, testCase.SelectedTurn, turnStreamAddress,
            [0x02, 0x02, 0x02, 0xf8, (byte)testCase.Destination]);

        var aimedTurn = new SamusState { Pose = testCase.SourcePose };
        aimedTurn.Kinematics.Position = aimedTurn.Kinematics.Position.WithWhole(new(64, 27));
        aimedTurn.Kinematics.Radius = new(5, 21);

        // `$91:F931` performs a 32-bit fixed-point add. These values force a fractional
        // carry, proving that the extra run component is folded before it is cleared.
        aimedTurn.HorizontalSpeed.BaseSpeed = new UnsignedSixteenSixteen(1, 0xd000);
        aimedTurn.HorizontalSpeed.ExtraRunSpeed = aimedTurn.HorizontalSpeed.ExtraRunSpeed.WithFraction(0x5000);
        SamusGroundedPoseTransitions.ApplyGroundedTurn(aimedTurn, testCase.GenericTurn);
        AssertEqual(testCase.SelectedTurn, aimedTurn.Pose, $"aimed turn selector case {caseIndex}");
        AssertEqual(2, aimedTurn.HorizontalSpeed.BaseSpeed.Whole, $"aimed turn speed carry case {caseIndex}");
        AssertEqual(0x2000, aimedTurn.HorizontalSpeed.BaseSpeed.Fraction, $"aimed turn folded fraction case {caseIndex}");
        AssertEqual(0, aimedTurn.HorizontalSpeed.ExtraRunSpeed.Fraction, $"aimed turn consumes extra fraction case {caseIndex}");
        AssertEqual(SamusAccelerationMode.Turning, aimedTurn.HorizontalSpeed.AccelerationMode, $"aimed turn mode one case {caseIndex}");

        GroundedMovementResult carriedMomentum = SamusGroundedMovement.StepTurningOnGround(

            aimedLevel,
            aimedTurn,
            frameCounter: (ushort)caseIndex);
        bool beganFacingRight = (caseIndex & 1) == 0;
        AssertTrue(
            beganFacingRight
                ? carriedMomentum.Horizontal.AcceptedDisplacement > 0
                : carriedMomentum.Horizontal.AcceptedDisplacement < 0,
            $"aimed turn preserves old momentum direction case {caseIndex}");

        for (int tick = 0; tick < 6; tick++)
            SamusPresentation.AnimateNoFx(aimedTurn, new SamusAnimationStep { Random = new SharedRandom() });
        AssertEqual(testCase.Destination, (aimedTurn.PendingTransitionalPose ?? throw new InvalidOperationException("Verification requires aimedTurn.PendingTransitionalPose.")), $"aimed turn publishes destination case {caseIndex}");
        AssertTrue(SamusPoseTransitions.ApplyPendingVerifiedAnimationTransition(aimedTurn), $"aimed turn transition applies case {caseIndex}");
        AssertEqual(testCase.Destination, aimedTurn.Pose, $"aimed turn destination case {caseIndex}");
        AssertEqual(0, aimedTurn.AnimationFrame, $"aimed turn target frame zero case {caseIndex}");
        AssertEqual(testCase.DestinationDelay, aimedTurn.AnimationFrameTimer.RawValue,
            $"aimed turn uses the native standing-aim delay case {caseIndex}");
    }

    SeedCrouchedAimTurnSpeedRecord(bus);

    AimedSource[] crouchedSources =
    [
        new AimedSource(SamusPoseId.CrouchingRightPose, [0x08, 0x05, 0x27, 0x02, 0x00, 0x00, 0x10, 0x00]),
        new AimedSource(SamusPoseId.CrouchingLeftPose, [0x04, 0x05, 0x28, 0x07, 0x00, 0x00, 0x10, 0x00]),
        new AimedSource(SamusPoseId.CrouchingAimUpRightPose, [0x08, 0x05, 0x27, 0x00, 0x00, 0x00, 0x10, 0x00]),
        new AimedSource(SamusPoseId.CrouchingAimUpLeftPose, [0x04, 0x05, 0x28, 0x09, 0x00, 0x00, 0x10, 0x00]),
        new AimedSource(SamusPoseId.CrouchingAimDiagonalUpRightPose, [0x08, 0x05, 0x27, 0x01, 0x00, 0x00, 0x10, 0x00]),
        new AimedSource(SamusPoseId.CrouchingAimDiagonalUpLeftPose, [0x04, 0x05, 0x28, 0x08, 0x00, 0x00, 0x10, 0x00]),
        new AimedSource(SamusPoseId.CrouchingAimDiagonalDownRightPose, [0x08, 0x05, 0x27, 0x03, 0x00, 0x00, 0x10, 0x00]),
        new AimedSource(SamusPoseId.CrouchingAimDiagonalDownLeftPose, [0x04, 0x05, 0x28, 0x06, 0x00, 0x00, 0x10, 0x00]),
    ];
    foreach ((SamusPoseId sourcePose, byte[] definition) in crouchedSources)
    {
        WritePoseDefinition(bus, (int)sourcePose, definition);
        WritePoseAnimationDelays(bus, sourcePose, (ushort)(0xc600 + (int)sourcePose * 2), [0x0a]);
    }

    // `$43/$44` really are movement type `$0E`; the six aimed records really are `$17`.
    // Keeping those literal bytes in the fixture protects the strange native dispatcher
    // split from a future cleanup that might look attractive but be historically wrong.
    AimedTurn[] crouchedTurns =
    [
        new AimedTurn(SamusPoseId.TurningRightToLeftCrouchingPose, [0x04, 0x0e, 0xff, 0xfb, 0x00, 0x00, 0x10, 0x00]),
        new AimedTurn(SamusPoseId.TurningLeftToRightCrouchingPose, [0x08, 0x0e, 0xff, 0xfb, 0x00, 0x00, 0x10, 0x00]),
        new AimedTurn(SamusPoseId.TurningRightToLeftCrouchingAimUpPose, [0x04, 0x17, 0x28, 0xfa, 0x00, 0x00, 0x10, 0x00]),
        new AimedTurn(SamusPoseId.TurningLeftToRightCrouchingAimUpPose, [0x08, 0x17, 0x28, 0xfa, 0x00, 0x00, 0x10, 0x00]),
        new AimedTurn(SamusPoseId.TurningRightToLeftCrouchingAimDiagonalDownPose, [0x04, 0x17, 0x28, 0xfc, 0x00, 0x00, 0x10, 0x00]),
        new AimedTurn(SamusPoseId.TurningLeftToRightCrouchingAimDiagonalDownPose, [0x08, 0x17, 0x28, 0xfc, 0x00, 0x00, 0x10, 0x00]),
        new AimedTurn(SamusPoseId.TurningRightToLeftCrouchingAimDiagonalUpPose, [0x04, 0x17, 0x28, 0xfa, 0x00, 0x00, 0x10, 0x00]),
        new AimedTurn(SamusPoseId.TurningLeftToRightCrouchingAimDiagonalUpPose, [0x08, 0x17, 0x28, 0xfa, 0x00, 0x00, 0x10, 0x00]),
    ];
    foreach ((SamusPoseId turnPose, byte[] definition) in crouchedTurns)
        WritePoseDefinition(bus, (int)turnPose, definition);

    CrouchedTurnCase[] crouchedTurnCases =
    [
        new CrouchedTurnCase(SamusPoseId.CrouchingRightPose, SamusPoseId.TurningRightToLeftCrouchingPose,
            SamusPoseId.TurningRightToLeftCrouchingPose, SamusPoseId.CrouchingLeftPose),
        new CrouchedTurnCase(SamusPoseId.CrouchingLeftPose, SamusPoseId.TurningLeftToRightCrouchingPose,
            SamusPoseId.TurningLeftToRightCrouchingPose, SamusPoseId.CrouchingRightPose),
        new CrouchedTurnCase(SamusPoseId.CrouchingAimUpRightPose, SamusPoseId.TurningRightToLeftCrouchingPose,
            SamusPoseId.TurningRightToLeftCrouchingAimUpPose, SamusPoseId.CrouchingAimUpLeftPose),
        new CrouchedTurnCase(SamusPoseId.CrouchingAimUpLeftPose, SamusPoseId.TurningLeftToRightCrouchingPose,
            SamusPoseId.TurningLeftToRightCrouchingAimUpPose, SamusPoseId.CrouchingAimUpRightPose),
        new CrouchedTurnCase(SamusPoseId.CrouchingAimDiagonalUpRightPose, SamusPoseId.TurningRightToLeftCrouchingPose,
            SamusPoseId.TurningRightToLeftCrouchingAimDiagonalUpPose, SamusPoseId.CrouchingAimDiagonalUpLeftPose),
        new CrouchedTurnCase(SamusPoseId.CrouchingAimDiagonalUpLeftPose, SamusPoseId.TurningLeftToRightCrouchingPose,
            SamusPoseId.TurningLeftToRightCrouchingAimDiagonalUpPose, SamusPoseId.CrouchingAimDiagonalUpRightPose),
        new CrouchedTurnCase(SamusPoseId.CrouchingAimDiagonalDownRightPose, SamusPoseId.TurningRightToLeftCrouchingPose,
            SamusPoseId.TurningRightToLeftCrouchingAimDiagonalDownPose, SamusPoseId.CrouchingAimDiagonalDownLeftPose),
        new CrouchedTurnCase(SamusPoseId.CrouchingAimDiagonalDownLeftPose, SamusPoseId.TurningLeftToRightCrouchingPose,
            SamusPoseId.TurningLeftToRightCrouchingAimDiagonalDownPose, SamusPoseId.CrouchingAimDiagonalDownRightPose),
    ];
    for (int caseIndex = 0; caseIndex < crouchedTurnCases.Length; caseIndex++)
    {
        var testCase = crouchedTurnCases[caseIndex];
        ushort turnStreamAddress = (ushort)(0xc800 + caseIndex * 0x10);
        WritePoseAnimationDelays(bus, testCase.SelectedTurn, turnStreamAddress,
            [0x02, 0x02, 0x02, 0xf8, (byte)testCase.Destination]);

        var crouchedTurn = new SamusState { Pose = testCase.SourcePose };
        crouchedTurn.Kinematics.Position = crouchedTurn.Kinematics.Position.WithWhole(new(64, 32));
        crouchedTurn.Kinematics.Radius = new(5, 16);
        crouchedTurn.HorizontalSpeed.BaseSpeed = new UnsignedSixteenSixteen(1, 0xd000);
        crouchedTurn.HorizontalSpeed.ExtraRunSpeed = crouchedTurn.HorizontalSpeed.ExtraRunSpeed.WithFraction(0x5000);
        SamusGroundedPoseTransitions.ApplyGroundedTurn(crouchedTurn, testCase.GenericTurn);
        AssertEqual(testCase.SelectedTurn, crouchedTurn.Pose, $"crouched turn selector case {caseIndex}");
        AssertEqual(16, crouchedTurn.Kinematics.Radius.Y, $"crouched turn radius case {caseIndex}");
        AssertEqual(SamusAccelerationMode.Turning, crouchedTurn.HorizontalSpeed.AccelerationMode, $"crouched turn mode one case {caseIndex}");

        GroundedMovementResult crouchedMomentum = SamusGroundedMovement.StepTurningOnGround(

            aimedLevel,
            crouchedTurn,
            frameCounter: (ushort)caseIndex);
        bool beganFacingRight = (caseIndex & 1) == 0;
        AssertTrue(
            beganFacingRight
                ? crouchedMomentum.Horizontal.AcceptedDisplacement > 0
                : crouchedMomentum.Horizontal.AcceptedDisplacement < 0,
            $"crouched turn preserves old momentum direction case {caseIndex}");
        AssertTrue(crouchedMomentum.Vertical.Collided, $"crouched turn grounding branch case {caseIndex}");

        if (caseIndex == 2)
        {
            // The same `$97-$A3` pose is allowed to enter `$90:A790`'s simple aerial arm
            // when an actor or external displacement has made Y direction nonzero. This was
            // formerly an explicit open-dispatch crash despite requiring no new pose data.
            var airborneCrouchedTurn = new SamusState { Pose = testCase.SelectedTurn };
            airborneCrouchedTurn.Kinematics.Position = airborneCrouchedTurn.Kinematics.Position.WithWhole(new(64, 32));
            airborneCrouchedTurn.Kinematics.Radius = new(5, 16);
            airborneCrouchedTurn.Kinematics.YDirection = SamusVerticalDirection.Up;
            airborneCrouchedTurn.Kinematics.YSpeed = airborneCrouchedTurn.Kinematics.YSpeed.WithWhole(1);
            uint airborneYBefore = airborneCrouchedTurn.Kinematics.Position.Y.RawValue;
            SamusAerialMovement.StepTurningInAir(

                aimedLevel,
                airborneCrouchedTurn,
                frameCounter: 0);
            AssertTrue(airborneCrouchedTurn.Kinematics.Position.Y.RawValue != airborneYBefore,
                "airborne crouched type-$17 turn executes simple Y movement");
        }

        for (int tick = 0; tick < 6; tick++)
            SamusPresentation.AnimateNoFx(crouchedTurn, new SamusAnimationStep { Random = new SharedRandom() });
        AssertEqual(testCase.Destination, (crouchedTurn.PendingTransitionalPose ?? throw new InvalidOperationException("Verification requires crouchedTurn.PendingTransitionalPose.")), $"crouched turn publishes destination case {caseIndex}");
        AssertTrue(SamusPoseTransitions.ApplyPendingVerifiedAnimationTransition(crouchedTurn), $"crouched turn transition applies case {caseIndex}");
        AssertEqual(testCase.Destination, crouchedTurn.Pose, $"crouched turn destination case {caseIndex}");
    }

    Console.WriteLine("  Samus reversal: standing/crouched selectors, mode-one carry, grounded type-$17, and $F8 agree.");
}

/// <summary>
/// Locks movement type `$10` to the literal bank-$90/$91 behavior: pose direction is the
/// travel direction despite opposite-facing art, the options word gates entry, definition
/// byte two exits on zero input, and `$BF-$C4` remain grounded until their `$F8` command or
/// transition table starts a real jump.
/// </summary>
internal static void VerifySamusMoonwalking()
{
    var bus = new TestAddressSpace();

    // Moonwalk's native row accelerates at 0.C000 and caps at 0.8000; it must not
    // borrow running's faster cap. Turn-art poses still dispatch their own native row.

    Stable[] stable =
    [
        new Stable(SamusPoseId.MoonwalkFacingLeftPose,
            [0x08, 0x10, 0x02, 0x07, 0x06, 0x00, 0x15, 0x00], SamusPoseId.FacingLeftNormalPose, 1),
        new Stable(SamusPoseId.MoonwalkFacingRightPose,
            [0x04, 0x10, 0x01, 0x02, 0x06, 0x00, 0x15, 0x00], SamusPoseId.FacingRightNormalPose, -1),
        new Stable(SamusPoseId.MoonwalkAimUpLeftPose,
            [0x08, 0x10, 0x06, 0x08, 0x06, 0x00, 0x15, 0x00], SamusPoseId.StandingAimDiagonalUpLeftPose, 1),
        new Stable(SamusPoseId.MoonwalkAimUpRightPose,
            [0x04, 0x10, 0x05, 0x01, 0x06, 0x00, 0x15, 0x00], SamusPoseId.StandingAimDiagonalUpRightPose, -1),
        new Stable(SamusPoseId.MoonwalkAimDownLeftPose,
            [0x08, 0x10, 0x08, 0x06, 0x06, 0x00, 0x15, 0x00], SamusPoseId.StandingAimDiagonalDownLeftPose, 1),
        new Stable(SamusPoseId.MoonwalkAimDownRightPose,
            [0x04, 0x10, 0x07, 0x03, 0x06, 0x00, 0x15, 0x00], SamusPoseId.StandingAimDiagonalDownRightPose, -1),
    ];
    foreach ((SamusPoseId pose, byte[] definition, _, _) in stable)
        WritePoseDefinition(bus, (int)pose, definition);

    // The standing definitions are the actual sources and no-button destinations for the
    // six records above. Their one-byte streams are enough because these focused checks do
    // not advance standing animation.
    SamusSpecialStatesPose[] standing =
    [
        new SamusSpecialStatesPose(SamusPoseId.FacingRightNormalPose, [0x08, 0x00, 0x01, 0x02, 0x06, 0x00, 0x15, 0x00]),
        new SamusSpecialStatesPose(SamusPoseId.FacingLeftNormalPose, [0x04, 0x00, 0x02, 0x07, 0x06, 0x00, 0x15, 0x00]),
        new SamusSpecialStatesPose(SamusPoseId.StandingAimDiagonalUpRightPose, [0x08, 0x00, 0x01, 0x01, 0x06, 0x00, 0x15, 0x00]),
        new SamusSpecialStatesPose(SamusPoseId.StandingAimDiagonalUpLeftPose, [0x04, 0x00, 0x02, 0x08, 0x06, 0x00, 0x15, 0x00]),
        new SamusSpecialStatesPose(SamusPoseId.StandingAimDiagonalDownRightPose, [0x08, 0x00, 0x01, 0x03, 0x06, 0x00, 0x15, 0x00]),
        new SamusSpecialStatesPose(SamusPoseId.StandingAimDiagonalDownLeftPose, [0x04, 0x00, 0x02, 0x06, 0x06, 0x00, 0x15, 0x00]),
    ];
    foreach ((SamusPoseId pose, byte[] definition) in standing)
    {
        WritePoseDefinition(bus, (int)pose, definition);
        WritePoseAnimationDelays(bus, pose, (ushort)(0xc000 + (int)pose), [10]);
    }

    // The disabled-option substitution enters ordinary right-to-left `$25`, including the
    // initializer's momentum fold and mode-one selection. `$25`'s short stream need not
    // complete here; merely initializing it proves the candidate was not retained.
    WritePoseDefinition(bus, (int)SamusPoseId.TurningRightToLeftPose, [0x04, 0x0e, 0xff, 0xfb, 0x06, 0x00, 0x15, 0x00]);
    WritePoseAnimationDelays(bus, SamusPoseId.TurningRightToLeftPose, 0xc100, [0x02, 0x02, 0x02, 0xf8, 0x02]);
    var disabled = new SamusState { Pose = SamusPoseId.FacingRightNormalPose };
    disabled.HorizontalSpeed.BaseSpeed = disabled.HorizontalSpeed.BaseSpeed.WithFraction(0x4000);
    disabled.HorizontalSpeed.ExtraRunSpeed = disabled.HorizontalSpeed.ExtraRunSpeed.WithFraction(0x2000);
    SamusGroundedPoseTransitions.ApplyMoonwalkPoseChange(disabled, SamusPoseId.MoonwalkFacingRightPose, moonwalkEnabled: false);
    AssertEqual(SamusPoseId.TurningRightToLeftPose, disabled.Pose,
        "disabled Moonwalk option substitutes ordinary turn");
    AssertEqual(0x6000, disabled.HorizontalSpeed.BaseSpeed.Fraction,
        "disabled Moonwalk substitution folds extra momentum");
    AssertEqual(SamusAccelerationMode.Turning, disabled.HorizontalSpeed.AccelerationMode,
        "disabled Moonwalk substitution starts mode one");

    // `$A4-$A7/$E0-$E7` all use the ordinary standing input records. A backward direction
    // during any short landing animation can therefore nominate `$4A/$49`; the target's
    // native Moonwalk initializer still performs the option-disabled turn substitution.
    // Exercise the whole named family so spin/aim/fire landing coverage cannot regress to
    // the original `$A4/$A5`-only approximation.
    SamusPoseId[] rightLandingPoses =
    [
        SamusPoseId.NormalLandingRightPose,
        SamusPoseId.SpinLandingRightPose,
        SamusPoseId.LandingAimUpRightPose,
        SamusPoseId.LandingAimDiagonalUpRightPose,
        SamusPoseId.LandingAimDiagonalDownRightPose,
        SamusPoseId.FiringLandingRightPose,
    ];
    foreach (SamusPoseId landingPose in rightLandingPoses)
    {
        WritePoseDefinition(bus, (int)landingPose,
            [0x08, 0x00, 0x01, 0x02, 0x00, 0x00, 0x15, 0x00]);
        var landingMoonwalk = new SamusState { Pose = landingPose };
        SamusGroundedPoseTransitions.ApplyMoonwalkPoseChange(landingMoonwalk, 
            SamusPoseId.MoonwalkFacingRightPose,
            moonwalkEnabled: false);
        SamusPoseId expectedTurn = landingPose switch
        {
            SamusPoseId.LandingAimUpRightPose => SamusPoseId.TurningRightToLeftAimUpPose,
            SamusPoseId.LandingAimDiagonalUpRightPose => SamusPoseId.TurningRightToLeftAimDiagonalUpPose,
            SamusPoseId.LandingAimDiagonalDownRightPose => SamusPoseId.TurningRightToLeftAimDiagonalDownPose,
            SamusPoseId.ForwardFacingPowerSuitPose or
                SamusPoseId.ForwardFacingSuitedPose or
                SamusPoseId.FacingRightNormalPose or
                SamusPoseId.FacingLeftNormalPose or
                SamusPoseId.MovingRightNormalPose or
                SamusPoseId.MovingLeftNormalPose or
                SamusPoseId.MovingRightGunExtendedPose or
                SamusPoseId.MovingLeftGunExtendedPose or
                SamusPoseId.UnusedPose65 or
                SamusPoseId.UnusedPose66 or
                SamusPoseId.MoonwalkFacingLeftPose or
                SamusPoseId.MoonwalkFacingRightPose or
                SamusPoseId.MoonwalkAimUpLeftPose or
                SamusPoseId.MoonwalkAimUpRightPose or
                SamusPoseId.MoonwalkAimDownLeftPose or
                SamusPoseId.MoonwalkAimDownRightPose or
                SamusPoseId.MoonwalkTurnJumpLeftPose or
                SamusPoseId.MoonwalkTurnJumpRightPose or
                SamusPoseId.MoonwalkTurnJumpAimUpLeftPose or
                SamusPoseId.MoonwalkTurnJumpAimUpRightPose or
                SamusPoseId.MoonwalkTurnJumpAimDownLeftPose or
                SamusPoseId.MoonwalkTurnJumpAimDownRightPose or
                SamusPoseId.ShinesparkWindupRightPose or
                SamusPoseId.ShinesparkWindupLeftPose or
                SamusPoseId.ShinesparkHorizontalRightPose or
                SamusPoseId.ShinesparkHorizontalLeftPose or
                SamusPoseId.ShinesparkVerticalRightPose or
                SamusPoseId.ShinesparkVerticalLeftPose or
                SamusPoseId.ShinesparkDiagonalRightPose or
                SamusPoseId.ShinesparkDiagonalLeftPose or
                SamusPoseId.CrystalFlashRightPose or
                SamusPoseId.CrystalFlashLeftPose or
                SamusPoseId.XrayingStandingRightPose or
                SamusPoseId.XrayingStandingLeftPose or
                SamusPoseId.DeathSequenceRightPose or
                SamusPoseId.DeathSequenceLeftPose or
                SamusPoseId.XrayingCrouchingRightPose or
                SamusPoseId.XrayingCrouchingLeftPose or
                SamusPoseId.UnusedPoseDb or
                SamusPoseId.UnusedPoseDc or
                SamusPoseId.UnusedPoseDd or
                SamusPoseId.UnusedPoseDe or
                SamusPoseId.UnusedPoseDf or
                SamusPoseId.DrainedCrouchingRightPose or
                SamusPoseId.DrainedCrouchingLeftPose or
                SamusPoseId.DrainedStandingRightPose or
                SamusPoseId.DrainedStandingLeftPose or
                SamusPoseId.RanIntoWallRightPose or
                SamusPoseId.RanIntoWallLeftPose or
                SamusPoseId.RanIntoWallAimUpRightPose or
                SamusPoseId.RanIntoWallAimUpLeftPose or
                SamusPoseId.RanIntoWallAimDownRightPose or
                SamusPoseId.RanIntoWallAimDownLeftPose or
                SamusPoseId.TurningRightToLeftPose or
                SamusPoseId.TurningLeftToRightPose or
                SamusPoseId.SpinJumpRightPose or
                SamusPoseId.SpinJumpLeftPose or
                SamusPoseId.SpaceJumpRightPose or
                SamusPoseId.SpaceJumpLeftPose or
                SamusPoseId.ScrewAttackRightPose or
                SamusPoseId.ScrewAttackLeftPose or
                SamusPoseId.TurningRightToLeftJumpPose or
                SamusPoseId.TurningLeftToRightJumpPose or
                SamusPoseId.WallJumpRightPose or
                SamusPoseId.WallJumpLeftPose or
                SamusPoseId.GrappleSwingRightPose or
                SamusPoseId.GrappleSwingLeftPose or
                SamusPoseId.GrappleStandingRightPose or
                SamusPoseId.GrappleStandingLeftPose or
                SamusPoseId.GrappleStandingDownRightPose or
                SamusPoseId.GrappleStandingDownLeftPose or
                SamusPoseId.GrappleCrouchingRightPose or
                SamusPoseId.GrappleCrouchingLeftPose or
                SamusPoseId.GrappleCrouchingDownRightPose or
                SamusPoseId.GrappleCrouchingDownLeftPose or
                SamusPoseId.GrappleWallContactLeftPose or
                SamusPoseId.GrappleWallContactRightPose or
                SamusPoseId.DraygonGrabbedNeutralLeftPose or
                SamusPoseId.DraygonGrabbedAimUpLeftPose or
                SamusPoseId.DraygonGrabbedFiringLeftPose or
                SamusPoseId.DraygonGrabbedAimDownLeftPose or
                SamusPoseId.DraygonGrabbedMovingLeftPose or
                SamusPoseId.DraygonGrabbedNeutralRightPose or
                SamusPoseId.DraygonGrabbedAimUpRightPose or
                SamusPoseId.DraygonGrabbedFiringRightPose or
                SamusPoseId.DraygonGrabbedAimDownRightPose or
                SamusPoseId.DraygonGrabbedMovingRightPose or
                SamusPoseId.DamageBoostLeftPose or
                SamusPoseId.DamageBoostRightPose or
                SamusPoseId.KnockbackRightPose or
                SamusPoseId.KnockbackLeftPose or
                SamusPoseId.TurningRightToLeftFallingPose or
                SamusPoseId.TurningLeftToRightFallingPose or
                SamusPoseId.TurningRightToLeftJumpAimUpPose or
                SamusPoseId.TurningLeftToRightJumpAimUpPose or
                SamusPoseId.TurningRightToLeftJumpAimDownPose or
                SamusPoseId.TurningLeftToRightJumpAimDownPose or
                SamusPoseId.TurningRightToLeftFallingAimUpPose or
                SamusPoseId.TurningLeftToRightFallingAimUpPose or
                SamusPoseId.TurningRightToLeftFallingAimDownPose or
                SamusPoseId.TurningLeftToRightFallingAimDownPose or
                SamusPoseId.TurningRightToLeftJumpAimDiagonalUpPose or
                SamusPoseId.TurningLeftToRightJumpAimDiagonalUpPose or
                SamusPoseId.TurningRightToLeftFallingAimDiagonalUpPose or
                SamusPoseId.TurningLeftToRightFallingAimDiagonalUpPose or
                SamusPoseId.MorphBallGroundRightPose or
                SamusPoseId.MorphBallMovingRightPose or
                SamusPoseId.MorphBallMovingLeftPose or
                SamusPoseId.MorphBallFallingRightPose or
                SamusPoseId.MorphBallFallingLeftPose or
                SamusPoseId.UnusedKnockbackRightPose or
                SamusPoseId.UnusedKnockbackLeftPose or
                SamusPoseId.MorphingTransitionRightPose or
                SamusPoseId.MorphingTransitionLeftPose or
                SamusPoseId.UnmorphingTransitionRightPose or
                SamusPoseId.UnmorphingTransitionLeftPose or
                SamusPoseId.MorphBallGroundLeftPose or
                SamusPoseId.SpringBallGroundRightPose or
                SamusPoseId.SpringBallGroundLeftPose or
                SamusPoseId.SpringBallMovingRightPose or
                SamusPoseId.SpringBallMovingLeftPose or
                SamusPoseId.SpringBallFallingRightPose or
                SamusPoseId.SpringBallFallingLeftPose or
                SamusPoseId.SpringBallJumpRightPose or
                SamusPoseId.SpringBallJumpLeftPose or
                SamusPoseId.FallingRightPose or
                SamusPoseId.FallingLeftPose or
                SamusPoseId.FallingGunExtendedRightPose or
                SamusPoseId.FallingGunExtendedLeftPose or
                SamusPoseId.StandingAimUpRightPose or
                SamusPoseId.StandingAimUpLeftPose or
                SamusPoseId.StandingAimDiagonalUpRightPose or
                SamusPoseId.StandingAimDiagonalUpLeftPose or
                SamusPoseId.StandingAimDiagonalDownRightPose or
                SamusPoseId.StandingAimDiagonalDownLeftPose or
                SamusPoseId.RunningAimUpRightPose or
                SamusPoseId.RunningAimUpLeftPose or
                SamusPoseId.RunningAimDiagonalUpRightPose or
                SamusPoseId.RunningAimDiagonalUpLeftPose or
                SamusPoseId.RunningAimDiagonalDownRightPose or
                SamusPoseId.RunningAimDiagonalDownLeftPose or
                SamusPoseId.NormalJumpGunExtendedRightPose or
                SamusPoseId.NormalJumpGunExtendedLeftPose or
                SamusPoseId.NormalJumpAimUpRightPose or
                SamusPoseId.NormalJumpAimUpLeftPose or
                SamusPoseId.NormalJumpAimDownRightPose or
                SamusPoseId.NormalJumpAimDownLeftPose or
                SamusPoseId.NormalJumpForwardRightPose or
                SamusPoseId.NormalJumpForwardLeftPose or
                SamusPoseId.NormalJumpTransitionAimUpRightPose or
                SamusPoseId.NormalJumpTransitionAimUpLeftPose or
                SamusPoseId.NormalJumpTransitionAimDiagonalUpRightPose or
                SamusPoseId.NormalJumpTransitionAimDiagonalUpLeftPose or
                SamusPoseId.NormalJumpTransitionAimDiagonalDownRightPose or
                SamusPoseId.NormalJumpTransitionAimDiagonalDownLeftPose or
                SamusPoseId.NormalJumpAimDiagonalUpRightPose or
                SamusPoseId.NormalJumpAimDiagonalUpLeftPose or
                SamusPoseId.NormalJumpAimDiagonalDownRightPose or
                SamusPoseId.NormalJumpAimDiagonalDownLeftPose or
                SamusPoseId.FallingAimUpRightPose or
                SamusPoseId.FallingAimUpLeftPose or
                SamusPoseId.FallingAimDownRightPose or
                SamusPoseId.FallingAimDownLeftPose or
                SamusPoseId.FallingAimDiagonalUpRightPose or
                SamusPoseId.FallingAimDiagonalUpLeftPose or
                SamusPoseId.FallingAimDiagonalDownRightPose or
                SamusPoseId.FallingAimDiagonalDownLeftPose or
                SamusPoseId.CrouchingAimDiagonalUpRightPose or
                SamusPoseId.CrouchingAimDiagonalUpLeftPose or
                SamusPoseId.CrouchingAimDiagonalDownRightPose or
                SamusPoseId.CrouchingAimDiagonalDownLeftPose or
                SamusPoseId.CrouchingAimUpRightPose or
                SamusPoseId.CrouchingAimUpLeftPose or
                SamusPoseId.TurningRightToLeftCrouchingPose or
                SamusPoseId.TurningLeftToRightCrouchingPose or
                SamusPoseId.TurningRightToLeftAimUpPose or
                SamusPoseId.TurningLeftToRightAimUpPose or
                SamusPoseId.TurningRightToLeftAimDiagonalDownPose or
                SamusPoseId.TurningLeftToRightAimDiagonalDownPose or
                SamusPoseId.TurningRightToLeftAimDiagonalUpPose or
                SamusPoseId.TurningLeftToRightAimDiagonalUpPose or
                SamusPoseId.TurningRightToLeftCrouchingAimUpPose or
                SamusPoseId.TurningLeftToRightCrouchingAimUpPose or
                SamusPoseId.TurningRightToLeftCrouchingAimDiagonalDownPose or
                SamusPoseId.TurningLeftToRightCrouchingAimDiagonalDownPose or
                SamusPoseId.TurningRightToLeftCrouchingAimDiagonalUpPose or
                SamusPoseId.TurningLeftToRightCrouchingAimDiagonalUpPose or
                SamusPoseId.LandingAimUpLeftPose or
                SamusPoseId.LandingAimDiagonalUpLeftPose or
                SamusPoseId.LandingAimDiagonalDownLeftPose or
                SamusPoseId.FiringLandingRightPose or
                SamusPoseId.FiringLandingLeftPose or
                SamusPoseId.CrouchingTransitionAimUpRightPose or
                SamusPoseId.CrouchingTransitionAimUpLeftPose or
                SamusPoseId.CrouchingTransitionAimDiagonalUpRightPose or
                SamusPoseId.CrouchingTransitionAimDiagonalUpLeftPose or
                SamusPoseId.CrouchingTransitionAimDiagonalDownRightPose or
                SamusPoseId.CrouchingTransitionAimDiagonalDownLeftPose or
                SamusPoseId.StandingTransitionAimUpRightPose or
                SamusPoseId.StandingTransitionAimUpLeftPose or
                SamusPoseId.StandingTransitionAimDiagonalUpRightPose or
                SamusPoseId.StandingTransitionAimDiagonalUpLeftPose or
                SamusPoseId.StandingTransitionAimDiagonalDownRightPose or
                SamusPoseId.StandingTransitionAimDiagonalDownLeftPose or
                SamusPoseId.CrouchingRightPose or
                SamusPoseId.CrouchingLeftPose or
                SamusPoseId.CrouchingTransitionRightPose or
                SamusPoseId.CrouchingTransitionLeftPose or
                SamusPoseId.StandingTransitionRightPose or
                SamusPoseId.StandingTransitionLeftPose or
                SamusPoseId.NeutralJumpTransitionRightPose or
                SamusPoseId.NeutralJumpTransitionLeftPose or
                SamusPoseId.NeutralJumpRightPose or
                SamusPoseId.NeutralJumpLeftPose or
                SamusPoseId.NormalLandingRightPose or
                SamusPoseId.NormalLandingLeftPose or
                SamusPoseId.SpinLandingRightPose or
                SamusPoseId.SpinLandingLeftPose or
                SamusPoseId.UnusedPose20 or
                SamusPoseId.UnusedPose21 or
                SamusPoseId.UnusedPose22 or
                SamusPoseId.UnusedPose23 or
                SamusPoseId.UnusedPose24 or
                SamusPoseId.UnusedPose39 or
                SamusPoseId.UnusedPose3A or
                SamusPoseId.UnusedPose3F or
                SamusPoseId.UnusedPose40 or
                SamusPoseId.UnusedPose42 or
                SamusPoseId.UnusedPose45 or
                SamusPoseId.UnusedPose46 or
                SamusPoseId.UnusedPose47 or
                SamusPoseId.UnusedPose48 or
                SamusPoseId.UnusedPose5B or
                SamusPoseId.UnusedPose5C or
                SamusPoseId.UnusedPose5D or
                SamusPoseId.UnusedPose5E or
                SamusPoseId.UnusedPose5F or
                SamusPoseId.UnusedPose60 or
                SamusPoseId.UnusedPose61 or
                SamusPoseId.UnusedPose62 or
                SamusPoseId.UnusedPose63 or
                SamusPoseId.UnusedPose64 or
                SamusPoseId.UnusedPoseAC or
                SamusPoseId.UnusedPoseAD or
                SamusPoseId.UnusedPoseAE or
                SamusPoseId.UnusedPoseAF or
                SamusPoseId.UnusedPoseB0 or
                SamusPoseId.UnusedPoseB1 or
                SamusPoseId.UnusedPoseC5 or
                SamusPoseId.UnusedPoseC6 => SamusPoseId.TurningRightToLeftPose,
            SamusPoseId.AdjacentCodePoseFD or
                SamusPoseId.AdjacentCodePoseFE or
                SamusPoseId.AdjacentCodePoseFF => SamusPoseId.TurningRightToLeftPose,
            _ => SamusPoseId.TurningRightToLeftPose,
        };
        AssertEqual(expectedTurn, landingMoonwalk.Pose,
            $"right landing ${(int)landingPose:X2} honors disabled Moonwalk substitution");
    }

    SamusPoseId[] leftLandingPoses =
    [
        SamusPoseId.NormalLandingLeftPose,
        SamusPoseId.SpinLandingLeftPose,
        SamusPoseId.LandingAimUpLeftPose,
        SamusPoseId.LandingAimDiagonalUpLeftPose,
        SamusPoseId.LandingAimDiagonalDownLeftPose,
        SamusPoseId.FiringLandingLeftPose,
    ];
    foreach (SamusPoseId landingPose in leftLandingPoses)
    {
        WritePoseDefinition(bus, (int)landingPose,
            [0x04, 0x00, 0x02, 0x07, 0x00, 0x00, 0x15, 0x00]);
        var landingMoonwalk = new SamusState { Pose = landingPose };
        SamusGroundedPoseTransitions.ApplyMoonwalkPoseChange(landingMoonwalk, 
            SamusPoseId.MoonwalkFacingLeftPose,
            moonwalkEnabled: false);
        SamusPoseId expectedTurn = landingPose switch
        {
            SamusPoseId.LandingAimUpLeftPose => SamusPoseId.TurningLeftToRightAimUpPose,
            SamusPoseId.LandingAimDiagonalUpLeftPose => SamusPoseId.TurningLeftToRightAimDiagonalUpPose,
            SamusPoseId.LandingAimDiagonalDownLeftPose => SamusPoseId.TurningLeftToRightAimDiagonalDownPose,
            SamusPoseId.ForwardFacingPowerSuitPose or
                SamusPoseId.ForwardFacingSuitedPose or
                SamusPoseId.FacingRightNormalPose or
                SamusPoseId.FacingLeftNormalPose or
                SamusPoseId.MovingRightNormalPose or
                SamusPoseId.MovingLeftNormalPose or
                SamusPoseId.MovingRightGunExtendedPose or
                SamusPoseId.MovingLeftGunExtendedPose or
                SamusPoseId.UnusedPose65 or
                SamusPoseId.UnusedPose66 or
                SamusPoseId.MoonwalkFacingLeftPose or
                SamusPoseId.MoonwalkFacingRightPose or
                SamusPoseId.MoonwalkAimUpLeftPose or
                SamusPoseId.MoonwalkAimUpRightPose or
                SamusPoseId.MoonwalkAimDownLeftPose or
                SamusPoseId.MoonwalkAimDownRightPose or
                SamusPoseId.MoonwalkTurnJumpLeftPose or
                SamusPoseId.MoonwalkTurnJumpRightPose or
                SamusPoseId.MoonwalkTurnJumpAimUpLeftPose or
                SamusPoseId.MoonwalkTurnJumpAimUpRightPose or
                SamusPoseId.MoonwalkTurnJumpAimDownLeftPose or
                SamusPoseId.MoonwalkTurnJumpAimDownRightPose or
                SamusPoseId.ShinesparkWindupRightPose or
                SamusPoseId.ShinesparkWindupLeftPose or
                SamusPoseId.ShinesparkHorizontalRightPose or
                SamusPoseId.ShinesparkHorizontalLeftPose or
                SamusPoseId.ShinesparkVerticalRightPose or
                SamusPoseId.ShinesparkVerticalLeftPose or
                SamusPoseId.ShinesparkDiagonalRightPose or
                SamusPoseId.ShinesparkDiagonalLeftPose or
                SamusPoseId.CrystalFlashRightPose or
                SamusPoseId.CrystalFlashLeftPose or
                SamusPoseId.XrayingStandingRightPose or
                SamusPoseId.XrayingStandingLeftPose or
                SamusPoseId.DeathSequenceRightPose or
                SamusPoseId.DeathSequenceLeftPose or
                SamusPoseId.XrayingCrouchingRightPose or
                SamusPoseId.XrayingCrouchingLeftPose or
                SamusPoseId.UnusedPoseDb or
                SamusPoseId.UnusedPoseDc or
                SamusPoseId.UnusedPoseDd or
                SamusPoseId.UnusedPoseDe or
                SamusPoseId.UnusedPoseDf or
                SamusPoseId.DrainedCrouchingRightPose or
                SamusPoseId.DrainedCrouchingLeftPose or
                SamusPoseId.DrainedStandingRightPose or
                SamusPoseId.DrainedStandingLeftPose or
                SamusPoseId.RanIntoWallRightPose or
                SamusPoseId.RanIntoWallLeftPose or
                SamusPoseId.RanIntoWallAimUpRightPose or
                SamusPoseId.RanIntoWallAimUpLeftPose or
                SamusPoseId.RanIntoWallAimDownRightPose or
                SamusPoseId.RanIntoWallAimDownLeftPose or
                SamusPoseId.TurningRightToLeftPose or
                SamusPoseId.TurningLeftToRightPose or
                SamusPoseId.SpinJumpRightPose or
                SamusPoseId.SpinJumpLeftPose or
                SamusPoseId.SpaceJumpRightPose or
                SamusPoseId.SpaceJumpLeftPose or
                SamusPoseId.ScrewAttackRightPose or
                SamusPoseId.ScrewAttackLeftPose or
                SamusPoseId.TurningRightToLeftJumpPose or
                SamusPoseId.TurningLeftToRightJumpPose or
                SamusPoseId.WallJumpRightPose or
                SamusPoseId.WallJumpLeftPose or
                SamusPoseId.GrappleSwingRightPose or
                SamusPoseId.GrappleSwingLeftPose or
                SamusPoseId.GrappleStandingRightPose or
                SamusPoseId.GrappleStandingLeftPose or
                SamusPoseId.GrappleStandingDownRightPose or
                SamusPoseId.GrappleStandingDownLeftPose or
                SamusPoseId.GrappleCrouchingRightPose or
                SamusPoseId.GrappleCrouchingLeftPose or
                SamusPoseId.GrappleCrouchingDownRightPose or
                SamusPoseId.GrappleCrouchingDownLeftPose or
                SamusPoseId.GrappleWallContactLeftPose or
                SamusPoseId.GrappleWallContactRightPose or
                SamusPoseId.DraygonGrabbedNeutralLeftPose or
                SamusPoseId.DraygonGrabbedAimUpLeftPose or
                SamusPoseId.DraygonGrabbedFiringLeftPose or
                SamusPoseId.DraygonGrabbedAimDownLeftPose or
                SamusPoseId.DraygonGrabbedMovingLeftPose or
                SamusPoseId.DraygonGrabbedNeutralRightPose or
                SamusPoseId.DraygonGrabbedAimUpRightPose or
                SamusPoseId.DraygonGrabbedFiringRightPose or
                SamusPoseId.DraygonGrabbedAimDownRightPose or
                SamusPoseId.DraygonGrabbedMovingRightPose or
                SamusPoseId.DamageBoostLeftPose or
                SamusPoseId.DamageBoostRightPose or
                SamusPoseId.KnockbackRightPose or
                SamusPoseId.KnockbackLeftPose or
                SamusPoseId.TurningRightToLeftFallingPose or
                SamusPoseId.TurningLeftToRightFallingPose or
                SamusPoseId.TurningRightToLeftJumpAimUpPose or
                SamusPoseId.TurningLeftToRightJumpAimUpPose or
                SamusPoseId.TurningRightToLeftJumpAimDownPose or
                SamusPoseId.TurningLeftToRightJumpAimDownPose or
                SamusPoseId.TurningRightToLeftFallingAimUpPose or
                SamusPoseId.TurningLeftToRightFallingAimUpPose or
                SamusPoseId.TurningRightToLeftFallingAimDownPose or
                SamusPoseId.TurningLeftToRightFallingAimDownPose or
                SamusPoseId.TurningRightToLeftJumpAimDiagonalUpPose or
                SamusPoseId.TurningLeftToRightJumpAimDiagonalUpPose or
                SamusPoseId.TurningRightToLeftFallingAimDiagonalUpPose or
                SamusPoseId.TurningLeftToRightFallingAimDiagonalUpPose or
                SamusPoseId.MorphBallGroundRightPose or
                SamusPoseId.MorphBallMovingRightPose or
                SamusPoseId.MorphBallMovingLeftPose or
                SamusPoseId.MorphBallFallingRightPose or
                SamusPoseId.MorphBallFallingLeftPose or
                SamusPoseId.UnusedKnockbackRightPose or
                SamusPoseId.UnusedKnockbackLeftPose or
                SamusPoseId.MorphingTransitionRightPose or
                SamusPoseId.MorphingTransitionLeftPose or
                SamusPoseId.UnmorphingTransitionRightPose or
                SamusPoseId.UnmorphingTransitionLeftPose or
                SamusPoseId.MorphBallGroundLeftPose or
                SamusPoseId.SpringBallGroundRightPose or
                SamusPoseId.SpringBallGroundLeftPose or
                SamusPoseId.SpringBallMovingRightPose or
                SamusPoseId.SpringBallMovingLeftPose or
                SamusPoseId.SpringBallFallingRightPose or
                SamusPoseId.SpringBallFallingLeftPose or
                SamusPoseId.SpringBallJumpRightPose or
                SamusPoseId.SpringBallJumpLeftPose or
                SamusPoseId.FallingRightPose or
                SamusPoseId.FallingLeftPose or
                SamusPoseId.FallingGunExtendedRightPose or
                SamusPoseId.FallingGunExtendedLeftPose or
                SamusPoseId.StandingAimUpRightPose or
                SamusPoseId.StandingAimUpLeftPose or
                SamusPoseId.StandingAimDiagonalUpRightPose or
                SamusPoseId.StandingAimDiagonalUpLeftPose or
                SamusPoseId.StandingAimDiagonalDownRightPose or
                SamusPoseId.StandingAimDiagonalDownLeftPose or
                SamusPoseId.RunningAimUpRightPose or
                SamusPoseId.RunningAimUpLeftPose or
                SamusPoseId.RunningAimDiagonalUpRightPose or
                SamusPoseId.RunningAimDiagonalUpLeftPose or
                SamusPoseId.RunningAimDiagonalDownRightPose or
                SamusPoseId.RunningAimDiagonalDownLeftPose or
                SamusPoseId.NormalJumpGunExtendedRightPose or
                SamusPoseId.NormalJumpGunExtendedLeftPose or
                SamusPoseId.NormalJumpAimUpRightPose or
                SamusPoseId.NormalJumpAimUpLeftPose or
                SamusPoseId.NormalJumpAimDownRightPose or
                SamusPoseId.NormalJumpAimDownLeftPose or
                SamusPoseId.NormalJumpForwardRightPose or
                SamusPoseId.NormalJumpForwardLeftPose or
                SamusPoseId.NormalJumpTransitionAimUpRightPose or
                SamusPoseId.NormalJumpTransitionAimUpLeftPose or
                SamusPoseId.NormalJumpTransitionAimDiagonalUpRightPose or
                SamusPoseId.NormalJumpTransitionAimDiagonalUpLeftPose or
                SamusPoseId.NormalJumpTransitionAimDiagonalDownRightPose or
                SamusPoseId.NormalJumpTransitionAimDiagonalDownLeftPose or
                SamusPoseId.NormalJumpAimDiagonalUpRightPose or
                SamusPoseId.NormalJumpAimDiagonalUpLeftPose or
                SamusPoseId.NormalJumpAimDiagonalDownRightPose or
                SamusPoseId.NormalJumpAimDiagonalDownLeftPose or
                SamusPoseId.FallingAimUpRightPose or
                SamusPoseId.FallingAimUpLeftPose or
                SamusPoseId.FallingAimDownRightPose or
                SamusPoseId.FallingAimDownLeftPose or
                SamusPoseId.FallingAimDiagonalUpRightPose or
                SamusPoseId.FallingAimDiagonalUpLeftPose or
                SamusPoseId.FallingAimDiagonalDownRightPose or
                SamusPoseId.FallingAimDiagonalDownLeftPose or
                SamusPoseId.CrouchingAimDiagonalUpRightPose or
                SamusPoseId.CrouchingAimDiagonalUpLeftPose or
                SamusPoseId.CrouchingAimDiagonalDownRightPose or
                SamusPoseId.CrouchingAimDiagonalDownLeftPose or
                SamusPoseId.CrouchingAimUpRightPose or
                SamusPoseId.CrouchingAimUpLeftPose or
                SamusPoseId.TurningRightToLeftCrouchingPose or
                SamusPoseId.TurningLeftToRightCrouchingPose or
                SamusPoseId.TurningRightToLeftAimUpPose or
                SamusPoseId.TurningLeftToRightAimUpPose or
                SamusPoseId.TurningRightToLeftAimDiagonalDownPose or
                SamusPoseId.TurningLeftToRightAimDiagonalDownPose or
                SamusPoseId.TurningRightToLeftAimDiagonalUpPose or
                SamusPoseId.TurningLeftToRightAimDiagonalUpPose or
                SamusPoseId.TurningRightToLeftCrouchingAimUpPose or
                SamusPoseId.TurningLeftToRightCrouchingAimUpPose or
                SamusPoseId.TurningRightToLeftCrouchingAimDiagonalDownPose or
                SamusPoseId.TurningLeftToRightCrouchingAimDiagonalDownPose or
                SamusPoseId.TurningRightToLeftCrouchingAimDiagonalUpPose or
                SamusPoseId.TurningLeftToRightCrouchingAimDiagonalUpPose or
                SamusPoseId.LandingAimUpRightPose or
                SamusPoseId.LandingAimDiagonalUpRightPose or
                SamusPoseId.LandingAimDiagonalDownRightPose or
                SamusPoseId.FiringLandingRightPose or
                SamusPoseId.FiringLandingLeftPose or
                SamusPoseId.CrouchingTransitionAimUpRightPose or
                SamusPoseId.CrouchingTransitionAimUpLeftPose or
                SamusPoseId.CrouchingTransitionAimDiagonalUpRightPose or
                SamusPoseId.CrouchingTransitionAimDiagonalUpLeftPose or
                SamusPoseId.CrouchingTransitionAimDiagonalDownRightPose or
                SamusPoseId.CrouchingTransitionAimDiagonalDownLeftPose or
                SamusPoseId.StandingTransitionAimUpRightPose or
                SamusPoseId.StandingTransitionAimUpLeftPose or
                SamusPoseId.StandingTransitionAimDiagonalUpRightPose or
                SamusPoseId.StandingTransitionAimDiagonalUpLeftPose or
                SamusPoseId.StandingTransitionAimDiagonalDownRightPose or
                SamusPoseId.StandingTransitionAimDiagonalDownLeftPose or
                SamusPoseId.CrouchingRightPose or
                SamusPoseId.CrouchingLeftPose or
                SamusPoseId.CrouchingTransitionRightPose or
                SamusPoseId.CrouchingTransitionLeftPose or
                SamusPoseId.StandingTransitionRightPose or
                SamusPoseId.StandingTransitionLeftPose or
                SamusPoseId.NeutralJumpTransitionRightPose or
                SamusPoseId.NeutralJumpTransitionLeftPose or
                SamusPoseId.NeutralJumpRightPose or
                SamusPoseId.NeutralJumpLeftPose or
                SamusPoseId.NormalLandingRightPose or
                SamusPoseId.NormalLandingLeftPose or
                SamusPoseId.SpinLandingRightPose or
                SamusPoseId.SpinLandingLeftPose or
                SamusPoseId.UnusedPose20 or
                SamusPoseId.UnusedPose21 or
                SamusPoseId.UnusedPose22 or
                SamusPoseId.UnusedPose23 or
                SamusPoseId.UnusedPose24 or
                SamusPoseId.UnusedPose39 or
                SamusPoseId.UnusedPose3A or
                SamusPoseId.UnusedPose3F or
                SamusPoseId.UnusedPose40 or
                SamusPoseId.UnusedPose42 or
                SamusPoseId.UnusedPose45 or
                SamusPoseId.UnusedPose46 or
                SamusPoseId.UnusedPose47 or
                SamusPoseId.UnusedPose48 or
                SamusPoseId.UnusedPose5B or
                SamusPoseId.UnusedPose5C or
                SamusPoseId.UnusedPose5D or
                SamusPoseId.UnusedPose5E or
                SamusPoseId.UnusedPose5F or
                SamusPoseId.UnusedPose60 or
                SamusPoseId.UnusedPose61 or
                SamusPoseId.UnusedPose62 or
                SamusPoseId.UnusedPose63 or
                SamusPoseId.UnusedPose64 or
                SamusPoseId.UnusedPoseAC or
                SamusPoseId.UnusedPoseAD or
                SamusPoseId.UnusedPoseAE or
                SamusPoseId.UnusedPoseAF or
                SamusPoseId.UnusedPoseB0 or
                SamusPoseId.UnusedPoseB1 or
                SamusPoseId.UnusedPoseC5 or
                SamusPoseId.UnusedPoseC6 => SamusPoseId.TurningLeftToRightPose,
            SamusPoseId.AdjacentCodePoseFD or
                SamusPoseId.AdjacentCodePoseFE or
                SamusPoseId.AdjacentCodePoseFF => SamusPoseId.TurningLeftToRightPose,
            _ => SamusPoseId.TurningLeftToRightPose,
        };
        AssertEqual(expectedTurn, landingMoonwalk.Pose,
            $"left landing ${(int)landingPose:X2} honors disabled Moonwalk substitution");
    }

    // `$91:F8F3-$F903` is specific to a turn whose PREVIOUS movement type is Moonwalk.
    // It publishes the source pose's shot direction with tag `$0100`; ordinary standing
    // turns, including the disabled-option substitution above, must not create this word.
    AssertEqual(0, disabled.PoseTransitionShotDirection,
        "ordinary standing turn does not publish moonwalk shot bridge");
    var moonwalkBridge = new SamusState { Pose = SamusPoseId.MoonwalkFacingRightPose };
    SamusGroundedPoseTransitions.ApplyGroundedTurn(moonwalkBridge, SamusPoseId.TurningRightToLeftPose);
    AssertEqual(0x0102, moonwalkBridge.PoseTransitionShotDirection,
        "moonwalk turn publishes tagged source shot direction");
    SamusAerialPoseTransitions.ClearPoseTransitionShotDirection(moonwalkBridge);
    AssertEqual(0, moonwalkBridge.PoseTransitionShotDirection,
        "moonwalk shot bridge is one-current-handler state");

    // Enabled entry must preserve every exact candidate, not merely the unaimed pair.
    foreach ((SamusPoseId target, _, SamusPoseId fallback, _) in stable)
    {
        var candidate = new SamusState { Pose = fallback };
        WritePoseAnimationDelays(bus, target, (ushort)(0xc200 + (int)target), [2]);
        SamusGroundedPoseTransitions.ApplyMoonwalkPoseChange(candidate, target, moonwalkEnabled: true);
        AssertEqual(target, candidate.Pose, $"enabled Moonwalk retains candidate ${(int)target:X2}");

        // Command two's zero-controller fallback is immediate for movement type `$10` and
        // uses the target record's byte two. This helper applies that already-read byte at
        // the normal end-of-frame transition seam.
        SamusGroundedPoseTransitions.ApplyMoonwalkPoseChange(candidate, fallback, moonwalkEnabled: true);
        AssertEqual(fallback, candidate.Pose, $"moonwalk ${(int)target:X2} fallback byte");
    }

    // A flat row of type-$8 solids isolates horizontal sign and the shared downward probe.
    const int width = 12;
    var foreground = new ushort[width * 3];
    for (int x = 0; x < width; x++)
        foreground[width + x] = 0x8000;
    RoomLevelData floor = CreateRoom(new(width, 3), foreground, new byte[foreground.Length]);
    foreach ((SamusPoseId pose, _, _, int direction) in stable)
    {
        var walker = new SamusState { Pose = pose, Position = FixedPosition<RoomPixels>.FromWhole(new(80, 11)) };
        walker.Kinematics.Radius = new(5, 5);
        GroundedMovementResult movement = SamusGroundedMovement.StepMoonwalking(
floor, walker, frameCounter: 0);
        AssertEqual(direction * 0x8000, movement.Horizontal.AcceptedDisplacement,
            $"moonwalk ${(int)pose:X2} uses literal reversed direction");
        AssertTrue(movement.Vertical.Collided, $"moonwalk ${(int)pose:X2} probes floor");
    }

    // `$90:A697` enters the complete shared X routine. An owned run-speed pair survives
    // because movement type `$10` cannot accelerate but `$0B3C` is still set; an unowned
    // pair is cleared at `$90:9808` before displacement is assembled.
    var momentumMoonwalk = new SamusState
    {
        Pose = SamusPoseId.MoonwalkFacingLeftPose,
        Position = FixedPosition<RoomPixels>.FromWhole(new(80, 11)),
    };
    momentumMoonwalk.Kinematics.Radius = new(5, 5);
    momentumMoonwalk.HorizontalSpeed.HasRunningMomentum = true;
    momentumMoonwalk.HorizontalSpeed.ExtraRunSpeed = momentumMoonwalk.HorizontalSpeed.ExtraRunSpeed.WithWhole(1);
    GroundedMovementResult momentumMoonwalkFrame = SamusGroundedMovement.StepMoonwalking(

        floor,
        momentumMoonwalk,
        frameCounter: 0);
    AssertEqual(0x00018000, momentumMoonwalkFrame.Horizontal.AcceptedDisplacement,
        "moonwalk preserves owned extra run speed");

    var unownedMoonwalk = new SamusState
    {
        Pose = SamusPoseId.MoonwalkFacingLeftPose,
        Position = FixedPosition<RoomPixels>.FromWhole(new(80, 11)),
    };
    unownedMoonwalk.Kinematics.Radius = new(5, 5);
    unownedMoonwalk.HorizontalSpeed.ExtraRunSpeed = unownedMoonwalk.HorizontalSpeed.ExtraRunSpeed.WithWhole(1);
    GroundedMovementResult unownedMoonwalkFrame = SamusGroundedMovement.StepMoonwalking(

        floor,
        unownedMoonwalk,
        frameCounter: 0);
    AssertEqual(0x00008000, unownedMoonwalkFrame.Horizontal.AcceptedDisplacement,
        "moonwalk clears unowned extra run speed");
    AssertEqual(0, unownedMoonwalk.HorizontalSpeed.ExtraRunSpeed.Whole,
        "moonwalk publishes native unowned-speed zero write");

    // Seed the six literal turn/jump records and delay streams. Each stream contains three
    // two-tick frames followed by `$F8,$1A/$19`, exactly `$91:B45B-$B478` for NTSC.
    SamusTerrainMovementTurn[] turns =
    [
        new SamusTerrainMovementTurn(SamusPoseId.MoonwalkFacingRightPose, SamusPoseId.MoonwalkTurnJumpLeftPose, [0x04, 0x0e, 0xff, 0xfb, 0x06, 0x00, 0x15, 0x00], SamusPoseId.SpinJumpLeftPose),
        new SamusTerrainMovementTurn(SamusPoseId.MoonwalkFacingLeftPose, SamusPoseId.MoonwalkTurnJumpRightPose, [0x08, 0x0e, 0xff, 0xfb, 0x06, 0x00, 0x15, 0x00], SamusPoseId.SpinJumpRightPose),
        new SamusTerrainMovementTurn(SamusPoseId.MoonwalkAimUpRightPose, SamusPoseId.MoonwalkTurnJumpAimUpLeftPose, [0x04, 0x0e, 0xff, 0xfa, 0x08, 0x00, 0x15, 0x00], SamusPoseId.SpinJumpLeftPose),
        new SamusTerrainMovementTurn(SamusPoseId.MoonwalkAimUpLeftPose, SamusPoseId.MoonwalkTurnJumpAimUpRightPose, [0x08, 0x0e, 0xff, 0xfa, 0x08, 0x00, 0x15, 0x00], SamusPoseId.SpinJumpRightPose),
        new SamusTerrainMovementTurn(SamusPoseId.MoonwalkAimDownRightPose, SamusPoseId.MoonwalkTurnJumpAimDownLeftPose, [0x04, 0x0e, 0xff, 0xfc, 0x08, 0x00, 0x15, 0x00], SamusPoseId.SpinJumpLeftPose),
        new SamusTerrainMovementTurn(SamusPoseId.MoonwalkAimDownLeftPose, SamusPoseId.MoonwalkTurnJumpAimDownRightPose, [0x08, 0x0e, 0xff, 0xfc, 0x08, 0x00, 0x15, 0x00], SamusPoseId.SpinJumpRightPose),
    ];
    foreach ((_, SamusPoseId target, byte[] definition, SamusPoseId spinTarget) in turns)
    {
        WritePoseDefinition(bus, (int)target, definition);
        WritePoseAnimationDelays(bus, target, (ushort)(0xc300 + ((int)target - 0xbf) * 8),
            [0x02, 0x02, 0x02, 0xf8, (byte)spinTarget]);
    }

    // Spin endpoints and dry-air constants are read through production code after `$F8`.
    WritePoseDefinition(bus, (int)SamusPoseId.SpinJumpRightPose, [0x08, 0x03, 0xff, 0xff, 0x00, 0x00, 0x0c, 0x00]);
    WritePoseDefinition(bus, (int)SamusPoseId.SpinJumpLeftPose, [0x04, 0x03, 0xff, 0xff, 0x00, 0x00, 0x0c, 0x00]);
    WritePoseAnimationDelays(bus, SamusPoseId.SpinJumpRightPose, 0xc400, [2]);
    WritePoseAnimationDelays(bus, SamusPoseId.SpinJumpLeftPose, 0xc401, [2]);
    SeedMoonwalkTurnJumpDryVerticalRows(bus);

    foreach ((SamusPoseId source, SamusPoseId target, _, _) in turns)
    {
        var turn = new SamusState { Pose = source, Position = FixedPosition<RoomPixels>.FromWhole(new(80, 27)) };
        SamusPoseMetadata.RefreshCollisionRadii(turn);
        turn.HorizontalSpeed.BaseSpeed = turn.HorizontalSpeed.BaseSpeed.WithWhole(1);
        turn.HorizontalSpeed.ExtraRunSpeed = turn.HorizontalSpeed.ExtraRunSpeed.WithFraction(0x4000);
        int sourceMuzzleDirection = SamusPoseMetadata.ReadShotDirection(turn);
        SamusGroundedPoseTransitions.ApplyMoonwalkTurnJump(turn, target);
        AssertEqual((ushort)(SamusShotDirectionHandoff.MoonwalkTag | sourceMuzzleDirection),
            turn.PoseTransitionShotDirection, "all six Moonwalk jump routes publish their original muzzle direction");
        AssertEqual(target, turn.Pose, $"moonwalk ${(int)source:X2} selects exact turn ${(int)target:X2}");
        AssertEqual(SamusAccelerationMode.Turning, turn.HorizontalSpeed.AccelerationMode,
            $"moonwalk turn ${(int)target:X2} preserves reversal mode");
        AssertEqual(SamusVerticalDirection.None, turn.Kinematics.YDirection,
            $"moonwalk turn ${(int)target:X2} remains grounded before completion");
    }

    // Exercise the terminal command on one mirrored route. Six decrements consume three
    // two-tick art frames; applying `$F8,$1A` then creates the real 4.E000 upward launch.
    var animated = new SamusState { Pose = SamusPoseId.MoonwalkFacingRightPose };
    SamusGroundedPoseTransitions.ApplyMoonwalkTurnJump(animated, SamusPoseId.MoonwalkTurnJumpLeftPose);
    for (int tick = 0; tick < 6; tick++)
        SamusPresentation.AnimateNoFx(animated, new SamusAnimationStep { Random = new SharedRandom() });
    AssertEqual(SamusPoseId.SpinJumpLeftPose, (animated.PendingTransitionalPose ?? throw new InvalidOperationException("Verification requires animated.PendingTransitionalPose.")),
        "moonwalk turn publishes literal spin-left operand");
    AssertTrue(SamusPoseTransitions.ApplyPendingVerifiedAnimationTransition(animated),
        "moonwalk terminal spin transition applies");
    AssertEqual(SamusPoseId.SpinJumpLeftPose, animated.Pose,
        "moonwalk terminal command enters spin jump");
    AssertEqual(0, animated.Kinematics.YSpeed.Whole,
        "moonwalk animation command bypasses jump-speed initialization");
    AssertEqual(0, animated.Kinematics.YSpeed.Fraction,
        "moonwalk animation command preserves vertical subspeed");
    AssertEqual(SamusVerticalDirection.None, animated.Kinematics.YDirection,
        "moonwalk animation command preserves direction-none for Moonfall");

    Console.WriteLine("  Moonwalk: option gate, six stable routes, reversed X, fallback, and $BF-$C4 jump art agree.");
}

/// <summary>
/// Verifies movement type `$15` and `$91:EADE`'s block-only producer, including the
/// otherwise easy-to-erase one-pixel arm-pump movement performed for prospective runs.
/// </summary>
internal static void VerifySamusRanIntoWall()
{
    var bus = new TestAddressSpace();

    SamusSpecialStatesPose[] wallPoses =
    [
        new SamusSpecialStatesPose(SamusPoseId.RanIntoWallRightPose,
            [0x08, 0x15, 0xff, 0x02, 0x06, 0x00, 0x15, 0x00]),
        new SamusSpecialStatesPose(SamusPoseId.RanIntoWallLeftPose,
            [0x04, 0x15, 0xff, 0x07, 0x06, 0x00, 0x15, 0x00]),
        new SamusSpecialStatesPose(SamusPoseId.RanIntoWallAimUpRightPose,
            [0x08, 0x15, 0x89, 0x01, 0x06, 0x00, 0x15, 0x00]),
        new SamusSpecialStatesPose(SamusPoseId.RanIntoWallAimUpLeftPose,
            [0x04, 0x15, 0x8a, 0x08, 0x06, 0x00, 0x15, 0x00]),
        new SamusSpecialStatesPose(SamusPoseId.RanIntoWallAimDownRightPose,
            [0x08, 0x15, 0x89, 0x03, 0x06, 0x00, 0x15, 0x00]),
        new SamusSpecialStatesPose(SamusPoseId.RanIntoWallAimDownLeftPose,
            [0x04, 0x15, 0x8a, 0x06, 0x06, 0x00, 0x15, 0x00]),
    ];
    foreach ((SamusPoseId pose, byte[] definition) in wallPoses)
    {
        WritePoseDefinition(bus, (int)pose, definition);
        WritePoseAnimationDelayPointer(bus, pose, 0xc500);
    }
    WriteAnimationDelayStream(bus, 0xc500, [0x10, 0xff]);

    // Minimal current/prospective definitions make direction and movement-type selection
    // auditable without copying unrelated animation data into this focused fixture.
    WritePoseDefinition(bus, (int)SamusPoseId.FacingRightNormalPose, [0x08, 0x00, 0x01, 0x02, 0x06, 0x00, 0x15, 0x00]);
    WritePoseDefinition(bus, (int)SamusPoseId.FacingLeftNormalPose, [0x04, 0x00, 0x02, 0x07, 0x06, 0x00, 0x15, 0x00]);
    WritePoseDefinition(bus, (int)SamusPoseId.MovingRightNormalPose, [0x08, 0x01, 0x01, 0x02, 0x06, 0x00, 0x15, 0x00]);
    WritePoseDefinition(bus, (int)SamusPoseId.MovingLeftNormalPose, [0x04, 0x01, 0x02, 0x07, 0x06, 0x00, 0x15, 0x00]);

    SamusPoseId[] selectedByShotDirection =
    [
        SamusPoseId.StandingAimUpRightPose,
        SamusPoseId.RanIntoWallAimUpRightPose,
        SamusPoseId.RanIntoWallRightPose,
        SamusPoseId.RanIntoWallAimDownRightPose,
        SamusPoseId.RanIntoWallRightPose,
        SamusPoseId.RanIntoWallLeftPose,
        SamusPoseId.RanIntoWallAimDownLeftPose,
        SamusPoseId.RanIntoWallLeftPose,
        SamusPoseId.RanIntoWallAimUpLeftPose,
        SamusPoseId.StandingAimUpLeftPose,
    ];
    ReadOnlySpan<SamusPoseId> nativePosesByShotDirection =
    [
        SamusPoseId.StandingAimUpRightPose,
        SamusPoseId.StandingAimDiagonalUpRightPose,
        SamusPoseId.FacingRightNormalPose,
        SamusPoseId.StandingAimDiagonalDownRightPose,
        SamusPoseId.NormalJumpAimDownRightPose,
        SamusPoseId.NormalJumpAimDownLeftPose,
        SamusPoseId.StandingAimDiagonalDownLeftPose,
        SamusPoseId.FacingLeftNormalPose,
        SamusPoseId.StandingAimDiagonalUpLeftPose,
        SamusPoseId.StandingAimUpLeftPose,
    ];
    for (byte direction = 0; direction < selectedByShotDirection.Length; direction++)
    {
        SamusPoseId sourcePose = nativePosesByShotDirection[direction];
        AssertEqual(direction, SamusPoseMetadata.ReadShotDirection(sourcePose),
            $"ran-into-wall fixture pose ${(int)sourcePose:X2} aim");
        AssertEqual(
            selectedByShotDirection[direction],
            SamusGroundedPoseTransitions.SelectRanIntoWallPose(sourcePose),
            $"ran-into-wall shot selector {direction}");
    }
    WritePoseDefinitionByte(bus, (int)SamusPoseId.MovingRightNormalPose, 3, 2);

    const int width = 12;
    var openForeground = new ushort[width * 4];
    for (int x = 0; x < width; x++)
        openForeground[width * 2 + x] = 0x8000;
    RoomLevelData openFloor = CreateRoom(new(width, 4), openForeground, new byte[openForeground.Length]);

    // A clear prospective run really moves one pixel; it is not merely a collision query.
    var armPump = new SamusState
    {
        Pose = SamusPoseId.FacingRightNormalPose,
        Position = FixedPosition<RoomPixels>.FromWhole(new(80, 27)),
    };
    armPump.Kinematics.Radius = new(5, 5);
    SamusPoseId? clearResult = SamusGroundedPoseTransitions.CheckProspectiveRunningPoseForWall(armPump, 

        openFloor,
        SamusPoseId.MovingRightNormalPose,
        currentXSpeedKilledByBlock: false,
        out BlockMoveResult? clearProbe);
    AssertTrue(clearResult is null, "clear arm-pump probe keeps prospective run");
    AssertTrue(clearProbe is { Collided: false }, "clear arm-pump probe reports no wall");
    AssertEqual(81, armPump.Position.X.Whole, "clear arm-pump probe retains one-pixel move");

    // Put a two-block-high wall immediately at X=96. Center 91/radius five has a current
    // right boundary at 95; the same +1.0000 request advances the sampled boundary to 96.
    var blockedForeground = (ushort[])openForeground.Clone();
    blockedForeground[6] = 0x8000;
    blockedForeground[width + 6] = 0x8000;
    RoomLevelData blockedFloor = CreateRoom(new(width, 4), blockedForeground, new byte[blockedForeground.Length]);
    var blocked = new SamusState
    {
        Pose = SamusPoseId.FacingRightNormalPose,
        Position = FixedPosition<RoomPixels>.FromWhole(new(91, 27)),
    };
    blocked.Kinematics.Radius = new(5, 5);
    SamusPoseId? blockedResult = SamusGroundedPoseTransitions.CheckProspectiveRunningPoseForWall(blocked, 

        blockedFloor,
        SamusPoseId.MovingRightNormalPose,
        currentXSpeedKilledByBlock: false,
        out BlockMoveResult? blockedProbe);
    AssertEqual<SamusPoseId?>(SamusPoseId.RanIntoWallRightPose, blockedResult,
        "blocked prospective run selects $89");
    AssertTrue(blockedProbe is { Collided: true }, "blocked arm-pump probe reports wall");
    AssertEqual(91, blocked.Position.X.Whole, "blocked arm-pump probe retains last-safe X");

    // $91:EB05-$EB3C probes solid enemies one pixel ahead before the block move. An enemy
    // whose leading edge touches Samus selects the wall pose and Samus does not move
    // (retail 100% movie: running left into the gunship).
    var enemyAhead = new SamusState
    {
        Pose = SamusPoseId.FacingRightNormalPose,
        Position = FixedPosition<RoomPixels>.FromWhole(new(80, 27)),
    };
    enemyAhead.Kinematics.Radius = new(5, 5);
    enemyAhead.Kinematics.InteractiveEnemies =
    [
        new SolidEnemyCollisionBody(Slot: 1, Index: 0x40, Position: new(89, 27), Radius: new(4, 8),
            Solid: true),
    ];
    SamusPoseId? enemyResult = SamusGroundedPoseTransitions.CheckProspectiveRunningPoseForWall(enemyAhead, 

        openFloor,
        SamusPoseId.MovingRightNormalPose,
        currentXSpeedKilledByBlock: false,
        out BlockMoveResult? enemyProbe);
    AssertEqual<SamusPoseId?>(SamusPoseId.RanIntoWallRightPose, enemyResult,
        "solid enemy ahead selects $89");
    AssertTrue(enemyProbe is null, "solid enemy ahead skips the one-pixel block move");
    AssertEqual(80, enemyAhead.Position.X.Whole, "solid enemy ahead leaves Samus in place");

    // A killed type-one move uses the CURRENT shot direction and performs no second probe.
    var killed = new SamusState { Pose = SamusPoseId.MovingRightNormalPose };
    SamusPoseId? killedResult = SamusGroundedPoseTransitions.CheckProspectiveRunningPoseForWall(killed, 

        openFloor,
        prospectivePose: null,
        currentXSpeedKilledByBlock: true,
        out BlockMoveResult? killedProbe);
    AssertEqual<SamusPoseId?>(SamusPoseId.RanIntoWallRightPose, killedResult,
        "killed running speed selects current wall pose");
    AssertTrue(killedProbe is null, "killed running speed skips one-pixel probe");

    // Every type-$15 pose executes no-base X, the shared grounding probe, and then clears
    // all five horizontal momentum words unconditionally.
    foreach ((SamusPoseId pose, _) in wallPoses)
    {
        var stopped = new SamusState { Pose = pose, Position = FixedPosition<RoomPixels>.FromWhole(new(80, 27)) };
        stopped.Kinematics.Radius = new(5, 5);
        stopped.HorizontalSpeed.BaseSpeed = new UnsignedSixteenSixteen(1, 0x4000);
        GroundedMovementResult movement = SamusGroundedMovement.StepRanIntoWall(

            openFloor,
            stopped,
            frameCounter: 0);
        AssertTrue(movement.Vertical.Collided, $"wall pose ${(int)pose:X2} remains grounded");
        AssertEqual(0u, stopped.HorizontalSpeed.BaseFixed,
            $"wall pose ${(int)pose:X2} clears base speed");
        AssertEqual(SamusAccelerationMode.Accelerating, stopped.HorizontalSpeed.AccelerationMode,
            $"wall pose ${(int)pose:X2} clears acceleration mode");
        SamusPresentation.InitializeAnimation(stopped);
        SamusPresentation.SetAnimationFrameFromSpecialHandler(stopped, 0, 9);
        SamusGroundedPoseTransitions.ApplyRanIntoWallPoseChange(stopped, pose);
        AssertEqual(9, stopped.AnimationFrameTimer.RawValue,
            $"same wall pose ${(int)pose:X2} preserves the running animation timer");
    }

    Console.WriteLine("  Ran into wall: ten-way selector, arm-pump pixel, six stable poses, grounding, and cleanup agree.");
}

/// <summary>
/// Running movement type one reads the normal-air entry at $90:9F61. These are the real
/// retail words already asserted independently by VerifySamusHorizontalSpeed.
/// </summary>
static void SeedRunningNormalAirSpeedRecord(TestAddressSpace bus)
{
    WriteTestWord(bus, 0x909f61, 0x0000); // acceleration whole
    WriteTestWord(bus, 0x909f63, 0x3000); // acceleration fraction
    WriteTestWord(bus, 0x909f65, 0x0002); // maximum whole
    WriteTestWord(bus, 0x909f67, 0xc000); // maximum fraction
    WriteTestWord(bus, 0x909f69, 0x0000); // deceleration whole
    WriteTestWord(bus, 0x909f6b, 0x8000); // deceleration fraction
}

/// <summary>
/// Crouched aim turns are the deliberate type-$17 oddity in their family. Gives that movement
/// type the same conspicuous synthetic speed record as type `$0E`; selecting the wrong table
/// address would otherwise read zero-filled fixture memory and stop.
/// </summary>
static void SeedCrouchedAimTurnSpeedRecord(TestAddressSpace bus)
{
    WriteTestWord(bus, 0x90a069, 0x0000);
    WriteTestWord(bus, 0x90a06b, 0x3000);
    WriteTestWord(bus, 0x90a06d, 0x0002);
    WriteTestWord(bus, 0x90a06f, 0xc000);
    WriteTestWord(bus, 0x90a071, 0x0000);
    WriteTestWord(bus, 0x90a073, 0x4000);
}

/// <summary>Seeds the dry-air normal-jump launch and gravity words read after a Moonwalk turn-jump's `$F8`.</summary>
static void SeedMoonwalkTurnJumpDryVerticalRows(TestAddressSpace bus)
{
    WriteTestWord(bus, 0x909eb9, 0x0004);
    WriteTestWord(bus, 0x909ebf, 0xe000);
    WriteTestWord(bus, 0x909ea1, 0x2800);
    WriteTestWord(bus, 0x909ea7, 0x0000);
}

    /// <summary>Runs the bank-$90/$91 grounded reversal suite through the verification command dispatcher.</summary>
    [VerificationCommand("--samus-grounded-reversal")]
    internal static int SamusGroundedReversalCommand()
    {
        Suite(nameof(VerifySamusGroundedReversal), () => VerifySamusGroundedReversal());
        return 0;
    }
}
