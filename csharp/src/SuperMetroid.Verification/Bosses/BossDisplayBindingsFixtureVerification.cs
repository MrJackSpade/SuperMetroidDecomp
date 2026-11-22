using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Assets;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Enemies.BossBodies;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.World;
using SuperMetroid.Core.World.RoomStates;

using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Verification.Harness;
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
        /// <summary>The place its boss's room placed the selected part in.</summary>
        internal RoomEnemyPlace Actor { get; }
        /// <summary>The boss whose body layer the actor draws on.</summary>
        private readonly BossBodyOwner bodyOwner;

        /// <summary>
        /// The body the actor's boss holds from its setup, which the actor's frames show on (#2554): Mother Brain attaches hers
        /// to the room's presentation only in her second phase, but her parts show their frames on it from the start.
        /// </summary>
        private BossBodyParts Body => (Actor.Entity as IBossBodyFrames
            ?? throw new InvalidOperationException($"Enemy place {Actor.Ordinal} holds {Actor.Occupant?.DefinitionId}, which shows no frames on a boss's body.")).BodyParts;

        /// <summary>The tilemap entry each cell of the boss's body layer shows now, from the frames its artwork installs (#1466).</summary>
        internal ushort[] Bg2Words => BossBodyWords.Of(Body.Publish(), bodyOwner, bodies.Frames);

        /// <summary>The frames the boss's body layer shows now, in draw order, for a failure message.</summary>
        internal string ShownFrames => string.Join(", ", Body.Publish().Frames.Select(frame => frame.Name));

        /// <summary>The fixture's boss frames and the body sheets composed from them, which its bosses draw from.</summary>
        private readonly BossBodyArtworkFixture bodies;

        /// <summary>
        /// Whether the actor's current frame's finished picture shows nothing, so that it draws nothing (#1538) where the
        /// cartridge writes its transparent sprites.
        /// </summary>
        internal bool ActorFrameShowsNothing =>
            Actor.Frame is { } frame && Enemies.Presentation.Artwork.Enemy(Actor.HeldOccupant().DefinitionId).Sheet.ShowsNothing(EnemySheetDefinitions.FrameName(frame));

        /// <summary>The body sheets composed from the fixture's boss frames.</summary>
        private BossBodySheets Sheets => bodies.Sheets;
        /// <summary>Bound production instruction interpreter for one actor step.</summary>
        private readonly Action<RoomEnemyPlace, SamusState, RoomLevelData, Position16<RoomPixels>, GameplayAction> instructions;

        /// <summary>
        /// Places <paramref name="definition"/>'s whole boss as its room's population places it, each part set up with the
        /// body placed before it (#2554), centres the part and enqueues it for the production draw path.
        /// </summary>
        internal BossDisplayFixture(EnemyDefinitionId definition, ImportEnemyArtwork artwork)
        {
            Enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(artwork.Enemies), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance,
                services: new VerificationRoomEnemyServices(null), installedBosses: artwork.Bosses);
            Enemies.Presentation.BindBackgrounds(BgArtworkFixtures.For(Vram));
            bodyOwner = BodyOwnerOf(definition) ?? throw new InvalidOperationException($"{definition} draws no body layer.");
            bodies = BossBodySheetFixtures.For(artwork.Bosses, bodyOwner);
            Enemies.BossArtworkBinding.BodySheets = Sheets;
            // The boss's room has not seen it beaten; the room's screens' scrolls, which Crocomire's setup writes, are the
            // room's own, and the check keeps no room around him.
            Enemies.VerificationServices().IsAreaBossDefeated = () => false;
            Enemies.VerificationServices().IsAreaMiniBossDefeated = () => false;
            Enemies.VerificationServices().SetRoomScrollState = (_, _) => { };
            RoomEnemyPopulationDefinition room = BossRooms[bodyOwner];
            IReadOnlyList<RoomEnemyPlace> places = Enemies.LoadFirstEntries(room, room.Placements.ToArray().TakeWhile(entry => BodyOwnerOf(entry.DefinitionId) == bodyOwner).Count(),
                RoomEnemyFrameFixtures.StandaloneSamus());
            Actor = places.First(place => place.Holds(definition));
            Actor.Position = Actor.Position.WithWhole(new(128, 128));
            Actor.Contact = EnemyContact.None;
            Actor.Processing = EnemyProcessing.Animates;
            Actor.Presence = EnemyPresence.None;
            Enemies.Drawing.Enqueue(Actor, reached: true);
            // The boss draws under the retail graphics set its room loads, whose sheets hold its frames.
            SpawnedObjectFixtures.LoadSetDrawing(Enemies, definition, Actor);
            // The boss's setup made the body its frames draw on; each check starts from it showing nothing.
            ClearBg2();
            instructions = new Action<RoomEnemyPlace, SamusState, RoomLevelData, Position16<RoomPixels>, GameplayAction>(EnemyAnimationPlayer.AdvanceAnimation);
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

        /// <summary>
        /// The retail population that places each boss with a body layer, whose leading entries are the boss's parts, its body
        /// first: the fight a living boss's room loads.
        /// </summary>
        private static readonly Dictionary<BossBodyOwner, RoomEnemyPopulationDefinition> BossRooms = new()
        {
            [BossBodyOwner.Phantoon] = RoomEnemyPopulationDefinitions.PhantoonDefault,
            [BossBodyOwner.Draygon] = RoomEnemyPopulationDefinitions.DraygonDefault,
            [BossBodyOwner.Crocomire] = RoomEnemyPopulationDefinitions.Crocomire,
            [BossBodyOwner.MotherBrain] = RoomEnemyPopulationDefinitions.MotherBrainBeforeDefeat,
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
            Enemies.Drawing.Draw(Enemies.Drawing.Advance(new(0, 0)), frame, new(0, 0), RoomEnemyFrameFixtures.StandaloneSamus());
        }


        /// <summary>Advances the selected actor's production instruction interpreter once.</summary>
        internal void Step() => instructions(Actor, RoomEnemyFrameFixtures.StandaloneSamus(), RoomEnemyFrameFixtures.OpenRoom(new(64, 64)), Position16<RoomPixels>.Zero, 0);
        /// <summary>Clears the body the boss holds, so no part shows anything on it, before a presentation check.</summary>
        internal void ClearBg2() => Body.Clear();
    }
}
