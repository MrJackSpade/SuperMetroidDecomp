using SuperMetroid.AssetExtraction.Bosses.Crocomire.Artwork;
using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Areas.Ceres.Artwork;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Crocomire.Artwork;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Bosses.Torizo.Artwork;
using L = SuperMetroid.Core.Bosses.Torizo.TorizoArtworkLook;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Gunship;
using SuperMetroid.Core.Enemies.Gunship.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Enemies.Projectiles.Artwork;
using SuperMetroid.Core.Enemies.SpriteObjects;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// Which frames each encounter draws in which of its looks (#1513, #1538). A sheet holds a frame's finished picture of each
/// look it is drawn in, and a frame drawn alike in every look it is drawn in has one picture. A frame no entry lists for
/// an encounter shows only its room's load under it. A look whose copies land in the same update as a later look's is
/// never seen and is listed for no frame. Ceres Ridley's escape warning shows through his room's backgrounds alone, so no
/// frame is listed for it.
/// </summary>
internal static class EncounterArtworkDrawingDefinitions
{
    /// <summary>The frames Mother Brain's dead brain shows as a corpse, the only frames the game shows her corpse's rot in.</summary>
    internal static readonly EnemyFrameId[] MotherBrainCorpseFrames =
    [
        EnemyFrameId.MotherBrainHeadDecapitatedLoop,
        EnemyFrameId.MotherBrainHeadDecapitatedSecond,
        EnemyFrameId.MotherBrainHeadStretchingPhaseThreeFirst,
        EnemyFrameId.MotherBrainHeadCorpseFourth,
        EnemyFrameId.MotherBrainHeadCorpseLoop,
    ];

    /// <summary>
    /// Draygon's Evirs: her opening readies them at her intro's first update; the opening Evirs spawn right after it, and
    /// the Evirs her death releases long after, so none is drawn before it.
    /// </summary>
    private static readonly EncounterArtworkDrawing DraygonEvirs =
        new(ComposedEncounter.DraygonEvirs, [EncounterLookUploads.DraygonEvirsLook])
        {
            SpriteObjects = [RoomSpriteObjectKind.DraygonIntroEvir, RoomSpriteObjectKind.DraygonDeathEvirFacingRight],
        };

    /// <summary>The gunship's hull, drawn landed and through its takeoff.</summary>
    private static readonly EncounterArtworkDrawing GunshipHull =
        new(ComposedEncounter.Gunship, [EncounterLook.LoadedName, GunshipTakeoffDefinitions.TakeoffLook.Name])
        {
            Frames = [.. GunshipAnimations.Segments.Frames],
        };

    /// <summary>The takeoff's dust, which the engines raise once they fire up: never while the gunship is landed.</summary>
    private static readonly EncounterArtworkDrawing GunshipDust =
        new(ComposedEncounter.Gunship, [GunshipTakeoffDefinitions.TakeoffLook.Name])
        {
            Projectiles = [.. GunshipDustVisualDefinitions.ShownProjectileFrames.Select(entry => entry.Value)],
        };

    /// <summary>Crocomire's skeleton, shown only once his death has bared it.</summary>
    private static readonly EncounterArtworkDrawing CrocomireSkeleton =
        new(ComposedEncounter.Crocomire, [CrocomireSkeletonLookDefinitions.SkeletonLook.Name])
        {
            Frames = [.. CrocomireSkeletonVisualDefinitions.Frames],
        };

    /// <summary>Crocomire's projectiles: his fight's before his death, and the spike wall's pieces after the skeleton shows.</summary>
    private static readonly EncounterArtworkDrawing CrocomireProjectiles =
        new(ComposedEncounter.Crocomire, [EncounterLook.LoadedName, CrocomireSkeletonLookDefinitions.SkeletonLook.Name])
        {
            Projectiles = [.. CrocomireProjectileVisualDefinitions.ShownProjectileFrames.Select(entry => entry.Value)],
        };

    /// <summary>
    /// Bomb Torizo's body: drawn as a statue and as its eyes light up and flicker back on; its gut's looks land in one update,
    /// seen blown up; its face's looks land in two updates, seen half blown away and blown away.
    /// </summary>
    private static readonly EncounterArtworkDrawing BombTorizoBody =
        new(ComposedEncounter.BombTorizo, [EncounterLook.LoadedName, .. Bomb(L.EyesLightingUp1, L.EyesLightingUp2, L.EyesLightingUp3, L.EyesLightingUp4,
            L.EyesLightingAgain1, L.EyesLightingAgain2, L.EyesLightingAgain3, L.EyesLightingAgain4, L.GutBlownUp, L.FaceBlowingUp4, L.FaceBlownUp)])
        {
            Frames = [.. TorizoAnimations.Segments.Frames],
        };

    /// <summary>Bomb Torizo's projectiles, which fly only once it has woken.</summary>
    private static readonly EncounterArtworkDrawing BombTorizoProjectiles =
        new(ComposedEncounter.BombTorizo, [.. Bomb(L.EyesLightingAgain4, L.GutBlownUp, L.FaceBlowingUp4, L.FaceBlownUp)])
        {
            Projectiles = [.. TorizoProjectileVisualDefinitions.ShownProjectileFrames.Select(entry => entry.Value)],
        };

    /// <summary>
    /// Golden Torizo's body and projectiles: its awakening is its first program's first cue, before any frame shows; it is
    /// drawn as its eyes light up and flicker back on, its chest's looks land two to an update, each seen once both its
    /// halves have, and its eye beam's glow and its eyes lighting up after a stun are seen at every look.
    /// </summary>
    private static readonly EncounterArtworkDrawing GoldenTorizo =
        new(ComposedEncounter.GoldenTorizo, [.. Golden(L.Awakening, L.EyesLightingUp1, L.EyesLightingUp2, L.EyesLightingUp3, L.EyesLightingUp4,
            L.EyesLightingAgain1, L.EyesLightingAgain2, L.EyesLightingAgain3, L.EyesLightingAgain4,
            L.ChestOpening1, L.ChestOpening2, L.ChestOpen, L.ChestHeldOpen, L.ChestClosing1, L.ChestClosing2, L.ChestClosed,
            L.EyeBeamGlow1, L.EyeBeamGlow2, L.EyeBeamGlow3, L.EyeBeamGlow4,
            L.EyesLightingAfterStun1, L.EyesLightingAfterStun2, L.EyesLightingAfterStun3, L.EyesLightingAfterStun4)])
        {
            Frames = [.. TorizoAnimations.Segments.Frames],
            Projectiles = [.. TorizoProjectileVisualDefinitions.ShownProjectileFrames.Select(entry => entry.Value)],
        };

    /// <summary>
    /// The Ceres doors but the rotating overlay: the pre-explosion overlay door shows its look at its initialization, before
    /// its first draw; the doors of the set's other rooms are drawn as their load leaves them.
    /// </summary>
    private static readonly EncounterArtworkDrawing CeresDoors =
        new(ComposedEncounter.CeresDoor, [EncounterLook.LoadedName, CeresDoorLookDefinitions.OverlayLook.Name])
        {
            Frames = [.. CeresDoorAnimations.Segments.Frames.Except([EnemyFrameId.CeresDoorRotatingOverlay])],
        };

    /// <summary>
    /// The Ceres doors' rotating overlay ($A6:F921), drawn only by the elevator shaft's pre-explosion overlay door (enemy
    /// parameter 2, $A6:F610), whose initialization copies the overlay's characters over the sprite characters from $100
    /// ($A6:F739) before the frame first draws (#1641). It is never drawn as a room's load leaves those characters: in Ceres
    /// Ridley's arena they are Ridley's own.
    /// </summary>
    private static readonly EncounterArtworkDrawing CeresDoorRotatingOverlay =
        new(ComposedEncounter.CeresDoor, [CeresDoorLookDefinitions.OverlayLook.Name])
        {
            Frames = [EnemyFrameId.CeresDoorRotatingOverlay],
        };

    /// <summary>The faces of Mother Brain's dead brain, which her legs' look holds.</summary>
    private static readonly EnemyFrameId[] MotherBrainDeadBrainFaces =
        [EnemyFrameId.MotherBrainHeadDecapitatedLoop, EnemyFrameId.MotherBrainHeadDecapitatedSecond, EnemyFrameId.MotherBrainHeadStretchingPhaseThreeFirst];

    /// <summary>Her collapsed corpse, which her corpse's look holds.</summary>
    private static readonly EnemyFrameId[] MotherBrainCollapsedCorpse = [EnemyFrameId.MotherBrainHeadCorpseFourth, EnemyFrameId.MotherBrainHeadCorpseLoop];

    /// <summary>The faces of her living brain her legs' look holds; her other living faces are her room's load.</summary>
    private static readonly EnemyFrameId[] MotherBrainAttackFaces = [EnemyFrameId.MotherBrainHeadStretchingPhaseThreeSecond, EnemyFrameId.MotherBrainHeadStretchingPhaseThreeThird];

    /// <summary>Her onion rings, whose characters her legs' look holds.</summary>
    private static readonly EnemyProjectileFrameId[] MotherBrainRings =
    [
        EnemyProjectileFrameId.MotherBrainBlueRingOnionRingFirst, EnemyProjectileFrameId.MotherBrainBlueRingOnionRingSecond, EnemyProjectileFrameId.MotherBrainBlueRingOnionRingThird,
        EnemyProjectileFrameId.MotherBrainBlueRingOnionRingFourth, EnemyProjectileFrameId.MotherBrainBlueRingOnionRingFifth, EnemyProjectileFrameId.MotherBrainBlueRingOnionRingSixth,
    ];

    /// <summary>Her bombs, which her attacks' look holds.</summary>
    private static readonly EnemyProjectileFrameId[] MotherBrainAttackEffects =
    [
        EnemyProjectileFrameId.MotherBrainBombFirst, EnemyProjectileFrameId.MotherBrainBombSecond, EnemyProjectileFrameId.MotherBrainBombThird,
        EnemyProjectileFrameId.MotherBrainBombFourth, EnemyProjectileFrameId.MotherBrainBombFifth, EnemyProjectileFrameId.MotherBrainBombSixth,
        EnemyProjectileFrameId.MotherBrainBombSeventh, EnemyProjectileFrameId.MotherBrainBombEighth, EnemyProjectileFrameId.MotherBrainBombNinth,
    ];

    /// <summary>The escape door's fragments, which the exploded door's look holds.</summary>
    private static readonly EnemyProjectileFrameId[] MotherBrainDoorFragments =
    [
        EnemyProjectileFrameId.MotherBrainEscapeDoorParticleFirst, EnemyProjectileFrameId.MotherBrainEscapeDoorParticleSecond, EnemyProjectileFrameId.MotherBrainEscapeDoorParticleThird,
        EnemyProjectileFrameId.MotherBrainEscapeDoorParticleFourth, EnemyProjectileFrameId.MotherBrainEscapeDoorParticleFifth, EnemyProjectileFrameId.MotherBrainEscapeDoorParticleSixth,
        EnemyProjectileFrameId.MotherBrainEscapeDoorParticleSeventh, EnemyProjectileFrameId.MotherBrainEscapeDoorParticleEighth,
    ];

    /// <summary>The escape's subtitle, which the escape message's look holds.</summary>
    private static readonly EnemyProjectileFrameId[] MotherBrainSubtitle = [EnemyProjectileFrameId.MotherBrainEscapeSubtitle];

    /// <summary>Mother Brain's living brain, whose faces her room's load holds.</summary>
    private static readonly EncounterArtworkDrawing MotherBrainHead =
        new(ComposedEncounter.MotherBrain, [EncounterLook.LoadedName])
        {
            Frames = [.. MotherBrainVisualDefinitions.HeadFrames.Select(entry => entry.Value).Except(MotherBrainCorpseFrames).Except(MotherBrainAttackFaces),
                MotherBrainVisualDefinitions.NeckJointFrame],
        };

    /// <summary>The faces of her living brain her legs' look holds.</summary>
    private static readonly EncounterArtworkDrawing MotherBrainHeadAttackFaces =
        new(ComposedEncounter.MotherBrain, [EncounterLookUploads.MotherBrainLegsLook])
        {
            Frames = MotherBrainAttackFaces,
            ArtworkOfLook = true,
        };

    /// <summary>Mother Brain's dead brain's faces, which her legs' look holds.</summary>
    private static readonly EncounterArtworkDrawing MotherBrainDeadBrain =
        new(ComposedEncounter.MotherBrain, [EncounterLookUploads.MotherBrainLegsLook])
        {
            Frames = MotherBrainDeadBrainFaces,
            ArtworkOfLook = true,
        };

    /// <summary>Her collapsed corpse, which her corpse's look holds.</summary>
    private static readonly EncounterArtworkDrawing MotherBrainCorpse =
        new(ComposedEncounter.MotherBrain, [EncounterLookUploads.MotherBrainCorpseLook])
        {
            Frames = MotherBrainCollapsedCorpse,
            ArtworkOfLook = true,
        };

    /// <summary>
    /// Mother Brain's body sprites, which her legs' look holds; a glitched fight draws them in phase one, as the Mother Brain
    /// glitch movie does, in that same look.
    /// </summary>
    private static readonly EncounterArtworkDrawing MotherBrainBody =
        new(ComposedEncounter.MotherBrain, [EncounterLookUploads.MotherBrainLegsLook])
        {
            Frames = [.. MotherBrainBodyVisualDefinitions.ShownFrames.Select(entry => entry.Value)],
            ArtworkOfLook = true,
        };

    /// <summary>The Baby, which the Baby's look holds.</summary>
    private static readonly EncounterArtworkDrawing BabyMetroid =
        new(ComposedEncounter.MotherBrain, [EncounterLookUploads.MotherBrainBabyLook])
        {
            Frames = [.. BabyMetroidVisualDefinitions.ShownFrames.Select(entry => entry.Value)],
            ArtworkOfLook = true,
        };

    /// <summary>Her onion rings, which her legs' look holds.</summary>
    private static readonly EncounterArtworkDrawing MotherBrainRingsDrawn =
        new(ComposedEncounter.MotherBrain, [EncounterLookUploads.MotherBrainLegsLook])
        {
            Projectiles = MotherBrainRings,
            ArtworkOfLook = true,
        };

    /// <summary>Her bombs, which her attacks' look holds.</summary>
    private static readonly EncounterArtworkDrawing MotherBrainAttacks =
        new(ComposedEncounter.MotherBrain, [EncounterLookUploads.MotherBrainAttackLook])
        {
            Projectiles = MotherBrainAttackEffects,
            ArtworkOfLook = true,
        };

    /// <summary>Mother Brain's other effects, her breath, drool and blasts, which her room's load holds.</summary>
    private static readonly EncounterArtworkDrawing MotherBrainEffects =
        new(ComposedEncounter.MotherBrain, [EncounterLook.LoadedName])
        {
            Projectiles = [.. MotherBrainEffectAnimations.Segments.Frames
                .Except(MotherBrainRings).Except(MotherBrainAttackEffects).Except(MotherBrainDoorFragments).Except(MotherBrainSubtitle)],
        };

    /// <summary>The escape door's fragments, which the exploded door's look holds.</summary>
    private static readonly EncounterArtworkDrawing MotherBrainDoorFragmentsDrawn =
        new(ComposedEncounter.MotherBrain, [EncounterLookUploads.MotherBrainExplodedDoorLook])
        {
            Projectiles = MotherBrainDoorFragments,
            ArtworkOfLook = true,
        };

    /// <summary>The escape's subtitle, which the escape message's look holds.</summary>
    private static readonly EncounterArtworkDrawing MotherBrainSubtitleDrawn =
        new(ComposedEncounter.MotherBrain, [EncounterLookUploads.MotherBrainEscapeTextLook])
        {
            Projectiles = MotherBrainSubtitle,
            ArtworkOfLook = true,
        };

    /// <summary>Mother Brain's red hand beam, which only her second phase fires and her attacks' look holds.</summary>
    private static readonly EncounterArtworkDrawing MotherBrainHandBeam =
        new(ComposedEncounter.MotherBrain, [EncounterLookUploads.MotherBrainAttackLook])
        {
            Projectiles = [.. MotherBrainHandBeamVisualDefinitions.ShownProjectileFrames.Select(entry => entry.Value)],
            ArtworkOfLook = true,
        };

    /// <summary>The dust rising from Mother Brain's death, her corpse's rot and the exploding escape door, which her room's load holds.</summary>
    private static readonly EncounterArtworkDrawing MotherBrainDust =
        new(ComposedEncounter.MotherBrain, [EncounterLook.LoadedName])
        {
            Projectiles = [.. MiscDustProjectileAnimations.Segments.Frames],
        };

    /// <summary>Every entry, by encounter.</summary>
    internal static IReadOnlyList<EncounterArtworkDrawing> All { get; } =
    [
        DraygonEvirs,
        GunshipHull,
        GunshipDust,
        CrocomireSkeleton,
        CrocomireProjectiles,
        BombTorizoBody,
        BombTorizoProjectiles,
        GoldenTorizo,
        CeresDoors,
        CeresDoorRotatingOverlay,
        MotherBrainHead,
        MotherBrainHeadAttackFaces,
        MotherBrainDeadBrain,
        MotherBrainCorpse,
        MotherBrainBody,
        BabyMetroid,
        MotherBrainRingsDrawn,
        MotherBrainAttacks,
        MotherBrainEffects,
        MotherBrainDoorFragmentsDrawn,
        MotherBrainSubtitleDrawn,
        MotherBrainHandBeam,
        MotherBrainDust,
    ];

    /// <summary>The names of Bomb Torizo's <paramref name="looks"/>.</summary>
    private static IEnumerable<string> Bomb(params TorizoArtworkLook[] looks) => looks.Select(look => look.Of(EncounterArtwork.BombTorizo).Name);

    /// <summary>The names of Golden Torizo's <paramref name="looks"/>.</summary>
    private static IEnumerable<string> Golden(params TorizoArtworkLook[] looks) => looks.Select(look => look.Of(EncounterArtwork.GoldenTorizo).Name);
}
