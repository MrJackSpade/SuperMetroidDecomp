using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Assets;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Enemies.BossBodies;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.World;

using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Enemies.Drawing;
namespace SuperMetroid.Verification.Bosses;

/// <summary>Builds a RAM-only boss actor fixture for queued OAM checks.</summary>
internal static class BossDisplayBindingsFixtureVerification
{
    /// <summary>One compiled boss actor with RAM only, exercising the public queued draw path.</summary>
    internal sealed class BossDisplayFixture
    {
        /// <summary>VRAM receiving boss tilemap transfers during draw checks.</summary>
        internal SnesVram Vram { get; } = new();
        /// <summary>Enemy owner whose ordinary queue and presentation path the fixture exercises.</summary>
        internal RoomEnemySystem Enemies { get; }
        /// <summary>The physical slot initialized as the selected boss definition.</summary>
        internal RoomEnemySlot Actor => Enemies.SlotTable.Slots[_slot];
        /// <summary>The actor's physical slot.</summary>
        private readonly int _slot;
        /// <summary>The boss whose body layer the actor draws on, or null for a boss that draws none.</summary>
        private readonly BossBodyOwner? bodyOwner;

        /// <summary>The boss whose body layer the actor draws on.</summary>
        private BossBodyOwner BodyOwner => bodyOwner ?? throw new InvalidOperationException($"{Actor.DefinitionId} draws no body layer.");

        /// <summary>The tilemap entry each cell of the boss's body layer shows now, from the frames its artwork installs (#1466).</summary>
        internal ushort[] Bg2Words => BossBodyWords.Of(Enemies.Presentation.RequireBossBody().Publish(), BodyOwner, bodies.Frames);

        /// <summary>The frames the boss's body layer shows now, in draw order, for a failure message.</summary>
        internal string ShownFrames => string.Join(", ", Enemies.Presentation.RequireBossBody().Publish().Frames.Select(frame => frame.Name));

        /// <summary>The fixture's boss frames and the body sheets composed from them, which its bosses draw from.</summary>
        private readonly BossBodyArtworkFixture bodies;

        /// <summary>
        /// Whether the actor's current frame's finished picture shows nothing, so that it draws nothing (#1538) where the
        /// cartridge writes its transparent sprites.
        /// </summary>
        internal bool ActorFrameShowsNothing =>
            Actor.Frame is { } frame && Enemies.Presentation.Artwork.Enemy(Actor.DefinitionId).Sheet.ShowsNothing(EnemySheetDefinitions.FrameName(frame));

        /// <summary>The body sheets composed from the fixture's boss frames.</summary>
        private BossBodySheets Sheets => bodies.Sheets;
        /// <summary>Bound production instruction interpreter for one actor step.</summary>
        private readonly Action<RoomEnemySlot, SamusState?, RoomLevelData?, Position16<RoomPixels>, ControllerButton> instructions;

        /// <summary>Installs one centered boss actor in <paramref name="slot"/> and enqueues it for the production draw path.</summary>
        internal BossDisplayFixture(EnemyDefinitionId definition, ImportEnemyArtwork artwork, int slot = 0)
        {
            _slot = slot;
            Enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(artwork.Enemies), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance)
            {
                BossArtwork = artwork.Bosses,
            };
            Enemies.Presentation.BindBackgrounds(BgArtworkFixtures.For(Vram));
            bodyOwner = BodyOwnerOf(definition);
            bodies = bodyOwner is { } drawn ? BossBodySheetFixtures.For(artwork.Bosses, drawn) : BossBodySheetFixtures.Stock;
            Enemies.BossArtworkBinding.BodySheets = Sheets;
            Actor.Bind(definition, Actor.Definition);
            Actor.Bind(Actor.DefinitionId, RoomEnemyHeaders.Get(definition));
            Actor.Position = Actor.Position.WithWhole(new(128, 128));
            Actor.Health = Actor.Definition.Health;
            Actor.Radius = new(Actor.Definition.Radius.X, Actor.Definition.Radius.Y);
            Actor.Contact = EnemyContact.None;
            Actor.Processing = EnemyProcessing.Animates;
            Actor.Presence = EnemyPresence.None;
            Enemies.Drawing.Enqueue(Actor, reached: true);
            // The boss draws under the retail graphics set its room loads, whose sheets hold its frames.
            SpawnedObjectFixtures.LoadSetDrawing(Enemies, definition, Actor);
            // A boss that owns BG2 attaches its body layer as its initialisation does, before any of its frames draw on it.
            ClearBg2();
            instructions = new Action<RoomEnemySlot, SamusState?, RoomLevelData?, Position16<RoomPixels>, ControllerButton>(EnemyAnimationPlayer.AdvanceAnimation);
        }

        /// <summary>The boss whose body layer <paramref name="definition"/> draws on, or null for one that draws none.</summary>
        internal static BossBodyOwner? BodyOwnerOf(EnemyDefinitionId definition) =>
            BodyOwners.TryGetValue(definition, out BossBodyOwner owner) ? owner : null;

        /// <summary>Each part of a boss that owns a body layer (#1446, #1466), with the boss whose layer it draws on.</summary>
        private static readonly Dictionary<EnemyDefinitionId, BossBodyOwner> BodyOwners = new()
        {
            [EnemyDefinitionId.PhantoonBody] = BossBodyOwner.Phantoon,
            [EnemyDefinitionId.PhantoonEye] = BossBodyOwner.Phantoon,
            [EnemyDefinitionId.PhantoonTentacles] = BossBodyOwner.Phantoon,
            [EnemyDefinitionId.PhantoonMouth] = BossBodyOwner.Phantoon,
            [EnemyDefinitionId.DraygonBody] = BossBodyOwner.Draygon,
            [EnemyDefinitionId.DraygonEye] = BossBodyOwner.Draygon,
            [EnemyDefinitionId.DraygonTail] = BossBodyOwner.Draygon,
            [EnemyDefinitionId.DraygonArms] = BossBodyOwner.Draygon,
            [EnemyDefinitionId.Crocomire] = BossBodyOwner.Crocomire,
            [EnemyDefinitionId.CrocomireTongue] = BossBodyOwner.Crocomire,
            [EnemyDefinitionId.MotherBrainBody] = BossBodyOwner.MotherBrain,
            [EnemyDefinitionId.MotherBrainHead] = BossBodyOwner.MotherBrain,
        };

        /// <summary>Selects an extended spritemap and optionally marks it as a new instruction frame.</summary>
        internal void SetFrame(ushort frame, bool fresh)
        {
            Actor.SpritemapPointer = frame;
            Actor.CollidesByFrameHitboxes = true;
            Actor.AnimationClock.ShowsNewFrame = fresh;
        }

        /// <summary>Runs queued enemy drawing into a fresh OAM frame.</summary>
        internal OamBuffer Draw()
        {
            var result = new OamBuffer(); result.BeginFrame();
            Enemies.Drawing.DrawAsOam(result, new(0, 0));
            return result;
        }

        /// <summary>Runs the production enemy drawing path once, publishing into <paramref name="frame"/>.</summary>
        internal void DrawInto(SpriteFrameBuffer frame)
        {
            Enemies.Drawing.Advance(new(0, 0));
            Enemies.Drawing.Draw(frame, new(0, 0));
        }


        /// <summary>Advances the selected actor's production instruction interpreter once.</summary>
        internal void Step() => instructions(Actor, null, null, Position16<RoomPixels>.Zero, 0);
        /// <summary>Attaches a body showing nothing before a presentation check.</summary>
        internal void ClearBg2()
        {
            if (bodyOwner is { } owner)
                Enemies.Presentation.AttachBossBody(new BossBodyParts(owner));
        }
    }
}
