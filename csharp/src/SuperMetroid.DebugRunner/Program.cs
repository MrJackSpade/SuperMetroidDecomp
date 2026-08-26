using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

// This runner is an intentionally thin debugger host, not a claim that the full game has
// already been ported. Its job is to exercise every translated frame boundary against the
// user's private ROM while presenting stable, obvious breakpoint locations.
// Keep faults in the CLI. Without this process policy Windows can display a modal "unknown
// software exception" dialog for an unhandled debugger assertion, steal desktop focus, and
// leave the build output locked until somebody dismisses it.
if (OperatingSystem.IsWindows())
    NativeConsoleProcess.SetErrorMode(0x0001 | 0x0002 | 0x8000);

try
{
DebugRunnerOptions options = DebugRunnerOptions.Parse(args);
SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(options.RomPath);
var runtime = new SuperMetroidRuntime(bus);
runtime.MoonwalkEnabled = options.MoonwalkScript;

Console.WriteLine($"Loaded {Path.GetFullPath(options.RomPath)} ({bus.Rom.Length:N0} bytes).");
Console.WriteLine($"Stepping {options.FrameCount:N0} translated frames using {options.TimerScenario} timer startup.");
if (options.ReversalScript)
{
    Console.WriteLine(
        "Input script: Start, release, Right for 60 frames, Left for 60, Right for 60, then release.");
}
else if (options.MoonwalkScript)
{
    Console.WriteLine(
        "Input script: enable Moonwalk, walk backward right-facing through neutral/up/down aim, release, re-enter, then execute the $BF -> $1A jump route.");
}
else if (options.RanIntoWallScript)
{
    Console.WriteLine(
        "Input script: press into a ROM-authored solid wall, change wall-stop aim up/down, release to neutral, then jump away through $4B.");
}
else if (options.ShinesparkScript)
{
    Console.WriteLine(
        "Input script: equip Speed Booster, charge stage four on ROM-authored terrain, crouch to store shine, jump into $C7 windup, then launch right through $C9.");
}
else if (options.SpeedBoosterScript)
{
    Console.WriteLine(
        "Input script: equip Speed Booster, hold Right+Dash through ROM-authored acceleration stages, jump with the accumulated boost bonus, then observe both cancellation echoes return to Samus.");
}
else if (options.ScrewAttackScript)
{
    Console.WriteLine(
        "Input script: equip Space Jump and Screw Attack, enter ROM pose $81, then issue fresh Jump edges only inside the native falling-speed window while observing contact damage and the late-frame palette cycle.");
}
else if (options.WaterSpaceJumpScript)
{
    Console.WriteLine(
        "Input script: host-place a water FX surface through Samus's center, equip Space Jump, launch with ROM water physics, then pulse Jump inside the native $0080..$04FF partial-submersion window.");
}
else if (options.SpaceJumpScript)
{
    Console.WriteLine(
        "Input script: equip Space Jump, enter ROM pose $1B, then issue fresh Jump edges only while the live falling speed is inside the native $0280..$04FF window.");
}
else if (options.RunScript)
{
    Console.WriteLine(
        "Input script: hold Right+Dash through the ordinary 2.0000 cap, carry that exact extra component into a spin jump, release Jump, then coast to landing.");
}
else if (options.JumpScript)
{
    Console.WriteLine(
        "Input script: Start, short neutral jump, run right, full spin jump, then release.");
}
else if (options.PostureScript)
{
    Console.WriteLine(
        "Input script: crouch/stand right, turn left, crouch/stand left, then release.");
}
else if (options.AimScript)
{
    Console.WriteLine(
        "Input script: straight/up-diagonal/down-diagonal aim right, turn left, then repeat.");
}
else if (options.AimRunScript)
{
    Console.WriteLine(
        "Input script: aimed running up/down right, turn, aimed running up/down left, then release.");
}
else if (options.AimAirScript)
{
    Console.WriteLine(
        "Input script: aimed up/down normal jumps right, turn left, repeat, then release.");
}
else if (options.AerialTurnScript)
{
    Console.WriteLine(
        "Input script: neutral jump right, reverse left in mid-air, retain momentum through the ROM turn animation, then land.");
}
else if (options.CompactAirScript)
{
    Console.WriteLine(
        "Input script: enter/exit compact straight-down jump right, land compact, turn left, then mirror it.");
}
else if (options.AimCrouchScript)
{
    Console.WriteLine(
        "Input script: aimed crouch/live aim/stand right, turn left, repeat, then release.");
}
else if (options.AimTurnScript)
{
    Console.WriteLine(
        "Input script: diagonal-up, straight-up, and diagonal-down grounded aim turns in both directions.");
}
else if (options.CrouchTurnScript)
{
    Console.WriteLine(
        "Input script: ordinary, straight-up, diagonal-up, and diagonal-down crouched turns in both directions.");
}
else if (options.CrouchJumpScript)
{
    Console.WriteLine(
        "Input script: direct crouch exit, ordinary crouch jump, aimed crouch jump, then direct exit again.");
}
else if (options.MorphBallScript)
{
    Console.WriteLine(
        "Input script: crouch/morph, roll right, reverse left, stop, then unmorph; Morph Ball item bit is host-enabled.");
}
else if (options.SpringBallScript)
{
    Console.WriteLine(
        "Input script: crouch into equipped Spring Ball, roll, powered jump, release, and land.");
}
else if (options.BombJumpScript)
{
    Console.WriteLine(
        "Input script: crouch/morph, place a real normal bomb, then follow its ROM countdown, overlap, explosion, and straight bomb-jump arc.");
}
else if (options.KnockbackScript)
{
    Console.WriteLine(
        "Input script: host-inject one enemy-side hit result, run native knockback, press Left+Jump for the retail damage boost, then hold that chord through its arc.");
}
else if (options.GrappleFireScript)
{
    Console.WriteLine(
        "Input script: select grapple, fire right using ROM pose tables, hold through live room collision/cutoff, then observe queued cancellation.");
}
else if (options.GrappleScript)
{
    Console.WriteLine(
        "Input script: host-publish one connected grapple anchor, pump the ROM pendulum with Left/Right while holding Shoot, then release into native $51/$52 velocity.");
}
else if (options.CrystalFlashScript)
{
    Console.WriteLine(
        "Input script: host-publish the untranslated power-bomb-cleanup seam with exact Down+L+R+Shoot input, then execute ROM poses $D3/$01 through all three native Crystal Flash handlers.");
}
else if (options.MotherBrainRainbowScript)
{
    Console.WriteLine(
        "Actor script: execute `$A9:B8EB-$BFCF/$C710-$CABC` with retail Mother Brain body/neck bytecode, forced drained-Samus handlers, live Baby tile DMA, sine/table flight, drain/corpse handshake, release, Samus latch, and healing.");
}
else if (options.DrainedSamusScript)
{
    Console.WriteLine(
        "Actor script: publish the real Mother Brain/Baby Metroid drained-controller calls, execute ROM poses $E8/$EA/$E8/$01, and use $F7's installed vertical handler against live terrain.");
}
else if (options.DraygonGrabScript)
{
    Console.WriteLine(
        "Actor script: enter the real right-facing Draygon grab, exercise ROM poses $EC-$F0, reach 60 counted D-pad patterns, then execute $90:E2DE release to $01.");
}

// Copy the first 16 bytes at the reset bank into an otherwise-unused VRAM diagnostic page
// through the same queue/NMI path used by room and sprite uploads. Word $7800 stays clear
// of BG data, the standard $6000-$76FF sprite upload, and the timer at $7E00.
runtime.VramWrites.Enqueue(
    sizeInBytes: 16,
    sourceAddress: 0x808000,
    encodedVramDestination: 0x7800);

// Populate the exact BG3 HUD graphics/tilemap VRAM regions as another real NMI workload.
// The current snapshot represents Ceres-era 99 energy and no collected ammunition.
runtime.LoadUpperCrateriaBackgroundPalette();
runtime.InitializeHud(HudSnapshot.CeresDebug);

// These are the exact first two entries of the Ceres escape-timer transfer table at
// $A6:C4CB. They land at the OBJ addresses selected by gameplay's OBSEL=$03.
runtime.QueueEscapeTimerSpriteTiles();

// Establish the room before frame one. The command-line runner used to do this after the
// stepping loop, which meant its requested frames could advance the timer but could not run
// Landing Site's room-main sky uploads. The 9x5 dimensions come directly from
// RoomHeader_LandingSite ($8F:91F8); no extracted PNG or raw-asset directory participates in
// this live path.
runtime.InitializeLandingSiteCamera();
ScrollBoundaryCamera camera = runtime.Camera!;

// The optional grounded scenario must choose its camera before the native initial viewport
// fill. Its world X and desired screen Y are explicitly host-authored; the returned floor,
// slope height, resting Y, pose data, and every subsequent movement value are ROM-backed.
DebugGroundedSamusPlacement? groundedPlacement = null;
DebugRanIntoWallSamusPlacement? wallPlacement = null;
if (options.GroundedRun)
{
    if (options.GrappleScript)
    {
        groundedPlacement = runtime.InitializeDebugGrappleSwing();
    }
    else if (options.RanIntoWallScript || options.ScrewAttackScript)
    {
        // The core scans the decompressed room for an ordinary type-$8 corner. Keeping
        // the returned coordinates here makes the host-authored placement as inspectable
        // as the cartridge-authored blocks that the live one-pixel probe will consume.
        wallPlacement = runtime.InitializeDebugRanIntoWallSamus();
        groundedPlacement = wallPlacement.Value.Grounded;

        if (options.ScrewAttackScript)
        {
            // `$90:9D96` exposes Screw Attack frames 26/27 only when the spinning body
            // actually reaches a wall. Begin three blocks left of the wall selected above:
            // enough runway to establish `$09`, but close enough for the airborne body to
            // contact the same cartridge-authored type-$8 column before landing. Rebuild
            // the diagnostic placement record so later floor/map probes remain truthful.
            ushort screwStartX = unchecked((ushort)(runtime.Samus!.XPosition - 48));
            runtime.Samus.XPosition = screwStartX;
            int screwFloorBlockX = screwStartX >> 4;
            DebugGroundedSamusPlacement original = groundedPlacement.Value;
            groundedPlacement = original with
            {
                XPosition = screwStartX,
                BlockX = screwFloorBlockX,
                FloorBlock = runtime.LevelData!.GetCollisionBlock(
                    screwFloorBlockX,
                    original.BlockY),
            };
        }
    }
    else
    {
        groundedPlacement = runtime.InitializeDebugGroundedSamus();
    }
}

if (options.SpeedBoosterScript || options.ShinesparkScript)
{
    // The ordinary debug placement reaches Landing Site's right-side type-$F door before
    // 112 hexadecimal `.1000` additions can reach 7.0000. Move only this diagnostic route
    // 256 pixels left along the same ROM-authored floor; collision and all subsequent
    // motion remain native-data driven, while the separate type-$F dispatcher stays honest.
    runtime.Samus!.XPosition = unchecked((ushort)(runtime.Samus.XPosition - 256));
}
InitialViewportResult initialViewport = runtime.InitializeLandingSiteViewport();

// The cutscene door does not define a normal-gameplay Samus spawn. In the default scenario,
// introduce stationary pose $01 at a clearly documented host point so the native palette,
// animation-definition, tile-DMA, screen-position, split-spritemap, and OAM paths can be
// stepped without pretending that the cinematic spawned gameplay Samus.
if (!options.GroundedRun)
    runtime.InitializeDebugStandingSamus();

MotherBrainRainbowBeamAttackSequence? rainbowAttack = null;
BabyMetroidCutsceneState? cutsceneBaby = null;
MotherBrainEnemyProjectileSystem? motherBrainProjectiles = null;

if (options.MorphBallScript || options.BombJumpScript)
{
    // The Landing Site debugger spawn has no save-file inventory. The ordinary route needs
    // Morph Ball `$0004`; the bomb route additionally needs Bombs `$1000`. These are the
    // only host grants: placement/countdown/instructions/overlap/movement remain ROM-backed.
    runtime.Samus!.EquippedItems |= options.BombJumpScript ? (ushort)0x1004 : (ushort)0x0004;
}
else if (options.SpringBallScript)
{
    // As with ordinary Morph Ball, inventory is explicit debugger stimulus. Grant both
    // Morph Ball `$0004` and Spring Ball `$0002`; F9 must choose its equipped operands.
    runtime.Samus!.EquippedItems |= 0x0006;
}
else if (options.SpeedBoosterScript || options.ShinesparkScript)
{
    // The debug spawn has no save inventory. Grant only retail Speed Booster bit `$2000`;
    // every counter, delay list, velocity, transition, and collision remains ROM-driven.
    runtime.Samus!.EquippedItems |= 0x2000;
}
else if (options.SpaceJumpScript || options.WaterSpaceJumpScript || options.ScrewAttackScript)
{
    // Landing Site's debugger spawn deliberately begins without save-file inventory.
    // Space Jump is bit `$0200`; the Screw route adds `$0008`. Granting both on the latter
    // route is important: it makes the real `$91:F624` priority rule choose Screw art while
    // `$90:A436` independently continues to permit Space Jump's repeated-jump physics.
    runtime.Samus!.EquippedItems |= options.ScrewAttackScript
        ? (ushort)0x0208
        : (ushort)0x0200;
}
else if (options.CrystalFlashScript)
{
    // The runner has no save-file loader or complete power-bomb explosion lifecycle yet.
    // Publish only the missing inventory and bank-$88 cleanup stimulus. TryBegin below still
    // performs the native exact-input, velocity, energy, reserve, ammo, direction, and pose
    // checks before any scripted handler is allowed to become active.
    runtime.Samus!.Health = 1;
    runtime.Samus.MaxHealth = 99;
    runtime.Samus.ReserveEnergy = 0;
    runtime.Samus.MaxReserveEnergy = 0;
    runtime.Samus.Missiles = 10;
    runtime.Samus.SuperMissiles = 10;
    runtime.Samus.PowerBombs = 10;
    const ushort crystalFlashChord =
        (ushort)(SnesButton.Down | SnesButton.L | SnesButton.R | SnesButton.X);
    if (!runtime.TryBeginCrystalFlashFromPowerBombCleanup(crystalFlashChord))
        throw new InvalidOperationException("Real-ROM Crystal Flash initiation rejected its canonical fixture.");
}
else if (options.MotherBrainRainbowScript)
{
    // The Landing Site supplies a real ROM/PPU/runtime host, not Mother Brain's room spawn.
    // Put only the encounter-local actor coordinates and inventory at their documented seam;
    // every wait, list opcode, pose, resource tick, and forced displacement remains translated.
    runtime.Samus!.Health = 800;
    runtime.Samus.MaxHealth = 899;
    runtime.Samus.Missiles = 80;
    runtime.Samus.SuperMissiles = 80;
    runtime.Samus.PowerBombs = 400;
    runtime.Samus.XPosition = 220;
    runtime.Samus.YPosition = 124;
    runtime.Samus.Kinematics.XSubposition = 0;
    runtime.Samus.Kinematics.YSubposition = 0;

    rainbowAttack = new MotherBrainRainbowBeamAttackSequence
    {
        BrainXPosition = 64,
        BrainYPosition = 96,
    };
    rainbowAttack.Body.XPosition = 64;
    rainbowAttack.Body.YPosition = 100;
    // Native brain-slot initialization `$A9:8705` creates the 48-entry rot table and
    // extracts the right-hand corpse graphics frame immediately, long before the death AI
    // consumes it. Do the same here against the live ROM/WRAM bus so late debugger stepping
    // observes real data and the normal VRAM queue can read its `$7E:9000` working buffer.
    rainbowAttack.InitializeCorpseRotting(bus);
    rainbowAttack.StartAttackCycle();
    motherBrainProjectiles = new MotherBrainEnemyProjectileSystem();
}
else if (options.DrainedSamusScript)
{
    // The complete bank-$A9 boss actor does not exist yet. Lift the normal grounded debug
    // placement by two blocks, then publish exactly controller function zero. Pose metadata,
    // animation bytecode, gravity, collision, spritemaps, tile DMA, and later controller
    // calls all remain live ROM data; only the missing actor's call timing is host-authored.
    runtime.Samus!.YPosition = unchecked((ushort)(runtime.Samus.YPosition - 32));
    runtime.Samus.Drained.LetFall(bus, runtime.Samus);
}
else if (options.DraygonGrabScript)
{
    // A live Draygon enemy actor is not part of Landing Site. Host-publish only its body
    // coordinate at the exact `$A5:94A9` seam, choosing it so the first application keeps
    // the grounded debug placement unchanged. Every pose, delay, tile definition, sprite-
    // map, input transition, escape count, release side effect, and later camera read comes
    // from translated logic plus this private ROM.
    runtime.Samus!.DraygonGrabbed.Begin(bus, runtime.Samus, draygonFacingRight: true);
    runtime.Samus.DraygonGrabbed.ApplyOwnerPosition(
        runtime.Samus,
        unchecked((ushort)(runtime.Samus.XPosition - 8)),
        unchecked((ushort)(runtime.Samus.YPosition - 0x28)),
        draygonFacingRight: true);
}

if (options.WaterSpaceJumpScript)
{
    // Landing Site has scrolling-sky FX rather than water. This route supplies only the
    // missing room-FX words at their normal producer/consumer seam; surface comparisons,
    // launch/gravity/X tables, animation delay, transition records, collision, and art all
    // remain live cartridge data. Placing the surface at center Y keeps Samus partially
    // submerged so `$90:A436` can exercise its distinct top- and bottom-boundary branches.
    ushort debugWaterSurface = runtime.Samus!.YPosition;
    runtime.Samus.LiquidPhysics.ConfigureWater(debugWaterSurface);
    runtime.Samus.LiquidPhysics.InitializeRememberedMedium(runtime.Samus);
    Console.WriteLine(
        $"Debug water FX stimulus: surface Y=${debugWaterSurface:X4}, " +
        $"top=${runtime.Samus.Kinematics.TopBoundary:X4}, " +
        $"bottom=${runtime.Samus.Kinematics.BottomBoundary:X4}, " +
        $"remembered medium={runtime.Samus.LiquidPhysics.LiquidPhysicsType}.");
}

if (options.GrappleFireScript)
    runtime.EnableDebugGrappleItemSelection();

Console.WriteLine(
    $"Loaded Landing Site scrolls $8F:9283 -> $7E:CD20: " +
    $"{Convert.ToHexString(camera.Scrolls.Storage[..camera.Scrolls.LogicalCellCount])}.");
Console.WriteLine(
    $"Bank $80 BG bookkeeping selected {initialViewport.UpdateRequestCount} initial column update request(s) " +
    $"at camera ({camera.XPosition},{camera.YPosition}).");
Console.WriteLine(
    $"Door $83:{runtime.LandingSiteEntry!.DoorPointer:X4} selected sky " +
    $"${runtime.LandingSiteEntry.SkySourceAddress >> 16:X2}:" +
    $"{runtime.LandingSiteEntry.SkySourceAddress & 0xffff:X4} -> " +
    $"VRAM ${runtime.LandingSiteEntry.SkyVramDestination:X4}, " +
    $"${runtime.LandingSiteEntry.SkyByteCount:X4} bytes.");
Console.WriteLine(
    $"Expanded those requests from real cartridge level data and executed " +
    $"{initialViewport.DmaSegmentCount} initial BG1 DMA segment(s); bank $88 also queued " +
    $"the first four circular BG2 sky rows for frame one's NMI.");
Console.WriteLine(
    $"Debug Samus uses cartridge pose ${runtime.Samus!.Pose:X2}, frame {runtime.Samus.AnimationFrame}, " +
    $"world position ({runtime.Samus.XPosition},{runtime.Samus.YPosition}); " +
    (groundedPlacement is DebugGroundedSamusPlacement placement
        ? $"floor=({placement.BlockX},{placement.BlockY}) type=${placement.FloorBlock.CollisionType:X1}/" +
          $"BTS ${placement.FloorBlock.Behavior:X2}, height={placement.FloorHeight}; only X/screen framing are host-selected."
        : "only that placement is host-selected."));
if (wallPlacement is DebugRanIntoWallSamusPlacement wallDiagnostic)
{
    Console.WriteLine(
        $"Wall regression uses ROM column ${wallDiagnostic.WallBlockX:X2}, rows " +
        $"${wallDiagnostic.WallTopBlockY:X2}-${wallDiagnostic.WallBottomBlockY:X2}; " +
        $"standing center X=${wallDiagnostic.Grounded.XPosition:X4} is exactly one " +
        "prospective running pixel from collision.");
}
if (options.GrappleScript || options.GrappleFireScript)
{
    // Inventory/PLM placement is not inferred from graphics. Inventory-like BTS type $E
    // is the only bank-$94 block family that can return a grapple connection, so list the
    // real room coordinates before running the script. This diagnostic is also useful when
    // choosing a future non-host-authored grapple firing route.
    var grappleBlocks = new List<string>();
    for (int blockY = 0; blockY < runtime.LevelData!.HeightInBlocks; blockY++)
    {
        for (int blockX = 0; blockX < runtime.LevelData.WidthInBlocks; blockX++)
        {
            RoomCollisionBlock block = runtime.LevelData.GetCollisionBlock(blockX, blockY);
            if (block.CollisionType == 0x0e)
                grappleBlocks.Add($"({blockX:X2},{blockY:X2}):{block.Behavior:X2}");
        }
    }
    Console.WriteLine(
        $"Landing Site grapple blocks (X,Y:BTS): " +
        (grappleBlocks.Count == 0 ? "none" : string.Join(' ', grappleBlocks)));
    Console.WriteLine(
        $"Grapple state: anchor=({runtime.Samus.Grapple.AnchorX},{runtime.Samus.Grapple.AnchorY}), " +
        $"beamStart=({runtime.Samus.Grapple.BeamStartX},{runtime.Samus.Grapple.BeamStartY}), " +
        $"length={runtime.Samus.Grapple.RopeLength}, angle=${runtime.Samus.Grapple.Angle:X4}, " +
        $"angularVelocity=${unchecked((ushort)runtime.Samus.Grapple.AngularVelocity):X4}.");
}

// Resolve this through the same two-stage pointer calculation used by live movement. This
// line is deliberately ROM-backed evidence, not a hard-coded description: ordinary air's
// base $9F55 plus running movement type 1 selects the 12-byte entry at $90:9F61.
SamusHorizontalSpeedState horizontalSpeed = runtime.Samus.HorizontalSpeed;
horizontalSpeed.SelectEnvironmentSpeedTable(
    runtime.Samus.LiquidPhysics.DetermineMovementMedium(runtime.Samus));
int runningSpeedAddress = horizontalSpeed.ResolveEntryAddress(movementType: 1);
SpeedTableEntry runningSpeed = horizontalSpeed.ReadEntry(bus, movementType: 1);
Console.WriteLine(
    $"Running speed table ${runningSpeedAddress >> 16:X2}:{runningSpeedAddress & 0xffff:X4}: " +
    $"accel {runningSpeed.Acceleration:X4}.{runningSpeed.AccelerationSubspeed:X4}, " +
    $"max {runningSpeed.MaximumSpeed:X4}.{runningSpeed.MaximumSubspeed:X4}, " +
    $"decel {runningSpeed.Deceleration:X4}.{runningSpeed.DecelerationSubspeed:X4}.");

// Pose parameters are eight-byte records at $91:B629. Offset six is y_radius, which bank
// $94 uses to choose every 16-pixel row crossed by horizontal collision and the bottom
// probe used by BlockInsideDetection. Log the real BG1/BTS pair at both points so the next
// collision slice begins with visible cartridge evidence.
int poseDefinitionAddress = 0x91b629 + runtime.Samus.Pose * 8;
byte samusYRadius = bus.ReadByte(poseDefinitionAddress + 6);
RoomCollisionBlock centerBlock = runtime.LevelData!.GetCollisionBlockAtPixel(
    runtime.Samus.XPosition,
    runtime.Samus.YPosition);
RoomCollisionBlock bottomBlock = runtime.LevelData.GetCollisionBlockAtPixel(
    runtime.Samus.XPosition,
    unchecked((ushort)(runtime.Samus.YPosition + samusYRadius - 1)));
Console.WriteLine(
    $"Debug Samus collision probes: radiusY={samusYRadius}; " +
    $"center index={centerBlock.Index} level=${centerBlock.LevelWord:X4} BTS=${centerBlock.Behavior:X2} type=${centerBlock.CollisionType:X1}; " +
    $"bottom index={bottomBlock.Index} level=${bottomBlock.LevelWord:X4} BTS=${bottomBlock.Behavior:X2} type=${bottomBlock.CollisionType:X1}.");

if (groundedPlacement is DebugGroundedSamusPlacement groundedDiagnostic)
{
    // Landing Site contains collision-authored surfaces whose graphics are transparent in
    // the static room composition. List every lower non-air candidate at the selected X so
    // a grounded preview can deliberately choose a surface with visible terrain instead of
    // silently assuming the first collision block must have artwork.
    var floorCandidates = new List<string>();
    for (int blockY = groundedDiagnostic.BlockY;
         blockY < runtime.LevelData.HeightInBlocks;
         blockY++)
    {
        RoomCollisionBlock candidate = runtime.LevelData.GetCollisionBlock(
            groundedDiagnostic.BlockX,
            blockY);
        if (candidate.CollisionType != 0)
        {
            floorCandidates.Add(
                $"{blockY:X2}:{candidate.CollisionType:X1}/{candidate.Behavior:X2}/" +
                $"{candidate.LevelWord:X4}");
        }
    }
    Console.WriteLine(
        $"Non-air candidates below grounded X (Y:type/BTS/level): " +
        string.Join(' ', floorCandidates));
}

// This is analysis output, not collision behavior: locate the next non-air dispatcher row
// below the host-selected stimulus. It makes an accidental floating spawn immediately
// obvious and gives the grounding port an exact target block/index/BTS triple.
for (int blockY = (runtime.Samus.YPosition + samusYRadius - 1) >> 4;
     blockY < runtime.LevelData.HeightInBlocks;
     blockY++)
{
    RoomCollisionBlock candidate = runtime.LevelData.GetCollisionBlock(
        runtime.Samus.XPosition >> 4,
        blockY);
    if (candidate.CollisionType == 0)
        continue;

    Console.WriteLine(
        $"First non-air block below debug Samus: block=({runtime.Samus.XPosition >> 4},{blockY}) " +
        $"index={candidate.Index} topY={blockY * 16} level=${candidate.LevelWord:X4} " +
        $"BTS=${candidate.Behavior:X2} type=${candidate.CollisionType:X1}.");

    // A compact neighboring-row dump reveals whether the candidate is an isolated special
    // block or part of a continuous slope/solid running lane. Each token is X:type/BTS.
    int firstDiagnosticX = Math.Max(0, (runtime.Samus.XPosition >> 4) - 8);
    int lastDiagnosticX = Math.Min(runtime.LevelData.WidthInBlocks - 1, firstDiagnosticX + 16);
    var rowTokens = new List<string>();
    for (int blockX = firstDiagnosticX; blockX <= lastDiagnosticX; blockX++)
    {
        RoomCollisionBlock neighbor = runtime.LevelData.GetCollisionBlock(blockX, blockY);
        rowTokens.Add($"{blockX:X2}:{neighbor.CollisionType:X1}/{neighbor.Behavior:X2}");
    }
    Console.WriteLine($"Collision row {blockY:X2} around stimulus: {string.Join(' ', rowTokens)}");

    // For a non-square floor, the height-table sample is measured downward from the
    // block's top. Derive the exact center Y that places Samus's bottom on that surface,
    // then execute a +1.0 native grounding probe and a separate +1.0 horizontal scan.
    // These copies do not relocate the visible debug stimulus.
    if (candidate.CollisionType == 1 &&
        (candidate.Behavior & 0x1f) >= 5 &&
        (candidate.Behavior & 0x80) == 0)
    {
        byte floorHeight = SamusSlopePhysics.ReadAlignmentHeight(
            bus,
            candidate.Behavior,
            runtime.Samus.XPosition);
        ushort restingCenterY = unchecked((ushort)(blockY * 16 + floorHeight - samusYRadius));

        var verticalProbe = new SamusKinematicsState
        {
            XPosition = runtime.Samus.XPosition,
            YPosition = restingCenterY,
            XRadius = runtime.Samus.Kinematics.XRadius,
            YRadius = samusYRadius,
        };
        BlockMoveResult grounding = SamusBlockCollision.MoveVertical(
            bus,
            runtime.LevelData,
            verticalProbe,
            displacement: 0x00010000,
            scanLeftToRight: (runtime.NmiFrameCounter & 1) == 0);

        var horizontalProbe = new SamusKinematicsState
        {
            XPosition = runtime.Samus.XPosition,
            YPosition = restingCenterY,
            XRadius = runtime.Samus.Kinematics.XRadius,
            YRadius = samusYRadius,
        };
        BlockMoveResult horizontal = SamusBlockCollision.MoveHorizontal(
            bus,
            runtime.LevelData,
            horizontalProbe,
            displacement: 0x00010000);

        Console.WriteLine(
            $"ROM floor probe: height={floorHeight}, restingY={restingCenterY}; " +
            $"down +1.0 => accepted=${grounding.AcceptedDisplacement:X8}, collision={grounding.Collided}; " +
            $"right +1.0 => accepted=${horizontal.AcceptedDisplacement:X8}, " +
            $"position={horizontalProbe.XPosition:X4}.{horizontalProbe.XSubposition:X4}, " +
            $"Y={horizontalProbe.YPosition}.");
    }
    break;
}

// Ordinary timer diagnostics still choose a host scenario at startup. The Mother Brain
// route must not do that: `$A9:B309` owns its real status-$0002 request near the very end of
// the death sequence, and prestarting it here would let the timer expire during the fight.
if (!options.MotherBrainRainbowScript)
{
    if (options.TimerScenario == TimerScenario.Ceres)
        runtime.EscapeTimer.RequestCeresStart();
    else
        runtime.EscapeTimer.RequestMotherBrainStart();
}

EscapeTimerState priorState = runtime.EscapeTimer.State;
bool priorEscapeTimerExpired = false;
ushort priorSamusFrame = runtime.Samus!.AnimationFrame;
byte priorSamusPose = runtime.Samus.Pose;
uint priorSamusX = runtime.Samus.Kinematics.XFixed;
uint priorSamusY = runtime.Samus.Kinematics.YFixed;
ushort? priorProspectivePose = null;
ushort? priorFallbackPose = null;
byte? priorWallCollisionPose = null;
bool observedBombJumpStart = false;
bool observedBombJumpEnd = false;
bool observedBombJumpRise = false;
bool observedBombPlacement = false;
bool observedBombExplosion = false;
bool observedBombDeletion = false;
bool observedStraightBombOverlap = false;
bool observedKnockbackMovement = false;
bool observedDamageBoostMovement = false;
bool observedGrappleSwing = false;
bool observedGrappleReleaseQueue = false;
bool observedGrappleRelease = false;
bool observedGrappleReleaseMovement = false;
bool observedGrappleTerrainCollision = false;
bool observedGrappleFire = false;
bool observedGrappleFireCancelQueue = false;
bool observedGrappleFireCancel = false;
bool observedBlockedRanIntoWallProbe = false;
bool observedDashMomentum = false;
bool observedDashAerialCarry = false;
uint maximumObservedExtraRunSpeed = 0;
byte maximumObservedSpeedBoostStage = 0;
bool observedSpeedBoostEcho = false;
bool observedSpeedBoostContactDamage = false;
bool observedSpeedBoostDeparture = false;
bool observedSpeedBoostDepartureFinished = false;
bool previousSpeedBoostDeparture = false;
bool observedStoredShine = false;
bool observedShinesparkWindup = false;
bool observedDirectionalShinespark = false;
bool observedShinesparkMovement = false;
bool observedShinesparkPalette = false;
bool observedShinesparkCrashOrbit = false;
bool observedShinesparkCrashEchoCircle = false;
bool observedShinesparkCrashFinish = false;
bool observedReleasedShinesparkEcho = false;
int priorReleasedShinesparkEchoCount = 0;
int observedSpaceJumpRestarts = 0;
bool observedScrewAttackContactDamage = false;
bool observedScrewAttackPaletteCycle = false;
bool observedCrystalFlashDrain = false;
bool observedCrystalFlashFinish = false;
bool observedCrystalFlashCompletion = false;
bool observedDrainedFallingHandler = false;
bool observedDrainedLanding = false;
bool observedDrainedStanding = false;
bool observedDrainedCrouching = false;
bool observedDrainedRelease = false;
bool observedDrainedHyperBeam = false;
bool observedDraygonAimUp = false;
bool observedDraygonFiring = false;
bool observedDraygonAimDown = false;
bool observedDraygonMoving = false;
bool observedDraygonNeutralFallback = false;
bool observedDraygonRelease = false;
bool issuedDrainedStandingCommand = false;
bool issuedDrainedCrouchingCommand = false;
bool issuedDrainedReleaseCommand = false;
bool issuedDrainedHyperBeamCommand = false;
int issuedSpaceJumpPulses = 0;
bool spaceJumpPulseMayBeIssued = true;
var observedRainbowPhases = new HashSet<MotherBrainRainbowBeamAttackPhase>();
var observedPhaseThreeAttacks = new HashSet<MotherBrainPhase3AttackKind>();
bool observedPhaseThreeForwardMovement = false;
var observedBabyPhases = new HashSet<BabyMetroidCutscenePhase>();
var observedBabyTileTransfers = new List<MotherBrainSpriteTileTransferRequest>();
var observedMotherBrainCorpseTileTransfers = new List<MotherBrainSpriteTileTransferRequest>();
var observedAttackTileTransfers = new List<MotherBrainSpriteTileTransferRequest>();
var observedBabyDeathPalettes = new List<BabyMetroidPaletteTransferRequest>();
var observedPhaseThreeBackgroundPalettes = new List<MotherBrainBackgroundPaletteTransferRequest>();
int observedBabyDeathExplosions = 0;
int allocatedBabyDeathExplosions = 0;
bool observedBabyPhaseThreeHandoff = false;
bool observedBabySpawnRequest = false;
bool observedFinalBeamSound = false;
bool observedBabyMotherBrainInterrupt = false;
bool observedBabyCeilingTableInstall = false;
bool observedBabyHealingCompletion = false;
int observedBabyLatchOntoSamusFrame = 0;
int observedBabyHealSamusFrame = 0;
int observedBabyHealingCompletionFrame = 0;
ushort observedBabyHealingCompletionHealth = 0;
ushort observedBabyHealingCompletionReserveEnergy = 0;
// Preserve each ROM-record boundary as a fixed-point witness. Merely reaching `$CA66`
// would not detect a carry bug that happened to converge on the same broad target rectangle.
var observedBabyRouteFrames = new Dictionary<ushort, int>();
var observedBabyRoutePoints = new Dictionary<ushort, BabyMetroidCutscenePoint>();
MotherBrainRainbowBeamAttackPhase previousRainbowPhase =
    rainbowAttack?.Phase ?? MotherBrainRainbowBeamAttackPhase.Inactive;
BabyMetroidCutscenePhase previousBabyPhase = BabyMetroidCutscenePhase.Inactive;
ushort previousBabyMovementTablePointer = 0;
// Keep the actual post-frame poses, rather than assuming the requested inputs succeeded.
// The dedicated ROM regression below fails unless both compact bodies and both native
// ordinary-landing records were genuinely installed by the translated frame pipeline.
var observedSamusPoses = new HashSet<byte> { runtime.Samus.Pose };
for (int frameIndex = 0; frameIndex < options.FrameCount; frameIndex++)
{
    if (options.DraygonGrabScript && runtime.Samus!.DraygonGrabbed.IsActive)
    {
        // EnemyMain runs before Samus's draw and calls `$A5:94A9` after updating Draygon.
        // This asset/handler route deliberately holds that absent enemy actor still; the
        // repeated call proves no ordinary Samus physics drifts away from the supplied claw
        // coordinate. It is an explicit fixed actor stimulus, not a fabricated boss flight.
        runtime.Samus.DraygonGrabbed.ApplyOwnerPosition(
            runtime.Samus,
            runtime.Samus.DraygonGrabbed.OwnerXPosition,
            runtime.Samus.DraygonGrabbed.OwnerYPosition,
            draygonFacingRight: true);
    }

    // The default script presses Start for one frame, releases it, then holds Right. The
    // explicit reversal script is a deterministic real-ROM regression route: enough time
    // to accelerate right, complete $25 toward the left, then complete $26 back right.
    // These are only controller samples; all pose choices still come from bank-$91 tables.
    bool specialSpinRoute = options.SpaceJumpScript ||
        options.WaterSpaceJumpScript ||
        options.ScrewAttackScript;

    // Unlike the fixed-input posture routes below, Space Jump's legal repeat instant is a
    // function of the live 16.16 vertical velocity. Derive the unaligned 8.8 magnitude in
    // the same way as `$90:A436`: high byte of subspeed below the integer speed. A pulse is
    // emitted for exactly one frame, so each accepted attempt has the required new-A edge.
    // The first held interval is still an ordinary grounded running jump; only later pulses
    // are velocity-gated. This keeps the harness deterministic without faking any movement.
    uint liveVerticalMagnitude8Point8 =
        ((uint)runtime.Samus.Kinematics.YSpeed << 8) |
        ((uint)runtime.Samus.Kinematics.YSubspeed >> 8);
    uint liveSpaceJumpMinimum = runtime.Samus.LiquidPhysics.LiquidPhysicsType !=
        SamusLiquidPhysicsState.Air
            ? 0x0080u
            : 0x0280u;
    bool liveSpaceJumpWindow =
        runtime.Samus.Kinematics.YDirection == 2 &&
        liveVerticalMagnitude8Point8 >= liveSpaceJumpMinimum &&
        liveVerticalMagnitude8Point8 < 0x0500;
    bool screwBodyReachedRightWall =
        options.ScrewAttackScript &&
        wallPlacement is DebugRanIntoWallSamusPlacement screwWall &&
        runtime.Samus.XPosition >= screwWall.Grounded.XPosition - 8;
    ushort specialSpinDirection = screwBodyReachedRightWall
        ? (ushort)SnesButton.Left
        : (ushort)SnesButton.Right;
    ushort specialSpinInput = frameIndex switch
    {
        0 => (ushort)SnesButton.Start,
        >= 2 and < 12 => specialSpinDirection,
        >= 12 and < 24 => (ushort)(specialSpinDirection | (ushort)SnesButton.A),
        _ when liveSpaceJumpWindow && spaceJumpPulseMayBeIssued && issuedSpaceJumpPulses < 2
            => (ushort)(specialSpinDirection | (ushort)SnesButton.A),
        _ => specialSpinDirection,
    };
    if (specialSpinRoute && frameIndex >= 24)
    {
        bool jumpPressedThisFrame = (specialSpinInput & (ushort)SnesButton.A) != 0;
        if (jumpPressedThisFrame)
        {
            issuedSpaceJumpPulses++;
            spaceJumpPulseMayBeIssued = false;
            Console.WriteLine(
                $"frame {frameIndex + 1,4}: issued Space Jump pulse {issuedSpaceJumpPulses} " +
                $"at falling magnitude ${liveVerticalMagnitude8Point8:X4}.");
        }
        else
        {
            // One released sample is sufficient to make the following A sample a fresh
            // edge, exactly as the controller new-input word on the SNES would require.
            spaceJumpPulseMayBeIssued = true;
        }
    }

    ushort yDirectionBeforeFrame = runtime.Samus.Kinematics.YDirection;
    ushort controllerInput = specialSpinRoute
        ? specialSpinInput
        : options.DraygonGrabScript
        ? frameIndex switch
        {
            // `$91:AE56` maps the default shoulder bindings to up/down aim and Shoot to
            // firing. A direction selects the six-frame moving/struggling body `$F0`.
            >= 16 and < 32 => (ushort)SnesButton.R,
            >= 32 and < 48 => (ushort)SnesButton.X,
            >= 48 and < 64 => (ushort)SnesButton.L,
            >= 64 and < 96 => (ushort)SnesButton.Right,

            // Zero input must use pose-definition byte two to return `$F0 -> $EC`.
            // The earlier Right edge that entered `$F0` already contributed one count to
            // `$90:E2A1`. Alternate Up/Down for the remaining 59 distinct samples so the
            // sixtieth call releases Samus, then leave subsequent frames genuinely blank.
            // That last detail keeps the route from immediately applying ordinary pose-$01
            // controller transitions after it has proved the native release state.
            >= 112 and < 171 => (frameIndex & 1) == 0
                ? (ushort)SnesButton.Up
                : (ushort)SnesButton.Down,
            _ => (ushort)0,
        }
        : options.CrystalFlashScript || options.DrainedSamusScript ||
          options.MotherBrainRainbowScript
        ? (ushort)0
        : options.GrappleFireScript
        ? frameIndex < 16 ? (ushort)SnesButton.X : (ushort)0
        : options.GrappleScript
        ? frameIndex switch
        {
            // X is the runtime's default Shoot binding. Pump left through the first half,
            // right through the second, then release so the two-frame $C79D/$CB8B seam is
            // visible in both logs and debugger watches.
            < 45 => (ushort)(SnesButton.X | SnesButton.Left),
            < 90 => (ushort)(SnesButton.X | SnesButton.Right),
            _ => (ushort)0,
        }
        : options.KnockbackScript
        ? frameIndex switch
        {
            0 => (ushort)SnesButton.Start,

            // Frame 20 below injects the only missing producer: the one-bit enemy-side
            // collision result. Leave this sample empty so `$53` gets one visible native
            // hurt frame. On frame 21, Left+Jump is canonical `$0280` and matches the
            // literal `$91:A8E4 -> $50` record in the cartridge.
            >= 21 and < 46 => (ushort)(SnesButton.Left | SnesButton.A),

            // `$50` is movement type `$19`, whose dispatcher entry calls the ordinary
            // jumping routine. Keeping `$0280` held selects `$50`'s self-record and proves
            // variable-height movement; release later lets it descend and land normally.
            _ => (ushort)0,
        }
        : options.MoonwalkScript
        ? frameIndex switch
        {
            0 => (ushort)SnesButton.Start,

            // Standing-right `$01` plus backward Left and Shoot proposes `$4A`. The host
            // option merely admits that cartridge candidate; type `$10` owns every step.
            >= 2 and < 30 => (ushort)(SnesButton.Left | SnesButton.X),

            // `$91:A8AC` changes only the moonwalk art/shot direction while the same
            // backward direction stays held. R selects `$76`; L then selects `$78`.
            >= 30 and < 50 => (ushort)(SnesButton.Left | SnesButton.X | SnesButton.R),
            >= 50 and < 70 => (ushort)(SnesButton.Left | SnesButton.X | SnesButton.L),

            // Zero input proves definition fallbacks `$78 -> $07 -> $01`. Re-enter the
            // neutral family, then replace Shoot with Jump while still moving backward.
            >= 80 and < 100 => (ushort)(SnesButton.Left | SnesButton.X),
            >= 100 and < 122 => (ushort)(SnesButton.Left | SnesButton.A),
            _ => (ushort)0,
        }
        : options.RanIntoWallScript
        ? frameIndex switch
        {
            0 => (ushort)SnesButton.Start,

            // Standing `$01` proposes running `$09`. The host placed the current body on
            // the last safe pixel, so `$91:EADE`'s real +1.0000 block move must reject it
            // and use `$09`'s shot direction two to select neutral wall pose `$89`.
            >= 2 and < 12 => (ushort)SnesButton.Right,

            // Preserve forward Right while changing the actual controller shoulder. The
            // unchanged `$91:AA38` records propose running aim `$0F/$11`; the same block
            // probe then maps their shot directions one/three to wall aim `$CF/$D1`.
            >= 12 and < 22 => (ushort)(SnesButton.Right | SnesButton.R),
            >= 22 and < 32 => (ushort)(SnesButton.Right | SnesButton.L),

            // Releasing every button exercises pose-definition fallback `$D1 -> $89`.
            // A fresh Jump edge then takes the literal wall table's `$89 -> $4B` route;
            // command `$FF` completes the normal `$4B -> $4D` jump handoff.
            >= 42 and < 52 => (ushort)SnesButton.A,
            _ => (ushort)0,
        }
        : options.ShinesparkScript
        ? frameIndex switch
        {
            0 => (ushort)SnesButton.Start,

            // This is the same natural stage-four route used by --speed-booster-script.
            // The host grants only inventory; `$90:973E`, the ROM delay lists, and the
            // translated block dispatcher own every acceleration and accepted pixel.
            >= 2 and < 118 => (ushort)(SnesButton.Right | SnesButton.B),

            // A fresh Down edge while stage four is still published selects the retail
            // running-to-crouch record. `$91:F7B0` must observe that stage before the
            // posture transition clears normal running momentum and store 180 frames.
            >= 118 and < 128 => (ushort)SnesButton.Down,

            // Jump from the settled crouch. The ordinary `$4B` transition must finish
            // through command `$FF` into `$4D`; only there may stored shine replace the
            // final pose with windup `$C7`, precisely matching `$90:CFFA`'s native seam.
            >= 138 and < 154 => (ushort)SnesButton.A,

            // Windup direction is chosen from a newly pressed direction. Delay Right
            // until after several held-still windup frames so this cannot accidentally
            // pass by merely preserving the direction from the charging run.
            >= 154 and < 190 => (ushort)(SnesButton.A | SnesButton.Right),
            _ => (ushort)0,
        }
        : options.SpeedBoosterScript
        ? frameIndex switch
        {
            0 => (ushort)SnesButton.Start,
            >= 2 and < 118 => (ushort)(SnesButton.Right | SnesButton.B),
            >= 118 and < 138 => (ushort)(SnesButton.Right | SnesButton.B | SnesButton.A),
            >= 138 and < 166 => (ushort)SnesButton.Right,
            _ => (ushort)0,
        }
        : options.RunScript
        ? frameIndex switch
        {
            0 => (ushort)SnesButton.Start,

            // The standing frame first lets the unchanged bank-$91 transition table
            // install `$09`. Every subsequent frame reaches `$90:973E` with movement type
            // one and canonical Dash/B `$8000`; C# contributes no velocity constant here.
            >= 2 and < 38 => (ushort)(SnesButton.Right | SnesButton.B),

            // A fresh Jump edge selects retail spin pose `$19`. B remains held briefly,
            // but type three must take `$90:9808` and retain rather than increment 2.0000.
            >= 38 and < 52 => (ushort)(SnesButton.Right | SnesButton.B | SnesButton.A),

            // Releasing B proves momentum, not held input, owns the airborne extra pair.
            // Releasing A later exercises the existing variable-height cutoff and descent.
            >= 52 and < 68 => (ushort)(SnesButton.Right | SnesButton.A),
            >= 68 and < 88 => (ushort)SnesButton.Right,
            _ => (ushort)0,
        }
        : options.ReversalScript
        ? frameIndex switch
        {
            0 => (ushort)SnesButton.Start,
            >= 2 and < 62 => (ushort)SnesButton.Right,
            >= 62 and < 122 => (ushort)SnesButton.Left,
            >= 122 and < 182 => (ushort)SnesButton.Right,
            _ => (ushort)0,
        }
        : options.JumpScript
            ? frameIndex switch
            {
                0 => (ushort)SnesButton.Start,
                >= 2 and < 12 => (ushort)SnesButton.A,
                >= 50 and < 90 => (ushort)SnesButton.Right,
                >= 90 and < 125 => (ushort)(SnesButton.Right | SnesButton.A),
                >= 125 and < 155 => (ushort)SnesButton.Right,
                _ => (ushort)0,
            }
        : options.PostureScript
            ? frameIndex switch
            {
                0 => (ushort)SnesButton.Start,
                >= 2 and < 10 => (ushort)SnesButton.Down,
                20 => (ushort)SnesButton.Up,
                >= 40 and < 65 => (ushort)SnesButton.Left,
                >= 75 and < 83 => (ushort)SnesButton.Down,
                95 => (ushort)SnesButton.Up,
                _ => (ushort)0,
            }
        : options.AimScript
            ? frameIndex switch
            {
                0 => (ushort)SnesButton.Start,
                >= 2 and < 10 => (ushort)SnesButton.Up,
                >= 20 and < 28 => (ushort)SnesButton.R,
                >= 38 and < 46 => (ushort)SnesButton.L,
                >= 60 and < 85 => (ushort)SnesButton.Left,
                >= 100 and < 108 => (ushort)SnesButton.Up,
                >= 118 and < 126 => (ushort)SnesButton.R,
                >= 136 and < 144 => (ushort)SnesButton.L,
                _ => (ushort)0,
            }
        : options.AimRunScript
            ? frameIndex switch
            {
                0 => (ushort)SnesButton.Start,
                >= 2 and < 33 => (ushort)(SnesButton.Right | SnesButton.R),
                >= 33 and < 41 => (ushort)SnesButton.R,
                >= 50 and < 81 => (ushort)(SnesButton.Right | SnesButton.L),
                >= 105 and < 136 => (ushort)SnesButton.Left,
                >= 150 and < 181 => (ushort)(SnesButton.Left | SnesButton.R),
                >= 181 and < 189 => (ushort)SnesButton.R,
                >= 195 and < 226 => (ushort)(SnesButton.Left | SnesButton.L),
                _ => (ushort)0,
            }
        : options.AimAirScript
            ? frameIndex switch
            {
                0 => (ushort)SnesButton.Start,
                >= 2 and < 43 => (ushort)(SnesButton.A | SnesButton.R),
                >= 43 and < 76 => (ushort)SnesButton.R,
                >= 85 and < 126 => (ushort)(SnesButton.A | SnesButton.L),
                >= 126 and < 159 => (ushort)SnesButton.L,
                >= 170 and < 196 => (ushort)SnesButton.Left,
                >= 210 and < 251 => (ushort)(SnesButton.A | SnesButton.R),
                >= 251 and < 286 => (ushort)SnesButton.R,
                >= 295 and < 336 => (ushort)(SnesButton.A | SnesButton.L),
                >= 336 and < 371 => (ushort)SnesButton.L,
                _ => (ushort)0,
            }
        : options.AerialTurnScript
            ? frameIndex switch
            {
                0 => (ushort)SnesButton.Start,

                // A starts `$4B->$4D`. On the next interval Left is the opposite-facing
                // held bit, so `$91:A2F6` publishes generic `$2F`; `$91:F952` then chooses
                // the exact unaimed record and preserves old momentum during reversal.
                >= 2 and < 10 => (ushort)SnesButton.A,
                >= 10 and < 48 => (ushort)(SnesButton.Left | SnesButton.A),
                >= 48 and < 80 => (ushort)SnesButton.Left,
                _ => (ushort)0,
            }
        : options.CompactAirScript
            ? frameIndex switch
            {
                0 => (ushort)SnesButton.Start,

                // Begin a right-facing diagonal-up normal jump. Down then selects compact
                // `$17`; R expands back to `$69`; the second Down is retained through the
                // descent so shot direction four must land through ordinary `$A4`.
                >= 2 and < 10 => (ushort)(SnesButton.A | SnesButton.R),
                >= 10 and < 20 => (ushort)(SnesButton.A | SnesButton.Down),
                >= 20 and < 22 => (ushort)(SnesButton.A | SnesButton.R),
                >= 22 and < 82 => (ushort)(SnesButton.A | SnesButton.Down),

                // Complete the grounded turn before starting the mirrored jump. Its Down
                // route is `$18`, shot direction five, and therefore landing pose `$A5`.
                >= 100 and < 126 => (ushort)SnesButton.Left,
                >= 140 and < 148 => (ushort)(SnesButton.A | SnesButton.R),
                >= 148 and < 158 => (ushort)(SnesButton.A | SnesButton.Down),
                >= 158 and < 166 => (ushort)(SnesButton.A | SnesButton.R),
                >= 166 and < 220 => (ushort)(SnesButton.A | SnesButton.Down),
                _ => (ushort)0,
            }
        : options.AimCrouchScript
            ? frameIndex switch
            {
                0 => (ushort)SnesButton.Start,
                >= 2 and < 12 => (ushort)(SnesButton.Down | SnesButton.R),
                >= 12 and < 22 => (ushort)SnesButton.R,
                >= 22 and < 32 => (ushort)SnesButton.L,
                >= 32 and < 42 => (ushort)(SnesButton.R | SnesButton.L),
                >= 60 and < 68 => (ushort)(SnesButton.Up | SnesButton.R),
                >= 90 and < 116 => (ushort)SnesButton.Left,
                >= 130 and < 140 => (ushort)(SnesButton.Down | SnesButton.R),
                >= 140 and < 150 => (ushort)SnesButton.R,
                >= 150 and < 160 => (ushort)SnesButton.L,
                >= 160 and < 170 => (ushort)(SnesButton.R | SnesButton.L),
                >= 190 and < 198 => (ushort)(SnesButton.Up | SnesButton.L),
                _ => (ushort)0,
            }
        : options.AimTurnScript
            ? frameIndex switch
            {
                // Start only dismisses the debug runner's timer state; every later pose
                // still comes from the retail bank-$91 input tables and turn selector.
                0 => (ushort)SnesButton.Start,

                // R alone selects diagonal-up aim. Adding the opposite direction requests
                // generic `$25/$26`; `$91:F8D3` must replace those with `$9C/$9D`.
                >= 2 and < 20 => (ushort)SnesButton.R,
                >= 20 and < 32 => (ushort)(SnesButton.Left | SnesButton.R),
                >= 32 and < 40 => (ushort)SnesButton.R,
                >= 40 and < 52 => (ushort)(SnesButton.Right | SnesButton.R),

                // Up selects straight-up aim. The same generic reversal pair must become
                // `$8B/$8C`, then command `$F8` must land on standing poses `$04/$03`.
                >= 52 and < 78 => (ushort)SnesButton.Up,
                >= 78 and < 90 => (ushort)(SnesButton.Left | SnesButton.Up),
                >= 90 and < 98 => (ushort)SnesButton.Up,
                >= 98 and < 110 => (ushort)(SnesButton.Right | SnesButton.Up),

                // L alone selects diagonal-down aim. These last two reversals exercise
                // `$8D/$8E` and their destinations `$08/$07`.
                >= 110 and < 136 => (ushort)SnesButton.L,
                >= 136 and < 148 => (ushort)(SnesButton.Left | SnesButton.L),
                >= 148 and < 156 => (ushort)SnesButton.L,
                >= 156 and < 168 => (ushort)(SnesButton.Right | SnesButton.L),
                _ => (ushort)0,
            }
        : options.CrouchTurnScript
            ? frameIndex switch
            {
                0 => (ushort)SnesButton.Start,

                // A single Down edge enters the ordinary crouch. Opposite directions then
                // make `$91:F8D3` select the crouching table's `$43/$44` entries.
                >= 2 and < 10 => (ushort)SnesButton.Down,
                20 => (ushort)SnesButton.Left,
                40 => (ushort)SnesButton.Right,

                // Both shoulders select straight-up crouched aim `$85/$86`.
                >= 60 and < 78 => (ushort)(SnesButton.R | SnesButton.L),
                78 => (ushort)(SnesButton.Left | SnesButton.R | SnesButton.L),
                >= 79 and < 98 => (ushort)(SnesButton.R | SnesButton.L),
                98 => (ushort)(SnesButton.Right | SnesButton.R | SnesButton.L),
                >= 99 and < 110 => (ushort)(SnesButton.R | SnesButton.L),

                // R alone selects diagonal-up crouched aim `$71/$72`.
                >= 110 and < 136 => (ushort)SnesButton.R,
                136 => (ushort)(SnesButton.Left | SnesButton.R),
                >= 137 and < 156 => (ushort)SnesButton.R,
                156 => (ushort)(SnesButton.Right | SnesButton.R),
                >= 157 and < 168 => (ushort)SnesButton.R,

                // L alone selects diagonal-down crouched aim `$73/$74`.
                >= 168 and < 194 => (ushort)SnesButton.L,
                194 => (ushort)(SnesButton.Left | SnesButton.L),
                >= 195 and < 214 => (ushort)SnesButton.L,
                214 => (ushort)(SnesButton.Right | SnesButton.L),
                >= 215 and < 226 => (ushort)SnesButton.L,
                _ => (ushort)0,
            }
        : options.CrouchJumpScript
            ? frameIndex switch
            {
                0 => (ushort)SnesButton.Start,

                // Enter stable `$27`, then release Down while retaining the facing
                // direction. `$91:A6A0` must install `$01` directly (there is no `$3B`).
                >= 2 and < 10 => (ushort)SnesButton.Down,
                20 => (ushort)SnesButton.Right,

                // Enter ordinary crouch again and press a fresh jump edge. The real table
                // selects `$4B`; `$91:FC8A` supplies its ordinary-crouch ten-pixel lift.
                >= 30 and < 38 => (ushort)SnesButton.Down,
                >= 48 and < 60 => (ushort)SnesButton.A,

                // After the first landing, enter diagonal-up aimed crouch `$71` and jump
                // while R remains held. This intentionally exercises the same `$4B` table
                // result without the literal `$27/$28`-only `$91:FC8A` adjustment.
                >= 120 and < 130 => (ushort)(SnesButton.Down | SnesButton.R),
                >= 130 and < 140 => (ushort)SnesButton.R,
                >= 140 and < 152 => (ushort)(SnesButton.A | SnesButton.R),

                // A final stable crouch/direct exit makes the immediate `$01` seam visible
                // at the end of the trace and in the output PNG.
                >= 220 and < 228 => (ushort)SnesButton.Down,
                238 => (ushort)SnesButton.Right,
                _ => (ushort)0,
            }
        : options.MorphBallScript || options.BombJumpScript
            ? frameIndex switch
            {
                0 => (ushort)SnesButton.Start,

                // The first Down edge selects `$35`, which reaches stable crouch `$27`.
                // Super Metroid does *not* morph from that continuously-held input: its
                // crouching table requires a second newly-pressed Down edge. Release for
                // two frames, press Down again, and let `$37`'s `$F9` choose grounded `$1D`.
                >= 2 and < 8 => (ushort)SnesButton.Down,
                >= 10 and < 20 => (ushort)SnesButton.Down,

                // The bomb-jump route stops moving here and presses the default Shoot/X
                // binding once. Everything after this edge is produced by the translated
                // five-slot projectile lifecycle and cartridge instruction data.
                25 when options.BombJumpScript => (ushort)SnesButton.X,
                >= 25 and < 80 when !options.BombJumpScript => (ushort)SnesButton.Right,
                >= 80 and < 140 when !options.BombJumpScript => (ushort)SnesButton.Left,

                // Let command one decelerate to definition fallback `$41`, then Up starts
                // `$3E`; its `$FD $28` endpoint proves the expanded body fits the terrain.
                >= 175 and < 185 when !options.BombJumpScript => (ushort)SnesButton.Up,
                _ => (ushort)0,
            }
        : options.SpringBallScript
            ? frameIndex switch
            {
                0 => (ushort)SnesButton.Start,
                >= 2 and < 8 => (ushort)SnesButton.Down,
                >= 10 and < 20 => (ushort)SnesButton.Down,
                >= 25 and < 55 => (ushort)SnesButton.Right,

                // A fresh Jump edge from `$79/$7B` selects `$7F`. Hold briefly for a
                // visible ascent, then release to exercise the native variable-height cut.
                >= 60 and < 72 => (ushort)SnesButton.A,
                _ => (ushort)0,
            }
        : frameIndex switch
        {
            0 => (ushort)SnesButton.Start,
            >= 2 when frameIndex - 2 < options.RightFrameCount => (ushort)SnesButton.Right,
            _ => (ushort)0,
        };

    if (options.KnockbackScript && frameIndex == 20)
    {
        // No enemy subsystem exists in the playable slice yet. This is therefore an
        // explicit debugger stimulus at exactly bank-$A0's producer/consumer seam: one
        // means the damage source was left of Samus, so physical knockback moves right.
        // Start() reads pose direction, movement type, speeds, radii, and animation from
        // the cartridge and installs the same `$90:DF38` handler as command one.
        SamusKnockbackMovement.Start(
            bus,
            runtime.Samus,
            controllerInput: 0,
            knockbackXDirection: 1);
        observedSamusPoses.Add(runtime.Samus.Pose);
        Console.WriteLine(
            $"frame {frameIndex + 1,4}: injected bank-$A0 knockback X direction 1; " +
            $"pose=${runtime.Samus.Pose:X2}, direction={runtime.Samus.KnockbackDirection}, " +
            $"timer={runtime.Samus.KnockbackTimer}.");
    }

    if (options.DrainedSamusScript)
    {
        // These four calls are the exact A9 actor-to-bank-91 interface. Conditions keep the
        // runner deterministic even if room collision changes the fall duration; the lower
        // bounds retain visible time in every stable ROM animation before the next command.
        if (!issuedDrainedStandingCommand && frameIndex >= 50 &&
            runtime.Samus.Drained.Phase == DrainedSamusPhase.OnFloor)
        {
            runtime.Samus.Drained.PutStanding(bus, runtime.Samus);
            issuedDrainedStandingCommand = true;
            Console.WriteLine($"frame {frameIndex + 1,4}: actor called drained controller 1 (standing).");
        }
        else if (!issuedDrainedCrouchingCommand && frameIndex >= 90 &&
                 runtime.Samus.Drained.Phase == DrainedSamusPhase.Standing)
        {
            runtime.Samus.Drained.PutCrouchingOrFalling(bus, runtime.Samus);
            issuedDrainedCrouchingCommand = true;
            Console.WriteLine($"frame {frameIndex + 1,4}: actor called drained controller 4 (crouching/falling).");
        }
        else if (!issuedDrainedReleaseCommand && frameIndex >= 120 &&
                 runtime.Samus.Drained.Phase == DrainedSamusPhase.Crouching)
        {
            runtime.Samus.Drained.Release(bus, runtime.Samus);
            issuedDrainedReleaseCommand = true;
            Console.WriteLine($"frame {frameIndex + 1,4}: actor called drained controller 2 (release).");
        }
        else if (!issuedDrainedHyperBeamCommand && frameIndex >= 150)
        {
            runtime.Samus.Drained.EnableHyperBeam(runtime.Samus);
            issuedDrainedHyperBeamCommand = true;
            Console.WriteLine($"frame {frameIndex + 1,4}: actor called drained controller 3 (hyper beam).");
        }
    }

    MotherBrainForcedSamusMovementResult? rainbowSamusMovement = null;
    if (rainbowAttack is not null)
    {
        // The retail outer frame loop calls `$80:8111` exactly once before dispatching
        // gameplay state. Earlier versions of this isolated encounter accidentally left
        // `$05E5` frozen at its reset seed, preventing `$C15C`'s sign-bit attack gate from
        // ever firing. Advancing the translated LFSR here supplies that missing global
        // producer without substituting a hand-authored boss random sequence.
        ushort motherBrainRandom = runtime.System.NextRandom();

        // Enemy AI executes before the ordinary enemy-instruction stage. Keep those calls
        // separate so `$B92B` can observe the pose/X values published by the previous frame's
        // retail `$A9:993A` opcode, just as the SNES scheduler does.
        MotherBrainRainbowBeamAttackStepResult actorResult = rainbowAttack.Step(
            bus,
            runtime.Samus,
            enemyFrameCounter: unchecked((ushort)frameIndex),
            mainEnemyExecutionCounter: unchecked((ushort)frameIndex),
            randomNumberSeed: motherBrainRandom,
            // Death explosions call the global generator once per simultaneous projectile.
            // Supplying the live owner keeps every later actor's random stream synchronized.
            nextRandomNumber: runtime.System.NextRandom);
        rainbowSamusMovement = actorResult.Movement;
        observedRainbowPhases.Add(actorResult.PhaseBefore);
        observedRainbowPhases.Add(actorResult.PhaseAfter);

        if (actorResult.SpriteTileTransfer is { } actorTiles)
        {
            // Feed either actor-owned transfer list into the ordinary WRAM queue before NMI.
            // `$8FE5` loads the Baby; `$9003` later replaces six pages with corpse graphics.
            runtime.VramWrites.Enqueue(
                actorTiles.Size,
                checked((int)actorTiles.SourceAddress),
                actorTiles.VramDestination);
            bool corpseTiles = actorResult.PhaseBefore ==
                MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceLoadCorpseTiles;
            if (corpseTiles)
                observedMotherBrainCorpseTileTransfers.Add(actorTiles);
            else
                observedBabyTileTransfers.Add(actorTiles);
            Console.WriteLine(
                $"frame {frameIndex + 1,4}: {(corpseTiles ? "Mother Brain corpse" : "Baby")} " +
                $"tile transfer {actorTiles.EntryIndex}: ${actorTiles.SourceAddress:X6} -> " +
                $"VRAM ${actorTiles.VramDestination:X4}, ${actorTiles.Size:X4} bytes.");
        }
        observedBabySpawnRequest |= actorResult.BabySpawnRequested;
        observedFinalBeamSound |= actorResult.FinalBeamSoundQueued;
        if (actorResult.Phase3Attack is { } phaseThreeAttack)
        {
            observedPhaseThreeAttacks.Add(phaseThreeAttack);
            Console.WriteLine(
                $"frame {frameIndex + 1,4}: phase-three attack {phaseThreeAttack}; " +
                $"RNG=${motherBrainRandom:X4}, head=${rainbowAttack.HeadInstructionList:X4}, " +
                $"walk={rainbowAttack.Phase3WalkingPhase}/" +
                $"${rainbowAttack.Phase3WalkCounter:X4}.");
        }
        foreach (MotherBrainDeathExplosionRequest deathExplosion in actorResult.DeathExplosions)
        {
            Console.WriteLine(
                $"frame {frameIndex + 1,4}: Mother Brain death explosion pattern " +
                $"{deathExplosion.PatternIndex} at ({deathExplosion.XPosition}," +
                $"{deathExplosion.YPosition}), offset ({deathExplosion.XOffset}," +
                $"{deathExplosion.YOffset}), parameter {deathExplosion.ProjectileParameter}, " +
                $"SFX ${deathExplosion.SoundEffect:X2}.");
        }
        foreach (MotherBrainSpriteTileTransferRequest transfer in
                 actorResult.CorpseRottingVramTransfers)
        {
            // Unlike the earlier six frame-spread corpse loads, `$A9:E1F4` appends all six
            // changing WRAM slices every active rotting frame. The standard NMI consumer
            // therefore sees precisely the same sources and encoded VRAM destinations.
            runtime.VramWrites.Enqueue(
                transfer.Size,
                checked((int)transfer.SourceAddress),
                transfer.VramDestination);
        }
        foreach (MotherBrainSpriteTileTransferRequest transfer in
                 actorResult.EscapeSequenceTileTransfers)
        {
            runtime.VramWrites.Enqueue(
                transfer.Size,
                checked((int)transfer.SourceAddress),
                transfer.VramDestination);
            Console.WriteLine(
                $"frame {frameIndex + 1,4}: escape-sequence tile transfer " +
                $"${transfer.SourceAddress:X6} -> VRAM ${transfer.VramDestination:X4}, " +
                $"${transfer.Size:X4} bytes.");
        }
        foreach (MotherBrainCorpseDustRequest dust in actorResult.CorpseDustRequests)
        {
            // `$A9:E23A` performs this allocation from the body enemy's instruction
            // interpreter. Enemy projectiles have not run yet, so placing the new slot in
            // the shared pool here lets the later high-to-low projectile pass consume its
            // first animation frame on the native spawn frame.
            int? slot = motherBrainProjectiles!.SpawnMiscDust(
                bus,
                dust.XPosition,
                dust.YPosition,
                dust.ProjectileParameter);
            Console.WriteLine(
                $"frame {frameIndex + 1,4}: Mother Brain corpse row {dust.EntryIndex} " +
                $"finished at dust ({dust.XPosition},{dust.YPosition}), " +
                $"parameter ${dust.ProjectileParameter:X4}, " +
                $"slot={slot?.ToString() ?? "full"}" +
                (dust.SoundEffectQueued ? $", SFX ${dust.SoundEffect:X2}." : "."));
        }
        if (actorResult.MusicStopQueued || actorResult.EscapeMusicQueued)
        {
            Console.WriteLine(
                $"frame {frameIndex + 1,4}: Mother Brain corpse finished; queued music " +
                $"${(actorResult.MusicStopQueued ? 0x0000 : 0xffff):X4} then " +
                $"${(actorResult.EscapeMusicQueued ? 0xff24 : 0xffff):X4}.");
        }
        if (actorResult.EscapeTypewriterSetupRequested)
        {
            Console.WriteLine(
                $"frame {frameIndex + 1,4}: escape start initialized; palette copy=" +
                $"{actorResult.ExplodedDoorPaletteRequested}, music7=" +
                $"{actorResult.EscapeMusicTrackQueued}, palette FX=" +
                $"{string.Join(',', actorResult.EscapePaletteFxRequests.Select(x => $"${x:X4}"))}.");
        }
        if (actorResult.EscapeDoorExplosion is { } doorExplosion)
        {
            // The door's periodic producer is body AI as well. These room coordinates are
            // deliberately near X zero; the generic dust pre-instruction, rather than the
            // runner, decides whether the origin lies inside the current layer-1 window.
            int? slot = motherBrainProjectiles!.SpawnMiscDust(
                bus,
                doorExplosion.XPosition,
                doorExplosion.YPosition,
                doorExplosion.ProjectileParameter);
            Console.WriteLine(
                $"frame {frameIndex + 1,4}: escape-door dust pattern " +
                $"{doorExplosion.PatternIndex} at ({doorExplosion.XPosition}," +
                $"{doorExplosion.YPosition}), parameter ${doorExplosion.ProjectileParameter:X4}, " +
                $"slot={slot?.ToString() ?? "full"}, SFX ${doorExplosion.SoundEffect:X2}.");
        }
        if (actorResult.TimeBombSetSubtitleSpawnRequested)
        {
            int? slot = motherBrainProjectiles!.SpawnTimeBombSetSubtitle();
            Console.WriteLine(
                $"frame {frameIndex + 1,4}: alternate time-bomb subtitle allocated " +
                $"slot {slot?.ToString() ?? "full"}.");
        }
        if (actorResult.MotherBrainEscapeTimerStartRequested)
        {
            // This is the concrete TimerStatus `$0002` consumer. Boss-area bits remain a
            // logged request because this isolated route is hosted in Landing Site and must
            // not corrupt Crateria's area byte while pretending it is Tourian.
            runtime.EscapeTimer.RequestMotherBrainStart();
            Console.WriteLine(
                $"frame {frameIndex + 1,4}: enabled Samus timer handling and requested " +
                $"Mother Brain escape timer; bossBit={actorResult.MotherBrainBossBitRequested}, " +
                $"event0E={actorResult.ZebesTimebombEventRequested}.");
        }
        foreach (MotherBrainEscapeDoorParticleSpawnRequest particleRequest in
                 actorResult.EscapeDoorParticleSpawns)
        {
            int? slot = motherBrainProjectiles!.SpawnEscapeDoorParticle(particleRequest);
            Console.WriteLine(
                $"frame {frameIndex + 1,4}: escape-door fragment " +
                $"{particleRequest.Parameter} allocated slot {slot?.ToString() ?? "full"}.");
        }
        if (actorResult.EscapeDoorPlm is { } escapeDoorPlm)
        {
            Console.WriteLine(
                $"frame {frameIndex + 1,4}: requested escape-door PLM " +
                $"${escapeDoorPlm.PlmEntry:X4} at block " +
                $"({escapeDoorPlm.BlockX},{escapeDoorPlm.BlockY}).");
        }

        if (actorResult.BabySpawnRequested)
        {
            // Native `$A9:BE1B` spawns population record `$BE28`, whose initialization AI
            // overwrites the record's coordinates and installs `$C7CC`. The runner creates
            // exactly that translated enemy once; its later movement reads the supplied
            // cartridge's real `$A0:B443` sine table on every curved-flight call.
            if (cutsceneBaby is not null)
                throw new InvalidOperationException("Mother Brain requested the cutscene Baby more than once.");
            cutsceneBaby = new BabyMetroidCutsceneState();
            cutsceneBaby.Initialize();
            previousBabyPhase = cutsceneBaby.Phase;
            Console.WriteLine(
                $"frame {frameIndex + 1,4}: initialized cutscene Baby at " +
                $"({cutsceneBaby.XPosition},{cutsceneBaby.YPosition}); " +
                $"phase={cutsceneBaby.Phase}, timer=${cutsceneBaby.FunctionTimer:X4}.");
        }

        MotherBrainBodyAnimationStepResult bodyResult = rainbowAttack.Body.Step(bus);
        observedPhaseThreeForwardMovement |=
            (actorResult.PhaseBefore is MotherBrainRainbowBeamAttackPhase.Phase3FightingMain or
                MotherBrainRainbowBeamAttackPhase.Phase3FightingAttackCooldown) &&
            unchecked((short)(bodyResult.XAfter - bodyResult.XBefore)) > 0;

        // Mother Brain's brain is the next occupied enemy slot after her body. Its `$91B8`
        // handler advances the two neck angles before the later cutscene-Baby slot polls the
        // corpse flag. This ordering is observable when `$BF56` waits for both raises to end.
        rainbowAttack.StepNeckMovement(bus, runtime.Samus);

        // Mother Brain's head animation is part of the brain enemy slot and therefore runs
        // after its main/neck AI but before the later Baby slot. It must continue after that
        // cutscene enemy deletes itself: phase-three's `$9F00` list owns the bomb attack.
        // Head opcodes only allocate here; the gameplay projectile pass below advances every
        // newly occupied bank-$86 slot after all enemy slots have finished.
        MotherBrainHeadAnimationStepResult headResult = rainbowAttack.StepHeadAnimation(
            bus,
            runtime.Samus,
            cutsceneBaby,
            runtime.System.RandomNumber);
        if (headResult.OnionRingSpawn is { } ringRequest)
        {
            int? slot = motherBrainProjectiles!.Spawn(bus, rainbowAttack, ringRequest);
            Console.WriteLine(
                $"frame {frameIndex + 1,4}: Mother Brain head spawned onion ring " +
                $"angle=${ringRequest.Angle:X2}, slot={slot?.ToString() ?? "full"}, " +
                $"head=${headResult.InstructionPointerAfter:X4}.");
        }
        if (headResult.BombSpawn is { } bombRequest)
        {
            int? slot = motherBrainProjectiles!.SpawnBomb(rainbowAttack, bombRequest);
            Console.WriteLine(
                $"frame {frameIndex + 1,4}: Mother Brain head spawned bomb " +
                $"afterburn={bombRequest.AfterburnCount}, slot={slot?.ToString() ?? "full"}, " +
                $"activeBombs={rainbowAttack.BombCounter}, " +
                $"head=${headResult.InstructionPointerAfter:X4}.");
        }
        if (headResult.PurpleBreathBigSpawnRequested)
        {
            int? slot = motherBrainProjectiles!.SpawnPurpleBreathBig(rainbowAttack);
            Console.WriteLine(
                $"frame {frameIndex + 1,4}: Mother Brain head spawned large purple breath " +
                $"slot={slot?.ToString() ?? "full"}.");
        }
        if (rainbowAttack.Phase != previousRainbowPhase)
        {
            Console.WriteLine(
                $"frame {frameIndex + 1,4}: rainbow actor {previousRainbowPhase} -> " +
                $"{rainbowAttack.Phase}; timer=${rainbowAttack.FunctionTimer:X4}, " +
                $"body=({rainbowAttack.Body.XPosition},{rainbowAttack.Body.YPosition})/" +
                $"pose {rainbowAttack.Body.Pose}, head=${rainbowAttack.HeadInstructionList:X4}, " +
                $"width=${rainbowAttack.AngularWidth:X4}.");
            previousRainbowPhase = rainbowAttack.Phase;
        }
        if (bodyResult.FootstepRequested)
        {
            Console.WriteLine(
                $"frame {frameIndex + 1,4}: Mother Brain body opcode footstep at " +
                $"({bodyResult.XAfter},{bodyResult.YAfter}); earthquake " +
                $"{bodyResult.EarthquakeType}/{bodyResult.EarthquakeTimer}.");
        }

        if (cutsceneBaby is { IsDeleted: false })
        {
            // Body slot zero executes before the later spawned enemy slot. This placement
            // also means the spawn frame can run the newly initialized Baby once, matching
            // the native increasing-slot enemy loop rather than adding an invented delay.
            // Blue-ring projectiles run after enemies, so hits from the preceding frame are
            // consumed here. Native `$A9:C6C8` clears the shared request word and queues one
            // cry even when several rings incremented it before this enemy turn.
            int pendingBabyCries = motherBrainProjectiles!.ConsumePendingBabyCries();
            if (pendingBabyCries != 0)
            {
                Console.WriteLine(
                    $"frame {frameIndex + 1,4}: Baby consumes Mother Brain ring cry request " +
                    $"({pendingBabyCries} hit{(pendingBabyCries == 1 ? string.Empty : "s")}).");
            }
            BabyMetroidCutsceneStepResult babyResult = cutsceneBaby.Step(
                bus,
                runtime.Samus,
                rainbowAttack,
                layer1X: 0,
                layer1Y: 0,
                enemyFrameCounter: unchecked((ushort)frameIndex),
                randomNumber: runtime.System.RandomNumber);
            observedBabyPhases.Add(babyResult.PhaseBefore);
            observedBabyPhases.Add(babyResult.PhaseAfter);
            observedBabyMotherBrainInterrupt |= babyResult.MotherBrainInterrupted;
            observedBabyCeilingTableInstall |=
                babyResult.PhaseAfter == BabyMetroidCutscenePhase.MoveToSamus &&
                babyResult.MovementTablePointer == BabyMetroidCutsceneState.CeilingToSamusMovementTable;
            if (babyResult.HealingCompleted)
            {
                observedBabyHealingCompletion = true;
                observedBabyHealingCompletionFrame = frameIndex + 1;
                observedBabyHealingCompletionHealth = runtime.Samus.Health;
                observedBabyHealingCompletionReserveEnergy = runtime.Samus.ReserveEnergy;
            }
            observedBabyPhaseThreeHandoff |= babyResult.PhaseThreeHandoff;
            if (babyResult.PhaseThreeHandoff)
            {
                Console.WriteLine(
                    $"frame {frameIndex + 1,4}: Baby deleted itself, restored Hyper Beam, " +
                    "and handed Mother Brain to phase-three recovery.");
            }

            foreach (BabyMetroidReleaseDustRequest dust in babyResult.ReleaseDustClouds)
            {
                // The Baby occupies a later enemy slot than Mother Brain. Its three helper
                // calls execute in list order and each allocator resumes at slot seventeen,
                // exactly like `$A9:C98C-C9C2`; all successful allocations are therefore
                // present before this frame's shared enemy-projectile pass begins.
                int? slot = motherBrainProjectiles!.SpawnMiscDust(
                    bus,
                    dust.XPosition,
                    dust.YPosition,
                    dust.ProjectileParameter);
                Console.WriteLine(
                    $"frame {frameIndex + 1,4}: Baby release dust at " +
                    $"({dust.XPosition},{dust.YPosition}), parameter " +
                    $"${dust.ProjectileParameter:X4}, slot={slot?.ToString() ?? "full"}.");
            }

            if (babyResult.DeathExplosion is { } deathExplosion)
            {
                observedBabyDeathExplosions++;
                int? slot = motherBrainProjectiles!.SpawnMiscDust(
                    bus,
                    deathExplosion.XPosition,
                    deathExplosion.YPosition,
                    deathExplosion.ProjectileParameter);
                if (slot is not null)
                    allocatedBabyDeathExplosions++;
                Console.WriteLine(
                    $"frame {frameIndex + 1,4}: Baby death explosion pattern " +
                    $"{deathExplosion.PatternIndex} at ({deathExplosion.XPosition}," +
                    $"{deathExplosion.YPosition}); projectile parameter " +
                    $"${deathExplosion.ProjectileParameter:X4}, " +
                    $"slot={slot?.ToString() ?? "full"}, SFX ${deathExplosion.SoundEffect:X2}.");
            }

            if (babyResult.BabyPaletteTransfer is { } babyPalette)
            {
                // The native destination `$01E2` is a byte offset into the 512-byte
                // palette buffer; SnesCgram accepts an actual colour number, hence `/2`.
                runtime.Cgram.LoadFromBus(
                    bus,
                    checked((int)babyPalette.SourceAddress),
                    babyPalette.ColorCount,
                    babyPalette.DestinationColorIndex / 2);
                observedBabyDeathPalettes.Add(babyPalette);
                Console.WriteLine(
                    $"frame {frameIndex + 1,4}: Baby black-fade palette " +
                    $"{babyPalette.PaletteIndex} from ${babyPalette.SourceAddress:X6}.");
            }

            if (babyResult.AttackTileTransfer is { } attackTiles)
            {
                // The same ordinary pre-NMI queue used for the earlier Baby graphics now
                // restores Mother Brain's four attack rows from bank `$B7`.
                runtime.VramWrites.Enqueue(
                    attackTiles.Size,
                    checked((int)attackTiles.SourceAddress),
                    attackTiles.VramDestination);
                observedAttackTileTransfers.Add(attackTiles);
                Console.WriteLine(
                    $"frame {frameIndex + 1,4}: Mother Brain attack tile transfer " +
                    $"{attackTiles.EntryIndex}: ${attackTiles.SourceAddress:X6} -> " +
                    $"VRAM ${attackTiles.VramDestination:X4}.");
            }

            if (babyResult.BackgroundPaletteTransfer is { } backgroundPalette)
            {
                // `$AD:F24B` writes fourteen background-palette-three colours followed by
                // fourteen background-palette-five colours from consecutive source words.
                int source = checked((int)backgroundPalette.SourceAddress);
                int colors = backgroundPalette.ColorsPerDestination;
                runtime.Cgram.LoadFromBus(
                    bus,
                    source,
                    colors,
                    backgroundPalette.FirstDestinationColorIndex / 2);
                runtime.Cgram.LoadFromBus(
                    bus,
                    source + colors * 2,
                    colors,
                    backgroundPalette.SecondDestinationColorIndex / 2);
                observedPhaseThreeBackgroundPalettes.Add(backgroundPalette);
                Console.WriteLine(
                    $"frame {frameIndex + 1,4}: phase-three room-light palette " +
                    $"{backgroundPalette.PaletteIndex} from ${backgroundPalette.SourceAddress:X6}.");
            }

            if (cutsceneBaby.Phase != previousBabyPhase)
            {
                if (cutsceneBaby.Phase == BabyMetroidCutscenePhase.LatchOntoSamus)
                    observedBabyLatchOntoSamusFrame = frameIndex + 1;
                if (cutsceneBaby.Phase == BabyMetroidCutscenePhase.HealSamusToFullHealth)
                    observedBabyHealSamusFrame = frameIndex + 1;
                Console.WriteLine(
                    $"frame {frameIndex + 1,4}: Baby actor {previousBabyPhase} -> " +
                    $"{cutsceneBaby.Phase}; position=({cutsceneBaby.XPosition:X4}." +
                    $"{cutsceneBaby.XSubposition:X4},{cutsceneBaby.YPosition:X4}." +
                    $"{cutsceneBaby.YSubposition:X4}), velocity=" +
                    $"({cutsceneBaby.XVelocity:X4},{cutsceneBaby.YVelocity:X4}), " +
                    $"angle/speed=${cutsceneBaby.Angle:X4}/${cutsceneBaby.Speed:X4}.");
                previousBabyPhase = cutsceneBaby.Phase;
            }
            if (cutsceneBaby.MovementTablePointer != previousBabyMovementTablePointer)
            {
                // The route advances through overlapping eight-byte ROM records. Logging
                // the literal pointer makes each leg independently breakpointable and also
                // exposes the final `$CA5C + 8 == $CA64` function-pointer overlay.
                Console.WriteLine(
                    $"frame {frameIndex + 1,4}: Baby movement table " +
                    $"${previousBabyMovementTablePointer:X4} -> " +
                    $"${cutsceneBaby.MovementTablePointer:X4}; position=" +
                    $"({cutsceneBaby.XPosition:X4}.{cutsceneBaby.XSubposition:X4}," +
                    $"{cutsceneBaby.YPosition:X4}.{cutsceneBaby.YSubposition:X4}).");
                observedBabyRouteFrames[cutsceneBaby.MovementTablePointer] = frameIndex + 1;
                observedBabyRoutePoints[cutsceneBaby.MovementTablePointer] = babyResult.After;
                previousBabyMovementTablePointer = cutsceneBaby.MovementTablePointer;
            }
        }

        // GameState_8 runs EprojRunAll after EnemyMain. This ordering lets a ring spawned by
        // the head above receive delay call one and animation frame one immediately, while
        // any collision it produces is not visible to the Baby's `$CABD` until next frame.
        MotherBrainEnemyProjectileFrameResult ringResult = motherBrainProjectiles!.StepFrame(
            bus,
            rainbowAttack,
            cutsceneBaby,
            runtime.Samus,
            layer1X: 0,
            samusBombs: runtime.BombProjectiles);
        foreach (MotherBrainOnionRingEvent ringEvent in ringResult.Events)
        {
            string target = ringEvent.Collision switch
            {
                MotherBrainOnionRingCollisionKind.BabyMetroid or
                    MotherBrainOnionRingCollisionKind.DeletedAfterBabyDeath => "Baby",
                MotherBrainOnionRingCollisionKind.Samus => "Samus",
                _ => "target",
            };
            Console.WriteLine(
                $"frame {frameIndex + 1,4}: onion ring slot {ringEvent.SlotIndex} " +
                $"{ringEvent.Collision} at ({ringEvent.XPosition},{ringEvent.YPosition}); " +
                $"{target} HP ${ringEvent.TargetHealthBefore:X4}->" +
                $"${ringEvent.TargetHealthAfter:X4}.");
        }
        foreach (MotherBrainEscapeDoorParticleDustRequest dust in
                 ringResult.EscapeDoorDustRequests)
        {
            Console.WriteLine(
                $"frame {frameIndex + 1,4}: escape-door fragment slot " +
                $"{dust.SourceSlotIndex} expired at ({dust.XPosition},{dust.YPosition}); " +
                $"spawn misc dust parameter ${dust.ProjectileParameter:X4}.");
        }
        foreach (MotherBrainBombEvent bombEvent in ringResult.BombEvents)
        {
            Console.WriteLine(
                $"frame {frameIndex + 1,4}: Mother Brain bomb slot {bombEvent.SlotIndex} " +
                $"{bombEvent.Kind} at ({bombEvent.XPosition},{bombEvent.YPosition}); " +
                $"bounceOffset=${bombEvent.BounceTableOffset:X2}, " +
                $"afterburn={(bombEvent.AfterburnCount?.ToString() ?? "none")}, " +
                $"dust=${bombEvent.DustParameter:X2}, drops={bombEvent.EnemyDropRequested}, " +
                $"SFX={(bombEvent.QueuedSoundLibraryThree is { } sound ? $"${sound:X2}" : "none")}.");
        }

        // The enemy graphics hook runs after actor processing. Its shake countdown is not a
        // body-AI timer: decrementing here lets the following `$BE96` call perform the exact
        // zero-to-fifty refresh instead of leaving the head frozen on one shake-table entry.
        rainbowAttack.StepBrainShakeForDraw();
    }

    // This isolated phase-three script keeps Mother Brain's native room-space coordinates,
    // whose layer-1 origin is (0,0), while the reusable Landing Site runtime has its own
    // camera at X=$0400. Inject both enemy-projectile passes with the actor's coordinate
    // system instead of incorrectly subtracting the unrelated playable-room camera.
    Action<OamBuffer>? drawHighPriorityEnemyProjectiles = motherBrainProjectiles is null
        ? null
        : oam => motherBrainProjectiles.DrawHighPriority(bus, oam, layer1X: 0, layer1Y: 0);
    Action<OamBuffer>? drawLowPriorityEnemyProjectiles = motherBrainProjectiles is null
        ? null
        : oam => motherBrainProjectiles.DrawLowPriority(bus, oam, layer1X: 0, layer1Y: 0);
    RuntimeFrameResult result = runtime.StepFrame(
        controllerInput,
        drawHighPriorityEnemyProjectiles,
        drawLowPriorityEnemyProjectiles);
    observedSamusPoses.Add(runtime.Samus.Pose);
    if (specialSpinRoute &&
        yDirectionBeforeFrame == 2 &&
        runtime.Samus.Kinematics.YDirection == 1)
    {
        observedSpaceJumpRestarts++;
        Console.WriteLine(
            $"frame {result.FrameNumber,4}: accepted Space Jump restart " +
            $"{observedSpaceJumpRestarts}; pose=${runtime.Samus.Pose:X2}, " +
            $"velocity={runtime.Samus.Kinematics.YSpeed:X4}." +
            $"{runtime.Samus.Kinematics.YSubspeed:X4}.");
    }
    observedScrewAttackContactDamage |=
        runtime.Samus.HorizontalSpeed.ContactDamageIndex == 3;
    observedScrewAttackPaletteCycle |=
        options.ScrewAttackScript &&
        SamusState.IsScrewAttackPose(runtime.Samus.Pose) &&
        runtime.Samus.AnimationFrame >= 27 &&
        runtime.Samus.HorizontalSpeed.SpecialPaletteFrame != 0;
    observedCrystalFlashDrain |= runtime.LastCrystalFlashMovement is
        { PhaseAfterStep: CrystalFlashPhase.DrainingAmmo };
    observedCrystalFlashFinish |= runtime.LastCrystalFlashMovement is
        { PhaseAfterStep: CrystalFlashPhase.Finishing };
    observedCrystalFlashCompletion |= runtime.LastCrystalFlashMovement is
        { Completed: true };
    observedDrainedFallingHandler |= runtime.LastDrainedSamusMovement is not null;
    observedDrainedLanding |= runtime.LastDrainedSamusMovement is { Landed: true };
    observedDrainedStanding |= runtime.Samus.Pose is
        SamusState.DrainedStandingRightPose or SamusState.DrainedStandingLeftPose;
    observedDrainedCrouching |= issuedDrainedCrouchingCommand &&
        (runtime.Samus.Pose is
            SamusState.DrainedCrouchingRightPose or SamusState.DrainedCrouchingLeftPose);
    observedDrainedRelease |= issuedDrainedReleaseCommand &&
        runtime.Samus.Drained.Phase == DrainedSamusPhase.Inactive;
    observedDrainedHyperBeam |= runtime.Samus.HyperBeam == 0x8000;
    observedDraygonAimUp |= runtime.Samus.Pose == SamusState.DraygonGrabbedAimUpRightPose;
    observedDraygonFiring |= runtime.Samus.Pose == SamusState.DraygonGrabbedFiringRightPose;
    observedDraygonAimDown |= runtime.Samus.Pose == SamusState.DraygonGrabbedAimDownRightPose;
    observedDraygonMoving |= runtime.Samus.Pose == SamusState.DraygonGrabbedMovingRightPose;
    observedDraygonNeutralFallback |= frameIndex >= 96 && frameIndex < 112 &&
        runtime.Samus.Pose == SamusState.DraygonGrabbedNeutralRightPose;
    observedDraygonRelease |= !runtime.Samus.DraygonGrabbed.IsActive &&
        runtime.Samus.Pose == SamusState.FacingRightNormalPose;
    observedKnockbackMovement |= runtime.LastKnockbackMovement is not null;
    observedDamageBoostMovement |= runtime.LastAerialSamusMovement is not null &&
        runtime.Samus.ReadMovementType(bus) == 0x19;
    observedGrappleSwing |= runtime.LastGrappleMovement is
        { Phase: GrapplePhase.ConnectedSwinging };
    observedGrappleReleaseQueue |= runtime.LastGrappleMovement is { ReleaseQueued: true };
    observedGrappleRelease |= runtime.LastGrappleMovement is { Released: true };
    // `$90:946E` is independent of the beam function. LastAerialSamusMovement proves beta
    // movement actually ran rather than merely observing `$9B:CB8B`'s jump-pose cleanup.
    observedGrappleReleaseMovement |=
        observedGrappleReleaseQueue && runtime.LastAerialSamusMovement is not null;
    if (!observedGrappleTerrainCollision &&
        runtime.LastGrappleMovement is { TerrainCollided: true } grappleTerrain)
    {
        Console.WriteLine(
            $"frame {result.FrameNumber,4}: grapple body collision at radial point " +
            $"{grappleTerrain.CollisionDistanceFromFeet}/6; safe angle=" +
            $"${runtime.Samus.Grapple.Angle:X4}, reflected velocity=" +
            $"${unchecked((ushort)runtime.Samus.Grapple.AngularVelocity):X4}, " +
            $"kickTimer={runtime.Samus.Grapple.CollisionBounceTimer}.");
    }
    observedGrappleTerrainCollision |= runtime.LastGrappleMovement is { TerrainCollided: true };
    observedGrappleFire |= runtime.LastGrappleMovement is { Fired: true };
    observedGrappleFireCancelQueue |= runtime.LastGrappleMovement is { CancelQueued: true };
    observedGrappleFireCancel |= runtime.LastGrappleMovement is { Cancelled: true };
    observedBlockedRanIntoWallProbe |= runtime.LastRanIntoWallProbe is { Collided: true };
    observedBombJumpStart |= runtime.LastBombJumpMovement is { Started: true };
    observedBombJumpEnd |= runtime.LastBombJumpMovement is { Ended: true };
    observedBombJumpRise |= runtime.LastBombJumpMovement is { Started: false, Ended: false };
    observedBombPlacement |= runtime.BombProjectiles.LastFrameResult.PlacedSlot is not null;
    observedBombExplosion |= runtime.BombProjectiles.LastFrameResult.ExplosionStarted;
    observedBombDeletion |= runtime.BombProjectiles.LastFrameResult.ProjectileDeleted;
    observedStraightBombOverlap |=
        runtime.BombProjectiles.LastFrameResult.PublishedBombJumpDirection == 2;
    uint currentExtraRunSpeed =
        ((uint)runtime.Samus.HorizontalSpeed.ExtraRunSpeed << 16) |
        runtime.Samus.HorizontalSpeed.ExtraRunSubspeed;
    maximumObservedExtraRunSpeed = Math.Max(maximumObservedExtraRunSpeed, currentExtraRunSpeed);
    maximumObservedSpeedBoostStage = Math.Max(
        maximumObservedSpeedBoostStage,
        unchecked((byte)(runtime.Samus.HorizontalSpeed.SpeedBoostCounter >> 8)));
    observedSpeedBoostEcho |= runtime.Samus.HorizontalSpeed.EchoSoundRequested;
    observedSpeedBoostContactDamage |= runtime.Samus.HorizontalSpeed.ContactDamageIndex == 1;
    bool currentSpeedBoostDeparture =
        (runtime.Samus.HorizontalSpeed.SpeedEchoIndex & 0x8000) != 0;
    observedSpeedBoostDeparture |= currentSpeedBoostDeparture;
    observedSpeedBoostDepartureFinished |=
        previousSpeedBoostDeparture && !currentSpeedBoostDeparture;
    if (currentSpeedBoostDeparture != previousSpeedBoostDeparture)
    {
        Console.WriteLine(
            $"frame {frameIndex + 1,4}: ordinary Speed Booster departure=" +
            $"{currentSpeedBoostDeparture}; " +
            $"slot0=({runtime.Samus.HorizontalSpeed.FirstSpeedEchoXPosition:X4}," +
            $"{runtime.Samus.HorizontalSpeed.FirstSpeedEchoYPosition:X4})/" +
            $"v{runtime.Samus.HorizontalSpeed.FirstSpeedEchoXSpeed:X4}, " +
            $"slot1=({runtime.Samus.HorizontalSpeed.SecondSpeedEchoXPosition:X4}," +
            $"{runtime.Samus.HorizontalSpeed.SecondSpeedEchoYPosition:X4})/" +
            $"v{runtime.Samus.HorizontalSpeed.SecondSpeedEchoXSpeed:X4}.");
    }
    previousSpeedBoostDeparture = currentSpeedBoostDeparture;
    observedStoredShine |= runtime.Samus.Shinespark.Phase == ShinesparkPhase.Stored;
    observedShinesparkWindup |= runtime.Samus.Shinespark.Phase == ShinesparkPhase.Windup;
    observedDirectionalShinespark |= runtime.Samus.Shinespark.Phase is
        ShinesparkPhase.Horizontal or ShinesparkPhase.Vertical or ShinesparkPhase.Diagonal;
    observedShinesparkMovement |= runtime.LastShinesparkMovement is
        { PhaseAtStart: ShinesparkPhase.Horizontal or ShinesparkPhase.Vertical or ShinesparkPhase.Diagonal };
    observedShinesparkPalette |= runtime.Samus.Shinespark.PaletteType is 1 or 6;
    observedShinesparkCrashOrbit |= runtime.Samus.Shinespark.Phase == ShinesparkPhase.Crash;
    observedShinesparkCrashEchoCircle |=
        runtime.Samus.Shinespark.Phase == ShinesparkPhase.CrashEchoCircle;
    // CrashFinish is an installed one-frame handler: its Step call restores standing and
    // publishes Inactive before this observer runs. Use the movement result as the native
    // handler-execution witness instead of hoping to sample a transient host enum value.
    observedShinesparkCrashFinish |=
        runtime.LastShinesparkMovement is { CrashSequenceFinished: true };
    observedReleasedShinesparkEcho |= runtime.Samus.Shinespark.ReleasedCrashEchoCount != 0;
    if (runtime.Samus.Shinespark.ReleasedCrashEchoCount != priorReleasedShinesparkEchoCount)
    {
        ShinesparkReleasedEcho firstReleased = runtime.Samus.Shinespark.FirstReleasedCrashEcho;
        ShinesparkReleasedEcho secondReleased = runtime.Samus.Shinespark.SecondReleasedCrashEcho;
        ShinesparkReleasedEchoClear? releasedClear =
            runtime.Samus.Shinespark.LastReleasedCrashEchoClear;
        Console.WriteLine(
            $"frame {result.FrameNumber,4}: released shinespark echoes=" +
            $"{runtime.Samus.Shinespark.ReleasedCrashEchoCount}; " +
            $"slot3={(firstReleased.Active ? $"${firstReleased.Angle:X2}/r{firstReleased.Radius}/({firstReleased.XPosition:X4},{firstReleased.YPosition:X4})" : "clear")}, " +
            $"slot4={(secondReleased.Active ? $"${secondReleased.Angle:X2}/r{secondReleased.Radius}/({secondReleased.XPosition:X4},{secondReleased.YPosition:X4})" : "clear")}" +
            (releasedClear is { } clear
                ? $"; last clear=slot{clear.NativeSlot}/{clear.Axis}/r{clear.Radius}/" +
                  $"world({clear.XPosition:X4},{clear.YPosition:X4})/" +
                  $"screen({clear.ScreenX},{clear.ScreenY?.ToString() ?? "not sampled"})"
                : string.Empty));
        priorReleasedShinesparkEchoCount = runtime.Samus.Shinespark.ReleasedCrashEchoCount;
    }
    observedDashMomentum |= runtime.Samus.HorizontalSpeed.HasRunningMomentum;
    observedDashAerialCarry |= runtime.Samus.HorizontalSpeed.HasRunningMomentum &&
        currentExtraRunSpeed != 0 &&
        runtime.Samus.ReadMovementType(bus) is 2 or 3 or 6;

    // Put a breakpoint here to inspect the complete runtime after any chosen frame. The
    // NoInlining attribute below keeps this method as a reliable stack frame in Debug and
    // Release builds instead of letting the JIT dissolve the hook into this loop.
    FrameBreakpoint(runtime, result);

    if (result.EscapeTimerState != priorState ||
        result.EscapeTimerExpired != priorEscapeTimerExpired)
    {
        Console.WriteLine(
            $"frame {result.FrameNumber,4}: timer={result.EscapeTimerState,-27} " +
            $"time={runtime.EscapeTimer.MinutesBcd:X2}:{runtime.EscapeTimer.SecondsBcd:X2}.{runtime.EscapeTimer.CentisecondsBcd:X2} " +
            $"position=({runtime.EscapeTimer.XPixel},{runtime.EscapeTimer.YPixel}) " +
            $"expired={result.EscapeTimerExpired}");
        priorState = result.EscapeTimerState;
        priorEscapeTimerExpired = result.EscapeTimerExpired;
    }

    if (runtime.Samus.AnimationFrame != priorSamusFrame)
    {
        // The Mother Brain trace already logs every actor, head opcode, projectile hit,
        // and route boundary. Drained Samus loops a four-frame animation hundreds of times;
        // suppressing only that redundant presentation log keeps the causal trace readable.
        if (!options.MotherBrainRainbowScript)
        {
            Console.WriteLine(
                $"frame {result.FrameNumber,4}: Samus animation {priorSamusFrame} -> " +
                $"{runtime.Samus.AnimationFrame}; timer={runtime.Samus.AnimationFrameTimer}; " +
                $"command={(runtime.Samus.LastAnimationDelayCommand is byte command ? $"${command:X2}" : "none")}; " +
                $"next definitions=${runtime.Samus.TileTransfers.TopDefinitionAddress:X6}/" +
                $"${runtime.Samus.TileTransfers.BottomDefinitionAddress:X6}");
        }
        priorSamusFrame = runtime.Samus.AnimationFrame;
    }

    if (runtime.Samus.Pose != priorSamusPose)
    {
        Console.WriteLine(
            $"frame {result.FrameNumber,4}: applied Samus pose ${priorSamusPose:X2} -> " +
            $"${runtime.Samus.Pose:X2} after movement/animation; " +
            $"frame={runtime.Samus.AnimationFrame}, timer={runtime.Samus.AnimationFrameTimer}");
        priorSamusPose = runtime.Samus.Pose;
        priorSamusFrame = runtime.Samus.AnimationFrame;
    }

    if (runtime.Samus.Kinematics.XFixed != priorSamusX)
    {
        BlockMoveResult? horizontal = runtime.LastGroundedSamusMovement?.Horizontal ??
            runtime.LastAerialSamusMovement?.Horizontal ??
            runtime.LastMorphBallMovement?.Horizontal ??
            runtime.LastBombJumpMovement?.Horizontal ??
            runtime.LastKnockbackMovement?.Horizontal ??
            runtime.LastShinesparkMovement?.Horizontal;
        if (horizontal is null && rainbowSamusMovement is null &&
            runtime.LastGrappleMovement is null &&
            runtime.LastShinesparkMovement is null)
            throw new InvalidOperationException("Samus X changed without a translated movement result.");
        string vertical = runtime.LastGroundedSamusMovement is GroundedMovementResult groundedMovement
            ? $"ground=${groundedMovement.Vertical.AcceptedDisplacement:X8}/collision={groundedMovement.Vertical.Collided}"
            : runtime.LastAerialSamusMovement?.Vertical is BlockMoveResult aerialVertical
                ? $"airY=${aerialVertical.AcceptedDisplacement:X8}/collision={aerialVertical.Collided}"
                : runtime.LastMorphBallMovement is MorphBallMovementResult morphMovement
                    ? $"morphY=${morphMovement.Vertical.AcceptedDisplacement:X8}/collision={morphMovement.Vertical.Collided}"
                    : runtime.LastBombJumpMovement?.Vertical is BlockMoveResult bombVertical
                        ? $"bombY=${bombVertical.AcceptedDisplacement:X8}/collision={bombVertical.Collided}"
                    : runtime.LastKnockbackMovement?.Vertical is BlockMoveResult knockbackVertical
                        ? $"hurtY=${knockbackVertical.AcceptedDisplacement:X8}/collision={knockbackVertical.Collided}"
                    : "airY=transition";
        Console.WriteLine(
            $"frame {result.FrameNumber,4}: Samus X={runtime.Samus.XPosition:X4}." +
            $"{runtime.Samus.Kinematics.XSubposition:X4}; " +
            $"base={runtime.Samus.HorizontalSpeed.BaseSpeed:X4}." +
            $"{runtime.Samus.HorizontalSpeed.BaseSubspeed:X4}, " +
            $"extra={runtime.Samus.HorizontalSpeed.ExtraRunSpeed:X4}." +
            $"{runtime.Samus.HorizontalSpeed.ExtraRunSubspeed:X4}, " +
            (horizontal is BlockMoveResult moved
                ? $"horizontal=${moved.AcceptedDisplacement:X8}, {vertical}"
                : rainbowSamusMovement is MotherBrainForcedSamusMovementResult forced
                    ? $"Mother Brain forced velocity=${forced.XVelocity:X4}/" +
                      $"${forced.YVelocity:X4}, carry={forced.NativeCarry}"
                : $"grapple angle=${runtime.Samus.Grapple.Angle:X4}, " +
                  $"velocity=${unchecked((ushort)runtime.Samus.Grapple.AngularVelocity):X4}, " +
                  $"beamStart=({runtime.Samus.Grapple.BeamStartX:X4},{runtime.Samus.Grapple.BeamStartY:X4})"));
        priorSamusX = runtime.Samus.Kinematics.XFixed;
    }

    if (runtime.Samus.Kinematics.YFixed != priorSamusY)
    {
        if (!options.MotherBrainRainbowScript)
        {
            Console.WriteLine(
                $"frame {result.FrameNumber,4}: Samus Y={runtime.Samus.YPosition:X4}." +
                $"{runtime.Samus.Kinematics.YSubposition:X4}; " +
                $"velocity={runtime.Samus.Kinematics.YSpeed:X4}." +
                $"{runtime.Samus.Kinematics.YSubspeed:X4}, " +
                $"direction={runtime.Samus.Kinematics.YDirection}, " +
                $"landed={runtime.LastAerialSamusMovement?.Landed ?? runtime.LastMorphBallMovement?.Landed ?? false}, " +
                $"ceiling={runtime.LastAerialSamusMovement?.HitCeiling ?? runtime.LastMorphBallMovement?.HitCeiling ?? false}, " +
                $"bombActive={runtime.Samus.BombJumpActive}");
        }
        priorSamusY = runtime.Samus.Kinematics.YFixed;
    }

    ushort? prospectivePose = runtime.ProspectiveSamusPose?.ProspectivePose;
    if (prospectivePose != priorProspectivePose)
    {
        if (runtime.ProspectiveSamusPose is SamusPoseTransition transition)
        {
            Console.WriteLine(
                $"frame {result.FrameNumber,4}: input matched ${transition.EntryAddress:X6}; " +
                $"prospective pose=${transition.ProspectivePose:X2} " +
                $"(required new=${transition.RequiredNewInput:X4}, held=${transition.RequiredHeldInput:X4}); " +
                (SamusState.IsDraygonGrabbedPose(
                     checked((byte)transition.ProspectivePose)) ||
                 (runtime.GroundedSamusMovementEnabled &&
                 transition.ProspectivePose is
                     SamusState.FacingRightNormalPose or
                     SamusState.FacingLeftNormalPose or
                     SamusState.MovingRightNormalPose or
                     SamusState.MovingLeftNormalPose or
                     SamusState.TurningRightToLeftPose or
                     SamusState.TurningLeftToRightPose or
                     SamusState.TurningRightToLeftAimUpPose or
                     SamusState.TurningLeftToRightAimUpPose or
                     SamusState.TurningRightToLeftAimDiagonalUpPose or
                     SamusState.TurningLeftToRightAimDiagonalUpPose or
                     SamusState.TurningRightToLeftAimDiagonalDownPose or
                     SamusState.TurningLeftToRightAimDiagonalDownPose or
                     SamusState.TurningRightToLeftCrouchingPose or
                     SamusState.TurningLeftToRightCrouchingPose or
                     SamusState.TurningRightToLeftCrouchingAimUpPose or
                     SamusState.TurningLeftToRightCrouchingAimUpPose or
                     SamusState.TurningRightToLeftCrouchingAimDiagonalUpPose or
                     SamusState.TurningLeftToRightCrouchingAimDiagonalUpPose or
                     SamusState.TurningRightToLeftCrouchingAimDiagonalDownPose or
                     SamusState.TurningLeftToRightCrouchingAimDiagonalDownPose or
                     SamusState.NeutralJumpTransitionRightPose or
                     SamusState.NeutralJumpTransitionLeftPose or
                     SamusState.SpinJumpRightPose or
                     SamusState.SpinJumpLeftPose or
                     SamusState.SpaceJumpRightPose or
                     SamusState.SpaceJumpLeftPose or
                     SamusState.ScrewAttackRightPose or
                     SamusState.ScrewAttackLeftPose or
                     SamusState.CrouchingTransitionRightPose or
                     SamusState.CrouchingTransitionLeftPose or
                     SamusState.StandingTransitionRightPose or
                     SamusState.StandingTransitionLeftPose or
                     SamusState.MorphBallGroundRightPose or
                     SamusState.MorphBallGroundLeftPose or
                     SamusState.MorphBallMovingRightPose or
                     SamusState.MorphBallMovingLeftPose or
                     SamusState.MorphBallFallingRightPose or
                     SamusState.MorphBallFallingLeftPose or
                     SamusState.MorphingTransitionRightPose or
                     SamusState.MorphingTransitionLeftPose or
                     SamusState.UnmorphingTransitionRightPose or
                     SamusState.UnmorphingTransitionLeftPose or
                     SamusState.SpringBallGroundRightPose or
                     SamusState.SpringBallGroundLeftPose or
                     SamusState.SpringBallMovingRightPose or
                     SamusState.SpringBallMovingLeftPose or
                     SamusState.SpringBallFallingRightPose or
                     SamusState.SpringBallFallingLeftPose or
                     SamusState.SpringBallJumpRightPose or
                     SamusState.SpringBallJumpLeftPose or
                     SamusState.CrouchingRightPose or
                     SamusState.CrouchingLeftPose or
                     SamusState.StandingAimUpRightPose or
                     SamusState.StandingAimUpLeftPose or
                     SamusState.StandingAimDiagonalUpRightPose or
                     SamusState.StandingAimDiagonalUpLeftPose or
                     SamusState.StandingAimDiagonalDownRightPose or
                     SamusState.StandingAimDiagonalDownLeftPose or
                     SamusState.RunningAimUpRightPose or
                     SamusState.RunningAimUpLeftPose or
                     SamusState.RunningAimDiagonalUpRightPose or
                     SamusState.RunningAimDiagonalUpLeftPose or
                     SamusState.RunningAimDiagonalDownRightPose or
                     SamusState.RunningAimDiagonalDownLeftPose or
                     SamusState.NormalJumpForwardRightPose or
                     SamusState.NormalJumpForwardLeftPose or
                     SamusState.NormalJumpAimUpRightPose or
                     SamusState.NormalJumpAimUpLeftPose or
                     SamusState.NormalJumpTransitionAimUpRightPose or
                     SamusState.NormalJumpTransitionAimUpLeftPose or
                     SamusState.NormalJumpTransitionAimDiagonalUpRightPose or
                     SamusState.NormalJumpTransitionAimDiagonalUpLeftPose or
                     SamusState.NormalJumpTransitionAimDiagonalDownRightPose or
                     SamusState.NormalJumpTransitionAimDiagonalDownLeftPose or
                     SamusState.NormalJumpAimDiagonalUpRightPose or
                     SamusState.NormalJumpAimDiagonalUpLeftPose or
                     SamusState.NormalJumpAimDiagonalDownRightPose or
                     SamusState.NormalJumpAimDiagonalDownLeftPose or
                     SamusState.NormalJumpAimDownRightPose or
                     SamusState.NormalJumpAimDownLeftPose or
                     SamusState.DamageBoostRightPose or
                     SamusState.DamageBoostLeftPose or
                     SamusState.FallingAimUpRightPose or
                     SamusState.FallingAimUpLeftPose or
                     SamusState.FallingAimDiagonalUpRightPose or
                     SamusState.FallingAimDiagonalUpLeftPose or
                     SamusState.FallingAimDiagonalDownRightPose or
                     SamusState.FallingAimDiagonalDownLeftPose or
                     SamusState.FallingAimDownRightPose or
                     SamusState.FallingAimDownLeftPose or
                     SamusState.CrouchingAimUpRightPose or
                     SamusState.CrouchingAimUpLeftPose or
                     SamusState.CrouchingAimDiagonalUpRightPose or
                     SamusState.CrouchingAimDiagonalUpLeftPose or
                     SamusState.CrouchingAimDiagonalDownRightPose or
                     SamusState.CrouchingAimDiagonalDownLeftPose or
                     SamusState.LandingAimUpRightPose or
                     SamusState.LandingAimUpLeftPose or
                     SamusState.LandingAimDiagonalUpRightPose or
                     SamusState.LandingAimDiagonalUpLeftPose or
                     SamusState.LandingAimDiagonalDownRightPose or
                     SamusState.LandingAimDiagonalDownLeftPose or
                     SamusState.CrouchingTransitionAimUpRightPose or
                     SamusState.CrouchingTransitionAimUpLeftPose or
                     SamusState.CrouchingTransitionAimDiagonalUpRightPose or
                     SamusState.CrouchingTransitionAimDiagonalUpLeftPose or
                     SamusState.CrouchingTransitionAimDiagonalDownRightPose or
                     SamusState.CrouchingTransitionAimDiagonalDownLeftPose or
                     SamusState.StandingTransitionAimUpRightPose or
                     SamusState.StandingTransitionAimUpLeftPose or
                     SamusState.StandingTransitionAimDiagonalUpRightPose or
                     SamusState.StandingTransitionAimDiagonalUpLeftPose or
                     SamusState.StandingTransitionAimDiagonalDownRightPose or
                     SamusState.StandingTransitionAimDiagonalDownLeftPose or
                     SamusState.MoonwalkFacingLeftPose or
                     SamusState.MoonwalkFacingRightPose or
                     SamusState.MoonwalkAimUpRightPose or
                     SamusState.MoonwalkAimUpLeftPose or
                     SamusState.MoonwalkAimDownRightPose or
                     SamusState.MoonwalkAimDownLeftPose or
                     SamusState.MoonwalkTurnJumpLeftPose or
                     SamusState.MoonwalkTurnJumpRightPose or
                     SamusState.MoonwalkTurnJumpAimUpLeftPose or
                     SamusState.MoonwalkTurnJumpAimUpRightPose or
                     SamusState.MoonwalkTurnJumpAimDownLeftPose or
                     SamusState.MoonwalkTurnJumpAimDownRightPose or
                     SamusState.RanIntoWallRightPose or
                     SamusState.RanIntoWallLeftPose or
                     SamusState.RanIntoWallAimUpRightPose or
                     SamusState.RanIntoWallAimUpLeftPose or
                     SamusState.RanIntoWallAimDownRightPose or
                     SamusState.RanIntoWallAimDownLeftPose or
                     SamusState.ShinesparkWindupRightPose or
                     SamusState.ShinesparkWindupLeftPose or
                     SamusState.ShinesparkHorizontalRightPose or
                     SamusState.ShinesparkHorizontalLeftPose or
                     SamusState.ShinesparkVerticalRightPose or
                     SamusState.ShinesparkVerticalLeftPose or
                     SamusState.ShinesparkDiagonalRightPose or
                     SamusState.ShinesparkDiagonalLeftPose or
                     SamusState.TurningRightToLeftJumpPose or
                     SamusState.TurningLeftToRightJumpPose or
                     SamusState.TurningRightToLeftFallingPose or
                     SamusState.TurningLeftToRightFallingPose)
                    ? "applied at a verified translated pose-transition seam"
                    : "not applied because its movement/transition side effects are not translated"));
        }
        priorProspectivePose = prospectivePose;
    }

    if (runtime.ProspectiveSamusFallbackPose != priorFallbackPose)
    {
        if (runtime.ProspectiveSamusFallbackPose is ushort fallback)
        {
            Console.WriteLine(
                $"frame {result.FrameNumber,4}: no-button fallback selected pose ${fallback:X2}; " +
                $"base={runtime.Samus.HorizontalSpeed.BaseSpeed:X4}." +
                $"{runtime.Samus.HorizontalSpeed.BaseSubspeed:X4}, " +
                $"accelMode={runtime.Samus.HorizontalSpeed.AccelerationMode}");
        }
        priorFallbackPose = runtime.ProspectiveSamusFallbackPose;
    }

    if (runtime.ProspectiveSamusWallCollisionPose != priorWallCollisionPose)
    {
        if (runtime.ProspectiveSamusWallCollisionPose is byte wallPose)
        {
            BlockMoveResult? probe = runtime.LastRanIntoWallProbe;
            Console.WriteLine(
                $"frame {result.FrameNumber,4}: `$91:EADE` selected wall pose ${wallPose:X2}; " +
                (probe is { } onePixel
                    ? $"one-pixel block probe collision={onePixel.Collided}, " +
                      $"accepted=${onePixel.AcceptedDisplacement:X8}."
                    : "current running X speed had already been killed by collision."));
        }
        priorWallCollisionPose = runtime.ProspectiveSamusWallCollisionPose;
    }
}

if (options.AerialTurnScript)
{
    byte[] requiredAerialTurnRoute = [
        SamusState.NeutralJumpTransitionRightPose,
        SamusState.NeutralJumpRightPose,
        SamusState.TurningRightToLeftJumpPose,
        SamusState.NormalJumpForwardLeftPose,
        SamusState.NormalLandingLeftPose,
    ];
    foreach (byte requiredPose in requiredAerialTurnRoute)
    {
        if (!observedSamusPoses.Contains(requiredPose))
            throw new InvalidOperationException(
                $"Aerial-turn ROM script did not observe required pose ${requiredPose:X2}.");
    }
    Console.WriteLine(
        $"Aerial-turn ROM route validated {requiredAerialTurnRoute.Length} deterministic pose milestones.");
}

if (options.CompactAirScript)
{
    // This is deliberately a real-ROM assertion: input records, pose definitions, delay
    // lists, collision geometry, and landing-table results all came from the user's image.
    // Shorter captures are useful for freezing the airborne artwork, so each documented
    // milestone validates everything the deterministic timeline has reached by that frame.
    var requiredCompactRoute = new List<byte>
    {
        SamusState.NormalJumpAimDownRightPose,
    };
    if (options.FrameCount >= 32)
        requiredCompactRoute.Add(SamusState.NormalLandingRightPose);
    if (options.FrameCount >= 149)
        requiredCompactRoute.Add(SamusState.NormalJumpAimDownLeftPose);
    if (options.FrameCount >= 235)
        requiredCompactRoute.Add(SamusState.NormalLandingLeftPose);
    foreach (byte requiredPose in requiredCompactRoute)
    {
        if (!observedSamusPoses.Contains(requiredPose))
        {
            throw new InvalidOperationException(
                $"Compact-air ROM script did not observe required pose ${requiredPose:X2}.");
        }
    }

    Console.WriteLine(
        $"Compact-air ROM route validated {requiredCompactRoute.Count} deterministic pose milestone(s).");
}

if (options.MorphBallScript)
{
    // These milestones come from the user's ROM tables and are deliberately stricter than
    // “the runner did not throw.” Keep the list frame-count-aware so a short run can freeze
    // a useful ball-art diagnostic without weakening the complete 220-frame route. Each
    // threshold is the first accepted NMI on which the full private-ROM run observed it.
    var requiredMorphRoute = new List<byte> {
        SamusState.MorphingTransitionRightPose,
    };
    if (options.FrameCount >= 17)
        requiredMorphRoute.Add(SamusState.MorphBallGroundRightPose);
    if (options.FrameCount >= 26)
        requiredMorphRoute.Add(SamusState.MorphBallMovingRightPose);
    if (options.FrameCount >= 81)
        requiredMorphRoute.Add(SamusState.MorphBallMovingLeftPose);
    if (options.FrameCount >= 149)
        requiredMorphRoute.Add(SamusState.MorphBallGroundLeftPose);
    if (options.FrameCount >= 176)
        requiredMorphRoute.Add(SamusState.UnmorphingTransitionLeftPose);
    if (options.FrameCount >= 182)
        requiredMorphRoute.Add(SamusState.CrouchingLeftPose);
    foreach (byte requiredPose in requiredMorphRoute)
    {
        if (!observedSamusPoses.Contains(requiredPose))
        {
            throw new InvalidOperationException(
                $"Morph-Ball ROM script did not observe required pose ${requiredPose:X2}.");
        }
    }

    Console.WriteLine(
        $"Morph-Ball ROM route validated {requiredMorphRoute.Count} deterministic pose milestones.");
}

if (options.SpringBallScript)
{
    byte[] requiredSpringRoute = [
        SamusState.MorphingTransitionRightPose,
        SamusState.SpringBallGroundRightPose,
        SamusState.SpringBallMovingRightPose,
        SamusState.SpringBallJumpRightPose,
    ];
    foreach (byte requiredPose in requiredSpringRoute)
    {
        if (!observedSamusPoses.Contains(requiredPose))
            throw new InvalidOperationException(
                $"Spring-Ball ROM script did not observe required pose ${requiredPose:X2}.");
    }
    Console.WriteLine(
        $"Spring-Ball ROM route validated {requiredSpringRoute.Length} deterministic pose milestones.");
}

if (options.BombJumpScript)
{
    // The route now proves the complete producer-to-consumer chain, not merely the special
    // movement handler: controller edge, slot allocation, timer-eight bank-$A0 overlap,
    // next-frame `$E025`, explosion instruction list/delete, rise, and termination.
    if (options.FrameCount >= 17 &&
        !observedSamusPoses.Contains(SamusState.MorphBallGroundRightPose))
        throw new InvalidOperationException("Bomb-jump ROM script never reached stable Morph Ball pose $1D.");

    // Short runs are intentionally supported as art/timing captures. Each threshold is
    // the first accepted NMI where the full real-ROM route can have observed that phase.
    bool missedReachedPhase =
        (options.FrameCount >= 26 && !observedBombPlacement) ||
        (options.FrameCount >= 77 && !observedStraightBombOverlap) ||
        (options.FrameCount >= 78 && !observedBombJumpStart) ||
        (options.FrameCount >= 79 && !observedBombJumpRise) ||
        (options.FrameCount >= 85 && !observedBombExplosion) ||
        (options.FrameCount >= 95 && !observedBombDeletion) ||
        (options.FrameCount >= 105 && !observedBombJumpEnd);
    if (missedReachedPhase)
    {
        throw new InvalidOperationException(
            $"Bomb route missed a phase: placed={observedBombPlacement}, " +
            $"overlap={observedStraightBombOverlap}, explosion={observedBombExplosion}, " +
            $"deleted={observedBombDeletion}, start={observedBombJumpStart}, " +
            $"rise={observedBombJumpRise}, end={observedBombJumpEnd}.");
    }
    Console.WriteLine(
        $"Bomb-jump ROM route validated every milestone reachable within {options.FrameCount} frame(s).");
}

if (options.KnockbackScript)
{
    // The only host-authored fact is which side the not-yet-translated enemy occupied.
    // Requiring both poses and both movement handlers makes the rest a real-ROM route:
    // `$53` art/type, `$91:A8E4` Jump+opposite-direction chord, `$50` art/type, and
    // ordinary jump dispatch.
    byte[] requiredKnockbackRoute = [
        SamusState.KnockbackRightPose,
        SamusState.DamageBoostRightPose,
    ];
    foreach (byte requiredPose in requiredKnockbackRoute)
    {
        if (!observedSamusPoses.Contains(requiredPose))
            throw new InvalidOperationException(
                $"Knockback ROM script did not observe required pose ${requiredPose:X2}.");
    }
    if (!observedKnockbackMovement || !observedDamageBoostMovement)
    {
        throw new InvalidOperationException(
            $"Knockback route missed a handler: hurt={observedKnockbackMovement}, " +
            $"damageBoost={observedDamageBoostMovement}.");
    }
    Console.WriteLine(
        "Knockback ROM route validated hurt movement, the Left+Jump damage-boost chord, and type-$19 jump movement.");
}

if (options.MoonwalkScript)
{
    // These are not host-selected poses. Every member must have been observed after the
    // unchanged retail transition matcher consumed the scripted chords. `$BF` is the
    // grounded turn art and `$1A` proves its input/animation handoff created a real jump.
    byte[] requiredMoonwalkRoute =
    [
        SamusState.MoonwalkFacingRightPose,
        SamusState.MoonwalkAimUpRightPose,
        SamusState.MoonwalkAimDownRightPose,
        SamusState.MoonwalkTurnJumpLeftPose,
        SamusState.SpinJumpLeftPose,
    ];
    foreach (byte requiredPose in requiredMoonwalkRoute)
    {
        if (!observedSamusPoses.Contains(requiredPose))
        {
            throw new InvalidOperationException(
                $"Moonwalk ROM script did not observe required pose ${requiredPose:X2}.");
        }
    }
    Console.WriteLine(
        "Moonwalk ROM route validated stable neutral/up/down movement, zero-input fallback, grounded $BF turn art, and $1A jump handoff.");
}

if (options.RanIntoWallScript)
{
    // Placement alone cannot satisfy these checks: the observed set is populated only
    // after complete runtime frames. Thus each wall pose proves controller matching,
    // prospective-pose filtering, bank-$94 collision, and the ten-way selector together.
    byte[] requiredWallRoute =
    [
        SamusState.RanIntoWallRightPose,
        SamusState.RanIntoWallAimUpRightPose,
        SamusState.RanIntoWallAimDownRightPose,
        SamusState.NeutralJumpTransitionRightPose,
        SamusState.NeutralJumpRightPose,
    ];
    foreach (byte requiredPose in requiredWallRoute)
    {
        if (!observedSamusPoses.Contains(requiredPose))
        {
            throw new InvalidOperationException(
                $"Ran-into-wall ROM script did not observe required pose ${requiredPose:X2}.");
        }
    }
    if (!observedBlockedRanIntoWallProbe)
    {
        throw new InvalidOperationException(
            "Ran-into-wall ROM script never observed a blocked one-pixel probe.");
    }
    Console.WriteLine(
        "Ran-into-wall ROM route validated neutral/up/down wall art, zero-input fallback, and the $89 -> $4B -> $4D jump exit.");
}

if (options.RunScript)
{
    // These are post-frame observations from the complete ROM-backed runtime. The checks
    // jointly prove transition-table admission, exact `$90:973E` accumulation/cap, and the
    // type-three `$90:9808` retention route; a host-authored displacement cannot satisfy
    // the state assertions by merely moving Samus farther.
    if (!observedSamusPoses.Contains(SamusState.MovingRightNormalPose))
        throw new InvalidOperationException("Dash ROM script never entered running pose $09.");
    if (!observedDashMomentum)
        throw new InvalidOperationException("Dash ROM script never established momentum flag $0B3C.");
    if (options.FrameCount >= 38 && maximumObservedExtraRunSpeed != 0x00020000u)
    {
        throw new InvalidOperationException(
            $"Dash ROM script expected maximum extra speed 2.0000, observed ${maximumObservedExtraRunSpeed:X8}.");
    }
    if (options.FrameCount >= 40 &&
        (!observedSamusPoses.Contains(SamusState.SpinJumpRightPose) || !observedDashAerialCarry))
    {
        throw new InvalidOperationException(
            "Dash ROM script did not retain the accumulated extra component through spin pose $19.");
    }
    Console.WriteLine(
        $"Dash ROM route validated momentum, ordinary cap ${maximumObservedExtraRunSpeed >> 16:X4}." +
        $"{maximumObservedExtraRunSpeed & 0xffff:X4}, shared running animation timing, and spin-jump carry.");
}

if (options.SpaceJumpScript || options.WaterSpaceJumpScript || options.ScrewAttackScript)
{
    byte requiredSpinPose = options.ScrewAttackScript
        ? SamusState.ScrewAttackRightPose
        : SamusState.SpaceJumpRightPose;
    if (options.FrameCount >= 15 && !observedSamusPoses.Contains(requiredSpinPose))
    {
        throw new InvalidOperationException(
            $"Special-spin ROM script never installed required pose ${requiredSpinPose:X2}.");
    }
    if (options.FrameCount >= (options.ScrewAttackScript ? 48 : 43) &&
        observedSpaceJumpRestarts == 0)
    {
        throw new InvalidOperationException(
            "Special-spin ROM script reached the repeat interval but never restarted upward.");
    }
    if (options.ScrewAttackScript &&
        options.FrameCount >= 15 &&
        !observedScrewAttackContactDamage)
    {
        throw new InvalidOperationException(
            "Screw Attack ROM script never published native contact-damage index 3.");
    }
    if (options.ScrewAttackScript &&
        options.FrameCount >= 34 &&
        !observedScrewAttackPaletteCycle)
    {
        throw new InvalidOperationException(
            "Screw Attack ROM script never reached the frame-27 palette cycle.");
    }
    Console.WriteLine(
        $"Special-spin ROM route validated pose ${requiredSpinPose:X2}, " +
        $"{observedSpaceJumpRestarts} accepted repeat(s)" +
        (options.ScrewAttackScript
            ? $", contact damage 3, and palette cycle={observedScrewAttackPaletteCycle}."
            : "."));
}

if (options.SpeedBoosterScript)
{
    if (!observedSamusPoses.Contains(SamusState.MovingRightNormalPose) || !observedDashMomentum)
        throw new InvalidOperationException("Speed Booster ROM script never established running momentum.");
    if (maximumObservedSpeedBoostStage == 0 && options.FrameCount >= 30)
        throw new InvalidOperationException("Speed Booster ROM script never loaded a staged counter.");
    if (options.FrameCount >= 103 &&
        (!observedSpeedBoostEcho || !observedSpeedBoostContactDamage || maximumObservedSpeedBoostStage != 4))
    {
        throw new InvalidOperationException(
            $"Speed Booster ROM script expected stage four with echo/contact; observed stage {maximumObservedSpeedBoostStage}, " +
            $"echo={observedSpeedBoostEcho}, contact={observedSpeedBoostContactDamage}.");
    }
    if (options.FrameCount >= 116 && maximumObservedExtraRunSpeed != 0x00070000u)
    {
        throw new InvalidOperationException(
            $"Speed Booster ROM script expected the 7.0000 cap, observed ${maximumObservedExtraRunSpeed:X8}.");
    }
    if (options.FrameCount >= 190 &&
        (!observedSpeedBoostDeparture || !observedSpeedBoostDepartureFinished))
    {
        throw new InvalidOperationException(
            "Speed Booster ROM script did not observe both the ordinary cancellation-echo departure and its completion.");
    }
    Console.WriteLine(
        $"Speed Booster ROM route observed maximum extra speed ${maximumObservedExtraRunSpeed >> 16:X4}." +
        $"{maximumObservedExtraRunSpeed & 0xffff:X4}, stage {maximumObservedSpeedBoostStage}, " +
        $"echo={observedSpeedBoostEcho}, contact={observedSpeedBoostContactDamage}, " +
        $"departure={observedSpeedBoostDeparture}/{observedSpeedBoostDepartureFinished}.");
}

if (options.ShinesparkScript)
{
    // These assertions deliberately observe states after full StepFrame calls. A direct
    // test helper that invoked SamusShinesparkState would miss the point of this route:
    // real bank-$91 input records, delayed animation commands, palette priority, room
    // collision, and runtime handler dispatch must all cooperate in their normal order.
    if (!observedStoredShine)
        throw new InvalidOperationException("Shinespark ROM script never stored a stage-four shine while crouching.");
    if (!observedShinesparkPalette)
        throw new InvalidOperationException("Shinespark ROM script never installed a stored/active shine palette handler.");
    if (options.FrameCount >= 155 && !observedShinesparkWindup)
        throw new InvalidOperationException("Shinespark ROM script never reached windup pose $C7/$C8.");
    if (options.FrameCount >= 160 && (!observedDirectionalShinespark || !observedShinesparkMovement))
    {
        throw new InvalidOperationException(
            "Shinespark ROM script never launched a directional spark through the live movement handler.");
    }
    if (options.FrameCount >= 220 && !observedShinesparkCrashOrbit)
        throw new InvalidOperationException("Shinespark ROM script never entered the collision crash orbit.");
    if (options.FrameCount >= 260 && !observedShinesparkCrashEchoCircle)
        throw new InvalidOperationException("Shinespark ROM script never entered the 30-frame crash echo circle.");
    if (options.FrameCount >= 291 &&
        (!observedShinesparkCrashFinish || !observedReleasedShinesparkEcho))
    {
        throw new InvalidOperationException(
            "Shinespark ROM script never completed crash into the departing projectile echoes.");
    }
    Console.WriteLine(
        $"Shinespark ROM route observed stored={observedStoredShine}, windup={observedShinesparkWindup}, " +
        $"directional={observedDirectionalShinespark}, movement={observedShinesparkMovement}, " +
        $"palette={observedShinesparkPalette}, crash={observedShinesparkCrashOrbit}, " +
        $"circle={observedShinesparkCrashEchoCircle}, finish={observedShinesparkCrashFinish}, " +
        $"releasedEcho={observedReleasedShinesparkEcho}.");
}

if (options.GrappleScript)
{
    if (!observedGrappleSwing)
        throw new InvalidOperationException("Grapple ROM script never executed connected swinging.");
    if (!observedGrappleTerrainCollision)
        throw new InvalidOperationException("Grapple ROM script never exercised connected terrain collision.");
    if (options.FrameCount >= 91 && !observedGrappleReleaseQueue)
        throw new InvalidOperationException("Grapple ROM script never queued release at $9B:C79D.");
    if (options.FrameCount >= 92 && !observedGrappleRelease)
        throw new InvalidOperationException("Grapple ROM script never completed release at $9B:CB8B.");
    if (options.FrameCount >= 91 && !observedGrappleReleaseMovement)
        throw new InvalidOperationException("Grapple ROM script never executed release movement handler $90:946E.");
    Console.WriteLine(
        $"Grapple ROM route validated connected pendulum stepping and six-point terrain reflection" +
        (options.FrameCount >= 92
            ? ", queued release, persistent $90:946E motion, and jump-pose handoff."
            : "."));
}

if (options.CrystalFlashScript)
{
    if (!observedSamusPoses.Contains(SamusState.CrystalFlashRightPose))
        throw new InvalidOperationException("Crystal Flash ROM route never rendered pose $D3.");
    if (options.FrameCount >= 11 && !observedCrystalFlashDrain)
        throw new InvalidOperationException("Crystal Flash ROM route never installed ammo handler $90:D6CE.");
    if (options.FrameCount >= 249 && !observedCrystalFlashFinish)
        throw new InvalidOperationException("Crystal Flash ROM route never consumed all three ammo families.");
    if (options.FrameCount >= 265 && !observedCrystalFlashCompletion)
        throw new InvalidOperationException("Crystal Flash ROM route never returned to normal movement.");
    Console.WriteLine(
        $"Crystal Flash ROM route observed drain={observedCrystalFlashDrain}, " +
        $"finish={observedCrystalFlashFinish}, complete={observedCrystalFlashCompletion}, " +
        $"energy={runtime.Samus.Health}, ammo={runtime.Samus.Missiles}/" +
        $"{runtime.Samus.SuperMissiles}/{runtime.Samus.PowerBombs}.");
}

if (options.MotherBrainRainbowScript)
{
    if (rainbowAttack is null)
        throw new InvalidOperationException("Mother Brain rainbow route was not initialized.");
    if (options.FrameCount >= 258 &&
        !observedRainbowPhases.Contains(MotherBrainRainbowBeamAttackPhase.RetractNeck))
    {
        throw new InvalidOperationException(
            "Mother Brain rainbow ROM route never reached the retracting body walk.");
    }
    if (options.FrameCount >= 571 &&
        !observedRainbowPhases.Contains(MotherBrainRainbowBeamAttackPhase.MoveSamusTowardWall))
    {
        throw new InvalidOperationException(
            "Mother Brain rainbow ROM route never completed charge into the active beam.");
    }
    if (options.FrameCount >= 880 &&
        !observedRainbowPhases.Contains(MotherBrainRainbowBeamAttackPhase.LetSamusFall))
    {
        throw new InvalidOperationException(
            "Mother Brain rainbow ROM route never drained and released Samus.");
    }
    if (options.FrameCount >= 1100 &&
        !observedRainbowPhases.Contains(MotherBrainRainbowBeamAttackPhase.FinishSamusOff))
    {
        throw new InvalidOperationException(
            $"Mother Brain rainbow ROM route did not enter finish-off AI; phase={rainbowAttack.Phase}.");
    }
    if (options.FrameCount >= 1200 &&
        !observedRainbowPhases.Contains(MotherBrainRainbowBeamAttackPhase.ChargeFinalRainbowBeam))
    {
        throw new InvalidOperationException(
            $"Mother Brain rainbow ROM route did not finish its body walk/stand delay; phase={rainbowAttack.Phase}.");
    }
    if (options.FrameCount >= 1450 &&
        (observedBabyTileTransfers.Count != 4 || !observedBabySpawnRequest))
    {
        throw new InvalidOperationException(
            $"Mother Brain rainbow ROM route did not DMA/spawn Baby exactly once; " +
            $"transfers={observedBabyTileTransfers.Count}, spawn={observedBabySpawnRequest}, " +
            $"phase={rainbowAttack.Phase}.");
    }
    if (options.FrameCount >= 1700 &&
        (!observedRainbowPhases.Contains(MotherBrainRainbowBeamAttackPhase.FinalRainbowBeamHolding) ||
         !observedFinalBeamSound))
    {
        throw new InvalidOperationException(
            $"Mother Brain rainbow ROM route did not reach final-beam hold; " +
            $"sound={observedFinalBeamSound}, phase={rainbowAttack.Phase}.");
    }
    if (options.FrameCount >= 2050 &&
        (cutsceneBaby is null ||
         !observedBabyPhases.Contains(BabyMetroidCutscenePhase.LatchOntoMotherBrain) ||
         !observedBabyMotherBrainInterrupt))
    {
        throw new InvalidOperationException(
            $"Cutscene Baby did not complete its ROM-backed entrance/latch by the " +
            $"regression boundary; phase={cutsceneBaby?.Phase.ToString() ?? "not spawned"}, " +
            $"interrupted={observedBabyMotherBrainInterrupt}.");
    }
    if (options.FrameCount >= 3340 &&
        (rainbowAttack.Phase2CorpseState == 0 || rainbowAttack.BrainHealth != 0x8ca0))
    {
        throw new InvalidOperationException(
            $"Mother Brain did not reach the ROM-backed corpse/revival producer; " +
            $"phase={rainbowAttack.Phase}, corpse={rainbowAttack.Phase2CorpseState}, " +
            $"brainHealth=${rainbowAttack.BrainHealth:X4}.");
    }
    if (options.FrameCount >= 3477 &&
        (cutsceneBaby is null ||
         !observedBabyPhases.Contains(BabyMetroidCutscenePhase.MoveToTheCeiling) ||
         !observedBabyCeilingTableInstall))
    {
        throw new InvalidOperationException(
            $"Cutscene Baby did not complete drain release and ceiling retreat; " +
            $"phase={cutsceneBaby?.Phase.ToString() ?? "not spawned"}, " +
            $"movementTable=${cutsceneBaby?.MovementTablePointer:X4}.");
    }
    if (options.FrameCount >= 3819)
    {
        // These values come from the private retail-ROM run itself after correcting the
        // header `$24/$24` words to the literal hitbox radii loaded by `$A0:8AFF/$8B05`.
        // Each point retains every earlier 8.8 carry and catches either a radius regression
        // or a movement-helper rounding error even when the route eventually converges.
        (ushort Pointer, int Frame, BabyMetroidCutscenePoint Point)[] expectedRoute =
        [
            (0xca2c, 3524, new(0x007e, 0x2800, 0x0051, 0x4100)),
            (0xca34, 3591, new(0x010d, 0x9600, 0x008e, 0x5f00)),
            (0xca3c, 3688, new(0x00e6, 0x2600, 0x004d, 0x3500)),
            (0xca44, 3689, new(0x00e4, 0xaa00, 0x004c, 0x2700)),
            (0xca4c, 3768, new(0x00c6, 0x6600, 0x0059, 0x3c00)),
            (0xca54, 3788, new(0x00ca, 0x9400, 0x0069, 0x1d00)),
            (0xca5c, 3805, new(0x00ce, 0x0200, 0x0079, 0xb500)),
        ];
        foreach ((ushort pointer, int frame, BabyMetroidCutscenePoint point) in expectedRoute)
        {
            bool sawFrame = observedBabyRouteFrames.TryGetValue(pointer, out int actualFrame);
            bool sawPoint = observedBabyRoutePoints.TryGetValue(
                pointer,
                out BabyMetroidCutscenePoint actualPoint);
            if (!sawFrame ||
                actualFrame != frame ||
                !sawPoint ||
                actualPoint != point)
            {
                throw new InvalidOperationException(
                    $"Baby ROM route witness ${pointer:X4} differed: expected frame {frame} " +
                    $"at {point}, got frame {actualFrame} at {actualPoint}.");
            }
        }
        if (observedBabyLatchOntoSamusFrame != 3819)
        {
            throw new InvalidOperationException(
                $"Baby did not install gradual Samus pursuit `$CA66` on frame 3819; " +
                $"observed {observedBabyLatchOntoSamusFrame}.");
        }
    }
    if (options.FrameCount >= 3837 && observedBabyHealSamusFrame != 3837)
    {
        throw new InvalidOperationException(
            $"Baby generic touch AI did not latch on frame 3837; " +
            $"observed {observedBabyHealSamusFrame}.");
    }
    if (options.FrameCount >= 4536 &&
        (cutsceneBaby is null ||
         !observedBabyHealingCompletion ||
         observedBabyHealingCompletionFrame != 4536 ||
         !observedBabyPhases.Contains(BabyMetroidCutscenePhase.IdleUntilNoHealth) ||
         observedBabyHealingCompletionHealth != runtime.Samus.MaxHealth ||
         observedBabyHealingCompletionReserveEnergy != runtime.Samus.MaxReserveEnergy))
    {
        throw new InvalidOperationException(
            $"Baby did not complete the ROM-backed 699-call heal on frame 4536; " +
            $"completion={observedBabyHealingCompletion}/" +
            $"{observedBabyHealingCompletionFrame}, phase=" +
            $"{cutsceneBaby?.Phase.ToString() ?? "not spawned"}, completion energy=" +
            $"{observedBabyHealingCompletionHealth}/{runtime.Samus.MaxHealth}, reserves=" +
            $"{observedBabyHealingCompletionReserveEnergy}/{runtime.Samus.MaxReserveEnergy}.");
    }
    if (options.FrameCount >= 5703 &&
        (cutsceneBaby is null ||
         !observedBabyPhases.Contains(BabyMetroidCutscenePhase.FinalCharge) ||
         !observedBabyPhases.Contains(BabyMetroidCutscenePhase.DeathSequence) ||
         cutsceneBaby.Health != 0))
    {
        throw new InvalidOperationException(
            $"Mother Brain's ROM-backed ring volleys/final charge did not reach the " +
            $"Baby death-sequence seam; phase={cutsceneBaby?.Phase.ToString() ?? "not spawned"}, " +
            $"health=${cutsceneBaby?.Health:X4}.");
    }
    if (options.FrameCount >= 6168 &&
        (cutsceneBaby is null ||
         !cutsceneBaby.IsDeleted ||
         !observedBabyPhaseThreeHandoff ||
         observedBabyDeathExplosions != 30 ||
         allocatedBabyDeathExplosions != 30 ||
         observedBabyDeathPalettes.Count != 6 ||
         observedAttackTileTransfers.Count != 4 ||
         observedPhaseThreeBackgroundPalettes.Count != 7 ||
         runtime.Samus.HyperBeam != 0x8000 ||
         runtime.Samus.Drained.RainbowPaletteEnabled))
    {
        throw new InvalidOperationException(
            $"Baby death/recovery producer differed at frame 6168: " +
            $"deleted={cutsceneBaby?.IsDeleted}, handoff={observedBabyPhaseThreeHandoff}, " +
            $"explosions={observedBabyDeathExplosions}/{allocatedBabyDeathExplosions} allocated, " +
            $"black palettes=" +
            $"{observedBabyDeathPalettes.Count}, attack DMA={observedAttackTileTransfers.Count}, " +
            $"room palettes={observedPhaseThreeBackgroundPalettes.Count}, " +
            $"hyper=${runtime.Samus.HyperBeam:X4}, " +
            $"rainbow={runtime.Samus.Drained.RainbowPaletteEnabled}.");
    }
    if (options.FrameCount >= 6202 &&
        (!observedRainbowPhases.Contains(
             MotherBrainRainbowBeamAttackPhase.Phase3FightingAttackCooldown) ||
         !observedPhaseThreeAttacks.Contains(MotherBrainPhase3AttackKind.FourOnionRings)))
    {
        throw new InvalidOperationException(
            $"Mother Brain did not finish recovery and select the deterministic first " +
            $"phase-three attack on frame 6202; phase={rainbowAttack.Phase}, attacks=" +
            $"{string.Join(",", observedPhaseThreeAttacks)}.");
    }
    if (options.FrameCount >= 6272 &&
        !observedPhaseThreeAttacks.Contains(MotherBrainPhase3AttackKind.Bomb))
    {
        throw new InvalidOperationException(
            $"Mother Brain did not select the deterministic phase-three bomb at the " +
            $"post-cooldown RNG boundary; phase={rainbowAttack.Phase}, attacks=" +
            $"{string.Join(",", observedPhaseThreeAttacks)}.");
    }
    if (options.FrameCount >= 6500 && !observedPhaseThreeForwardMovement)
    {
        throw new InvalidOperationException(
            $"Mother Brain's phase-three walking scheduler never produced positive body " +
            $"movement; body X={rainbowAttack.Body.XPosition}, walk=" +
            $"{rainbowAttack.Phase3WalkingPhase}/${rainbowAttack.Phase3WalkCounter:X4}.");
    }
    if (options.FrameCount >= 1450)
    {
        // The actor request alone is not enough evidence: prove the ordinary NMI queue
        // copied every byte from each real LoROM source into the encoded VRAM word address.
        // Before death these rows must contain the Baby source. Once `$CCC0` deliberately
        // overwrites them, only the later bank-$B7 assertion below describes final VRAM.
        foreach (MotherBrainSpriteTileTransferRequest transfer in
                 observedAttackTileTransfers.Count == 0
                     ? observedBabyTileTransfers
                     : [])
        {
            int vramByteAddress = transfer.VramDestination * 2;
            for (int byteOffset = 0; byteOffset < transfer.Size; byteOffset++)
            {
                byte expected = bus.ReadByte(checked((int)transfer.SourceAddress) + byteOffset);
                byte actual = runtime.Vram.ReadByte(vramByteAddress + byteOffset);
                if (actual != expected)
                {
                    throw new InvalidOperationException(
                        $"Baby tile DMA entry {transfer.EntryIndex} differs at byte " +
                        $"${byteOffset:X4}: expected ${expected:X2}, got ${actual:X2}.");
                }
            }
        }

        foreach (MotherBrainSpriteTileTransferRequest transfer in observedAttackTileTransfers)
        {
            // The death sequence overwrites exactly the same VRAM rows. Prove the final
            // bytes came from bank `$B7`, not merely that four requests were observed.
            int vramByteAddress = transfer.VramDestination * 2;
            for (int byteOffset = 0; byteOffset < transfer.Size; byteOffset++)
            {
                byte expected = bus.ReadByte(checked((int)transfer.SourceAddress) + byteOffset);
                byte actual = runtime.Vram.ReadByte(vramByteAddress + byteOffset);
                if (actual != expected)
                {
                    throw new InvalidOperationException(
                        $"Attack tile DMA entry {transfer.EntryIndex} differs at byte " +
                        $"${byteOffset:X4}: expected ${expected:X2}, got ${actual:X2}.");
                }
            }
        }
    }
    Console.WriteLine(
        $"Mother Brain rainbow ROM route ended at {rainbowAttack.Phase}; " +
        $"body=({rainbowAttack.Body.XPosition},{rainbowAttack.Body.YPosition})/" +
        $"pose {rainbowAttack.Body.Pose}, Samus=({runtime.Samus.XPosition}," +
        $"{runtime.Samus.YPosition}) energy={runtime.Samus.Health}, " +
        $"ammo={runtime.Samus.Missiles}/{runtime.Samus.SuperMissiles}/" +
        $"{runtime.Samus.PowerBombs}, Baby DMA/spawn=" +
        $"{observedBabyTileTransfers.Count}/{observedBabySpawnRequest}, Baby AI=" +
        $"{cutsceneBaby?.Phase.ToString() ?? "not spawned"}/" +
        $"({cutsceneBaby?.XPosition:X4},{cutsceneBaby?.YPosition:X4}), phase-three attacks=" +
        $"{string.Join("/", observedPhaseThreeAttacks)}, forward=" +
        $"{observedPhaseThreeForwardMovement}.");
}

if (options.DrainedSamusScript)
{
    if (options.FrameCount >= 20 && !observedDrainedFallingHandler)
        throw new InvalidOperationException("Drained ROM script never executed `$F7` handler $90:94CB.");
    if (options.FrameCount >= 50 && !observedDrainedLanding)
        throw new InvalidOperationException("Drained ROM script never collided with the live room floor.");
    if (options.FrameCount >= 91 && (!observedDrainedStanding || !observedDrainedCrouching))
        throw new InvalidOperationException("Drained ROM script missed controller-one/four poses $EA/$E8.");
    if (options.FrameCount >= 150 && !observedDrainedRelease)
        throw new InvalidOperationException("Drained ROM script never completed its `$FD,$01` release.");
    if (options.FrameCount >= 151 && !observedDrainedHyperBeam)
        throw new InvalidOperationException("Drained ROM script never installed hyper beam word $8000.");
    Console.WriteLine(
        $"Drained Samus ROM route observed falling={observedDrainedFallingHandler}, " +
        $"landing={observedDrainedLanding}, standing={observedDrainedStanding}, " +
        $"crouching={observedDrainedCrouching}, release={observedDrainedRelease}, " +
        $"hyper={observedDrainedHyperBeam}.");
}

if (options.DraygonGrabScript)
{
    if (options.FrameCount >= 17 && !observedDraygonAimUp)
        throw new InvalidOperationException("Draygon ROM route never reached aim-up pose $ED.");
    if (options.FrameCount >= 33 && !observedDraygonFiring)
        throw new InvalidOperationException("Draygon ROM route never reached firing pose $EE.");
    if (options.FrameCount >= 49 && !observedDraygonAimDown)
        throw new InvalidOperationException("Draygon ROM route never reached aim-down pose $EF.");
    if (options.FrameCount >= 65 && !observedDraygonMoving)
        throw new InvalidOperationException("Draygon ROM route never reached moving pose $F0.");
    if (options.FrameCount >= 97 && !observedDraygonNeutralFallback)
        throw new InvalidOperationException("Draygon ROM route never applied $F0 -> $EC no-input fallback.");
    if (options.FrameCount >= 171 && !observedDraygonRelease)
        throw new InvalidOperationException("Draygon ROM route did not release on its sixtieth counted D-pad pattern.");
    Console.WriteLine(
        $"Draygon ROM route validated every pose/escape milestone reachable within " +
        $"{options.FrameCount} frame(s); escape count=" +
        $"{runtime.Samus!.DraygonGrabbed.EscapeButtonCounter}, " +
        $"owner release={runtime.Samus.DraygonGrabbed.ReleasePublishedToOwner}.");
}

if (options.GrappleFireScript)
{
    if (!observedGrappleFire)
        throw new InvalidOperationException("Grapple-fire ROM script never initialized or extended a beam.");
    if (options.FrameCount >= 13 && !observedGrappleFireCancelQueue)
        throw new InvalidOperationException("Grapple-fire ROM script never queued collision/range cancellation.");
    if (options.FrameCount >= 14 && !observedGrappleFireCancel)
        throw new InvalidOperationException("Grapple-fire ROM script never completed queued cancellation.");
    Console.WriteLine(
        $"Grapple-fire ROM route validated pose-table initialization and every firing/cancellation milestone reachable within {options.FrameCount} frame(s).");
}

Console.WriteLine(
    $"Finished at accepted NMI {runtime.NmiFrameCounter}; " +
    $"timer {runtime.EscapeTimer.MinutesBcd:X2}:{runtime.EscapeTimer.SecondsBcd:X2}.{runtime.EscapeTimer.CentisecondsBcd:X2}; " +
    $"camera=({camera.XPosition:X4}.{camera.XSubposition:X4},{camera.YPosition:X4}.{camera.YSubposition:X4}); " +
    $"minimap=({runtime.Hud.MinimapCenterX},{runtime.Hud.MinimapCenterY}); " +
    $"OAM staged/displayed sprites={runtime.Oam.LastFinalizedSpriteCount}/{runtime.DisplayedOam.LastFinalizedSpriteCount}; " +
    $"VRAM[$F000..$F00F] = {Convert.ToHexString(runtime.Vram.Bytes[0xf000..0xf010])}.");

OamEntry firstSamusSprite = runtime.DisplayedOam.GetEntry(0);
Console.WriteLine(
    $"First staged gameplay OBJ (bombs precede Samus while active): X={firstSamusSprite.X}, Y={firstSamusSprite.Y}, " +
    $"tile=${firstSamusSprite.TileNumber:X3}, palette={firstSamusSprite.Palette}, " +
    $"priority={firstSamusSprite.Priority}, large={firstSamusSprite.IsLarge}. " +
    $"definitions=${runtime.Samus!.TileTransfers.TopDefinitionAddress:X6}/" +
    $"${runtime.Samus.TileTransfers.BottomDefinitionAddress:X6}.");

// Convert the same finalized OAM, VRAM, and CGRAM buffers a real NMI would send to the PPU
// into a transparent desktop image. This is intentionally after the breakpoint loop so a
// developer can compare the PNG against those three live hardware-model objects.
Rgba32[] objFrame = SnesObjRenderer.Render(runtime.DisplayedOam, runtime.Vram, runtime.Cgram, obsel: 0x03);
string objectOutputPath = Path.Combine(
    Path.GetDirectoryName(Path.GetFullPath(options.OutputPath))!,
    Path.GetFileNameWithoutExtension(options.OutputPath) + ".objects.png");
PngWriter.WriteRgba(objectOutputPath, width: 256, height: 224, objFrame);

// Render transparent diagnostics from the exact VRAM state left by the final accepted NMI.
// Physical scanline 32 is the first visible gameplay line. BG scroll registers continue
// counting behind the HUD, so desktop row zero of each 192-line diagnostic samples VOFS+32.
ushort gameplayBg1Y = unchecked((ushort)(runtime.BackgroundScroll.Bg1VerticalScroll + SnesGameplayFrameRenderer.HudHeight));
ushort gameplayBg2Y = unchecked((ushort)(runtime.ScrollingSky!.VerticalScroll + SnesGameplayFrameRenderer.HudHeight));
ushort[] skyHorizontalScrolls = runtime.ScrollingSky.BuildGameplayHorizontalScrolls(camera.YPosition);

Rgba32[] liveBg1 = SnesBgTilemapRenderer.Render4BppViewport(
    runtime.Vram,
    runtime.Cgram,
    tilemapBaseWord: 0x5000,
    characterBaseWord: 0,
    runtime.BackgroundScroll.Bg1HorizontalScroll,
    gameplayBg1Y,
    width: 256,
    height: 192);

// Do not make a developer infer whether a transparent PNG means valid empty space or a
// broken producer. BG1's two horizontal $400-word screens occupy $5000-$57FF; counting
// their populated words separately from the renderer's opaque pixels localizes failures to
// either streaming/map state or character/palette sampling without inventing any fix.
int populatedBg1MapWords = 0;
for (int word = 0x5000; word < 0x5800; word++)
{
    if (runtime.Vram.ReadWord((ushort)word) != 0)
        populatedBg1MapWords++;
}
int opaqueBg1Pixels = 0;
foreach (Rgba32 pixel in liveBg1)
{
    if (pixel.A != 0)
        opaqueBg1Pixels++;
}
Console.WriteLine(
    $"Live BG1 diagnostics: {populatedBg1MapWords:N0}/2,048 populated tilemap words; " +
    $"{opaqueBg1Pixels:N0}/49,152 opaque viewport pixels.");

// Decode the map address for the first physical gameplay pixel exactly as the desktop PPU
// renderer does. Printing the selected entry, character byte address, and nonzero plane
// bytes makes a wrong VRAM unit/base visible in one debugger run.
int diagnosticWorldX = runtime.BackgroundScroll.Bg1HorizontalScroll & 0x01ff;
int diagnosticWorldY = gameplayBg1Y & 0x00ff;
int diagnosticTileX = diagnosticWorldX >> 3;
int diagnosticTileY = diagnosticWorldY >> 3;
int diagnosticScreenColumn = diagnosticTileX >> 5;
ushort diagnosticMapWord = (ushort)(
    0x5000 +
    diagnosticScreenColumn * 0x0400 +
    (diagnosticTileY & 31) * 32 +
    (diagnosticTileX & 31));
ushort diagnosticEntry = runtime.Vram.ReadWord(diagnosticMapWord);
int diagnosticCharacterByte = (diagnosticEntry & 0x03ff) * 32;
int diagnosticCharacterNonzeroBytes = 0;
for (int index = 0; index < 32; index++)
{
    if (runtime.Vram.ReadByte(diagnosticCharacterByte + index) != 0)
        diagnosticCharacterNonzeroBytes++;
}
Console.WriteLine(
    $"First BG1 sample: scroll=({diagnosticWorldX},{diagnosticWorldY}), " +
    $"map=${diagnosticMapWord:X4}, entry=${diagnosticEntry:X4}, " +
    $"character byte=${diagnosticCharacterByte:X4} " +
    $"({diagnosticCharacterNonzeroBytes}/32 nonzero plane bytes).");

// The grounded stimulus supplies an especially useful second probe: Samus's bottom is
// resting on a collision-authored floor, so the corresponding visual tile should not be
// guessed from an arbitrary corner of the viewport.
if (groundedPlacement is DebugGroundedSamusPlacement grounded)
{
    int floorTileX = (grounded.XPosition & 0x01ff) >> 3;
    int floorWorldY = grounded.BlockY * 16;
    int floorTileY = (floorWorldY & 0x00ff) >> 3;
    int floorScreenColumn = floorTileX >> 5;
    ushort floorMapWord = (ushort)(
        0x5000 +
        floorScreenColumn * 0x0400 +
        (floorTileY & 31) * 32 +
        (floorTileX & 31));
    ushort floorEntry = runtime.Vram.ReadWord(floorMapWord);
    int floorCharacterByte = (floorEntry & 0x03ff) * 32;
    int floorCharacterNonzeroBytes = 0;
    for (int index = 0; index < 32; index++)
    {
        if (runtime.Vram.ReadByte(floorCharacterByte + index) != 0)
            floorCharacterNonzeroBytes++;
    }
    Console.WriteLine(
        $"Grounded-floor BG1 sample: world=({grounded.XPosition},{floorWorldY}), " +
        $"map=${floorMapWord:X4}, entry=${floorEntry:X4}, " +
        $"character byte=${floorCharacterByte:X4} " +
        $"({floorCharacterNonzeroBytes}/32 nonzero plane bytes).");
}
string bg1OutputPath = Path.Combine(
    Path.GetDirectoryName(Path.GetFullPath(options.OutputPath))!,
    Path.GetFileNameWithoutExtension(options.OutputPath) + ".background1.png");
PngWriter.WriteRgba(bg1OutputPath, 256, 192, liveBg1);
Console.WriteLine($"Wrote transparent live BG1 layer to {Path.GetFullPath(bg1OutputPath)}.");

Rgba32[] liveBg2 = SnesBgTilemapRenderer.Render4BppViewport(
    runtime.Vram,
    runtime.Cgram,
    tilemapBaseWord: 0x4800,
    characterBaseWord: 0,
    horizontalScroll: 0,
    verticalScroll: gameplayBg2Y,
    width: 256,
    height: 192,
    tilemapWidthInTiles: 32,
    tilemapHeightInTiles: 64,
    horizontalScrollByLine: skyHorizontalScrolls);
string bg2OutputPath = Path.Combine(
    Path.GetDirectoryName(Path.GetFullPath(options.OutputPath))!,
    Path.GetFileNameWithoutExtension(options.OutputPath) + ".background2.png");
PngWriter.WriteRgba(bg2OutputPath, 256, 192, liveBg2);
Console.WriteLine($"Wrote live bank-$88 scrolling-sky BG2 layer to {Path.GetFullPath(bg2OutputPath)}.");

Rgba32[] gameplayFrame = SnesGameplayFrameRenderer.RenderHudLiveBackgroundsAndObjs(
    runtime.Vram,
    runtime.Cgram,
    runtime.DisplayedOam,
    runtime.BackgroundScroll.Bg1HorizontalScroll,
    runtime.BackgroundScroll.Bg1VerticalScroll,
    runtime.ScrollingSky.VerticalScroll,
    skyHorizontalScrolls);
PngWriter.WriteRgba(options.OutputPath, width: 256, height: 224, gameplayFrame);
Console.WriteLine($"Wrote ROM-backed HUD/OBJ frame to {Path.GetFullPath(options.OutputPath)}.");
Console.WriteLine($"Wrote transparent OBJ layer to {Path.GetFullPath(objectOutputPath)}.");
return 0;
}
catch (Exception exception)
{
    // Keep a bad ROM path, failed assertion, or unfinished translation in the debugger's
    // console. An explicit failing exit code preserves automation semantics without allowing
    // the CLR/Windows error reporter to display a modal "unknown software exception" box.
    Console.Error.WriteLine(exception);
    return 1;
}

[MethodImpl(MethodImplOptions.NoInlining)]
static void FrameBreakpoint(SuperMetroidRuntime runtime, RuntimeFrameResult result)
{
    // Intentionally empty. Both parameters remain debugger-visible and give a breakpoint a
    // stable name that will survive as the main runtime grows around it.
    _ = runtime;
    _ = result;
}

/// <summary>Command-line choices kept explicit so invalid debug sessions fail helpfully.</summary>
readonly record struct DebugRunnerOptions(
    string RomPath,
    int FrameCount,
    TimerScenario TimerScenario,
    string OutputPath,
    bool GroundedRun,
    int RightFrameCount,
    bool ReversalScript,
    bool MoonwalkScript,
    bool RanIntoWallScript,
    bool RunScript,
    bool SpeedBoosterScript,
    bool ShinesparkScript,
    bool SpaceJumpScript,
    bool WaterSpaceJumpScript,
    bool ScrewAttackScript,
    bool JumpScript,
    bool PostureScript,
    bool AimScript,
    bool AimRunScript,
    bool AimAirScript,
    bool AerialTurnScript,
    bool CompactAirScript,
    bool AimCrouchScript,
    bool AimTurnScript,
    bool CrouchTurnScript,
    bool CrouchJumpScript,
    bool MorphBallScript,
    bool SpringBallScript,
    bool BombJumpScript,
    bool KnockbackScript,
    bool GrappleScript,
    bool GrappleFireScript,
    bool CrystalFlashScript,
    bool MotherBrainRainbowScript,
    bool DrainedSamusScript,
    bool DraygonGrabScript)
{
    public static DebugRunnerOptions Parse(string[] arguments)
    {
        string? romPath = null;
        int frameCount = 240;
        TimerScenario timerScenario = TimerScenario.Ceres;
        string? outputPath = null;
        bool groundedRun = false;
        int rightFrameCount = int.MaxValue;
        bool reversalScript = false;
        bool moonwalkScript = false;
        bool ranIntoWallScript = false;
        bool runScript = false;
        bool speedBoosterScript = false;
        bool shinesparkScript = false;
        bool spaceJumpScript = false;
        bool waterSpaceJumpScript = false;
        bool screwAttackScript = false;
        bool jumpScript = false;
        bool postureScript = false;
        bool aimScript = false;
        bool aimRunScript = false;
        bool aimAirScript = false;
        bool aerialTurnScript = false;
        bool compactAirScript = false;
        bool aimCrouchScript = false;
        bool aimTurnScript = false;
        bool crouchTurnScript = false;
        bool crouchJumpScript = false;
        bool morphBallScript = false;
        bool springBallScript = false;
        bool bombJumpScript = false;
        bool knockbackScript = false;
        bool grappleScript = false;
        bool grappleFireScript = false;
        bool crystalFlashScript = false;
        bool motherBrainRainbowScript = false;
        bool drainedSamusScript = false;
        bool draygonGrabScript = false;

        for (int index = 0; index < arguments.Length; index++)
        {
            string argument = arguments[index];
            switch (argument)
            {
                case "--frames":
                    frameCount = ParsePositiveInt(ReadValue(arguments, ref index, argument), argument);
                    break;

                case "--timer":
                    string scenario = ReadValue(arguments, ref index, argument);
                    timerScenario = scenario.ToLowerInvariant() switch
                    {
                        "ceres" => TimerScenario.Ceres,
                        "mother-brain" => TimerScenario.MotherBrain,
                        _ => throw new ArgumentException("--timer must be 'ceres' or 'mother-brain'."),
                    };
                    break;

                case "--output":
                    outputPath = ReadValue(arguments, ref index, argument);
                    break;

                case "--grounded-run":
                    groundedRun = true;
                    break;

                case "--right-frames":
                    rightFrameCount = ParsePositiveInt(
                        ReadValue(arguments, ref index, argument),
                        argument);
                    break;

                case "--reversal-script":
                    reversalScript = true;
                    groundedRun = true;
                    break;

                case "--moonwalk-script":
                    moonwalkScript = true;
                    groundedRun = true;
                    break;

                case "--ran-into-wall-script":
                    ranIntoWallScript = true;
                    groundedRun = true;
                    break;

                case "--run-script":
                    runScript = true;
                    groundedRun = true;
                    break;

                case "--speed-booster-script":
                    speedBoosterScript = true;
                    groundedRun = true;
                    break;

                case "--shinespark-script":
                    shinesparkScript = true;
                    groundedRun = true;
                    break;

                case "--space-jump-script":
                    spaceJumpScript = true;
                    groundedRun = true;
                    break;

                case "--water-space-jump-script":
                    waterSpaceJumpScript = true;
                    groundedRun = true;
                    break;

                case "--screw-attack-script":
                    screwAttackScript = true;
                    groundedRun = true;
                    break;

                case "--jump-script":
                    jumpScript = true;
                    groundedRun = true;
                    break;

                case "--posture-script":
                    postureScript = true;
                    groundedRun = true;
                    break;

                case "--aim-script":
                    aimScript = true;
                    groundedRun = true;
                    break;

                case "--aim-run-script":
                    aimRunScript = true;
                    groundedRun = true;
                    break;

                case "--aim-air-script":
                    aimAirScript = true;
                    groundedRun = true;
                    break;

                case "--aerial-turn-script":
                    aerialTurnScript = true;
                    groundedRun = true;
                    break;

                case "--compact-air-script":
                    compactAirScript = true;
                    groundedRun = true;
                    break;

                case "--aim-crouch-script":
                    aimCrouchScript = true;
                    groundedRun = true;
                    break;

                case "--aim-turn-script":
                    aimTurnScript = true;
                    groundedRun = true;
                    break;

                case "--crouch-turn-script":
                    crouchTurnScript = true;
                    groundedRun = true;
                    break;

                case "--crouch-jump-script":
                    crouchJumpScript = true;
                    groundedRun = true;
                    break;

                case "--morph-ball-script":
                    morphBallScript = true;
                    groundedRun = true;
                    break;

                case "--spring-ball-script":
                    springBallScript = true;
                    groundedRun = true;
                    break;

                case "--bomb-jump-script":
                    bombJumpScript = true;
                    groundedRun = true;
                    break;

                case "--knockback-script":
                    knockbackScript = true;
                    groundedRun = true;
                    break;

                case "--grapple-script":
                    grappleScript = true;
                    groundedRun = true;
                    break;

                case "--grapple-fire-script":
                    grappleFireScript = true;
                    groundedRun = true;
                    break;

                case "--crystal-flash-script":
                    crystalFlashScript = true;
                    groundedRun = true;
                    break;

                case "--mother-brain-rainbow-script":
                    motherBrainRainbowScript = true;
                    break;

                case "--drained-samus-script":
                    drainedSamusScript = true;
                    groundedRun = true;
                    break;

                case "--draygon-grab-script":
                    draygonGrabScript = true;
                    groundedRun = true;
                    break;

                default:
                    if (argument.StartsWith('-'))
                        throw new ArgumentException($"Unknown option '{argument}'.");
                    if (romPath is not null)
                        throw new ArgumentException("Specify exactly one ROM path.");
                    romPath = argument;
                    break;
            }
        }

        if (romPath is null)
        {
            throw new ArgumentException(
                "ROM path is required. Example: dotnet run --project src/SuperMetroid.DebugRunner -- " +
                "../Super Metroid.smc --frames 240 --timer ceres");
        }

        outputPath ??= Path.Combine(
            Path.GetDirectoryName(Path.GetFullPath(romPath))!,
            "standalone-assets",
            "runtime",
            "EscapeTimerFrame.png");

        // The frame runtime reads compressed room data, graphics, palette, door metadata,
        // and library-background tilemaps directly from the ROM. It deliberately has no
        // extracted-assets argument; PNG/raw folders are outputs and diagnostics, not an
        // alternative source of gameplay truth.
        return new DebugRunnerOptions(
            romPath,
            frameCount,
            timerScenario,
            outputPath,
            groundedRun,
            rightFrameCount,
            reversalScript,
            moonwalkScript,
            ranIntoWallScript,
            runScript,
            speedBoosterScript,
            shinesparkScript,
            spaceJumpScript,
            waterSpaceJumpScript,
            screwAttackScript,
            jumpScript,
            postureScript,
            aimScript,
            aimRunScript,
            aimAirScript,
            aerialTurnScript,
            compactAirScript,
            aimCrouchScript,
            aimTurnScript,
            crouchTurnScript,
            crouchJumpScript,
            morphBallScript,
            springBallScript,
            bombJumpScript,
            knockbackScript,
            grappleScript,
            grappleFireScript,
            crystalFlashScript,
            motherBrainRainbowScript,
            drainedSamusScript,
            draygonGrabScript);
    }

    private static string ReadValue(string[] arguments, ref int index, string option)
    {
        if (++index >= arguments.Length)
            throw new ArgumentException($"{option} requires a value.");
        return arguments[index];
    }

    private static int ParsePositiveInt(string value, string option)
    {
        if (!int.TryParse(value, out int parsed) || parsed <= 0)
            throw new ArgumentException($"{option} requires a positive integer, not '{value}'.");
        return parsed;
    }
}

enum TimerScenario
{
    Ceres,
    MotherBrain,
}

/// <summary>
/// Makes the command-line debugger genuinely non-interactive on Windows. The CLR retains
/// its ordinary stderr stack trace and exit code; only OS-owned modal error boxes are barred.
/// </summary>
static class NativeConsoleProcess
{
    [DllImport("kernel32.dll")]
    internal static extern uint SetErrorMode(uint errorMode);
}
