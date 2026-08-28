using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{

/// <summary>Aerial turn, wall-jump, knockback, and damage-boost verification.</summary>
static void VerifySamusAerialTurnsAndWallJump()
{
    var bus = new TestAddressSpace();

    const int width = 8;
    const int height = 8;
    var foreground = new ushort[width * height];
    for (int x = 0; x < width; x++)
        foreground[6 * width + x] = 0x8000;
    for (int y = 1; y < 6; y++)
        foreground[y * width + 4] = 0x8000; // vertical wall begins at world X=64
    RoomLevelData level = CreateRoom(
        width, height, foreground, new byte[foreground.Length]);

    // Source poses are real retail pose numbers covering shot directions zero through nine.
    // Compact down-aim records `$17/$18/$2D/$2E` use radius ten; every other source uses 19.
    byte[] jumpSources = [0x15, 0x69, 0x51, 0x6b, 0x17, 0x18, 0x6c, 0x52, 0x6a, 0x16];
    byte[] fallSources = [0x2b, 0x6d, 0x29, 0x6f, 0x2d, 0x2e, 0x70, 0x2a, 0x6e, 0x2c];
    byte[] jumpTargets = [0x8f, 0x9e, 0x2f, 0x91, 0x91, 0x92, 0x92, 0x30, 0x9f, 0x90];
    byte[] fallTargets = [0x93, 0xa0, 0x87, 0x95, 0x95, 0x96, 0x96, 0x88, 0xa1, 0x94];

    for (int direction = 0; direction < 10; direction++)
    {
        VerifySelector(jumping: true, jumpSources[direction], jumpTargets[direction], direction);
        VerifySelector(jumping: false, fallSources[direction], fallTargets[direction], direction);
    }

    void VerifySelector(bool jumping, byte sourcePose, byte expectedPose, int shotDirection)
    {
        bool sourceFacesLeft = shotDirection >= 5;
        byte sourceXDirection = sourceFacesLeft ? (byte)4 : (byte)8;
        byte sourceRadius = shotDirection is 4 or 5 ? (byte)10 : (byte)19;
        byte sourceMovementType = jumping ? (byte)2 : (byte)6;
        WritePoseDefinition(bus, sourcePose, [
            sourceXDirection, sourceMovementType, 0xff, (byte)shotDirection,
            0, 0, sourceRadius, 0,
        ]);

        bool targetFacesLeft = shotDirection < 5;
        byte targetXDirection = targetFacesLeft ? (byte)4 : (byte)8;
        byte targetMovementType = jumping ? (byte)0x17 : (byte)0x18;
        WritePoseDefinition(bus, expectedPose, [
            targetXDirection, targetMovementType, 0xff, 0xfb,
            8, 0, 19, 0,
        ]);
        WriteTestWord(bus, 0x91b010 + expectedPose * 2, 0xc000);
        bus.WriteBytes(0x91c000, [2]);

        var samus = new SamusState
        {
            Pose = sourcePose,
            XPosition = 32,
            YPosition = 48,
        };
        samus.RefreshCollisionRadii(bus);
        samus.HorizontalSpeed.BaseSpeed = 1;
        samus.HorizontalSpeed.ExtraRunSubspeed = 0x8000;
        byte genericTarget = jumping
            ? targetFacesLeft ? SamusState.TurningRightToLeftJumpPose : SamusState.TurningLeftToRightJumpPose
            : targetFacesLeft ? SamusState.TurningRightToLeftFallingPose : SamusState.TurningLeftToRightFallingPose;
        AssertTrue(
            samus.TryApplyAerialTurn(bus, level, genericTarget, nmiFrameCounter: 0),
            $"{(jumping ? "jump" : "fall")} turn direction {shotDirection} fits");
        AssertEqual(expectedPose, samus.Pose, $"{(jumping ? "jump" : "fall")} selector direction {shotDirection}");
        AssertEqual(0x00018000u, samus.HorizontalSpeed.BaseFixed, "aerial selector folds extra speed");
        AssertEqual(1, samus.HorizontalSpeed.AccelerationMode, "aerial selector starts turn mode");
    }

    // Isolate one diagonal-up jumping turn with its real three-frame `$F8,$6A` stream.
    WritePoseDefinition(bus, 0x69, [8, 2, 0xff, 1, 8, 0, 19, 0]);
    WritePoseDefinition(bus, 0x9e, [4, 0x17, 0xff, 0xfb, 8, 0, 19, 0]);
    WritePoseDefinition(bus, 0x6a, [4, 2, 0xff, 8, 8, 0, 19, 0]);
    WriteTestWord(bus, 0x91b010 + 0x9e * 2, 0xc100);
    WriteTestWord(bus, 0x91b010 + 0x6a * 2, 0xc110);
    bus.WriteBytes(0x91c100, [2, 2, 2, 0xf8, 0x6a]);
    bus.WriteBytes(0x91c110, [3]);
    WriteSpeedRecord(bus, movementType: 0x17, accelerationSub: 0, maximumSpeed: 2, decelerationSub: 0x1000);
    WriteTestWord(bus, 0x909ea1, 0x2800);
    WriteTestWord(bus, 0x909ea7, 0);
    var turn = new SamusState { Pose = 0x69, XPosition = 32, YPosition = 48 };
    turn.RefreshCollisionRadii(bus);
    turn.HorizontalSpeed.BaseSpeed = 1;
    turn.HorizontalSpeed.ExtraRunSubspeed = 0x8000;
    turn.Kinematics.YDirection = 1;
    turn.Kinematics.YSpeed = 2;
    turn.Kinematics.YSubacceleration = 0x2800;
    AssertTrue(turn.TryApplyAerialTurn(bus, level, 0x2f, 0), "diagonal-up aerial turn installs");
    AerialMovementResult turnFrame = SamusAerialMovement.StepTurningInAir(bus, level, turn, 0);
    AssertEqual(0x00017000, turnFrame.Horizontal.AcceptedDisplacement, "turn retains old rightward momentum");
    AssertEqual(33, turn.XPosition, "turn moves in old direction despite new facing");
    for (int tick = 0; tick < 6; tick++)
        turn.AnimateNoFx(bus);
    AssertEqual(0xf8, turn.LastAnimationDelayCommand!.Value, "aerial turn reaches F8");
    AssertTrue(turn.ApplyPendingVerifiedAnimationTransition(bus), "aerial turn F8 applies");
    AssertEqual(0x6a, turn.Pose, "aerial turn preserves diagonal-up aim endpoint");

    // Ordinary spin art, wall-jump art, and both dry launch table pairs.
    WritePoseDefinition(bus, 0x19, [8, 3, 0xff, 0xff, 0, 0, 12, 0]);
    WritePoseDefinition(bus, 0x83, [8, 0x14, 0x19, 0xff, 8, 0, 19, 0]);
    WriteTestWord(bus, 0x91b010 + 0x19 * 2, 0xc200);
    WriteTestWord(bus, 0x91b010 + 0x83 * 2, 0xc220);
    bus.WriteBytes(0x91c200, [2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0xff]);
    bus.WriteBytes(0x91c220, [4, 4, 0xfb, 2, 2, 2, 2, 2, 2, 2, 2, 0xfe, 8]);
    WriteSpeedRecord(bus, movementType: 3, accelerationSub: 0x2000, maximumSpeed: 1, decelerationSub: 0x1000);
    WriteSpeedRecord(bus, movementType: 0x14, accelerationSub: 0x1000, maximumSpeed: 1, decelerationSub: 0x1000);
    WriteTestWord(bus, 0x909ed1, 4);
    WriteTestWord(bus, 0x909ed7, 0xa000);

    var earlyContact = CreateSpinSamus(animationFrame: 0);
    earlyContact.SolidVerticalCollisionResult = 0x7777;
    AerialMovementResult contactFrame = SamusAerialMovement.StepSpinJump(
        bus, level, earlyContact, (ushort)(SnesButton.Left | SnesButton.A), 0, 0);
    AssertTrue(contactFrame.WallContact && !contactFrame.WallJumpTriggered, "early wall chord contacts without launch");
    AssertEqual(0x0a, earlyContact.AnimationFrame, "early wall contact rewinds to frame A");
    AssertEqual(0x7777, earlyContact.SolidVerticalCollisionResult,
        "early wall contact does not publish launch collision result");

    // Merely holding Jump after the eligible frame is still contact, not a launch. In
    // particular it must not leak the result-five write from the accepted paths below.
    var heldOnly = CreateSpinSamus(animationFrame: 0x0b);
    AerialMovementResult heldOnlyFrame = SamusAerialMovement.StepSpinJump(
        bus, level, heldOnly, (ushort)(SnesButton.Left | SnesButton.A), 0, 0);
    AssertTrue(heldOnlyFrame.WallContact && !heldOnlyFrame.WallJumpTriggered,
        "eligible held Jump without a fresh edge remains contact only");
    AssertEqual(0, heldOnly.SolidVerticalCollisionResult,
        "held-only wall contact leaves collision result clear");

    var eligible = CreateSpinSamus(animationFrame: 0x0b);
    ushort beforeTriggerY = eligible.YPosition;
    AerialMovementResult triggerFrame = SamusAerialMovement.StepSpinJump(
        bus,
        level,
        eligible,
        (ushort)(SnesButton.Left | SnesButton.A),
        0,
        (ushort)SnesButton.A);
    AssertTrue(triggerFrame.WallJumpTriggered, "eligible fresh jump press triggers wall jump");
    AssertTrue(triggerFrame.Vertical is null, "wall trigger carry skips vertical movement");
    AssertEqual(beforeTriggerY, eligible.YPosition, "wall trigger frame preserves Y");
    AssertEqual(7, triggerFrame.WallDistance, "wall trigger reports clipped seven-pixel distance");
    AssertEqual(5, eligible.SolidVerticalCollisionResult,
        "terrain wall jump publishes native solid-vertical result five");

    // Repeat the same eligible chord with a native solid-enemy snapshot in front of the
    // terrain. `$90:9E64` must publish that exact slot for enemy AI's shake response; a
    // terrain-backed wall jump deliberately does not write this word.
    var enemyEligible = CreateSpinSamus(animationFrame: 0x0b);
    enemyEligible.Kinematics.InteractiveEnemies =
    [
        new SolidEnemyCollisionBody(
            Index: 0x0240,
            XPosition: 69,
            YPosition: 48,
            XRadius: 5,
            YRadius: 5,
            FreezeTimer: 0,
            Properties: 0x8000),
    ];
    AerialMovementResult enemyTrigger = SamusAerialMovement.StepSpinJump(
        bus,
        level,
        enemyEligible,
        (ushort)(SnesButton.Left | SnesButton.A),
        0,
        (ushort)SnesButton.A);
    AssertTrue(enemyTrigger.WallJumpTriggered, "solid enemy can trigger ordinary wall jump");
    AssertEqual(7, enemyTrigger.WallDistance, "enemy wall jump retains directional gap");
    AssertEqual(5, enemyEligible.SolidVerticalCollisionResult,
        "enemy wall jump publishes native solid-vertical result five");
    AssertEqual(0x0240, enemyEligible.EnemyIndexToShake,
        "enemy wall jump publishes contacted slot for shake");

    // Bank $91:F2D3 clears only base speed; bank $90:9949 installs Y launch speed and
    // likewise leaves the Dash pair/flag alone. Seed a visible fractional value here so
    // an over-broad wall-jump cleanup cannot masquerade as a harmless zero-state write.
    eligible.HorizontalSpeed.ExtraRunSpeed = 1;
    eligible.HorizontalSpeed.ExtraRunSubspeed = 0x7000;
    eligible.HorizontalSpeed.HasRunningMomentum = true;
    eligible.LiquidPhysics.BeginFrameSoundRequests();
    eligible.ApplyWallJumpTrigger(bus);
    AssertEqual(0x83, eligible.Pose, "right-facing spin selects right wall-jump pose");
    AssertEqual(4, eligible.Kinematics.YSpeed, "wall jump reads whole launch speed");
    AssertEqual(0xa000, eligible.Kinematics.YSubspeed, "wall jump reads fractional launch speed");
    AssertEqual(1, eligible.HorizontalSpeed.ExtraRunSpeed, "wall jump preserves Dash whole speed");
    AssertEqual(0x7000, eligible.HorizontalSpeed.ExtraRunSubspeed, "wall jump preserves Dash fraction");
    AssertTrue(eligible.HorizontalSpeed.HasRunningMomentum, "wall jump preserves Dash momentum flag");
    AssertEqual(new SamusSoundRequest(3, 0x05, 6), eligible.LiquidPhysics.SoundRequests.Single(),
        "ordinary wall trigger queues library-three sound five max six");
    for (int tick = 0; tick < 8; tick++)
        eligible.AnimateNoFx(bus);
    AssertEqual(0xfb, eligible.LastAnimationDelayCommand!.Value, "wall animation reaches FB");
    AssertEqual(3, eligible.AnimationFrame, "ordinary dry wall animation selects frame three");

    eligible.ProjectileFlareCounter = 0x003c;
    ushort wallStartY = eligible.YPosition;
    AerialMovementResult wallFrame = SamusAerialMovement.StepWallJump(
        bus,
        level,
        eligible,
        (ushort)(SnesButton.Right | SnesButton.A),
        1);
    AssertTrue(wallFrame.Vertical is { Collided: false }, "wall launch remains airborne");
    AssertEqual((wallStartY - 5), eligible.YPosition, "wall launch moves by old 4.A000 speed");
    AssertEqual(4, eligible.HorizontalSpeed.ContactDamageIndex,
        "charged wall-jump frames three through 22 publish damage index four");

    eligible.HorizontalSpeed.ContactDamageIndex = 0;
    eligible.AnimationFrame = 0x17;
    SamusAerialMovement.StepWallJump(
        bus,
        level,
        eligible,
        (ushort)(SnesButton.Right | SnesButton.A),
        2);
    AssertEqual(3, eligible.HorizontalSpeed.ContactDamageIndex,
        "wall-jump frame 23 publishes Screw-style damage index three");

    SamusState CreateSpinSamus(ushort animationFrame)
    {
        var samus = new SamusState { Pose = 0x19, XPosition = 52, YPosition = 48 };
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus, 0);
        samus.AnimationFrame = animationFrame;
        samus.Kinematics.YDirection = 1;
        samus.Kinematics.YSpeed = 2;
        samus.Kinematics.YSubacceleration = 0x2800;
        return samus;
    }

    void WriteSpeedRecord(
        TestAddressSpace addressSpace,
        byte movementType,
        ushort accelerationSub,
        ushort maximumSpeed,
        ushort decelerationSub)
    {
        int address = 0x909f55 + movementType * 12;
        WriteTestWord(addressSpace, address + 0, 0);
        WriteTestWord(addressSpace, address + 2, accelerationSub);
        WriteTestWord(addressSpace, address + 4, maximumSpeed);
        WriteTestWord(addressSpace, address + 6, 0);
        WriteTestWord(addressSpace, address + 8, 0);
        WriteTestWord(addressSpace, address + 10, decelerationSub);
    }

    Console.WriteLine("  Samus aerial turns/wall jump: selectors, momentum, sounds, contact damage, wall gate, FB, and launch agree.");
}

/// <summary>
/// Verifies command-seven bottom alignment, movement types five/$0F, $FD completion, and
/// the rejected stand-up case where ceiling and floor leave room for crouch but not stand.
/// </summary>
/// <summary>
/// Verifies the dry-air, ordinary-body branch of the retail hurt handler and its hidden
/// damage-boost exit. The fixture bytes below are literal records from banks $90/$91;
/// the production path still reads them through <see cref="ISnesAddressSpace"/> instead of
/// embedding friendly host-side velocities or pose metadata.
/// </summary>
static void VerifySamusKnockbackAndDamageBoost()
{
    var bus = new TestAddressSpace();

    // Pose definitions live at `$91:B629 + pose * 8`. Only the fields consumed by this
    // slice are nonzero: X direction, movement type, vertical graphics offset, and radius.
    // `$53/$54` are type-$0A hurt poses; `$4F/$50` are type-$19 damage-boost poses.
    bus.WriteBytes(0x91b631, [0x08, 0x00, 0xff, 0x02, 0x06, 0x00, 0x15, 0x00]); // $01
    bus.WriteBytes(0x91b771, [0x08, 0x06, 0xff, 0x07, 0x08, 0x00, 0x13, 0x00]); // $29
    bus.WriteBytes(0x91b891, [0x08, 0x02, 0xff, 0x02, 0x08, 0x00, 0x13, 0x00]); // $4D
    bus.WriteBytes(0x91b8a1, [0x08, 0x19, 0x4e, 0xff, 0x08, 0x00, 0x13, 0x00]); // $4F
    bus.WriteBytes(0x91b8a9, [0x04, 0x19, 0x4d, 0xff, 0x08, 0x00, 0x13, 0x00]); // $50
    bus.WriteBytes(0x91b8c1, [0x08, 0x0a, 0xff, 0xff, 0x06, 0x00, 0x15, 0x00]); // $53
    bus.WriteBytes(0x91b8c9, [0x04, 0x0a, 0xff, 0xff, 0x06, 0x00, 0x15, 0x00]); // $54
    bus.WriteBytes(0x91bb51, [0x04, 0x00, 0xff, 0x07, 0x03, 0x00, 0x15, 0x00]); // $A5

    // One representative pose for each retail Morph/Spring movement type admitted by
    // `$90:DF15/$91:EE27`. Alternating facings make the pose-direction-only knockback
    // selection independently observable from the enemy's X-side word.
    WritePoseDefinition(bus, SamusState.MorphBallGroundRightPose,
        [0x08, 0x04, 0xff, 0xff, 0x00, 0x00, 0x07, 0x00]);
    WritePoseDefinition(bus, SamusState.MorphBallFallingLeftPose,
        [0x04, 0x08, 0xff, 0xff, 0x00, 0x00, 0x07, 0x00]);
    WritePoseDefinition(bus, SamusState.SpringBallGroundRightPose,
        [0x08, 0x11, 0xff, 0xff, 0x00, 0x00, 0x07, 0x00]);
    WritePoseDefinition(bus, SamusState.SpringBallJumpLeftPose,
        [0x04, 0x12, 0xff, 0xff, 0x00, 0x00, 0x07, 0x00]);
    WritePoseDefinition(bus, SamusState.SpringBallFallingRightPose,
        [0x08, 0x13, 0xff, 0xff, 0x00, 0x00, 0x07, 0x00]);

    // Animation-pointer table entries are `$91:B010 + pose * 2`. One long ordinary
    // delay is enough to keep animation unrelated to this movement-focused assertion.
    WriteTestWord(bus, 0x91b062, 0xc100); // $29
    WriteTestWord(bus, 0x91b0ae, 0xc110); // $4F
    WriteTestWord(bus, 0x91b0b0, 0xc120); // $50
    WriteTestWord(bus, 0x91b0b6, 0xc130); // $53
    WriteTestWord(bus, 0x91b0b8, 0xc140); // $54
    WriteTestWord(bus, 0x91b15a, 0xc150); // $A5
    bus.WriteBytes(0x91c100, [0x10]);
    bus.WriteBytes(0x91c110, [0x08]);
    bus.WriteBytes(0x91c120, [0x08]);
    bus.WriteBytes(0x91c130, [0x02]);
    bus.WriteBytes(0x91c140, [0x02]);
    bus.WriteBytes(0x91c150, [0x04]);

    // Every ball pose shares this synthetic rolling stream. Starting at frame three proves
    // the same-pose knockback transition does not accidentally call InitializeAnimation.
    foreach (byte pose in new byte[]
    {
        SamusState.MorphBallGroundRightPose,
        SamusState.MorphBallFallingLeftPose,
        SamusState.SpringBallGroundRightPose,
        SamusState.SpringBallJumpLeftPose,
        SamusState.SpringBallFallingRightPose,
    })
    {
        WriteTestWord(bus, 0x91b010 + pose * 2, 0xc160);
    }
    bus.WriteBytes(0x91c160, [0x09, 0x09, 0x09, 0x09, 0x09, 0x09, 0xff]);

    // `$90:99D6` selects dry-air knockback magnitude 5.0000. Damage boost subsequently
    // calls Make_Samus_Jump, whose independent dry-air value is 4.E000. Both share the
    // same 0.2800 gravity record in this no-water/no-lava fixture.
    // `$90:99D6` is the selector's code address, while the named arrays themselves live at
    // `$90:9EE9/$9EEF`. Their non-adjacent placement is explicit in the symbol map.
    WriteTestWord(bus, 0x909ee9, 0x0005);
    WriteTestWord(bus, 0x909eef, 0x0000);
    WriteTestWord(bus, 0x909eb9, 0x0004);
    WriteTestWord(bus, 0x909ebf, 0xe000);
    WriteTestWord(bus, 0x909ea1, 0x2800);
    WriteTestWord(bus, 0x909ea7, 0x0000);

    // Knockback's type-$0A normal-air record begins at `$90:9FCD`. A small 0.4000
    // acceleration makes the first frame's direction and fixed-point displacement exact.
    WriteTestWord(bus, 0x909fcd, 0x0000);
    WriteTestWord(bus, 0x909fcf, 0x4000);
    WriteTestWord(bus, 0x909fd1, 0x0005);
    WriteTestWord(bus, 0x909fd3, 0x0000);
    WriteTestWord(bus, 0x909fd5, 0x0000);
    WriteTestWord(bus, 0x909fd7, 0x1000);

    // Type `$19` really indexes the same generic table rather than using a damage-boost
    // special case. Its entry may remain zero because no direction is held in the first
    // translated damage-boost frame below.
    for (int address = 0x90a081; address < 0x90a08d; address += 2)
        WriteTestWord(bus, address, 0);

    const int width = 16;
    const int height = 16;
    var foreground = new ushort[width * height];
    RoomLevelData empty = CreateRoom(
        width, height, foreground, new byte[foreground.Length]);

    var samus = new SamusState
    {
        Pose = SamusState.FacingRightNormalPose,
        XPosition = 96,
        YPosition = 96,
    };

    // A source to Samus's left publishes X direction one (move right). With no forward
    // input, `$91:EDB0` chooses up-right direction two and `$90:99D6` installs 5.0000.
    SamusKnockbackMovement.Start(bus, samus, controllerInput: 0, knockbackXDirection: 1);
    AssertEqual(SamusState.KnockbackRightPose, samus.Pose, "right-facing knockback pose");
    AssertEqual(2, samus.KnockbackDirection, "up-right knockback direction");
    AssertEqual(1, samus.KnockbackXDirection, "knockback X direction publication");
    AssertEqual(5, samus.KnockbackTimer, "enemy hurt timer publication");
    AssertEqual(1, samus.HurtFlashCounter,
        "knockback command starts shared hurt-flash palette counter");
    AssertTrue(samus.KnockbackActive, "special knockback handler installed");
    AssertEqual(5, samus.Kinematics.YSpeed, "knockback dry-air whole speed");
    AssertEqual(0, samus.Kinematics.YSubspeed, "knockback dry-air subspeed");

    // Intro Rinkas publish eleven in `$18AA` before the shared command-one initializer.
    // That initializer must consume the producer-owned value rather than replacing it with
    // ordinary enemy contact's five; otherwise the cinematic reaction is cut in half.
    var introTimedKnockback = new SamusState
    {
        Pose = SamusState.FacingRightNormalPose,
        XPosition = 96,
        YPosition = 96,
    };
    SamusKnockbackMovement.Start(
        bus,
        introTimedKnockback,
        controllerInput: 0,
        knockbackXDirection: 1,
        knockbackTimer: 11);
    AssertEqual(11, introTimedKnockback.KnockbackTimer,
        "intro Rinka preserves producer-owned eleven-frame knockback timer");
    AssertEqual(SamusState.KnockbackRightPose, introTimedKnockback.Pose,
        "intro Rinka enters visible humanoid hurt pose");

    KnockbackMovementResult hurtFrame = SamusKnockbackMovement.Step(bus, empty, samus, 0);
    // Movement consumes the current timer; gameplay state eight then calls `$A0:9169`
    // after drawing/room work. Keep that distinct owner visible in this direct subsystem test.
    samus.DecrementHurtTimers();
    AssertEqual(4, samus.KnockbackTimer, "first hurt frame decrements timer");
    AssertEqual(0x00004000, hurtFrame.Horizontal!.Value.AcceptedDisplacement,
        "knockback moves in bank-$A0 X direction");
    AssertEqual(unchecked((int)0xfffb0000), hurtFrame.Vertical!.Value.AcceptedDisplacement,
        "knockback moves by old 5.0000 vertical speed");
    AssertEqual(91, samus.YPosition, "knockback upward whole position");
    AssertEqual(4, samus.Kinematics.YSpeed, "knockback gravity next whole speed");
    AssertEqual(0xd800, samus.Kinematics.YSubspeed, "knockback gravity next subspeed");

    // Held forward selects down-right direction four. Place the radius-21 body flush with a
    // square floor so `$90:923F`'s no-speed down probe collides immediately. The special
    // handler clears live velocity after collision, while its result must retain the 5.0000
    // magnitude seen by `$91:F078` for hard-landing sound selection.
    var downForeground = new ushort[width * height];
    for (int x = 0; x < width; x++)
        downForeground[6 * width + x] = 0x8000;
    RoomLevelData floorLevel = CreateRoom(
        width, height, downForeground, new byte[downForeground.Length]);
    var downKnockback = new SamusState
    {
        Pose = SamusState.FacingRightNormalPose,
        XPosition = 96,
        YPosition = 75,
    };
    downKnockback.RefreshCollisionRadii(bus);
    SamusKnockbackMovement.Start(
        bus,
        downKnockback,
        controllerInput: (ushort)SnesButton.Right,
        knockbackXDirection: 0);
    KnockbackMovementResult downImpact = SamusKnockbackMovement.Step(
        bus, floorLevel, downKnockback, nmiFrameCounter: 0);
    AssertTrue(downImpact.Landed, "downward knockback collision publishes landing");
    AssertEqual(5, downImpact.ImpactYSpeed,
        "downward knockback retains pre-clear whole impact speed");
    AssertEqual(0, downImpact.ImpactYSubspeed,
        "downward knockback retains pre-clear fractional impact speed");
    AssertEqual(0, downKnockback.Kinematics.YSpeed,
        "downward knockback clears live whole speed after snapshot");

    // `$53` plus Left+Jump (`$0280`) selects `$50`. The pose-family crossing runs the native normal
    // input initializer, clears the special handler, and starts a fresh 4.E000 jump.
    SamusKnockbackMovement.ApplyDamageBoostTransition(
        bus,
        samus,
        SamusState.DamageBoostRightPose);
    AssertEqual(SamusState.DamageBoostRightPose, samus.Pose, "damage-boost entry pose");
    AssertEqual(0x19, samus.ReadMovementType(bus), "damage-boost movement type");
    AssertTrue(!samus.KnockbackActive, "damage boost restores normal handler");
    AssertEqual(0, samus.KnockbackDirection, "damage boost clears knockback direction");
    AssertEqual(0, samus.KnockbackTimer, "damage boost clears hurt timer");
    AssertEqual(4, samus.Kinematics.YSpeed, "damage boost fresh jump whole speed");
    AssertEqual(0xe000, samus.Kinematics.YSubspeed, "damage boost fresh jump subspeed");

    AerialMovementResult boostFrame = SamusAerialMovement.StepDamageBoost(
        bus,
        empty,
        samus,
        (ushort)SnesButton.A,
        nmiFrameCounter: 1);
    AssertEqual(unchecked((int)0xfffb2000), boostFrame.Vertical!.Value.AcceptedDisplacement,
        "damage boost reuses ordinary old-speed jumping movement");
    AssertEqual(4, samus.Kinematics.YSpeed, "damage boost gravity next whole speed");
    AssertEqual(0xb800, samus.Kinematics.YSubspeed, "damage boost gravity next subspeed");

    // Jump alone (`$0080`) exits right-facing `$50` to neutral-jump `$4D`. This is an ordinary pose
    // change inside movement type two, so the already-live 16.16 trajectory must survive.
    ushort preservedYSpeed = samus.Kinematics.YSpeed;
    ushort preservedYSubspeed = samus.Kinematics.YSubspeed;
    SamusKnockbackMovement.ApplyDamageBoostPoseTransition(
        bus,
        samus,
        SamusState.NeutralJumpRightPose);
    AssertEqual(SamusState.NeutralJumpRightPose, samus.Pose, "damage-boost neutral exit pose");
    AssertEqual(preservedYSpeed, samus.Kinematics.YSpeed, "damage-boost exit preserves whole Y speed");
    AssertEqual(preservedYSubspeed, samus.Kinematics.YSubspeed, "damage-boost exit preserves Y subspeed");

    // Damage-boost pose definitions store shot direction `$FF`. `$91:E95D` handles that
    // sentinel before the aimed-landing table and selects ordinary `$A4/$A5` from X
    // direction. `$50` intentionally stores reversed X direction four, producing `$A5`;
    // this guards the exact real-ROM landing branch found by the scripted run.
    var boostLanding = new SamusState
    {
        Pose = SamusState.DamageBoostRightPose,
        XPosition = 96,
        YPosition = 96,
    };
    boostLanding.RefreshCollisionRadii(bus);
    boostLanding.ApplyAerialLanding(bus, wasSpinning: false);
    AssertEqual(SamusState.NormalLandingLeftPose, boostLanding.Pose,
        "damage-boost FF shot direction selects ordinary metadata-direction landing");
    AssertEqual(94, boostLanding.YPosition,
        "damage-boost landing radius expansion preserves feet");

    // A separate uninterrupted fixture proves the timer owns the special handler's exact
    // five movement frames. The sixth call chooses falling `$29` without another move.
    var expires = new SamusState
    {
        Pose = SamusState.FacingRightNormalPose,
        XPosition = 96,
        YPosition = 96,
    };
    SamusKnockbackMovement.Start(bus, expires, 0, knockbackXDirection: 1);
    for (int frame = 0; frame < 5; frame++)
    {
        AssertTrue(!SamusKnockbackMovement.Step(bus, empty, expires, (ushort)frame).Ended,
            $"hurt movement frame {frame + 1} remains active");
        expires.DecrementHurtTimers();
    }
    ushort humanoidYBeforeCompletion = expires.YPosition;
    KnockbackMovementResult expired = SamusKnockbackMovement.Step(bus, empty, expires, 5);
    AssertTrue(expired.Ended, "zero hurt timer ends special handler");
    AssertEqual(SamusState.FallingRightPose, expires.Pose, "expired right knockback selects falling right");
    AssertTrue(!expires.KnockbackActive, "expired knockback restores normal handler");
    AssertEqual(0, expires.KnockbackDirection, "expired knockback clears direction");

    // `$90:DF15` republishes a ball's current pose rather than substituting `$53/$54`.
    // `$91:EE27` then ignores the hit side and held-forward rule when selecting vertical
    // direction, while `$90:8EDF` still uses the enemy-produced X side for horizontal travel.
    foreach ((byte pose, ushort hitSide, ushort expectedDirection) in new[]
    {
        (SamusState.MorphBallGroundRightPose, (ushort)0, (ushort)2),
        (SamusState.MorphBallFallingLeftPose, (ushort)1, (ushort)1),
        (SamusState.SpringBallGroundRightPose, (ushort)0, (ushort)2),
        (SamusState.SpringBallJumpLeftPose, (ushort)1, (ushort)1),
        (SamusState.SpringBallFallingRightPose, (ushort)0, (ushort)2),
    })
    {
        var ball = new SamusState
        {
            Pose = pose,
            XPosition = 96,
            YPosition = 96,
            BombJumpDirection = 0x0802,
            MorphBallBounceState = 0x0602,
        };
        ball.RefreshCollisionRadii(bus);
        ball.InitializeAnimation(bus, initialFrame: 3);
        ball.HorizontalSpeed.ContactDamageIndex = 3;
        ushort preservedFrame = ball.AnimationFrame;
        ushort preservedTimer = ball.AnimationFrameTimer;

        SamusKnockbackMovement.Start(
            bus,
            ball,
            controllerInput: (ushort)(SnesButton.Left | SnesButton.Right),
            knockbackXDirection: hitSide);
        AssertEqual(pose, ball.Pose, $"morphed type ${ball.ReadMovementType(bus):X2} retains pose");
        AssertEqual(expectedDirection, ball.KnockbackDirection,
            $"morphed pose ${pose:X2} chooses direction from facing only");
        AssertEqual(preservedFrame, ball.AnimationFrame,
            $"morphed pose ${pose:X2} retains rolling animation frame");
        AssertEqual(preservedTimer, ball.AnimationFrameTimer,
            $"morphed pose ${pose:X2} retains rolling animation timer");
        AssertEqual(0, ball.BombJumpDirection,
            $"morphed pose ${pose:X2} start clears pending bomb jump");
        AssertEqual(0, ball.HorizontalSpeed.ContactDamageIndex,
            $"morphed pose ${pose:X2} start clears contact damage");
        AssertEqual(0x0602, ball.MorphBallBounceState,
            $"morphed pose ${pose:X2} start leaves bounce state until completion");
    }

    // Let a sixth ball fixture reach the shared `$91:F31D` completion handler. Unlike the
    // humanoid path it must keep both pose and animation, clear bounce/velocity, and publish
    // downward direction two so normal ball physics resumes on the following frame.
    var ballExpires = new SamusState
    {
        Pose = SamusState.MorphBallGroundRightPose,
        XPosition = 96,
        YPosition = 96,
        MorphBallBounceState = 2,
    };
    ballExpires.RefreshCollisionRadii(bus);
    ballExpires.InitializeAnimation(bus, initialFrame: 3);
    SamusKnockbackMovement.Start(
        bus,
        ballExpires,
        controllerInput: (ushort)SnesButton.Right,
        knockbackXDirection: 0);
    ushort retainedBallFrame = ballExpires.AnimationFrame;
    ushort retainedBallTimer = ballExpires.AnimationFrameTimer;
    for (int frame = 0; frame < 5; frame++)
    {
        SamusKnockbackMovement.Step(bus, empty, ballExpires, unchecked((ushort)frame));
        ballExpires.DecrementHurtTimers();
    }
    KnockbackMovementResult ballExpired = SamusKnockbackMovement.Step(
        bus,
        empty,
        ballExpires,
        nmiFrameCounter: 5);
    AssertTrue(ballExpired.Ended, "zero hurt timer ends morphed special handler");
    AssertEqual(SamusState.MorphBallGroundRightPose, ballExpires.Pose,
        "expired morphed knockback retains current ball pose");
    AssertEqual(retainedBallFrame, ballExpires.AnimationFrame,
        "expired morphed knockback retains rolling frame");
    AssertEqual(retainedBallTimer, ballExpires.AnimationFrameTimer,
        "expired morphed knockback retains rolling timer");
    AssertEqual(0, ballExpires.MorphBallBounceState,
        "expired morphed knockback clears bounce state");
    AssertEqual(0, ballExpires.Kinematics.YSpeed,
        "expired morphed knockback clears whole Y speed");
    AssertEqual(0, ballExpires.Kinematics.YSubspeed,
        "expired morphed knockback clears fractional Y speed");
    AssertEqual(2, ballExpires.Kinematics.YDirection,
        "expired morphed knockback resumes with direction two");

    // The same command-one cleanup also corrects the existing humanoid route: `$53` radius
    // 21 becomes `$29` radius 19, so center Y moves down two pixels to preserve the feet.
    AssertEqual(0, expires.Kinematics.YSpeed,
        "expired humanoid knockback clears whole Y speed");
    AssertEqual(0, expires.Kinematics.YSubspeed,
        "expired humanoid knockback clears fractional Y speed");
    AssertEqual(2, expires.Kinematics.YDirection,
        "expired humanoid knockback publishes falling direction two");
    AssertEqual(unchecked((ushort)(humanoidYBeforeCompletion + 2)), expires.YPosition,
        "expired humanoid knockback aligns radius-19 falling body to radius-21 feet");

    Console.WriteLine("  Samus knockback: humanoid/ball starts, timer, 16.16 hurt arc, same-pose animation, cleanup, and damage-boost handoff agree.");
}

/// <summary>
/// Exercises the bank-$9B/$94 connected-pendulum order with deliberately tiny ROM tables.
/// Hard-coded positions and velocities make this independent of production helper formulas.
/// </summary>
}
