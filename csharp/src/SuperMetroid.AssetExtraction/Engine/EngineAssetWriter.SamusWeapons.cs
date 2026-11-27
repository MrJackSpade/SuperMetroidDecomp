using SuperMetroid.Core;
using SuperMetroid.Core.Spatial;
using System.Globalization;
using System.Text.Json.Nodes;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Compatibility;
using SuperMetroid.Core.Earthquakes;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Projectiles;
using Segment = SuperMetroid.Core.Samus.Projectiles.SamusProjectileSegment;

namespace SuperMetroid.AssetExtraction.Engine;

/// <summary>
/// Samus's weapons: the beam in every combination she can carry, plain and charged, and the hyper beam; missiles and
/// super missiles; bombs and power bombs; the special attacks and the echoes of a shinespark. Each shot's hit is what
/// the enemies' reactions are written against: a beam's carries "beam" and each of its beams' kinds, and "charged"
/// when it is; the rest carry the one kind of what they are.
/// </summary>
public sealed partial class EngineAssetWriter
{
    /// <summary>The data files of her weapons, in the order the Godot game is given them.</summary>
    private static readonly string[] SamusWeaponFiles =
    [
        "samus/weapons/beam.json", "samus/weapons/missile.json", "samus/weapons/super_missile.json", "samus/weapons/bomb.json",
        "samus/weapons/power_bomb.json",
    ];

    /// <summary>The data file of the shot of a shinespark's echoes.</summary>
    private const string ShinesparkEchoFile = "samus/shots/shinespark_echo.json";

    /// <summary>The pool of places her beams, missiles and a crash's echoes share.</summary>
    private const string ShotPool = "shots";

    /// <summary>The beams that combine, with the equipment each is in the Godot game and the kind its hit carries.</summary>
    private static readonly (SamusBeamFlags Beam, string Equipment, string Kind)[] SamusBeams =
    [
        (SamusBeamFlags.Wave, "wave_beam", "wave"), (SamusBeamFlags.Ice, "ice_beam", "ice"), (SamusBeamFlags.Spazer, "spazer", "spazer"),
        (SamusBeamFlags.Plasma, "plasma_beam", "plasma"),
    ];

    /// <summary>The ways a shot can be launched, as the Godot game names them, with the old direction of each.</summary>
    private static readonly (string Name, SamusProjectileDirection Direction, SamusProjectileDirection? Twin)[] ShotDirections =
    [
        ("right", SamusProjectileDirection.Right, null), ("up_right", SamusProjectileDirection.UpRight, null),
        ("up", SamusProjectileDirection.UpFacingRight, SamusProjectileDirection.UpFacingLeft), ("up_left", SamusProjectileDirection.UpLeft, null),
        ("left", SamusProjectileDirection.Left, null), ("down_left", SamusProjectileDirection.DownLeft, null),
        ("down", SamusProjectileDirection.DownFacingRight, SamusProjectileDirection.DownFacingLeft), ("down_right", SamusProjectileDirection.DownRight, null),
    ];

    private InstalledProjectileFrameBindings? _shotFrames;
    private readonly Dictionary<string, EngineSheetPictures> _shotPictures = new(StringComparer.Ordinal);

    /// <summary>Which picture of a sheet each frame of her shots' animations shows.</summary>
    private InstalledProjectileFrameBindings ShotFrames => _shotFrames ??= _installation.LoadProjectiles().FrameBindings;

    /// <summary>The name of a combination of beams in file names.</summary>
    private static string BeamsName(SamusBeamCombination beams) => EngineSheets.FileName(SamusBeamCombinationKeys.Installed.Of(beams));

    /// <summary>The pictures of the shots as one combination of beams colours them, cut once into a folder named for it.</summary>
    private EngineSheetPictures ShotPictures(SamusBeamCombination beams)
    {
        string sheet = SpriteSheetDefinitions.FileNames[ProjectileSheetFrames.For(beams)];
        if (_shotPictures.TryGetValue(sheet, out EngineSheetPictures? cut))
            return cut;
        // The flare at the end of her arm cannon is cut from the same sheet, in the same colours, and so are the
        // sparkles and smoke her shots leave behind them, whose names keep their "trail" so as not to meet a shot's.
        const string shots = "Projectile.";
        IEnumerable<string> frames = _sheets.FramesOf(SpriteFamily, sheet)
            .Where(frame => frame.StartsWith(shots, StringComparison.Ordinal) || frame.StartsWith(FlareFrames, StringComparison.Ordinal)
                || frame.StartsWith(TrailFrames, StringComparison.Ordinal));
        _shotFolders[sheet] = BeamsName(beams);
        return _shotPictures[sheet] = _sheets.CutNamed(_tree, SpriteFamily, $"samus/shots/{BeamsName(beams)}/pictures", sheet, frames, null,
            frame => frame.StartsWith(TrailFrames, StringComparison.Ordinal)
                ? EngineSheets.FileName(frame)
                : EngineSheets.FileName(frame[(frame.StartsWith(shots, StringComparison.Ordinal) ? shots : FlareFrames).Length..]));
    }

    /// <summary>The name of the folder each sheet of her shots was cut into, by the sheet.</summary>
    private readonly Dictionary<string, string> _shotFolders = new(StringComparer.Ordinal);

    /// <summary>What the names of the trails' frames on a sheet of her shots begin with.</summary>
    private const string TrailFrames = "Trail.";

    /// <summary>What the names of the flare's frames on a sheet of her shots begin with.</summary>
    private const string FlareFrames = "Flare.";

    /// <summary>
    /// The flare at the end of her arm cannon while she holds a charge: when it shows, its three parts' timelines,
    /// where its pictures are for each combination of beams, and where it lies by how she stands, faces and aims.
    /// The old flare's parts are counted down a step before they are drawn, which the Godot flare does too.
    /// </summary>
    private JsonObject ChargeFlareWritten()
    {
        const string main = "FlareMain";
        string[] rings = ["Widest", "Wider", "Wide", "Narrow", "Narrower", "Narrowest"];
        EngineSheetPictures power = ShotPictures(SamusBeamCombination.Power);

        // A step's picture is written by its name alone: the same name in the folder of each combination of beams.
        JsonObject Step(string part, string picture, int updates)
        {
            string path = power.Of($"{FlareFrames}{part}.{picture}");
            return new JsonObject { ["picture"] = Path.GetFileNameWithoutExtension(path), ["updates"] = updates };
        }

        // The glow: embers that swell to a white ball, then the ball and its starburst in turn for good.
        const int held = 4, embers = SuperMetroid.Core.Samus.Projectiles.ChargeFlareAnimationDefinitions.EmberGlowPlays, swelling = embers;
        var glow = new JsonArray();
        for (int turn = 0; turn < embers; turn++)
        {
            glow.Add(Step(main, "Ember", held));
            glow.Add(Step(main, "Glow", held));
        }

        for (int turn = 0; turn < swelling; turn++)
        {
            glow.Add(Step(main, "WhiteBall", held));
            glow.Add(Step(main, "Glow", held));
        }

        // The loop is written as one round of it, each of its turns: the hyper beam counts the glow back through them
        // one by one before it reaches the swelling.
        int loop = glow.Count;
        for (int turn = 0; turn < SuperMetroid.Core.Samus.Projectiles.ChargeFlareAnimationDefinitions.StarburstPlays; turn++)
        {
            glow.Add(Step(main, "WhiteBall", held));
            glow.Add(Step(main, "Starburst", held));
        }

        JsonObject Ring(string speed, string side, int[] updates)
        {
            var steps = new JsonArray();
            for (int index = 0; index < rings.Length; index++)
                steps.Add(Step($"Flare{speed}Sparks{side}", "Sparks" + rings[index], updates[index]));
            return new JsonObject { ["steps"] = steps, ["loops_to"] = 0 };
        }

        int[] slow = [6, 5, 4, 4, 4, 4], fast = [5, 4, 3, 3, 3, 3];
        // The pictures of each combination of beams: the folder they were cut into, which combinations that share a
        // sheet share, and where a picture's corner lies from the flare's place.
        var byBeams = new JsonObject();
        foreach (SamusBeamCombination beams in Enum.GetValues<SamusBeamCombination>())
        {
            EngineSheetPictures pictures = ShotPictures(beams);
            string ember = pictures.Of($"{FlareFrames}{main}.Ember");
            string folder = ember[..ember.LastIndexOf('/')];
            byBeams[BeamsName(beams)] = new JsonObject { ["folder"] = folder, ["x"] = pictures.OffsetX, ["y"] = pictures.OffsetY };
        }

        SuperMetroid.Core.Samus.Artwork.BeamFlarePlacements placed = _installation.LoadProjectiles().FlarePlacement;

        // The grapple's flare has places of its own, which are also where its line is laid from.
        ChargeFlarePlacements grapplePlaced = _installation.LoadProjectiles().Grapple.FlarePlacement;
        JsonObject Places(bool running) => PlacesOf(placed, running);
        static JsonObject PlacesOf(ChargeFlarePlacements placed, bool running)
        {
            var right = new JsonObject();
            var left = new JsonObject();
            foreach ((string aim, SamusProjectileDirection toRight, SamusProjectileDirection toLeft) in SamusAims)
            {
                Offset16<RoomPixels> rightward = placed.AtMuzzle(running, toRight), leftward = placed.AtMuzzle(running, toLeft);
                right[aim] = new JsonObject { ["x"] = rightward.X, ["y"] = rightward.Y };
                left[aim] = new JsonObject { ["x"] = leftward.X, ["y"] = leftward.Y };
            }

            return new JsonObject { ["right"] = right, ["left"] = left };
        }

        var turning = new JsonObject();
        foreach ((string name, SuperMetroid.Core.Samus.Poses.SamusTurningAim aim) in new[]
                 {
                     ("up", SuperMetroid.Core.Samus.Poses.SamusTurningAim.Up), ("neutral", SuperMetroid.Core.Samus.Poses.SamusTurningAim.Neutral),
                     ("down", SuperMetroid.Core.Samus.Poses.SamusTurningAim.Down),
                 })
        {
            Offset16<RoomPixels> at = placed.WhileTurning(aim);
            turning[name] = new JsonObject { ["x"] = at.X, ["y"] = at.Y };
        }

        return new JsonObject
        {
            ["shows_from"] = SuperMetroid.Core.Samus.Projectiles.ChargeFlareAnimationDefinitions.MainFlareShownFrom,
            ["sparks_from"] = SuperMetroid.Core.Samus.Projectiles.ChargeFlareAnimationDefinitions.SparksShownFrom,
            ["hyper_picture_updates"] = SuperMetroid.Core.Samus.Projectiles.ChargeFlareAnimationDefinitions.HyperBeamPictureUpdates,
            ["hyper_first_picture_updates"] = SuperMetroid.Core.Samus.Projectiles.ChargeFlareAnimationDefinitions.HyperBeamFirstPictureUpdates,
            ["pictures"] = byBeams,
            ["main"] = new JsonObject { ["steps"] = glow, ["loops_to"] = loop },
            ["slow_right"] = Ring("Slow", "Right", slow),
            ["slow_left"] = Ring("Slow", "Left", slow),
            ["fast_right"] = Ring("Fast", "Right", fast),
            ["fast_left"] = Ring("Fast", "Left", fast),
            ["places"] = new JsonObject { ["standing"] = Places(running: false), ["running"] = Places(running: true) },
            ["grapple_places"] = new JsonObject { ["standing"] = PlacesOf(grapplePlaced, running: false), ["running"] = PlacesOf(grapplePlaced, running: true) },
            ["turning_places"] = turning,
        };
    }

    /// <summary>The picture a frame of a shot's animation shows; empty for a frame that shows nothing.</summary>
    private string ShotPicture(EngineSheetPictures pictures, SamusProjectileFrameId frame) =>
        ShotFrames.Resolve(frame) is { } sprite ? pictures.Of(ProjectileSheetFrames.ProjectileFrame(sprite)) : "";

    /// <summary>
    /// The steps of one of her shots' animations, from the segment it starts with, each step's picture the name of the
    /// old frame it shows, marked as one (<see cref="ShotFrameNamed"/>): what the shot's timeline and its trail are both
    /// written from, step for step.
    /// </summary>
    private static JsonObject NamedShotTimeline(Segment segment)
    {
        (List<IAnimationBeat<SamusProjectileFrameId, NoAnimationCue>> lead, List<IAnimationBeat<SamusProjectileFrameId, NoAnimationCue>>? cycle) =
            EngineTimelines.Played(SamusProjectileAnimations.Segments, SamusProjectileAnimations.StartingWith(segment));
        return EngineTimelines.Of<SamusProjectileFrameId, NoAnimationCue>(lead, cycle, static frame => "#" + frame, static _ => null);
    }

    /// <summary>The old frame a step of <see cref="NamedShotTimeline"/> shows, by the name it was given there.</summary>
    private static SamusProjectileFrameId ShotFrameNamed(string named) =>
        named.StartsWith('#') ? Enum.Parse<SamusProjectileFrameId>(named[1..]) : throw new InvalidDataException($"A step of one of Samus's shots shows {named}, which names no frame.");

    /// <summary>
    /// The timeline of one of her shots' animations, from the segment it starts with. Where it is the timeline of
    /// something that hits, each step where the old frame's size changes gives the cue that makes the projectile that
    /// size.
    /// </summary>
    private JsonObject ShotTimeline(EngineSheetPictures pictures, Segment segment, bool sized, bool restsAtEnd = false)
    {
        JsonObject timeline = NamedShotTimeline(segment);
        JsonArray steps = timeline["steps"]!.AsArray();
        if (restsAtEnd && timeline["loops_to"] is null)
            timeline["loops_to"] = steps.Count - 1;
        int loopsTo = timeline["loops_to"]?.GetValue<int>() ?? -1;
        (int Width, int Height)? size = null;
        for (int index = 0; index < steps.Count; index++)
        {
            JsonObject step = steps[index]!.AsObject();
            string named = step["picture"]!.GetValue<string>();
            if (named.Length == 0)
                continue;
            SamusProjectileFrameId frame = ShotFrameNamed(named);
            step["picture"] = ShotPicture(pictures, frame);
            if (!sized)
                continue;
            Extent16<RoomPixels> radius = SamusProjectileFrameDefinitions.Radius(frame);
            (int Width, int Height) now = (radius.X * 2, radius.Y * 2);
            if (now.Width <= 0 || now.Height <= 0)
                throw new InvalidDataException($"The frame {frame} of one of Samus's shots has no size, which something that hits must have.");
            if (size != now || index == loopsTo)
                step["cues"] = new JsonArray(string.Create(CultureInfo.InvariantCulture, $"size:{now.Width}:{now.Height}"));
            size = now;
        }

        timeline["offset"] = new JsonObject { ["x"] = pictures.OffsetX, ["y"] = pictures.OffsetY };
        return timeline;
    }

    /// <summary>
    /// Writes the timelines of a kind of shot, one for each way it can be launched, under <paramref name="folder"/>,
    /// and gives the path that names them all, with the way's place in it.
    /// </summary>
    private string WriteShotTimelines(string folder, string name, EngineSheetPictures pictures, Func<SamusProjectileDirection, Segment> animation)
    {
        foreach ((string way, SamusProjectileDirection direction, SamusProjectileDirection? twin) in ShotDirections)
        {
            Segment segment = animation(direction);
            if (twin is { } other && animation(other) != segment)
                throw new InvalidDataException($"Samus's {name} shots look one way fired straight {way} facing right and another facing left, which the Godot game cannot say.");
            _tree.WriteData($"{folder}/{name}_{way}.json", ShotTimeline(pictures, segment, sized: true));
        }

        return $"{folder}/{name}_{{direction}}.json";
    }

    /// <summary>The first size of a shot's animation, as the old game gives a shot its size as it is fired.</summary>
    private static (int Width, int Height) FirstShotSize(Segment segment)
    {
        Extent16<RoomPixels> radius = SamusProjectilePlayback.InitialRadius(segment);
        return (Math.Max(1, radius.X * 2), Math.Max(1, radius.Y * 2));
    }

    /// <summary>What every shot that flies shares: it is gone a way outside the view, as far as the old game lets it fly.</summary>
    private static JsonObject FlyingShot(Segment first, string timeline)
    {
        (int width, int height) = FirstShotSize(first);
        int margin = SamusProjectileMotionDefinitions.MovementWindowApron;
        if (SamusProjectileMotionDefinitions.MovementWindowFarEdge - ScreenGeometry.ScreenWidthPixels != margin)
            throw new InvalidDataException("Samus's shots fly further past one side of the view than the other, which one margin cannot say.");
        return new JsonObject
        {
            ["width"] = width,
            ["height"] = height,
            ["motion"] = "accelerating",

            // The cartridge draws her shots straight after her, and so behind her and in front of the enemies that
            // stand behind her: one fired from within her outline comes out from behind her.
            ["standing"] = "just_behind",
            ["view_margin"] = margin,
            ["view_margin_below"] = SamusProjectileMotionDefinitions.MovementWindowFarEdge - ScreenGeometry.ScreenHeightPixels,
            ["timeline"] = timeline,
        };
    }

    /// <summary>One of the speeds the old game gives along the axis a shot flies on, which it gives signed for the way.</summary>
    private static SignedEightEight Along(PixelVelocity<RoomPixels> acceleration) =>
        acceleration.X.IsZero ? throw new InvalidDataException("A shot fired across gains no speed across.") : acceleration.X;

    /// <summary>
    /// The projectile of a beam shot of one combination: how it flies, what it passes through, how it looks and what it
    /// leaves, the trail behind it among that unless it is one, as the hyper beam's, that leaves none.
    /// </summary>
    private JsonObject BeamProjectile(SamusBeamCombination beams, SamusBeamCombination pictured, bool charged, string name, bool passesWalls,
        bool leavesTrail = true)
    {
        SamusProjectileKind kind = SamusProjectileSelection.BeamShot(beams, charged);
        EngineSheetPictures pictures = ShotPictures(pictured);
        string folder = $"samus/shots/{BeamsName(pictured)}";
        Segment AnimationOf(SamusProjectileDirection direction) => SamusProjectileKinds.Animation(kind, direction);
        string timeline = WriteShotTimelines(folder, name, pictures, AnimationOf);
        JsonObject projectile = FlyingShot(SamusProjectileKinds.Animation(kind, SamusProjectileDirection.Right), timeline);
        if (leavesTrail && ShotTrail($"{BeamsName(pictured)}/{name}", SamusProjectileType.Beam(beams, charged), pictured,
                BeamTrailInterval(beams, charged), FlyingWays(AnimationOf)) is { } trail)
            projectile["trail"] = trail;
        SignedEightEight straight = Along(SamusProjectileAccelerations.For(SamusProjectileDirection.Right).Beam);
        SignedEightEight slanted = Along(SamusProjectileAccelerations.For(SamusProjectileDirection.DownRight).Beam);
        projectile["acceleration"] = EngineNames.Number(straight);
        if (slanted != straight)
            projectile["diagonal_acceleration"] = EngineNames.Number(slanted);

        // A beam with the wave beam in it passes through walls, striking what it crosses; one with the plasma beam in
        // it passes through what it hurts and can hurt it again.
        projectile["meeting_terrain"] = passesWalls ? "passes_through_and_hits" : "ends_and_hits";

        // A beam meets the room's blocks by the edge it leads with, once for each way it moves, and is stopped only
        // when every block across that edge stops it; and what it leaves on ending is left at its front.
        projectile["terrain_by"] = "front_edge";
        projectile["ends_at_front"] = true;
        projectile["meeting_target"] = beams.HasPlasma ? "passes_through_and_rehits" : "ends";
        if (!passesWalls)
        {
            string burst = $"{folder}/burst.json";
            _tree.WriteData(burst, ShotTimeline(pictures, Segment.BeamExplosion, sized: false));
            projectile["ending_effect"] = burst;
            // The cartridge draws the explosions of her shots and her bombs before all else, over everything.
            projectile["ending_effect_standing"] = "topmost";
            projectile["ending_sound"] = EngineNames.Sound(EnemySound.BeamImpact);
        }

        return projectile;
    }

    /// <summary>Writes the beam: every combination she can carry, plain and charged, and the hyper beam.</summary>
    private void WriteSamusBeam()
    {
        const SamusBeamCombination plain = SamusBeamCombination.Power;
        static JsonArray Kinds(SamusBeamCombination beams, bool charged = false)
        {
            var kinds = new JsonArray("beam");
            foreach ((SamusBeamFlags beam, string _, string kind) in SamusBeams)
            {
                if ((beams.Components & beam) != SamusBeamFlags.None)
                    kinds.Add(kind);
            }

            if (charged)
                kinds.Add("charged");
            return kinds;
        }

        JsonArray Shots(JsonObject projectile) => [new JsonObject { ["side"] = 0, ["projectile"] = projectile }];
        // Nothing turns a beam's shot away.
        string? Sound(SamusBeamCombination beams, bool charged) => SamusProjectileSoundRouting.Resolve(charged, beams) is { } sound
            ? EngineNames.Sound(sound, SoundRequestPriority.Essential)
            : null;
        string SoundOf(SamusBeamCombination beams, bool charged) =>
            Sound(beams, charged) ?? throw new InvalidDataException($"The {beams} beam makes no sound as it is fired, which this expects it to.");

        // The old game moves a beam on from the speed it is fired at before it first moves; the Godot game after. So
        // it is launched with the first gain already in its speed.
        SignedEightEight straight = Along(SamusProjectileAccelerations.For(SamusProjectileDirection.Right).Beam);
        SignedEightEight slanted = Along(SamusProjectileAccelerations.For(SamusProjectileDirection.DownRight).Beam);
        var weapon = new JsonObject
        {
            ["name"] = "beam",
            ["damage"] = SamusProjectileDamageDefinitions.UnchargedBeamDamage[plain],
            ["kinds"] = Kinds(plain),
            ["speed"] = EngineNames.Number(SamusProjectileMotionDefinitions.CardinalSpeed + straight),
            ["diagonal_speed"] = EngineNames.Number(SamusProjectileMotionDefinitions.DiagonalSpeed + slanted),
            ["inherits_speed"] = true,
            ["cooldown"] = SamusProjectileCooldowns.Read(plain, BeamShotCooldowns.Shot.Uncharged),
            ["held_cooldown"] = SamusProjectileCooldowns.Read(plain, BeamShotCooldowns.Shot.HeldFire),
            ["max_live"] = SamusProjectileSystem.PlaceCount,
            // Beams and missiles share the five places: a beam takes one however many projectiles show it.
            ["pool"] = ShotPool,
            ["places"] = 1,
            ["charge_updates"] = BeamChargeDefinitions.FullyChargedCounter,
            ["charge_needs"] = "charge_beam",
            ["part_charge_fires"] = true,
            ["charged_damage_times"] = SamusProjectileDamageDefinitions.ChargedBeamDamageMultiplier,
            ["charged_kind"] = "charged",
            ["charged_cooldown"] = SamusProjectileCooldowns.Read(plain, BeamShotCooldowns.Shot.Charged),
            ["sound"] = SoundOf(plain, charged: false),
            ["charged_sound"] = SoundOf(plain, charged: true),
            ["projectile"] = BeamProjectile(plain, plain, charged: false, "plain", passesWalls: false),
        };

        var variants = new JsonArray();
        foreach (SamusBeamCombination beams in SamusBeamCombinationKeys.ListedOrder)
        {
            if (!beams.CanBeEquipped || beams == plain)
                continue;
            if (SamusProjectileCooldowns.Read(beams, BeamShotCooldowns.Shot.Charged) != SamusProjectileCooldowns.Read(plain, BeamShotCooldowns.Shot.Charged) ||
                SamusProjectileCooldowns.Read(beams, BeamShotCooldowns.Shot.HeldFire) != SamusProjectileCooldowns.Read(plain, BeamShotCooldowns.Shot.HeldFire))
                throw new InvalidDataException($"The {beams} beam waits its own time after a charged or a held shot, which the weapon's one wait for each cannot say.");
            var when = new JsonArray();
            foreach ((SamusBeamFlags beam, string equipment, string _) in SamusBeams)
            {
                if ((beams.Components & beam) != SamusBeamFlags.None)
                    when.Add(equipment);
            }

            var variant = new JsonObject
            {
                ["when"] = when,
                ["damage"] = SamusProjectileDamageDefinitions.UnchargedBeamDamage[beams],
                ["kinds"] = Kinds(beams),
                ["cooldown"] = SamusProjectileCooldowns.Read(beams, BeamShotCooldowns.Shot.Uncharged),
                ["shots"] = Shots(BeamProjectile(beams, beams, charged: false, "plain", beams.HasWave)),
                ["charged_shots"] = Shots(BeamProjectile(beams, beams, charged: true, "charged", beams.HasWave)),
            };
            if (Sound(beams, charged: false) is { } sound)
                variant["sound"] = sound;
            if (Sound(beams, charged: true) is { } chargedSound)
                variant["charged_sound"] = chargedSound;
            variants.Add(variant);
        }

        // The power beam with the charge beam alone, for its charged shot. It comes after the beams that combine: of
        // shots naming as much of what she carries, the Godot game takes the first.
        variants.Add(new JsonObject
        {
            ["when"] = new JsonArray("charge_beam"),
            ["damage"] = SamusProjectileDamageDefinitions.UnchargedBeamDamage[plain],
            ["kinds"] = Kinds(plain),
            ["shots"] = Shots(BeamProjectile(plain, plain, charged: false, "plain", passesWalls: false)),
            ["charged_shots"] = Shots(BeamProjectile(plain, plain, charged: true, "charged", passesWalls: false)),
        });

        // The hyper beam: a charged plasma shot to whatever it strikes, with its own damage, drawn in the colours of the
        // wave and plasma beams it is given with, passing through walls as a wave beam does. It fires for as long as
        // the attack is held and cannot be charged. Its equipment is named with the beams it switches on, so that it
        // is the shot that names the most of what she carries.
        const SamusBeamCombination hyper = SamusBeamCombination.Plasma, hyperPictured = SamusBeamCombination.PlasmaWave;
        variants.Add(new JsonObject
        {
            ["when"] = new JsonArray("hyper_beam", "charge_beam", "wave_beam", "plasma_beam"),
            ["damage"] = SamusProjectileDamageDefinitions.HyperBeamDamage,
            ["kinds"] = Kinds(hyper, charged: true),
            ["cooldown"] = SamusProjectileCooldownDefinitions.HyperBeamShot,
            ["held_cooldown"] = SamusProjectileCooldownDefinitions.HyperBeamShot,
            ["charges"] = false,
            ["sound"] = SoundOf(hyper, charged: true),
            ["shots"] = Shots(BeamProjectile(hyper, hyperPictured, charged: true, "hyper", passesWalls: true, leavesTrail: false)),
        });
        weapon["variants"] = variants;
        _tree.WriteData(SamusWeaponFiles[0], weapon);
    }

    /// <summary>A shake of the room the old game starts, as a projectile's settings say one.</summary>
    private static JsonObject Shake(EarthquakeShake shake, int updates) => new()
    {
        ["strength"] = ShakeStrength(shake),
        ["updates"] = updates,
        ["way"] = ShakeWayName(shake),
        ["reach"] = ShakeReachName(shake),
    };

    /// <summary>Writes the missile or the super missile.</summary>
    private void WriteSamusMissile(bool super)
    {
        const SamusBeamCombination pictured = SamusBeamCombination.Power;
        SamusProjectileKind kind = SamusProjectileSelection.Missile(super);
        EngineSheetPictures pictures = ShotPictures(pictured);
        string name = super ? "super_missile" : "missile";
        Segment AnimationOf(SamusProjectileDirection direction) => SamusProjectileKinds.Animation(kind, direction);
        string timeline = WriteShotTimelines("samus/shots", name, pictures, AnimationOf);
        JsonObject projectile = FlyingShot(SamusProjectileKinds.Animation(kind, SamusProjectileDirection.Right), timeline);

        // A missile leaves its smoke every so often, a super missile twice as often after its first. The old game
        // draws the smoke from the sheet of the beams she carries, whose smoke is the same on every sheet.
        projectile["trail"] = ShotTrail(name, SamusProjectileType.Of(super ? SamusProjectileFamily.SuperMissile : SamusProjectileFamily.Missile),
                pictured, super ? ProjectileTimingDefinitions.SuperMissileTrailInterval : ProjectileTimingDefinitions.TrailInterval, FlyingWays(AnimationOf))
            ?? throw new InvalidDataException($"Samus's {name} leaves no trail, which the old game's does.");
        SamusDirectionAccelerations straight = SamusProjectileAccelerations.For(SamusProjectileDirection.Right);
        SamusDirectionAccelerations slanted = SamusProjectileAccelerations.For(SamusProjectileDirection.DownRight);

        // Once lit, a missile gains what a beam gains and then its own; a super missile only its own.
        projectile["acceleration"] = EngineNames.Number(super ? Along(straight.SuperMissile) : Along(straight.Beam) + Along(straight.Missile));
        projectile["diagonal_acceleration"] = EngineNames.Number(super ? Along(slanted.SuperMissile) : Along(slanted.Beam) + Along(slanted.Missile));
        projectile["meeting_terrain"] = "ends_and_hits";
        // A missile tests the terrain at its middle point, not across its width.
        projectile["terrain_by"] = "middle";
        projectile["meeting_target"] = "ends";
        string burst = $"samus/shots/{name}_burst.json";
        _tree.WriteData(burst, ShotTimeline(pictures, super ? Segment.SuperMissileExplosion : Segment.MissileExplosion, sized: false));
        projectile["ending_effect"] = burst;
        projectile["ending_effect_standing"] = "topmost";
        projectile["ending_sound"] = EngineNames.Sound(EnemySound.WeaponExplosion);
        if (super)
        {
            projectile["terrain_shake"] = Shake(SuperMissileDefinitions.WallImpactEarthquake, SuperMissileDefinitions.EarthquakeDuration);
            projectile["target_shake"] = Shake(SuperMissileDefinitions.EnemyHitEarthquake, SuperMissileDefinitions.EarthquakeDuration);
        }

        // The old game keeps a place among her shots for the unseen thing that trails a super missile, so she has
        // half as many of them out at once as that leaves room for.
        var weapon = new JsonObject
        {
            ["name"] = name,
            ["damage"] = SamusProjectileDamage.Read(kind),
            ["kinds"] = new JsonArray(name),

            // A missile leaves at the speed it lights at along each axis it flies on, straight or slanted.
            ["speed"] = EngineNames.Number(SamusProjectileMotionDefinitions.MissileIgnitionSpeed),
            ["diagonal_speed"] = EngineNames.Number(SamusProjectileMotionDefinitions.MissileIgnitionSpeed),
            ["inherits_speed"] = true,
            ["cooldown"] = super ? SamusProjectileCooldownDefinitions.SuperMissileShot : SamusProjectileCooldownDefinitions.MissileShot,
            // A super missile takes a second place for its unseen link: none is fired with four places held.
            ["max_live"] = SamusProjectileSystem.PlaceCount,
            ["pool"] = ShotPool,
            ["places"] = super ? SamusProjectileSystem.PlaceCount - SuperMissileDefinitions.AdmissionLimit + 1 : 1,
            ["costs"] = super ? "super_missiles" : "missiles",
            ["cost"] = 1,
            ["sound"] = EngineNames.Sound(super ? SamusProjectileSoundRoutingDefinitions.SuperMissileRequest : SamusProjectileSoundRoutingDefinitions.MissileRequest),
            ["projectile"] = projectile,
        };

        // A super missile is a class of its own: fast, it tries a second point against the terrain (the game's
        // SuperMissile), which the cartridge's unseen link is.
        if (super)
            weapon["scene"] = "res://game/samus/super_missile.tscn";
        _tree.WriteData(SamusWeaponFiles[super ? 2 : 1], weapon);
    }

    /// <summary>
    /// The timeline of a bomb's or a power bomb's fuse: its animation, and from so many updates before the end the
    /// quick one, picked up at the frame the slow one had reached.
    /// </summary>
    private JsonObject FuseTimeline(EngineSheetPictures pictures, Segment slow, Segment quick, bool restsAtEnd)
    {
        static List<(SamusProjectileFrameId Frame, int Updates)> Frames(Segment segment)
        {
            var frames = new List<(SamusProjectileFrameId, int)>();
            foreach (IAnimationBeat<SamusProjectileFrameId, NoAnimationCue> beat in SamusProjectileAnimations.Segments.Beats(segment))
            {
                frames.Add(beat is AnimationBeat<SamusProjectileFrameId, NoAnimationCue>.Frame shown
                    ? (shown.Shown, shown.Duration)
                    : throw new InvalidDataException($"A bomb's fuse has a beat this does not know: {beat.GetType().Name}."));
            }

            return frames;
        }

        List<(SamusProjectileFrameId Frame, int Updates)> slowFrames = Frames(slow), quickFrames = Frames(quick);
        if (slowFrames.Count != quickFrames.Count)
            throw new InvalidDataException("A bomb's quick fuse has not a frame for each of its slow one's.");
        int fuse = MorphBallBombDefinitions.InitialBombTimer, quickFrom = fuse - SamusPowerBombFuseData.FastAnimationTimer;
        var shown = new List<SamusProjectileFrameId>();
        int at = 0, left = slowFrames[0].Updates;
        for (int update = 0; update < fuse; update++)
        {
            if (update == quickFrom)
                left = Math.Min(left, quickFrames[at].Updates);
            shown.Add((update < quickFrom ? slowFrames : quickFrames)[at].Frame);
            if (--left == 0)
            {
                at = (at + 1) % slowFrames.Count;
                left = (update + 1 < quickFrom ? slowFrames : quickFrames)[at].Updates;
            }
        }

        var steps = new JsonArray();
        for (int update = 0; update < shown.Count;)
        {
            int run = 1;
            while (update + run < shown.Count && shown[update + run] == shown[update])
                run++;
            steps.Add(new JsonObject { ["picture"] = ShotPicture(pictures, shown[update]), ["updates"] = run });
            update += run;
        }

        Extent16<RoomPixels> radius = SamusProjectileFrameDefinitions.Radius(slowFrames[0].Frame);
        steps[0]!["cues"] = new JsonArray(string.Create(CultureInfo.InvariantCulture, $"size:{radius.X * 2}:{radius.Y * 2}"));
        var timeline = new JsonObject { ["steps"] = steps, ["offset"] = new JsonObject { ["x"] = pictures.OffsetX, ["y"] = pictures.OffsetY } };
        if (restsAtEnd)
            timeline["loops_to"] = steps.Count - 1;
        return timeline;
    }

    /// <summary>Writes the bomb and the power bomb, and the power bomb's blast.</summary>
    private void WriteSamusBombs()
    {
        const SamusBeamCombination pictured = SamusBeamCombination.Power;
        EngineSheetPictures pictures = ShotPictures(pictured);
        SamusProjectileKind bomb = SamusProjectileSelection.Bomb(SamusBombKind.Bomb), powerBomb = SamusProjectileSelection.Bomb(SamusBombKind.PowerBomb);

        // A bomb lies until its fuse is out and leaves its burst, which hurts what it reaches as it swells. The bomb
        // itself strikes the room's blocks as it goes.
        const string fuse = "samus/shots/bomb.json", burst = "samus/shots/bomb_burst.json", burstTimeline = "samus/shots/bomb_burst_timeline.json";
        _tree.WriteData(fuse, FuseTimeline(pictures, Segment.Bomb, Segment.FastBomb, restsAtEnd: false));
        _tree.WriteData(burstTimeline, ShotTimeline(pictures, Segment.BombExplosion, sized: true));
        (int burstWidth, int burstHeight) = FirstShotSize(Segment.BombExplosion);
        _tree.WriteData(burst, new JsonObject
        {
            ["damage"] = SamusProjectileDamage.Read(bomb),
            ["kinds"] = new JsonArray("bomb"),
            ["projectile"] = new JsonObject
            {
                ["width"] = burstWidth,
                ["height"] = burstHeight,
                ["motion"] = "still",
                ["standing"] = "topmost",
                ["meeting_terrain"] = "passes_through",
                ["meeting_target"] = "passes_through",
                // What it does no harm shows the dud where the bomb lies, and the blast plays on.
                ["outlasts_dud"] = true,
                ["view_margin"] = -1,
                ["timeline"] = burstTimeline,
            },
        });
        (int width, int height) = FirstShotSize(Segment.Bomb);
        _tree.WriteData(SamusWeaponFiles[3], new JsonObject
        {
            ["name"] = "bomb",
            ["damage"] = SamusProjectileDamage.Read(bomb),
            ["kinds"] = new JsonArray("bomb"),
            ["speed"] = 0,
            ["cooldown"] = MorphBallBombDefinitions.NormalBombCooldown,
            ["max_live"] = MorphBallBombDefinitions.PlaceCount,
            ["requires"] = "bombs",
            ["scene"] = "res://game/samus/bomb.tscn",
            ["projectile"] = new JsonObject
            {
                ["width"] = width,
                ["height"] = height,
                ["motion"] = "still",
                ["standing"] = "topmost",
                ["delivers_hit"] = false,
                ["meeting_terrain"] = "passes_through",
                ["lasts_updates"] = MorphBallBombDefinitions.InitialBombTimer,
                ["view_margin"] = -1,
                ["timeline"] = fuse,
                ["ending_burst"] = burst,
                ["ending_sound"] = EngineNames.Sound(EnemySound.WeaponExplosion),
            },
        });

        // A power bomb lies until its fuse is out; its blast is its own class's. One at a time.
        const string powerFuse = "samus/shots/power_bomb.json";
        _tree.WriteData(powerFuse, FuseTimeline(pictures, Segment.PowerBomb, Segment.FastPowerBomb, restsAtEnd: true));
        (int powerWidth, int powerHeight) = FirstShotSize(Segment.PowerBomb);
        _tree.WriteData(SamusWeaponFiles[4], new JsonObject
        {
            ["name"] = "power_bomb",
            ["damage"] = SamusProjectileDamage.Read(powerBomb),
            ["kinds"] = new JsonArray("power_bomb"),
            ["speed"] = 0,
            ["cooldown"] = MorphBallBombDefinitions.PowerBombCooldown,
            ["max_live"] = 1,
            ["costs"] = "power_bombs",
            ["cost"] = 1,
            ["scene"] = "res://game/samus/power_bomb.tscn",
            ["projectile"] = new JsonObject
            {
                ["width"] = powerWidth,
                ["height"] = powerHeight,
                ["motion"] = "still",
                ["standing"] = "topmost",
                ["delivers_hit"] = false,
                ["meeting_terrain"] = "passes_through",
                ["view_margin"] = -1,
                ["timeline"] = powerFuse,
            },
        });

        // The blast: how far it reaches on each update, as the old blast's own steps give it, from the update after
        // the fuse runs out until its glow has faded. The step that ends it is its cleanup, not a reach: the bomb ends
        // on that update, the one after its last reach.
        var blast = new PowerBombBlastTimeline();
        InstalledPowerBombGlowColors colours = _installation.LoadMaps().PowerBombGlowColors;
        // What is seen of it on each of those updates: the oval it shows, and the colour it adds inside it.
        static int Glow(RgbColor colour) => (colour.Red << 16) | (colour.Green << 8) | colour.Blue;
        blast.Start();
        var reaches = new JsonArray();
        var ovals = new JsonArray();
        var glows = new JsonArray();
        blast.StepFrame(colours);
        while (blast.IsRunning)
        {
            reaches.Add(blast.ExplosionRadius.WholePixels);
            ovals.Add(blast.ShownRadius);
            glows.Add(Glow(blast.GlowColor));
            if (reaches.Count > 4096)
                throw new InvalidDataException("The old power bomb's blast does not end.");
            blast.StepFrame(colours);
        }

        // The blast a crystal flash starts: it reaches its first reach on the update it begins, and then as its own
        // steps give it, until its glow has faded.
        blast.BeginCrystalFlash();
        var flashReaches = new JsonArray { blast.ExplosionRadius.WholePixels };
        var flashOvals = new JsonArray { blast.ShownRadius };
        var flashGlows = new JsonArray { Glow(blast.GlowColor) };
        blast.StepFrame(colours);
        while (blast.IsRunning)
        {
            flashReaches.Add(blast.ExplosionRadius.WholePixels);
            flashOvals.Add(blast.ShownRadius);
            flashGlows.Add(Glow(blast.GlowColor));
            if (flashReaches.Count > 4096)
                throw new InvalidDataException("The old crystal flash's blast does not end.");
            blast.StepFrame(colours);
        }

        new EngineObjectFile(_tree, "samus/power_bomb.json", "PowerBomb")
            .Value("fuse_updates", MorphBallBombDefinitions.InitialBombTimer)
            .Value("blast_reaches", reaches)
            .Value("flash_blast_reaches", flashReaches)
            .Value("blast_ovals", ovals)
            .Value("blast_glows", glows)
            .Value("flash_blast_ovals", flashOvals)
            .Value("flash_blast_glows", flashGlows)
            .Value("height_parts", PowerBombExplosionDefinitions.OvalHeightParts)
            .Value("width_parts", PowerBombExplosionDefinitions.OvalWidthParts)
            .Value("rehit_after", EnemyCollisionDefinitions.PowerBombInvincibilityUpdates)
            .Write();
    }

    /// <summary>The data file of the shot of one of the special attacks' lights.</summary>
    private static string SpecialAttackFile(SamusComboKind kind) => $"samus/shots/special_attack_{EngineSheets.FileName(kind.ToString())}.json";

    /// <summary>The data file of the shot of the spazer special attack's trailing pair of lights.</summary>
    private const string SpazerAttackTrailFile = "samus/shots/special_attack_spazer_trail.json";

    /// <summary>
    /// Writes the shot of the spazer special attack's trailing pair: from the start they are the spazer echo, an
    /// uncharged spazer shot with its own damage and pictures, the lights' pictures facing down and left, in the
    /// colours of <paramref name="pictured"/>, the beams of the attack.
    /// </summary>
    private void WriteSpazerAttackTrail(EngineSheetPictures pictures, SamusBeamCombination pictured)
    {
        SamusProjectileKind trail = SamusProjectileSelection.Echo(SamusBeamEcho.SpazerComboTrail);
        SamusBeamCombination beams = SamusProjectileType.EchoOf(SamusBeamEcho.SpazerComboTrail).BeamCombination;
        Segment segment = SamusProjectileKinds.Animation(trail, SamusProjectileDirection.DownFacingLeft);
        string timeline = SpazerAttackTrailFile[..SpazerAttackTrailFile.LastIndexOf('.')] + "_timeline.json";
        _tree.WriteData(timeline, ShotTimeline(pictures, segment, sized: true));
        (int width, int height) = FirstShotSize(segment);
        var kinds = new JsonArray("beam");
        foreach ((SamusBeamFlags one, string _, string named) in SamusBeams)
        {
            if ((beams.Components & one) != SamusBeamFlags.None)
                kinds.Add(named);
        }

        // The pair leave ice sparkles by their own lists, as they are launched downward and as all four fall. Both
        // sparkles run the left one's animation, whose falls move the left sparkle, so it falls twice and the right
        // one not at all.
        string trailFile = ShotTrail(Path.GetFileNameWithoutExtension(SpazerAttackTrailFile), SamusProjectileType.EchoOf(SamusBeamEcho.SpazerComboTrail),
                pictured, ProjectileTimingDefinitions.TrailInterval,
                [new ShotWay(null, SamusProjectileDirection.DownFacingLeft, null, segment)])
            ?? throw new InvalidDataException("The spazer special attack's trailing pair leaves no trail, which the old game's does.");
        _tree.WriteData(SpazerAttackTrailFile, new JsonObject
        {
            ["damage"] = SamusProjectileDamage.Read(trail),
            ["kinds"] = kinds,
            ["scene"] = $"res://game/samus/special_attack_{EngineSheets.FileName(nameof(SamusComboKind.Spazer))}.tscn",
            ["projectile"] = new JsonObject
            {
                ["width"] = width,
                ["height"] = height,
                ["meeting_terrain"] = "passes_through",
                ["meeting_target"] = beams.HasPlasma ? "passes_through_and_rehits" : "ends",
                ["view_margin"] = -1,
                // A light of a special attack is a shot of hers, and lies where her shots do.
                ["standing"] = "just_behind",
                ["timeline"] = timeline,
                ["trail"] = trailFile,
            },
        });
    }

    /// <summary>Writes the shots of the four special attacks and of a shinespark's echoes.</summary>
    private void WriteSamusSpecialShots()
    {
        foreach ((SamusBeamFlags beam, SamusComboKind kind) in SamusComboDefinitions.KindsOfSingleBeams)
        {
            SamusBeamCombination beams = SamusBeamCombinations.Of(beam);
            EngineSheetPictures pictures = ShotPictures(beams);

            // The ice attack's lights are charged ice shots; the others have a shot of their own.
            SamusProjectileKind shot = kind == SamusComboKind.Ice
                ? SamusProjectileSelection.BeamShot(beams, charged: true)
                : SamusProjectileSelection.SpecialAttackParticles(beams);
            Segment segment = kind == SamusComboKind.Ice
                ? SamusProjectileKinds.Animation(shot, SamusProjectileDirection.UpFacingRight)
                : SamusProjectileKinds.SoleAnimation(shot);
            string file = SpecialAttackFile(kind), timeline = file[..file.LastIndexOf('.')] + "_timeline.json";
            _tree.WriteData(timeline, ShotTimeline(pictures, segment, sized: true));
            (int width, int height) = FirstShotSize(segment);
            var kinds = new JsonArray("beam");
            foreach ((SamusBeamFlags one, string _, string named) in SamusBeams)
            {
                if (one == beam)
                    kinds.Add(named);
            }

            kinds.Add("charged");
            var projectile = new JsonObject
            {
                ["width"] = width,
                ["height"] = height,
                ["meeting_terrain"] = "passes_through",
                ["meeting_target"] = beams.HasPlasma ? "passes_through_and_rehits" : "ends",
                ["view_margin"] = -1,
                ["standing"] = "just_behind",
                ["timeline"] = timeline,
            };

            // The ice and wave lights leave the trail of their charged beam, read as shots fired straight up. The
            // plasma lights leave none, and the spazer lights only as the trailing pair's shot.
            if (kind is SamusComboKind.Ice or SamusComboKind.Wave)
            {
                projectile["trail"] = ShotTrail(Path.GetFileNameWithoutExtension(file), SamusProjectileType.Beam(beams, charged: true), beams,
                        ProjectileTimingDefinitions.TrailInterval, [new ShotWay(null, SamusProjectileDirection.UpFacingRight, null, segment)])
                    ?? throw new InvalidDataException($"The {kind} special attack's lights leave no trail, which the old game's do.");
            }

            _tree.WriteData(file, new JsonObject
            {
                ["damage"] = SamusProjectileDamage.Read(shot),
                ["kinds"] = kinds,
                ["sound"] = EngineNames.Sound(BeamComboDefinitions.ActivationSounds[kind]),
                ["scene"] = $"res://game/samus/special_attack_{EngineSheets.FileName(kind.ToString())}.tscn",
                ["projectile"] = projectile,
            });

            if (kind == SamusComboKind.Spazer)
                WriteSpazerAttackTrail(pictures, beams);
        }

        // A shinespark's echoes hit as a plasma and wave beam's shot of their own great damage. The old game draws
        // nothing for the shot itself.
        SamusProjectileKind echo = SamusProjectileSelection.Echo(SamusBeamEcho.ShinesparkEcho);
        SamusBeamCombination echoes = SamusProjectileType.EchoOf(SamusBeamEcho.ShinesparkEcho).BeamCombination;
        (int echoWidth, int echoHeight) = FirstShotSize(SamusProjectileKinds.Animation(echo, SamusProjectileDirection.Right));
        var echoKinds = new JsonArray("beam");
        foreach ((SamusBeamFlags one, string _, string named) in SamusBeams)
        {
            if ((echoes.Components & one) != SamusBeamFlags.None)
                echoKinds.Add(named);
        }

        _tree.WriteData(ShinesparkEchoFile, new JsonObject
        {
            ["damage"] = SamusProjectileDamage.Read(echo),
            ["kinds"] = echoKinds,
            ["scene"] = "res://game/samus/shinespark_echo.tscn",
            ["projectile"] = new JsonObject
            {
                ["width"] = echoWidth,
                ["height"] = echoHeight,
                ["meeting_terrain"] = "passes_through",
                ["meeting_target"] = echoes.HasPlasma ? "passes_through_and_rehits" : "ends",
                ["view_margin"] = -1,
                // An echo is a shot of hers, and lies where her shots do.
                ["standing"] = "just_behind",
            },
        });
    }
}
