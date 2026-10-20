using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// The two bank-$86 enemy projectiles spawned by <c>$90:F1E9</c> when a new game enters
/// Ceres: the moving pad under Samus and the stationary elevator platform behind it.
/// </summary>
/// <remarks>
/// These are not host-authored cutscene sprites. Construction consumes compiled copies of
/// the retail definitions at <c>$86:A387/$A395</c>; frame stepping executes their translated
/// bank-$86 instruction programs and pre-instructions <c>$A328/$A364</c>. Keeping this tiny
/// pair separate avoids pretending that the larger general enemy-projectile engine has
/// already been translated.
/// </remarks>
[RoomLifetime]
public sealed class CeresElevatorArrivalState
{
    /// <summary>First spawned projectile, which moves Samus before the platform checks landing.</summary>
    internal readonly CeresElevatorProjectile pad;
    /// <summary>Second spawned projectile, which waits behind the pad until touchdown.</summary>
    internal readonly CeresElevatorProjectile platform;
    /// <summary>Installed visual compositions rebound after restoring a debugger snapshot.</summary>
    [NonSerialized] private EnemyProjectileSpritemapCatalog? projectileSpritemaps;

    /// <summary>Installs both compiled definitions and performs their initialization AIs.</summary>
    public CeresElevatorArrivalState(
        SamusState samus,
        EnemyProjectileSpritemapCatalog? projectileSpritemaps = null)
    {
        Ensure.NotNull(samus);
        this.projectileSpritemaps = projectileSpritemaps;

        // SpawnEprojWithGfx first copies enemy slot zero's tile/palette word. The shared
        // initializer then clears it, so both actors use room-loaded OBJ tiles and the
        // palettes authored by their bank-$8D spritemaps. Retaining the transient word is
        // what previously made the level-data concealer teal until touchdown.
        pad = LoadProjectile(
            CeresElevatorArrivalDefinitions.MovingPad,
            MovingPadUnder(samus));
        pad.WaitTimer = NativeCountdown.Start(CeresElevatorArrivalDefinitions.MovingPadWaitFrames);

        platform = LoadProjectile(
            CeresElevatorArrivalDefinitions.StationaryPlatform,
            samus.Position.Whole.WithY(CeresElevatorArrivalDefinitions.StationaryPlatformY));
    }

    /// <summary>True after both projectiles delete themselves when Samus reaches Y=72.</summary>
    public bool IsComplete => !pad.Active && !platform.Active;

    /// <summary>Rebinds installed compositions after a debugger-state restore.</summary>
    public void BindProjectileSpritemaps(EnemyProjectileSpritemapCatalog? catalog) =>
        projectileSpritemaps = catalog;

    /// <summary>
    /// Executes <c>EprojRunAll</c>'s relative order for the two freshly spawned slots.
    /// </summary>
    /// <returns>True on and after the frame that calls Samus command <c>$0E</c>.</returns>
    public bool Step(SamusState samus)
    {
        Ensure.NotNull(samus);

        // SpawnEprojInner takes the highest free physical slot. The pad is spawned first
        // into slot $22, the platform second into $20, and EprojRunAll scans downward;
        // therefore the pad pre-instruction must move Samus before the platform tests Y=72.
        if (pad.Active)
        {
            if (pad.WaitTimer.RawValue > 1)
            {
                pad.WaitTimer = pad.WaitTimer.Decrement().Remaining;
            }
            else
            {
                if (pad.WaitTimer.RawValue == 1)
                    pad.WaitTimer = NativeCountdown.Start(0);

                // `$86:A342` samples the old Samus position for the pad, then increments
                // Samus. That one-pixel separation is visible throughout the descent.
                pad.Position = pad.Position.WithY(MovingPadUnder(samus).Y);
                samus.Position += new Offset16<RoomPixels>(0, 1);
                if ((short)unchecked((ushort)(
                    samus.Position.Y.Whole - CeresElevatorArrivalDefinitions.LandingThresholdY)) >= 0)
                {
                    samus.Position = samus.Position.WithWholeY(CeresElevatorArrivalDefinitions.LandingSamusY);
                    pad.Start(CeresElevatorArrivalProgram.Delete);
                }
            }
            ProcessInstructionList(pad);
        }

        if (platform.Active)
        {
            if (samus.Position.Y.Whole == CeresElevatorArrivalDefinitions.LandingSamusY)
            {
                platform.Start(CeresElevatorArrivalProgram.Delete);
            }
            ProcessInstructionList(platform);
        }

        return IsComplete;
    }

    /// <summary>
    /// Emits the low-priority bank-$8D spritemaps selected by the live instruction lists.
    /// </summary>
    /// <param name="sprites">The frame's sprite description.</param>
    /// <param name="objects">The gameplay OBJ character page.</param>
    /// <param name="camera">Layer-one camera position in whole room pixels.</param>
    public void Draw(SpriteFrameBuffer sprites, SpriteArtwork objects, Position16<RoomPixels> camera)
    {
        Ensure.NotNull(sprites);
        Ensure.NotNull(objects);
        DrawProjectile(pad, 0, sprites, objects, camera);
        DrawProjectile(platform, 1, sprites, objects, camera);
    }

    /// <summary>The moving pad's position: Samus's whole position, <c>MovingPadYOffset</c> lower.</summary>
    private static Position16<RoomPixels> MovingPadUnder(SamusState samus) =>
        samus.Position.Whole + new Offset16<RoomPixels>(0, (short)CeresElevatorArrivalDefinitions.MovingPadYOffset);

    /// <summary>Creates one arrival actor from its compiled definition and room position.</summary>
    private static CeresElevatorProjectile LoadProjectile(
        CeresElevatorProjectileDefinition definition,
        Position16<RoomPixels> position)
    {
        return new CeresElevatorProjectile(
            definition.Program,
            position,
            CeresElevatorArrivalDefinitions.NativeGraphicsIndex);
    }

    /// <summary>Runs a projectile's typed program (#641) when its frame expires: a new pose, or the delete.</summary>
    private static void ProcessInstructionList(CeresElevatorProjectile projectile)
    {
        ushort oldTimer = projectile.InstructionTimer.RawValue;
        projectile.InstructionTimer = projectile.InstructionTimer.Decrement().Remaining;
        if (oldTimer != 1)
            return;

        AnimationProgramCursor<CeresElevatorArrivalProgram> cursor = projectile.Program;
        // The arrival lists use no counted loops; their counter is never loaded.
        NativeCountdown counter = NativeCountdown.Start(0);
        AnimationProgramAdvance<CeresElevatorArrivalPose> advance = CeresElevatorArrivalAnimations.Programs.Advance(ref cursor, ref counter,
            (cue, _) => throw new InvalidOperationException($"The Ceres elevator arrival programs publish no cue; got {cue}."));
        projectile.Program = cursor;
        switch (advance.Kind)
        {
            case AnimationProgramAdvanceKind.Show:
                projectile.InstructionTimer = NativeCountdown.FromRaw((ushort)advance.Duration);
                projectile.Pose = advance.Frame;
                return;
            case AnimationProgramAdvanceKind.Stop:
                projectile.Active = false;
                return;
            case AnimationProgramAdvanceKind.Wait:
            case AnimationProgramAdvanceKind.Hold:
            default:
                throw new InvalidOperationException($"A Ceres elevator arrival program ended an update with {advance.Kind}.");
        }
    }

    /// <summary>Draws an active on-screen projectile through its installed bank-8D composition.</summary>
    private void DrawProjectile(
        CeresElevatorProjectile projectile,
        int instance,
        SpriteFrameBuffer sprites,
        SpriteArtwork objects,
        Position16<RoomPixels> camera)
    {
        if (!projectile.Active || projectile.Pose is not { } pose)
            return;

        Position16<ScreenPixels> screen = projectile.Position.ToScreen(camera);
        // Only origins within 128 pixels of the screen on each axis draw: X + 128 and Y + 128 below 512.
        if (screen.X + 128 >= 512 || screen.Y + 128 >= 512)
        {
            return;
        }

        var installed = projectileSpritemaps ?? throw new InvalidOperationException(
            "Ceres elevator projectiles require installed sprite artwork.");
        ushort paletteBits = new SnesObjAttributeWord(checked((ushort)projectile.GraphicsIndex)).PaletteBits;
        EnemyPresentation.PublishParts(sprites, objects, new SpriteSource(SpriteSourceKind.CeresElevatorArrival, instance),
            EnemySpriteFrames.Frame(installed.Get(CeresElevatorArrivalDefinitions.Spritemap(pose)), unchecked((byte)projectile.GraphicsIndex), paletteBits),
            screen, paletteBits);
    }

    /// <summary>Small per-slot state for the two Ceres arrival enemy projectiles.</summary>
    internal sealed class CeresElevatorProjectile
    {
        /// <summary>Initializes a projectile's program, position, and graphics index.</summary>
        public CeresElevatorProjectile(
            CeresElevatorArrivalProgram program,
            Position16<RoomPixels> position,
            int graphicsIndex)
        {
            Program = AnimationProgramCursor<CeresElevatorArrivalProgram>.Start(program);
            Position = position;
            GraphicsIndex = graphicsIndex;
        }

        /// <summary>Native graphics word supplying palette and tile-base bits for drawing.</summary>
        public int GraphicsIndex
        {
            get;
            private init => field = Ensure.BetweenInclusive(0, ushort.MaxValue, value);
        }
        /// <summary>Whether the projectile still occupies its arrival slot.</summary>
        public bool Active { get; set; } = true;
        /// <summary>Whole room position; the pre-instruction only rewrites Y.</summary>
        public Position16<RoomPixels> Position { get; set; }
        /// <summary>Pad-only startup delay before it begins moving Samus toward the platform.</summary>
        public NativeCountdown WaitTimer { get; set; }
        /// <summary>Where the projectile's typed program resumes when its frame expires.</summary>
        internal AnimationProgramCursor<CeresElevatorArrivalProgram> Program { get; set; }

        /// <summary>Starts <paramref name="program"/> on the next update.</summary>
        internal void Start(CeresElevatorArrivalProgram program)
        {
            InstructionTimer = NativeCountdown.Start(1);
            Program = AnimationProgramCursor<CeresElevatorArrivalProgram>.Start(program);
        }
        /// <summary>Frames until this projectile's next instruction pass.</summary>
        public NativeCountdown InstructionTimer { get; set; } = NativeCountdown.Start(1);
        /// <summary>The pose the projectile shows, none before its first frame.</summary>
        internal CeresElevatorArrivalPose? Pose { get; set; }
    }
}
