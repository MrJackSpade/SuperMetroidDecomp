using System.Runtime.CompilerServices;
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
DebugRunnerOptions options = DebugRunnerOptions.Parse(args);
SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(options.RomPath);
var runtime = new SuperMetroidRuntime(bus);

Console.WriteLine($"Loaded {Path.GetFullPath(options.RomPath)} ({bus.Rom.Length:N0} bytes).");
Console.WriteLine($"Stepping {options.FrameCount:N0} translated frames using {options.TimerScenario} timer startup.");
if (options.ReversalScript)
{
    Console.WriteLine(
        "Input script: Start, release, Right for 60 frames, Left for 60, Right for 60, then release.");
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
DebugGroundedSamusPlacement? groundedPlacement = options.GroundedRun
    ? runtime.InitializeDebugGroundedSamus()
    : null;
InitialViewportResult initialViewport = runtime.InitializeLandingSiteViewport();

// The cutscene door does not define a normal-gameplay Samus spawn. In the default scenario,
// introduce stationary pose $01 at a clearly documented host point so the native palette,
// animation-definition, tile-DMA, screen-position, split-spritemap, and OAM paths can be
// stepped without pretending that the cinematic spawned gameplay Samus.
if (!options.GroundedRun)
    runtime.InitializeDebugStandingSamus();

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

// Resolve this through the same two-stage pointer calculation used by live movement. This
// line is deliberately ROM-backed evidence, not a hard-coded description: ordinary air's
// base $9F55 plus running movement type 1 selects the 12-byte entry at $90:9F61.
SamusHorizontalSpeedState horizontalSpeed = runtime.Samus.HorizontalSpeed;
horizontalSpeed.SelectNormalAirSpeedTable();
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

if (options.TimerScenario == TimerScenario.Ceres)
    runtime.EscapeTimer.RequestCeresStart();
else
    runtime.EscapeTimer.RequestMotherBrainStart();

EscapeTimerState priorState = runtime.EscapeTimer.State;
ushort priorSamusFrame = runtime.Samus!.AnimationFrame;
byte priorSamusPose = runtime.Samus.Pose;
uint priorSamusX = runtime.Samus.Kinematics.XFixed;
uint priorSamusY = runtime.Samus.Kinematics.YFixed;
ushort? priorProspectivePose = null;
ushort? priorFallbackPose = null;
bool observedBombJumpStart = false;
bool observedBombJumpEnd = false;
bool observedBombJumpRise = false;
bool observedBombPlacement = false;
bool observedBombExplosion = false;
bool observedBombDeletion = false;
bool observedStraightBombOverlap = false;
bool observedKnockbackMovement = false;
bool observedDamageBoostMovement = false;
// Keep the actual post-frame poses, rather than assuming the requested inputs succeeded.
// The dedicated ROM regression below fails unless both compact bodies and both native
// ordinary-landing records were genuinely installed by the translated frame pipeline.
var observedSamusPoses = new HashSet<byte> { runtime.Samus.Pose };
for (int frameIndex = 0; frameIndex < options.FrameCount; frameIndex++)
{
    // The default script presses Start for one frame, releases it, then holds Right. The
    // explicit reversal script is a deterministic real-ROM regression route: enough time
    // to accelerate right, complete $25 toward the left, then complete $26 back right.
    // These are only controller samples; all pose choices still come from bank-$91 tables.
    ushort controllerInput = options.KnockbackScript
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

    RuntimeFrameResult result = runtime.StepFrame(controllerInput);
    observedSamusPoses.Add(runtime.Samus.Pose);
    observedKnockbackMovement |= runtime.LastKnockbackMovement is not null;
    observedDamageBoostMovement |= runtime.LastAerialSamusMovement is not null &&
        runtime.Samus.ReadMovementType(bus) == 0x19;
    observedBombJumpStart |= runtime.LastBombJumpMovement is { Started: true };
    observedBombJumpEnd |= runtime.LastBombJumpMovement is { Ended: true };
    observedBombJumpRise |= runtime.LastBombJumpMovement is { Started: false, Ended: false };
    observedBombPlacement |= runtime.BombProjectiles.LastFrameResult.PlacedSlot is not null;
    observedBombExplosion |= runtime.BombProjectiles.LastFrameResult.ExplosionStarted;
    observedBombDeletion |= runtime.BombProjectiles.LastFrameResult.ProjectileDeleted;
    observedStraightBombOverlap |=
        runtime.BombProjectiles.LastFrameResult.PublishedBombJumpDirection == 2;

    // Put a breakpoint here to inspect the complete runtime after any chosen frame. The
    // NoInlining attribute below keeps this method as a reliable stack frame in Debug and
    // Release builds instead of letting the JIT dissolve the hook into this loop.
    FrameBreakpoint(runtime, result);

    if (result.EscapeTimerState != priorState || result.EscapeTimerExpired)
    {
        Console.WriteLine(
            $"frame {result.FrameNumber,4}: timer={result.EscapeTimerState,-27} " +
            $"time={runtime.EscapeTimer.MinutesBcd:X2}:{runtime.EscapeTimer.SecondsBcd:X2}.{runtime.EscapeTimer.CentisecondsBcd:X2} " +
            $"position=({runtime.EscapeTimer.XPixel},{runtime.EscapeTimer.YPixel}) " +
            $"expired={result.EscapeTimerExpired}");
        priorState = result.EscapeTimerState;
    }

    if (runtime.Samus.AnimationFrame != priorSamusFrame)
    {
        Console.WriteLine(
            $"frame {result.FrameNumber,4}: Samus animation {priorSamusFrame} -> " +
            $"{runtime.Samus.AnimationFrame}; timer={runtime.Samus.AnimationFrameTimer}; " +
            $"command={(runtime.Samus.LastAnimationDelayCommand is byte command ? $"${command:X2}" : "none")}; " +
            $"next definitions=${runtime.Samus.TileTransfers.TopDefinitionAddress:X6}/" +
            $"${runtime.Samus.TileTransfers.BottomDefinitionAddress:X6}");
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
        BlockMoveResult horizontal = runtime.LastGroundedSamusMovement?.Horizontal ??
            runtime.LastAerialSamusMovement?.Horizontal ??
            runtime.LastMorphBallMovement?.Horizontal ??
            runtime.LastBombJumpMovement?.Horizontal ??
            runtime.LastKnockbackMovement?.Horizontal ??
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
            $"horizontal=${horizontal.AcceptedDisplacement:X8}, {vertical}");
        priorSamusX = runtime.Samus.Kinematics.XFixed;
    }

    if (runtime.Samus.Kinematics.YFixed != priorSamusY)
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
                     SamusState.TurningRightToLeftJumpPose or
                     SamusState.TurningLeftToRightJumpPose or
                     SamusState.TurningRightToLeftFallingPose or
                     SamusState.TurningLeftToRightFallingPose
                    ? "applied at the verified post-animation transition seam"
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
    bool KnockbackScript)
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
            knockbackScript);
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
