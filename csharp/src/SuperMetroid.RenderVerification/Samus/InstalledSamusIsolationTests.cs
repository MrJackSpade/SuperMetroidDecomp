using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Palettes;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Death;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.Core.Samus.Drawing;
namespace SuperMetroid.RenderVerification.Samus;

using SuperMetroid.Core.Rendering;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Progression;

/// <summary>
/// Finite, source-selected production-owner fixtures for #541. These compare stock
/// and edited presentation, not a playthrough or a search for cartridge access.
/// </summary>
internal sealed class InstalledSamusIsolationTests
{
    /// <summary>Stock and edited Samus body art used by paired production actors.</summary>
    private readonly InstalledSamusArtworkFixture artwork;
    /// <summary>Stock and edited suit colors selected by the actors' presentation owners.</summary>
    private readonly StockEditedPair<SamusSuitColorCatalog> suits;
    /// <summary>Stock and edited Speed Booster, Screw Attack, and shinespark cycle colors.</summary>
    private readonly StockEditedPair<SamusFullBodyCycleColorCatalog> cycles;
    /// <summary>Stock and edited Crystal Flash palette phases.</summary>
    private readonly StockEditedPair<CrystalFlashColorCatalog> crystal;
    /// <summary>Installed beam palette data shared by both actor fixtures.</summary>
    private readonly BeamPaletteCatalog beams;
    /// <summary>Number of stock and edited mechanics comparisons.</summary>
    private int comparisons;
    /// <summary>Number of production Samus draws performed by paired actors.</summary>
    private int draws;
    /// <summary>Draws in which installed spritemap edits changed OAM.</summary>
    private int changedOam;
    /// <summary>Draws in which installed character edits changed VRAM.</summary>
    private int changedTiles;
    /// <summary>Comparisons in which installed palette edits changed CGRAM.</summary>
    private int changedColors;
    /// <summary>Bounded actor setups run under different movement and sequence conditions.</summary>
    private int scenarios;

    /// <summary>Loads the selected stock and edited art catalogs from the installation for paired Samus runs.</summary>
    private InstalledSamusIsolationTests(InstalledSamusArtworkFixture artwork, string root)
    {
        this.artwork = artwork;
        var source = new GameInstallation(Path.GetFullPath(root));
        suits = SamusIsolationArtworkEdits.Colors(source, artwork.Installation,
            SamusSuitColorFormat.FileName, SamusSuitColorCatalog.Load);
        cycles = SamusIsolationArtworkEdits.Colors(source, artwork.Installation,
            SamusFullBodyCycleColorFormat.FileName, SamusFullBodyCycleColorCatalog.Load);
        crystal = SamusIsolationArtworkEdits.Colors(source, artwork.Installation,
            CrystalFlashColorFormat.FileName, CrystalFlashColorCatalog.Load);
        using var stream = File.OpenRead(Path.Combine(source.ProjectileDirectory, BeamPaletteDefinitions.FileName));
        beams = BeamPaletteCatalog.Load(stream);
    }

    /// <summary>Checks stock and edited Samus state across movement, collision, and special sequences while requiring visible art differences.</summary>
    internal static void Run(InstalledSamusArtworkFixture artwork, string root)
    {
        var tests = new InstalledSamusIsolationTests(artwork, root);
        CheckSnapshotSensitivity();
        tests.CheckMovement();
        tests.CheckPoseCollision();
        tests.CheckSpecialSequences();
        Require(tests.changedOam > 0 && tests.changedTiles > 0 && tests.changedColors > 0,
            "The fixture did not exercise observable OAM, tile and color edits.");
        Console.WriteLine($"Samus visual isolation: {tests.scenarios} finite scenarios, " +
            $"{tests.comparisons} complete mutable-state comparisons, {tests.draws} production draws; " +
            $"{tests.changedOam} changed OAM, {tests.changedTiles} changed VRAM and " +
            $"{tests.changedColors} changed CGRAM observations. No ROM, import or player data is used.");
    }

    /// <summary>Creates one source-selected pair in a constructed room with optional liquid or low ceiling.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native OAM tables")]
    private Pair Create(SamusPoseId pose = SamusPoseId.FacingRightNormalPose,
        SamusEquipmentFlags equipment = SamusEquipmentFlags.None, SamusLiquidMedium medium = SamusLiquidMedium.Air, bool lowCeiling = false)
    {
        scenarios++;
        return new Pair(this, pose, equipment, medium, lowCeiling);
    }

    /// <summary>One Samus simulation with its own mutable memory, room, color memory, and installed art.</summary>
    [ArchitectureReview("context-bag", "A test fixture whose purpose is to carry one isolated Samus simulation's owners side by side with its pair.")]
    private sealed class Actor
    {
        /// <summary>Cartridge-free mutable bus used by this actor's production owners.</summary>
        internal readonly SuperMetroidAddressSpace Memory = SuperMetroidAddressSpace.CreateWithoutCartridge();
        /// <summary>Gameplay state whose mechanics must match the paired actor.</summary>
        internal readonly SamusState Samus;
        /// <summary>Constructed floor and optional ceiling used for both collision fixtures.</summary>
        internal readonly RoomLevelData Level;
        /// <summary>Color memory receiving suit and sequence palette updates.</summary>
        internal readonly SnesCgram Colors = new();
        /// <summary>Tile memory receiving installed Samus transfers.</summary>
        internal readonly SnesVram Tiles = new();
        /// <summary>Selected body, arm-cannon, death, and spritemap art.</summary>
        internal readonly SamusBodyArtworkCatalog Body;
        /// <summary>Selected base suit palette catalog.</summary>
        internal readonly SamusSuitColorCatalog Suits;
        /// <summary>Selected full-body cycle color catalog.</summary>
        internal readonly SamusFullBodyCycleColorCatalog Cycles;
        /// <summary>Selected Crystal Flash color catalog.</summary>
        internal readonly CrystalFlashColorCatalog Crystal;

        /// <summary>Builds a cartridge-free Samus actor on the fixture floor with the selected visual catalog set.</summary>
        internal Actor(InstalledSamusIsolationTests tests, bool edited, SamusPoseId pose,
            SamusEquipmentFlags equipment, SamusLiquidMedium medium, bool lowCeiling)
        {
            Body = edited ? tests.artwork.Edited : tests.artwork.Stock;
            Suits = edited ? tests.suits.Edited : tests.suits.Stock;
            Cycles = edited ? tests.cycles.Edited : tests.cycles.Stock;
            Crystal = edited ? tests.crystal.Edited : tests.crystal.Stock;
            Samus = new() { Pose = pose, Position = FixedPosition<RoomPixels>.FromWhole(new(128, 363)),
                EquippedItems = equipment, Health = 99, MaxHealth = 299,
                Presentation = { SuitColors = Suits, FullBodyCycleColors = Cycles } };
            Samus.Presentation.TileTransfers.BindBodyFixture(Body);
            Samus.Presentation.TileTransfers.BindStandardObjects(SpriteFrameTestArtwork.StandardObjects);
            Samus.ArmCannon.Artwork = Body.ArmCannon;
            SamusPoseMetadata.RefreshCollisionRadii(Samus);
            Samus.Position = Samus.Position.WithWholeY((ushort)(FloorY - Samus.Kinematics.Radius.Y));
            SamusPresentation.InitializeAnimation(Samus);
            Samus.CommitPoseHistory();
            if (medium == SamusLiquidMedium.Water) Samus.LiquidPhysics.ConfigureWater(0);
            else if (medium == SamusLiquidMedium.LavaOrAcid) Samus.LiquidPhysics.ConfigureLavaAcid(0);
            Samus.Presentation.LoadSuitPalette(Samus, Colors);
            var blocks = new ushort[RoomWidth * RoomHeight];
            for (int x = 0; x < RoomWidth; x++)
            {
                blocks[FloorRow * RoomWidth + x] = (int)RoomCollisionType.SolidBlock << 12;
                if (lowCeiling) blocks[(FloorRow - 3) * RoomWidth + x] = (int)RoomCollisionType.SolidBlock << 12;
            }
            Level = new(new(RoomWidth, RoomHeight),
                new RoomLevelPlanes(blocks, new byte[blocks.Length], new ushort[blocks.Length]), new byte[8]);
        }
    }

    /// <summary>Runs each operation on stock and edited actors and compares mechanics after every step.</summary>
    private sealed class Pair
    {
        /// <summary>Accumulates scenario and visible-change counts in the enclosing fixture.</summary>
        private readonly InstalledSamusIsolationTests tests;
        /// <summary>Actor using unedited installation art.</summary>
        internal Actor Stock { get; }
        /// <summary>Actor using the paired visual overrides.</summary>
        internal Actor Edited { get; }
        /// <summary>Creates both actors from the same initial pose, equipment, medium, and room geometry.</summary>
        internal Pair(InstalledSamusIsolationTests tests, SamusPoseId pose,
            SamusEquipmentFlags equipment, SamusLiquidMedium medium, bool lowCeiling)
        {
            this.tests = tests;
            Stock = new(tests, false, pose, equipment, medium, lowCeiling);
            Edited = new(tests, true, pose, equipment, medium, lowCeiling);
            Check("initialization");
        }

        /// <summary>Applies an operation to both actors, compares mutable mechanics, then optionally draws them.</summary>
        internal void Apply(string context, Action<Actor> operation, bool draw = true)
        {
            operation(Stock); operation(Edited);
            Check(context);
            if (draw) Draw(context);
        }

        /// <summary>Compares a returned gameplay result as well as state and optional presentation.</summary>
        internal T Apply<T>(string context, Func<Actor, T> operation, bool draw = true)
        {
            T expected = operation(Stock), actual = operation(Edited);
            Require(EqualityComparer<T>.Default.Equals(expected, actual), context + ": result changed");
            Check(context);
            if (draw) Draw(context);
            return expected;
        }

        /// <summary>Checks Samus, WRAM, SRAM, and collision planes while recording color differences.</summary>
        internal void Check(string context)
        {
            new SamusMechanicsSnapshot(Stock.Samus).RequireSame(new(Edited.Samus), context);
            Require(Stock.Memory.WorkRam.SequenceEqual(Edited.Memory.WorkRam), context + ": mutable memory changed");
            Require(Stock.Level.ForegroundEntries.Span.SequenceEqual(Edited.Level.ForegroundEntries.Span),
                context + ": collision room changed");
            tests.comparisons++;
            if (!new CgramPalettes(Stock.Colors).ImageEquals(new CgramPalettes(Edited.Colors))) tests.changedColors++;
        }

        /// <summary>Draws both actors, checks drawing leaves mechanics intact, and records OAM or VRAM changes.</summary>
        [PpuLayoutBoundary("exact hardware test: checks native OAM tables")]
        internal void Draw(string context, ushort frame = 0)
        {
            var beforeStock = new SamusMechanicsSnapshot(Stock.Samus);
            var beforeEdited = new SamusMechanicsSnapshot(Edited.Samus);
            var aFrame = new SpriteFrameBuffer(); var bFrame = new SpriteFrameBuffer();
            aFrame.BeginFrame(); bFrame.BeginFrame();
            ushort cameraX = (ushort)Math.Max(0, Stock.Samus.Position.X.Whole - 128);
            ushort cameraY = (ushort)Math.Max(0, Stock.Samus.Position.Y.Whole - 128);
            bool visible = Stock.Samus.Presentation.Draw(Stock.Samus, aFrame, new(cameraX, cameraY), frame);
            Require(visible == Edited.Samus.Presentation.Draw(Edited.Samus, bFrame, new(cameraX, cameraY), frame),
                context + ": visibility changed");
            Stock.Samus.Presentation.TileTransfers.TransferToVram(Stock.Tiles);
            Edited.Samus.Presentation.TileTransfers.TransferToVram(Edited.Tiles);
            beforeStock.RequireSame(new(Stock.Samus), context + ": stock draw mutated mechanics");
            beforeEdited.RequireSame(new(Edited.Samus), context + ": edited draw mutated mechanics");
            Check(context + " after draw");
            tests.draws++;
            OamBuffer a = aFrame.FinalizeAsOam(), b = bFrame.FinalizeAsOam();
            if (!a.LowTable.SequenceEqual(b.LowTable) || !a.HighTable.SequenceEqual(b.HighTable)) tests.changedOam++;
            if (!Stock.Tiles.ImageEquals(Edited.Tiles)) tests.changedTiles++;
        }
    }

    /// <summary>Constructed room width in blocks for the paired Samus fixtures.</summary>
    private const int RoomWidth = 32;
    /// <summary>Constructed room height in blocks for the paired Samus fixtures.</summary>
    private const int RoomHeight = 32;
    /// <summary>Solid floor row used by jump and posture scenarios.</summary>
    private const int FloorRow = 24;
    /// <summary>Floor's pixel coordinate used to place Samus by her current collision radius.</summary>
    private const int FloorY = FloorRow * 16;

    /// <summary>Confirms mechanics snapshots detect changes to position, animation, radius, and private sequence state.</summary>
    private static void CheckSnapshotSensitivity()
    {
        var samus = InstalledFixtures.CreateSamus();
        var baseline = new SamusMechanicsSnapshot(samus);
        MustDiffer(() => samus.Position += new Offset16<RoomPixels>(1, 0), "whole position");
        samus = new(); baseline = new(samus);
        MustDiffer(() => samus.Kinematics.Position = samus.Kinematics.Position.WithFractionX(unchecked((ushort)(samus.Kinematics.Position.X.Fraction + 1))), "fractional position");
        samus = new(); baseline = new(samus);
        MustDiffer(() => samus.AnimationFrame++, "animation frame");
        samus = new(); baseline = new(samus);
        MustDiffer(() => samus.Kinematics.Radius = samus.Kinematics.Radius.WithY(unchecked((ushort)(samus.Kinematics.Radius.Y + 1))), "collision radius");
        samus = PrepareFlash(new());
        var sequence = new SamusMechanicsSnapshot(samus.CrystalFlash);
        Require(samus.CrystalFlash.TryBegin(samus, FlashChord),
            "Sensitivity setup could not activate the sequence");
        try { sequence.RequireSame(new(samus.CrystalFlash), "private sequence sensitivity"); }
        catch (InvalidOperationException) { return; }
        throw new InvalidOperationException("Snapshot ignored changed private sequence state");

        void MustDiffer(Action change, string field)
        {
            change();
            try { baseline.RequireSame(new(samus), "sensitivity"); }
            catch (InvalidOperationException) { return; }
            throw new InvalidOperationException("Snapshot ignored changed " + field);
        }
    }

    /// <summary>Stops a source-isolation scenario with its specific gameplay or presentation mismatch.</summary>
    private static void Require(bool condition, string message)
    { if (!condition) throw new InvalidOperationException(message); }

    /// <summary>
    /// The drained draw offsets have their own byte-indexed table. Exercise their
    /// real animation-triggered falling owner rather than assigning a plausible pose.
    /// </summary>
    private void CheckDrained(bool left)
    {
        Pair pair = Create(left ? SamusPoseId.FacingLeftNormalPose : SamusPoseId.FacingRightNormalPose);
        string context = $"drained left={left}";
        pair.Apply(context + " airborne setup", actor =>
        {
            actor.Samus.Position -= new Offset16<RoomPixels>(0, 80);
            SamusAerialMovement.ConfigureEnvironmentGravity(actor.Samus);
            actor.Samus.Drained.LetFall(actor.Samus);
        });
        bool fell = false;
        int frames = 0;
        while (pair.Stock.Samus.Drained.Phase != DrainedSamusPhase.OnFloor && frames < 180)
        {
            ushort frame = (ushort)frames++;
            pair.Apply(context + $" fall {frame}", actor =>
            {
                if (actor.Samus.Drained.Phase == DrainedSamusPhase.Falling)
                    actor.Samus.Drained.StepFalling(actor.Level, actor.Samus, frame);
                Animate(actor, frame);
                actor.Samus.Drained.UpdatePalette(actor.Colors, actor.Samus.EquippedItems, actor.Suits);
            });
            fell |= pair.Stock.Samus.Drained.Phase == DrainedSamusPhase.Falling;
        }
        Require(fell && pair.Stock.Samus.Drained.Phase == DrainedSamusPhase.OnFloor,
            context + ": animation command must install falling and terrain collision must end it");
        pair.Apply(context + " stand", actor => actor.Samus.Drained.PutStanding(actor.Samus));
        for (ushort frame = 0; frame < 24; frame++)
            pair.Apply(context + $" standing {frame}", actor => Animate(actor, frame));
        pair.Apply(context + " crouch", actor => actor.Samus.Drained.PutCrouchingOrFalling(actor.Samus));
        for (ushort frame = 0; frame < 24; frame++)
            pair.Apply(context + $" crouching {frame}", actor => Animate(actor, frame));
        pair.Apply(context + " release", actor => actor.Samus.Drained.Release(actor.Samus));
        CompleteAnimation(pair, left ? SamusPoseId.FacingLeftNormalPose : SamusPoseId.FacingRightNormalPose,
            context + " released");
        Require(pair.Stock.Samus.Drained.Phase == DrainedSamusPhase.Inactive,
            context + ": compiled terminal animation must release the drained owner");
        Console.WriteLine($"  {context}: {frames} matching fall calls plus stand/crouch/release animation; edited draw offsets stay cosmetic.");
    }

    /// <summary>Compares running, turning, jumping, posture changes, and knockback across air and liquids.</summary>
    private void CheckMovement()
    {
        foreach (SamusLiquidMedium medium in new[] { SamusLiquidMedium.Air,
            SamusLiquidMedium.Water, SamusLiquidMedium.LavaOrAcid })
        foreach (bool left in new[] { false, true })
        {
            Pair pair = Create(left ? SamusPoseId.FacingLeftNormalPose : SamusPoseId.FacingRightNormalPose,
                medium: medium);
            string label = $"medium {medium}, facing {(left ? "left" : "right")}";
            pair.Apply(label + " enter run", actor =>
            {
                if (left) SamusGroundedPoseTransitions.ApplyStandingLeftToRunningLeft(actor.Samus);
                else SamusGroundedPoseTransitions.ApplyStandingRightToRunningRight(actor.Samus);
            });
            ushort initialX = pair.Stock.Samus.Position.X.Whole;
            for (ushort frame = 0; frame < 24; frame++)
                pair.Apply(label + $" run {frame}", actor =>
                {
                    SamusPoseMetadata.RefreshCollisionRadii(actor.Samus);
                    GroundedMovementResult result = left
                        ? SamusGroundedMovement.StepRunning(actor.Level, actor.Samus, frame,
                            (SnesButton.Left | SnesButton.B))
                        : SamusGroundedMovement.StepRunning(actor.Level, actor.Samus, frame,
                            (SnesButton.Right | SnesButton.B));
                    Animate(actor, frame);
                    return result;
                });
            Require(pair.Stock.Samus.Position.X.Whole != initialX, label + ": run fixture never moved");
            pair.Apply(label + " turn admission", actor => SamusGroundedPoseTransitions.ApplyGroundedTurn(actor.Samus, left ? SamusPoseId.TurningLeftToRightPose : SamusPoseId.TurningRightToLeftPose));
            int turnFrames = 0;
            while (SamusPoseMetadata.ReadMovementKind(pair.Stock.Samus) == SamusMovementType.TurningOnGround && turnFrames < 120)
            {
                ushort frame = (ushort)turnFrames++;
                pair.Apply(label + $" turn {frame}", actor =>
                {
                    var result = SamusGroundedMovement.StepTurningOnGround(actor.Level, actor.Samus, frame);
                    Animate(actor, frame);
                    return result;
                });
            }
            Require(pair.Stock.Samus.Pose == (left ? SamusPoseId.FacingRightNormalPose : SamusPoseId.FacingLeftNormalPose),
                label + ": authored turn did not complete");
            CheckJump(medium, left, highJump: false);
            CheckJump(medium, left, highJump: true);
            CheckPosture(medium, left);
            Pair hurt = Create(left ? SamusPoseId.FacingLeftNormalPose : SamusPoseId.FacingRightNormalPose, medium: medium);
            Require(hurt.Apply(label + " hit admission", actor => SamusKnockbackMovement.Start(actor.Samus, 0, new KnockbackLaunch { KnockbackXDirection = left ? KnockbackHorizontalDirection.Right : KnockbackHorizontalDirection.Left }, level: actor.Level)), label + ": hurt was not admitted");
            for (ushort frame = 0; frame < 8; frame++)
                hurt.Apply(label + $" knockback {frame}", actor =>
                {
                    SamusPoseMetadata.RefreshCollisionRadii(actor.Samus);
                    var result = SamusKnockbackMovement.Step(actor.Level, actor.Samus, frame);
                    Animate(actor, frame);
                    return result;
                });
        }
    }

    /// <summary>Requires a real rise and floor landing with selected facing, liquid, and Hi-Jump equipment.</summary>
    private void CheckJump(SamusLiquidMedium medium, bool left, bool highJump)
    {
        Pair pair = Create(left ? SamusPoseId.FacingLeftNormalPose : SamusPoseId.FacingRightNormalPose,
            equipment: highJump ? SamusEquipmentFlags.HiJumpBoots : SamusEquipmentFlags.None, medium: medium);
        string label = $"jump medium {medium}, left={left}, high={highJump}";
        ushort startY = pair.Stock.Samus.Position.Y.Whole, minimumY = startY;
        pair.Apply(label + " admission", actor => SamusAerialPoseTransitions.ApplyOrdinaryJumpTransition(actor.Samus, left ? SamusPoseId.NeutralJumpTransitionLeftPose : SamusPoseId.NeutralJumpTransitionRightPose));
        bool landed = false;
        for (ushort frame = 0; frame < 240 && !landed; frame++)
        {
            AerialMovementResult result = pair.Apply(label + $" frame {frame}", actor =>
            {
                SamusPoseMetadata.RefreshCollisionRadii(actor.Samus);
                var movement = SamusAerialMovement.StepNormalJump(actor.Level, actor.Samus,
                    SnesButton.A, frame);
                Animate(actor, frame);
                if (movement.Landed) Require(SamusPoseTransitions.TryApplyAerialLanding(actor.Samus, 
                    actor.Level, wasSpinning: false, controllerInput: SnesButton.A,
                    nmiFrameCounter: frame), label + ": landing rejected");
                return movement;
            });
            minimumY = Math.Min(minimumY, pair.Stock.Samus.Position.Y.Whole);
            landed = result.Landed;
        }
        Require(landed && minimumY < startY, label + ": must rise and collide with the floor, not merely stay equal");
    }

    /// <summary>Completes crouch, morph, and unmorph animation transitions under the selected liquid physics.</summary>
    private void CheckPosture(SamusLiquidMedium medium, bool left)
    {
        Pair pair = Create(left ? SamusPoseId.FacingLeftNormalPose : SamusPoseId.FacingRightNormalPose,
            SamusEquipmentFlags.MorphBall, medium);
        string label = $"posture medium {medium}, left={left}";
        Require(pair.Apply(label + " crouch", actor => SamusPosturePoseTransitions.TryApplyPostureTransition(actor.Samus, 
            actor.Level, left ? SamusPoseId.CrouchingTransitionLeftPose : SamusPoseId.CrouchingTransitionRightPose, 0)),
            label + ": crouch not admitted");
        CompleteAnimation(pair, left ? SamusPoseId.CrouchingLeftPose : SamusPoseId.CrouchingRightPose, label + " crouching");
        Require(pair.Apply(label + " morph", actor => SamusPosturePoseTransitions.TryApplyMorphTransition(actor.Samus, 
            actor.Level, left ? SamusPoseId.MorphingTransitionLeftPose : SamusPoseId.MorphingTransitionRightPose, 0)),
            label + ": morph not admitted");
        CompleteAnimation(pair, left ? SamusPoseId.MorphBallGroundLeftPose : SamusPoseId.MorphBallGroundRightPose, label + " morphing");
        Require(pair.Apply(label + " unmorph", actor => SamusPosturePoseTransitions.TryApplyMorphTransition(actor.Samus, 
            actor.Level, left ? SamusPoseId.UnmorphingTransitionLeftPose : SamusPoseId.UnmorphingTransitionRightPose, 0)),
            label + ": unmorph not admitted");
        CompleteAnimation(pair, left ? SamusPoseId.CrouchingLeftPose : SamusPoseId.CrouchingRightPose, label + " unmorphing");
    }

    /// <summary>Checks standing from crouch is admitted only when the constructed ceiling leaves room.</summary>
    private void CheckPoseCollision()
    {
        foreach (bool blocked in new[] { false, true })
        foreach (bool left in new[] { false, true })
        {
            Pair pair = Create(left ? SamusPoseId.CrouchingLeftPose : SamusPoseId.CrouchingRightPose,
                lowCeiling: blocked);
            bool accepted = pair.Apply($"expand pose left={left}, blocked={blocked}", actor =>
                SamusPosturePoseTransitions.TryApplyDirectCrouchToStandingTransition(actor.Samus, actor.Level,
                    left ? SamusPoseId.FacingLeftNormalPose : SamusPoseId.FacingRightNormalPose, 0));
            Require(accepted != blocked, "Synthetic ceiling did not exercise the expected eligibility decision");
        }
    }

    /// <summary>Runs production Samus animation and commits any verified pose transition.</summary>
    private static void Animate(Actor actor, ushort frame)
    {
        SamusPresentation.AnimateNoFx(actor.Samus, new SamusAnimationStep { Random = new CartridgeRandom(), NmiFrameCounter = frame });
        if (SamusPoseTransitions.ApplyPendingVerifiedAnimationTransition(actor.Samus)) actor.Samus.CommitPoseHistory();
    }

    /// <summary>Advances both animations to an authored target pose within the bounded transition lifetime.</summary>
    private static void CompleteAnimation(Pair pair, SamusPoseId target, string context)
    {
        for (ushort frame = 0; frame < 120 && pair.Stock.Samus.Pose != target; frame++)
            pair.Apply(context + $" frame {frame}", actor =>
            {
                SamusPoseMetadata.RefreshCollisionRadii(actor.Samus);
                Animate(actor, frame);
            });
        Require(pair.Stock.Samus.Pose == target, context + ": authored animation transition did not finish");
    }

    /// <summary>Controller chord used to enter the Crystal Flash sequence in this fixture.</summary>
    private const SnesButton FlashChord = (SnesButton.Down | SnesButton.L | SnesButton.R | SnesButton.X);

    /// <summary>Checks Flash, shine, suit pickup, death, reserve refill, and drained poses with installed art.</summary>
    private void CheckSpecialSequences()
    {
        foreach (bool left in new[] { false, true }) CheckCrystalFlash(left);
        CheckStoredShine();
        foreach (SamusPoseId pose in new[] { SamusPoseId.ShinesparkHorizontalRightPose,
            SamusPoseId.ShinesparkHorizontalLeftPose, SamusPoseId.ShinesparkVerticalRightPose,
            SamusPoseId.ShinesparkVerticalLeftPose, SamusPoseId.ShinesparkDiagonalRightPose,
            SamusPoseId.ShinesparkDiagonalLeftPose }) CheckShinespark(pose);
        foreach (SamusSuitPickupKind kind in Enum.GetValues<SamusSuitPickupKind>()) CheckSuitPickup(kind);
        foreach (SamusEquipmentFlags suit in new[] { SamusEquipmentFlags.None, SamusEquipmentFlags.VariaSuit,
            SamusEquipmentFlags.GravitySuit })
        foreach (SamusPoseId source in new[] { SamusPoseId.FacingRightNormalPose, SamusPoseId.MorphBallGroundLeftPose,
            SamusPoseId.SpinJumpRightPose }) CheckDeath(source, suit);
        CheckReserve();
        foreach (bool left in new[] { false, true }) CheckDrained(left);
    }

    /// <summary>Seeds the health and ammo prerequisites for a real Crystal Flash admission.</summary>
    private static SamusState PrepareFlash(SamusState samus)
    {
        samus.Health = 1; samus.MaxHealth = 1599;
        samus.Missiles = 10; samus.SuperMissiles = 10; samus.PowerBombs = 10;
        return samus;
    }

    /// <summary>Completes Crystal Flash and checks movement, all ammo drains, palette frames, and pose release.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots")]
    private void CheckCrystalFlash(bool left)
    {
        Pair pair = Create(left ? SamusPoseId.FacingLeftNormalPose : SamusPoseId.FacingRightNormalPose);
        string context = $"Crystal Flash left={left}";
        pair.Apply(context + " setup", actor => { PrepareFlash(actor.Samus); });
        Require(!pair.Apply(context + " rejected chord", actor => actor.Samus.CrystalFlash.TryBegin(
            actor.Samus, (FlashChord | SnesButton.A))), "Extra Flash input must be rejected");
        ushort initialY = pair.Stock.Samus.Position.Y.Whole;
        Require(pair.Apply(context + " admission", actor => actor.Samus.CrystalFlash.TryBegin(
            actor.Samus, FlashChord)), "Flash was not admitted");
        int frames = 0;
        while (pair.Stock.Samus.CrystalFlash.Phase != CrystalFlashPhase.Inactive && frames < 360)
        {
            ushort frame = (ushort)frames++;
            pair.Apply(context + $" frame {frame}", actor =>
            {
                var result = actor.Samus.CrystalFlash.Step(actor.Samus, frame);
                Animate(actor, frame);
                actor.Samus.CrystalFlash.UpdatePalette(actor.Colors, actor.Samus, beams, actor.Crystal);
                return result;
            });
            if (frames == 10) Require(pair.Stock.Samus.Position.Y.Whole == initialY - 20,
                "Flash must complete its ten-call, twenty-pixel raise");
            if (frames == 1)
            {
                foreach (Actor actor in new[] { pair.Stock, pair.Edited })
                {
                    for (int color = 0; color < CrystalFlashColorFormat.BodyColorCount; color++)
                        Require(actor.Colors.Colors[SamusPaletteRomData.CrystalFlash.BodyCgramStart + color] ==
                            actor.Crystal.ResolveBody(0, color), "Flash body colors were not applied");
                    for (int color = 0; color < CrystalFlashColorFormat.BubbleColorCount; color++)
                        Require(actor.Colors.Colors[SamusPaletteRomData.CrystalFlash.BubbleCgramStart + color] ==
                            actor.Crystal.ResolveBubble(0, color), "Flash bubble colors were not applied");
                }
            }
        }
        Require(pair.Stock.Samus.CrystalFlash.Phase == CrystalFlashPhase.Inactive &&
            pair.Stock.Samus.Missiles == 0 && pair.Stock.Samus.SuperMissiles == 0 && pair.Stock.Samus.PowerBombs == 0 &&
            pair.Stock.Samus.Health == 1501 && !pair.Stock.Samus.InputSnapshot.CrystalFlashPoseInputLocked,
            context + ": resource drain, pose release or completion did not occur");
        Console.WriteLine($"  {context}: {frames} identical movement/animation/palette calls; all thirty ammo drains and release verified.");
    }

    /// <summary>Advances the stored-shine palette through its 180-call expiration with edited colors.</summary>
    private void CheckStoredShine()
    {
        Pair pair = Create();
        Require(pair.Apply("stored shine admission", actor => actor.Samus.Shinespark.TryStoreFromSpeedBooster(new SamusSpeedBoostCounter(
            SamusSpecialSequenceRomData.Shinespark.ActiveSpeedBoostCounter))),
            "Stored shine was not admitted");
        for (int frame = 0; frame < 180; frame++)
            pair.Apply($"stored shine palette {frame}", actor => actor.Samus.Shinespark.UpdatePalette(
                actor.Colors, actor.Samus.EquippedItems, actor.Suits, actor.Cycles));
        Require(pair.Stock.Samus.Shinespark.Phase == ShinesparkPhase.Inactive && pair.Stock.Samus.Shinespark.ShineTimer.IsZero,
            "Edited stored-shine colors must retain the 180-call expiration");
    }

    /// <summary>Runs windup, directional flight, crash, and energy drain for one shinespark pose.</summary>
    private void CheckShinespark(SamusPoseId target)
    {
        bool left = ((int)target & 1) == 0;
        Pair pair = Create(left ? SamusPoseId.ShinesparkWindupLeftPose : SamusPoseId.ShinesparkWindupRightPose);
        string context = $"shinespark pose {(int)target:X2}";
        pair.Apply(context + " admission", actor =>
        {
            Require(actor.Samus.Shinespark.TryStoreFromSpeedBooster(new SamusSpeedBoostCounter(
                SamusSpecialSequenceRomData.Shinespark.ActiveSpeedBoostCounter)), "Spark charge rejected");
            actor.Samus.Shinespark.BeginWindup(actor.Samus);
        });
        for (ushort frame = 0; frame < 29; frame++)
            pair.Apply(context + $" windup {frame}", actor =>
            {
                var result = actor.Samus.Shinespark.Step(new SamusTerrain { Level = actor.Level }, actor.Samus, new ShinesparkFrame { NmiFrameCounter = frame });
                Animate(actor, frame);
                return result;
            });
        Require(pair.Stock.Samus.Shinespark.Phase == ShinesparkPhase.Windup &&
            pair.Stock.Samus.Shinespark.StartStopTimer.RawValue == 1, "Spark windup duration changed");
        pair.Apply(context + " launch", actor => actor.Samus.Shinespark.BeginDirectionalLaunch(actor.Samus, target));
        int frames = 0;
        while (pair.Stock.Samus.Shinespark.Phase != ShinesparkPhase.Inactive && frames < 300)
        {
            ushort frame = (ushort)frames++;
            pair.Apply(context + $" motion/crash {frame}", actor =>
            {
                var result = actor.Samus.Shinespark.Step(new SamusTerrain { Level = actor.Level }, actor.Samus, new ShinesparkFrame { NmiFrameCounter = frame, GameTimeFrames = frame });
                Animate(actor, frame);
                actor.Samus.Shinespark.UpdatePalette(actor.Colors, actor.Samus.EquippedItems, actor.Suits, actor.Cycles);
                return result;
            });
        }
        Require(pair.Stock.Samus.Shinespark.Phase == ShinesparkPhase.Inactive && pair.Stock.Samus.Health < 99,
            context + ": actual movement/energy drain/crash/release was not completed");
        Console.WriteLine($"  {context}: 29 windup calls and {frames} identical launch/crash/release calls.");
    }

    /// <summary>Runs all suit-pickup stages while comparing window geometry, Samus state, and installed colors.</summary>
    private void CheckSuitPickup(SamusSuitPickupKind kind)
    {
        Pair pair = Create();
        var stock = new SamusSuitPickupState(); var edited = new SamusSuitPickupState();
        stock.Begin(pair.Stock.Samus, new(0, 240), kind);
        edited.Begin(pair.Edited.Samus, new(0, 240), kind);
        int frames = 0;
        while (stock.IsActive && frames < 300)
        {
            stock.Step(pair.Stock.Samus, pair.Stock.Colors);
            edited.Step(pair.Edited.Samus, pair.Edited.Colors);
            new SamusMechanicsSnapshot(stock).RequireSame(new(edited), $"{kind} suit state {frames}");
            Require(stock.Beam == edited.Beam, $"{kind} suit beam geometry changed");
            pair.Check($"{kind} suit transformation {frames}");
            pair.Draw($"{kind} suit transformation {frames++}");
        }
        Require(!stock.IsActive && !edited.IsActive && !pair.Stock.Samus.Control.AlphaLocked,
            $"{kind} suit transformation did not release control");
        Require(pair.Stock.Samus.EquippedItems.HasAny(kind == SamusSuitPickupKind.Varia
            ? SamusEquipmentFlags.VariaSuit : SamusEquipmentFlags.GravitySuit), "Suit was not granted");
        Console.WriteLine($"  {kind} suit: {frames} identical stage/window/pose/control calls; edited suit colors applied.");
    }

    /// <summary>Checks the full death lifetime, pose art, explosion draws and explosion artwork for a source pose and suit.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native OAM tables")]
    private void CheckDeath(SamusPoseId source, SamusEquipmentFlags suit)
    {
        Pair pair = Create(source, suit);
        string context = $"death source {(int)source:X2}, suit {suit.ToNativeWord():X4}";
        pair.Apply(context + " admission", actor => actor.Samus.DeathSequence.Begin(actor.Samus, new(0, 240)), draw: false);
        int frames = 0;
        while (pair.Stock.Samus.DeathSequence.Phase != SamusDeathSequencePhase.Complete && frames < 240)
        {
            var stockStep = pair.Stock.Samus.DeathSequence.Step(pair.Stock.Samus,
                pair.Stock.Colors, pair.Stock.Body.DeathPalettes);
            var editedStep = pair.Edited.Samus.DeathSequence.Step(pair.Edited.Samus,
                pair.Edited.Colors, pair.Edited.Body.DeathPalettes);
            Require(stockStep == editedStep, context + ": death timing/publication changed");
            pair.Check(context + $" frame {frames}");
            if (stockStep.DrawPose) pair.Draw(context + $" pose {frames}");
            if (stockStep.DrawExplosion)
            {
                var before = new SamusMechanicsSnapshot(pair.Stock.Samus);
                OamBuffer a = SpriteFrameBuffer.DrawnOam(frame =>
                    pair.Stock.Samus.DeathSequence.DrawExplosion(frame, pair.Stock.Body.Spritemaps, pair.Stock.Body.DeathTiles));
                OamBuffer b = SpriteFrameBuffer.DrawnOam(frame =>
                    pair.Edited.Samus.DeathSequence.DrawExplosion(frame, pair.Edited.Body.Spritemaps, pair.Edited.Body.DeathTiles));
                before.RequireSame(new(pair.Stock.Samus), context + ": death draw mutated state");
                pair.Check(context + " explosion draw");
                draws++;
                if (!a.LowTable.SequenceEqual(b.LowTable)) changedOam++;
                if (!pair.Stock.Body.DeathTiles.Explosion.Characters.SequenceEqual(pair.Edited.Body.DeathTiles.Explosion.Characters)) changedTiles++;
            }
            frames++;
        }
        Require(frames == 211 && pair.Stock.Samus.DeathSequence.Phase == SamusDeathSequencePhase.Complete,
            context + ": death must retain its exact 16+60+135 call lifetime");
    }

    /// <summary>Confirms reserve refill freezes Samus animation while restoring one health per call and releasing input.</summary>
    private void CheckReserve()
    {
        Pair pair = Create();
        pair.Apply("reserve setup", actor =>
        { actor.Samus.Health = 0; actor.Samus.ReserveEnergy = 42; actor.Samus.ReserveTankMode = ReserveTankMode.Auto; });
        var stock = new SamusReserveAutoRecoveryState(); var edited = new SamusReserveAutoRecoveryState();
        stock.Begin(pair.Stock.Samus); edited.Begin(pair.Edited.Samus);
        int initialFrame = pair.Stock.Samus.AnimationFrame;
        for (ushort frame = 0; frame < 42; frame++)
        {
            Require(stock.StepAfterNmi(pair.Stock.Samus, frame) == edited.StepAfterNmi(pair.Edited.Samus, frame),
                "Reserve refill or sound cadence changed");
            new SamusMechanicsSnapshot(stock).RequireSame(new(edited), "reserve owner");
            pair.Check($"reserve frame {frame}"); pair.Draw($"reserve frame {frame}");
            Require(pair.Stock.Samus.AnimationFrame == initialFrame, "Reserve recovery must not advance Samus animation");
        }
        Require(!stock.IsActive && pair.Stock.Samus.Health == 42 && pair.Stock.Samus.ReserveEnergy == 0 &&
            !pair.Stock.Samus.Control.AlphaLocked, "Reserve recovery failed to refill and release");
    }
}
