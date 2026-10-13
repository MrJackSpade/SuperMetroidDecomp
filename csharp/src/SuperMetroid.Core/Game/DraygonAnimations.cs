using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using P = SuperMetroid.Core.Game.DraygonProgram;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Draygon's body, eye, tail and arms programs, in the order the cartridge authors them in bank $A5. The four parts
/// run programs of one set: a body cue starts all four, and the arms' dying program runs on into the body's.
/// </summary>
public enum DraygonProgram
{
    /// <summary><c>InstList_Draygon_Sleep</c> (<c>$A5:97B9</c>).</summary>
    Sleep,
    /// <summary><c>InstList_DraygonBody_FacingLeft_Reset</c> (<c>$A5:97BB</c>).</summary>
    BodyFacingLeftReset,
    /// <summary><c>InstList_DraygonBody_FacingRight_Reset</c> (<c>$A5:97D1</c>).</summary>
    BodyFacingRightReset,
    /// <summary><c>InstList_DraygonArms_FacingLeft_Idle_0</c> (<c>$A5:97E7</c>).</summary>
    ArmsFacingLeftIdle,
    /// <summary><c>InstList_DraygonArms_FacingLeft_NearSwoopApex</c> (<c>$A5:9813</c>).</summary>
    ArmsFacingLeftNearSwoopApex,
    /// <summary><c>InstList_DraygonArms_FacingLeft_Grab</c> (<c>$A5:9845</c>).</summary>
    ArmsFacingLeftGrab,
    /// <summary><c>InstList_DraygonArms_FacingLeft_Dying</c> (<c>$A5:9867</c>).</summary>
    ArmsFacingLeftDying,
    /// <summary><c>InstList_DraygonBody_FacingLeft_Idle</c> (<c>$A5:9889</c>).</summary>
    BodyFacingLeftIdle,
    /// <summary><c>InstList_DraygonBody_Dying_0</c> (<c>$A5:989B</c>).</summary>
    BodyDying,
    /// <summary><c>InstList_DraygonBody_Dying_1</c> (<c>$A5:98A5</c>).</summary>
    BodyDyingExplosions,
    /// <summary><c>InstList_DraygonBody_Dying_2</c> (<c>$A5:98BF</c>).</summary>
    BodyDyingExplosionsLoop,
    /// <summary><c>InstList_Draygon_Delete</c> (<c>$A5:98ED</c>).</summary>
    Delete,
    /// <summary><c>InstList_DraygonBody_FacingLeft_FireGoop</c> (<c>$A5:98FE</c>).</summary>
    BodyFacingLeftFireGoop,
    /// <summary><c>InstList_DraygonBody_FacingLeft_Roar</c> (<c>$A5:9922</c>).</summary>
    BodyFacingLeftRoar,
    /// <summary><c>InstList_DraygonEye_FacingLeft_Idle</c> (<c>$A5:9944</c>).</summary>
    EyeFacingLeftIdle,
    /// <summary><c>InstList_DraygonEye_FacingLeft_Dying_0</c> (<c>$A5:997A</c>).</summary>
    EyeFacingLeftDying,
    /// <summary><c>InstList_DraygonEye_FacingLeft_Dying_1</c> (<c>$A5:997E</c>).</summary>
    EyeFacingLeftDyingLoop,
    /// <summary><c>InstList_DraygonEye_FacingLeft_Dead</c> (<c>$A5:999C</c>).</summary>
    EyeFacingLeftDead,
    /// <summary><c>InstList_DraygonEye_FacingLeft_LookingLeft</c> (<c>$A5:99AE</c>).</summary>
    EyeFacingLeftLookingLeft,
    /// <summary><c>InstList_DraygonEye_FacingLeft_LookingRight</c> (<c>$A5:99B4</c>).</summary>
    EyeFacingLeftLookingRight,
    /// <summary><c>InstList_DraygonEye_FacingLeft_LookingUp</c> (<c>$A5:99BA</c>).</summary>
    EyeFacingLeftLookingUp,
    /// <summary><c>InstList_DraygonEye_FacingLeft_LookingDown</c> (<c>$A5:99C0</c>).</summary>
    EyeFacingLeftLookingDown,
    /// <summary><c>InstList_DraygonTail_FacingLeft_Idle_0</c> (<c>$A5:99C6</c>).</summary>
    TailFacingLeftIdle,
    /// <summary><c>InstList_DraygonTail_FacingLeft_FakeTailWhip</c> (<c>$A5:99FC</c>).</summary>
    TailFacingLeftInitialFakeWhip,
    /// <summary><c>InstList_DraygonTail_FacingLeft_FinalTailWhips_0</c> (<c>$A5:9A68</c>).</summary>
    TailFacingLeftFinalWhips,
    /// <summary><c>InstList_DraygonTail_FacingLeft_FinalTailWhips_1</c> (<c>$A5:9A6C</c>).</summary>
    TailFacingLeftFinalWhipsLoop,
    /// <summary><c>InstList_DraygonTail_FacingLeft_TailWhip</c> (<c>$A5:9AE8</c>).</summary>
    TailFacingLeftWhip,
    /// <summary><c>InstList_DraygonTail_FacingLeft_TailFlail</c> (<c>$A5:9B5A</c>).</summary>
    TailFacingLeftFlail,
    /// <summary><c>InstList_DraygonArms_FacingRight_Idle_0</c> (<c>$A5:9BDA</c>).</summary>
    ArmsFacingRightIdle,
    /// <summary><c>InstList_DraygonArms_FacingRight_NearSwoopApex</c> (<c>$A5:9C06</c>).</summary>
    ArmsFacingRightNearSwoopApex,
    /// <summary><c>InstList_DraygonArms_FacingRight_Grab</c> (<c>$A5:9C38</c>).</summary>
    ArmsFacingRightGrab,
    /// <summary><c>InstList_DraygonArms_FacingRight_Dying_0</c> (<c>$A5:9C5A</c>).</summary>
    ArmsFacingRightDying,
    /// <summary><c>InstList_DraygonBody_FacingRight_Idle</c> (<c>$A5:9C7E</c>).</summary>
    BodyFacingRightIdle,
    /// <summary><c>InstList_DraygonBody_FacingRight_FireGoop</c> (<c>$A5:9C90</c>).</summary>
    BodyFacingRightFireGoop,
    /// <summary><c>InstList_DraygonBody_FacingRight_Roar</c> (<c>$A5:9CB4</c>).</summary>
    BodyFacingRightRoar,
    /// <summary><c>InstList_DraygonEye_FacingRight_Idle</c> (<c>$A5:9CD6</c>).</summary>
    EyeFacingRightIdle,
    /// <summary><c>InstList_DraygonEye_FacingRight_Dying_0</c> (<c>$A5:9D1C</c>).</summary>
    EyeFacingRightDying,
    /// <summary><c>InstList_DraygonEye_FacingRight_Dying_1</c> (<c>$A5:9D20</c>).</summary>
    EyeFacingRightDyingLoop,
    /// <summary><c>InstList_DraygonEye_FacingRight_Dead</c> (<c>$A5:9D3E</c>).</summary>
    EyeFacingRightDead,
    /// <summary><c>InstList_DraygonEye_FacingRight_LookingRight</c> (<c>$A5:9D50</c>).</summary>
    EyeFacingRightLookingRight,
    /// <summary><c>InstList_DraygonEye_FacingRight_LookingLeft</c> (<c>$A5:9D56</c>).</summary>
    EyeFacingRightLookingLeft,
    /// <summary><c>InstList_DraygonEye_FacingRight_LookingUp</c> (<c>$A5:9D5C</c>).</summary>
    EyeFacingRightLookingUp,
    /// <summary><c>InstList_DraygonEye_FacingRight_LookingDown</c> (<c>$A5:9D62</c>).</summary>
    EyeFacingRightLookingDown,
    /// <summary><c>InstList_DraygonTail_FacingRight_Idle_0</c> (<c>$A5:9D68</c>).</summary>
    TailFacingRightIdle,
    /// <summary><c>InstList_DraygonTail_FacingRight_FinalTailWhips_0</c> (<c>$A5:9E21</c>).</summary>
    TailFacingRightFinalWhips,
    /// <summary><c>InstList_DraygonTail_FacingRight_FinalTailWhips_1</c> (<c>$A5:9E25</c>).</summary>
    TailFacingRightFinalWhipsLoop,
    /// <summary><c>InstList_DraygonTail_FacingRight_TailWhip_0</c> (<c>$A5:9EA1</c>).</summary>
    TailFacingRightWhip,
    /// <summary><c>InstList_DraygonTail_FacingRight_TailFlail_0</c> (<c>$A5:9F15</c>).</summary>
    TailFacingRightFlail,
}

/// <summary>One Draygon frame: an index into <see cref="DraygonFrameDefinitions"/>.</summary>
/// <param name="Index">The frame's position in the catalog.</param>
public readonly record struct DraygonFrame(int Index)
{
    /// <summary>The installed extended spritemap the frame shows.</summary>
    internal ushort Spritemap => DraygonFrameDefinitions.Spritemap(this);
}

/// <summary>A gameplay cue of Draygon's programs.</summary>
internal abstract record DraygonCue
{
    /// <summary>Starts a program on each of the four parts, as <c>Instruction_Draygon_SetInstList_Body_Eye_Tail_Arms</c> does.</summary>
    internal sealed record StartPartPrograms(DraygonProgram Body, DraygonProgram Eye, DraygonProgram Tail, DraygonProgram Arms) : DraygonCue;

    /// <summary>Selects the room-loading interrupt's HUD-draw command.</summary>
    internal sealed record BeginHudDraw : DraygonCue
    {
        /// <summary>The cue's only value.</summary>
        internal static BeginHudDraw Instance { get; } = new();
    }

    /// <summary>Gives the eye its tracking function.</summary>
    internal sealed record SetEyeFunction(DraygonEyeFunction Function) : DraygonCue;

    /// <summary>Gives the part running the program (the eye) its tracking function.</summary>
    internal sealed record SetOwnEyeFunction(DraygonEyeFunction Function) : DraygonCue;

    /// <summary>Displaces the body's BG2 graphics while the tail whips.</summary>
    internal sealed record DisplaceBodyGraphics(Offset16<Bg2Pixels> Offset) : DraygonCue;

    /// <summary>Queues a sound effect with a six-entry admission limit.</summary>
    internal sealed record QueueSound(SoundEffectLibrary Library, ushort Sound) : DraygonCue;

    /// <summary>Spawns a dying-Draygon sprite object at a random point of the body.</summary>
    internal sealed record SpawnDyingObject(RoomSpriteObjectKind Kind) : DraygonCue;

    /// <summary>Puts the tail and arms to sleep.</summary>
    internal sealed record ParalyseTailAndArms : DraygonCue
    {
        /// <summary>The cue's only value.</summary>
        internal static ParalyseTailAndArms Instance { get; } = new();
    }

    /// <summary>Makes the body ignore Samus collision.</summary>
    internal sealed record SetBodyIntangible : DraygonCue
    {
        /// <summary>The cue's only value.</summary>
        internal static SetBodyIntangible Instance { get; } = new();
    }

    /// <summary>Gives the body a new AI function.</summary>
    internal sealed record SetBodyFunction(DraygonAiFunction Function) : DraygonCue;

    /// <summary>Applies a tail whip's hit to a grabbed Samus.</summary>
    internal sealed record TailWhipHit : DraygonCue
    {
        /// <summary>The cue's only value.</summary>
        internal static TailWhipHit Instance { get; } = new();
    }

    /// <summary>Spawns a goop projectile.</summary>
    internal sealed record SpawnGoop(bool MovingRight) : DraygonCue;
}

/// <summary>Draygon's typed programs (#641), in place of its bank-$A5 instruction lists.</summary>
internal static class DraygonAnimations
{
    /// <summary>Every body, eye, tail and arms program.</summary>
    internal static AnimationProgramSet<DraygonProgram, DraygonFrame, DraygonCue> Programs { get; } = new(
        new Dictionary<DraygonProgram, AnimationProgramStep<DraygonProgram, DraygonFrame, DraygonCue>[]>
    {
        [P.Sleep] =
        [
            Hold,
        ],
        [P.BodyFacingLeftReset] =
        [
            Cue(new DraygonCue.StartPartPrograms(P.BodyFacingLeftIdle, P.EyeFacingLeftIdle, P.TailFacingLeftIdle, P.ArmsFacingLeftIdle)),
            Cue(DraygonCue.BeginHudDraw.Instance),
            Cue(new DraygonCue.SetEyeFunction(DraygonEyeFunction.FacingLeft)),
            Show(22, 1),
            Hold,
        ],
        [P.BodyFacingRightReset] =
        [
            Cue(new DraygonCue.StartPartPrograms(P.BodyFacingRightIdle, P.EyeFacingRightIdle, P.TailFacingRightIdle, P.ArmsFacingRightIdle)),
            Cue(DraygonCue.BeginHudDraw.Instance),
            Cue(new DraygonCue.SetEyeFunction(DraygonEyeFunction.FacingRight)),
            Show(63, 1),
            Hold,
        ],
        [P.ArmsFacingLeftIdle] =
        [
            Show(0, 5),
            Show(1, 5),
            Show(2, 5),
            Show(3, 5),
            Show(4, 5),
            Show(5, 5),
            GoTo(P.ArmsFacingLeftIdle),
        ],
        [P.ArmsFacingLeftNearSwoopApex] =
        [
            Show(23, 1),
            Show(24, 1),
            Show(25, 1),
            Show(26, 64),
            Hold,
        ],
        [P.ArmsFacingLeftGrab] =
        [
            Show(23, 1),
            Show(24, 1),
            Show(25, 1),
            Show(26, 8),
            Show(25, 1),
            Show(24, 1),
            Show(23, 1),
            Show(24, 1),
            Hold,
        ],
        [P.ArmsFacingLeftDying] =
        [
            Show(6, 5),
            Show(7, 5),
            Show(8, 5),
            Show(9, 5),
            GoTo(P.BodyDying),
        ],
        [P.BodyFacingLeftIdle] =
        [
            Cue(DraygonCue.BeginHudDraw.Instance),
            Cue(new DraygonCue.SetEyeFunction(DraygonEyeFunction.FacingLeft)),
            Show(22, 1),
            Hold,
        ],
        [P.BodyDying] =
        [
            Cue(new DraygonCue.QueueSound(SoundEffectLibrary.Library3, DraygonSoundDefinitions.DyingCry)),
            Cue(DraygonCue.SetBodyIntangible.Instance),
            SetCounter(8),
            FallInto(P.BodyDyingExplosions),
        ],
        [P.BodyDyingExplosions] =
        [
            Wait(12),
            Cue(new DraygonCue.SpawnDyingObject(RoomSpriteObjectKind.BotwoonLargeExplosion)),
            Cue(new DraygonCue.SpawnDyingObject(RoomSpriteObjectKind.SporeSpawnDyingExplosion)),
            Cue(new DraygonCue.SpawnDyingObject(RoomSpriteObjectKind.DustCloud)),
            Cue(new DraygonCue.SpawnDyingObject(RoomSpriteObjectKind.DraygonBreathBubble)),
            Cue(new DraygonCue.QueueSound(SoundEffectLibrary.Library2, DraygonSoundDefinitions.Explosion)),
            RepeatWhileCounting(P.BodyDyingExplosions),
            Wait(1),
            Cue(DraygonCue.ParalyseTailAndArms.Instance),
            FallInto(P.BodyDyingExplosionsLoop),
        ],
        [P.BodyDyingExplosionsLoop] =
        [
            Wait(16),
            Cue(new DraygonCue.SpawnDyingObject(RoomSpriteObjectKind.BotwoonLargeExplosion)),
            Cue(new DraygonCue.SpawnDyingObject(RoomSpriteObjectKind.SporeSpawnDyingExplosion)),
            Cue(new DraygonCue.SpawnDyingObject(RoomSpriteObjectKind.DustCloud)),
            Cue(new DraygonCue.SpawnDyingObject(RoomSpriteObjectKind.DraygonBreathBubble)),
            Cue(new DraygonCue.QueueSound(SoundEffectLibrary.Library2, DraygonSoundDefinitions.Explosion)),
            GoTo(P.BodyDyingExplosionsLoop),
        ],
        [P.Delete] =
        [
            Stop,
        ],
        [P.BodyFacingLeftFireGoop] =
        [
            Show(10, 1),
            Show(11, 2),
            Show(12, 3),
            Cue(new DraygonCue.SpawnGoop(MovingRight: false)),
            Cue(new DraygonCue.QueueSound(SoundEffectLibrary.Library2, DraygonSoundDefinitions.GoopSpit)),
            Show(13, 3),
            Show(12, 2),
            Show(11, 2),
            Show(10, 1),
            Hold,
        ],
        [P.BodyFacingLeftRoar] =
        [
            Cue(new DraygonCue.QueueSound(SoundEffectLibrary.Library2, DraygonSoundDefinitions.Roar)),
            Show(10, 6),
            Show(11, 6),
            Show(12, 6),
            Show(13, 6),
            Show(12, 6),
            Show(11, 6),
            Show(10, 6),
            Hold,
        ],
        [P.EyeFacingLeftIdle] =
        [
            Show(14, 21),
            Show(15, 5),
            Show(16, 5),
            Show(15, 10),
            Show(18, 10),
            Show(18, 10),
            Show(20, 10),
            Show(20, 10),
            Show(21, 10),
            Show(16, 5),
            Show(15, 5),
            Show(14, 5),
            Cue(new DraygonCue.SetOwnEyeFunction(DraygonEyeFunction.FacingLeft)),
            Hold,
        ],
        [P.EyeFacingLeftDying] =
        [
            SetCounter(4),
            FallInto(P.EyeFacingLeftDyingLoop),
        ],
        [P.EyeFacingLeftDyingLoop] =
        [
            Show(18, 4),
            Show(20, 4),
            Show(19, 4),
            Show(21, 4),
            RepeatWhileCounting(P.EyeFacingLeftDyingLoop),
            Show(17, 32),
            Show(16, 16),
            Hold,
        ],
        [P.EyeFacingLeftDead] =
        [
            Show(17, 32),
            Show(16, 32),
            Show(15, 32),
            Show(14, 1),
            Hold,
        ],
        [P.EyeFacingLeftLookingLeft] =
        [
            Show(18, 1),
            Hold,
        ],
        [P.EyeFacingLeftLookingRight] =
        [
            Show(19, 1),
            Hold,
        ],
        [P.EyeFacingLeftLookingUp] =
        [
            Show(20, 1),
            Hold,
        ],
        [P.EyeFacingLeftLookingDown] =
        [
            Show(21, 1),
            Hold,
        ],
        [P.TailFacingLeftIdle] =
        [
            Show(27, 8),
            Show(28, 7),
            Show(29, 6),
            Show(30, 6),
            Show(31, 6),
            Show(32, 6),
            Show(33, 6),
            Show(32, 6),
            Show(31, 6),
            Show(30, 6),
            Show(29, 6),
            Show(28, 7),
            GoTo(P.TailFacingLeftIdle),
        ],
        [P.TailFacingLeftInitialFakeWhip] =
        [
            Cue(new DraygonCue.DisplaceBodyGraphics(new(-1, -1))),
            Show(29, 16),
            Cue(new DraygonCue.DisplaceBodyGraphics(new(-2, -2))),
            Show(34, 6),
            Cue(new DraygonCue.DisplaceBodyGraphics(new(-3, -3))),
            Show(35, 5),
            Cue(new DraygonCue.DisplaceBodyGraphics(new(-4, -4))),
            Show(36, 4),
            Cue(new DraygonCue.DisplaceBodyGraphics(new(-5, -5))),
            Show(37, 3),
            Cue(new DraygonCue.DisplaceBodyGraphics(new(-6, -6))),
            Show(38, 2),
            Cue(new DraygonCue.DisplaceBodyGraphics(new(-8, -8))),
            Show(39, 1),
            Cue(new DraygonCue.DisplaceBodyGraphics(new(0, 0))),
            Show(40, 16),
            Show(39, 1),
            Show(38, 2),
            Show(37, 3),
            Show(36, 4),
            Show(35, 5),
            Show(34, 6),
            GoTo(P.TailFacingLeftIdle),
        ],
        [P.TailFacingLeftFinalWhips] =
        [
            SetCounter(4),
            FallInto(P.TailFacingLeftFinalWhipsLoop),
        ],
        [P.TailFacingLeftFinalWhipsLoop] =
        [
            Cue(new DraygonCue.DisplaceBodyGraphics(new(-1, -1))),
            Show(29, 2),
            Cue(new DraygonCue.DisplaceBodyGraphics(new(-2, -2))),
            Show(34, 6),
            Cue(new DraygonCue.DisplaceBodyGraphics(new(-3, -3))),
            Show(35, 5),
            Cue(new DraygonCue.DisplaceBodyGraphics(new(-4, -4))),
            Show(36, 4),
            Cue(new DraygonCue.DisplaceBodyGraphics(new(-5, -5))),
            Show(37, 3),
            Cue(new DraygonCue.DisplaceBodyGraphics(new(-6, -6))),
            Show(38, 2),
            Cue(new DraygonCue.DisplaceBodyGraphics(new(-8, -8))),
            Show(39, 1),
            Cue(new DraygonCue.DisplaceBodyGraphics(new(0, 0))),
            Cue(DraygonCue.TailWhipHit.Instance),
            Cue(new DraygonCue.QueueSound(SoundEffectLibrary.Library2, DraygonSoundDefinitions.Explosion)),
            Show(40, 3),
            Show(39, 1),
            Show(38, 2),
            Show(37, 3),
            Show(36, 4),
            Show(35, 5),
            Show(34, 6),
            RepeatWhileCounting(P.TailFacingLeftFinalWhipsLoop),
            Cue(new DraygonCue.SetBodyFunction(DraygonAiFunction.ReleaseSamus)),
            GoTo(P.TailFacingLeftIdle),
        ],
        [P.TailFacingLeftWhip] =
        [
            Cue(new DraygonCue.DisplaceBodyGraphics(new(-1, -1))),
            Show(29, 2),
            Cue(new DraygonCue.DisplaceBodyGraphics(new(-2, -2))),
            Show(34, 6),
            Cue(new DraygonCue.DisplaceBodyGraphics(new(-3, -3))),
            Show(35, 5),
            Cue(new DraygonCue.DisplaceBodyGraphics(new(-4, -4))),
            Show(36, 4),
            Cue(new DraygonCue.DisplaceBodyGraphics(new(-5, -5))),
            Show(37, 3),
            Cue(new DraygonCue.DisplaceBodyGraphics(new(-6, -6))),
            Show(38, 2),
            Cue(new DraygonCue.DisplaceBodyGraphics(new(-8, -8))),
            Show(39, 1),
            Cue(new DraygonCue.DisplaceBodyGraphics(new(0, 0))),
            Cue(DraygonCue.TailWhipHit.Instance),
            Cue(new DraygonCue.QueueSound(SoundEffectLibrary.Library2, DraygonSoundDefinitions.Explosion)),
            Show(40, 3),
            Show(39, 1),
            Show(38, 2),
            Show(37, 3),
            Show(36, 4),
            Show(35, 5),
            Show(34, 6),
            GoTo(P.TailFacingLeftIdle),
        ],
        [P.TailFacingLeftFlail] =
        [
            Show(29, 2),
            Show(34, 6),
            Show(35, 5),
            Show(36, 4),
            Show(37, 3),
            Show(38, 2),
            Show(39, 1),
            Cue(new DraygonCue.QueueSound(SoundEffectLibrary.Library2, DraygonSoundDefinitions.Explosion)),
            Show(40, 3),
            Show(39, 1),
            Show(38, 2),
            Show(37, 3),
            Show(36, 4),
            Show(35, 5),
            Show(34, 6),
            GoTo(P.TailFacingLeftIdle),
        ],
        [P.ArmsFacingRightIdle] =
        [
            Show(41, 5),
            Show(42, 5),
            Show(43, 5),
            Show(44, 5),
            Show(45, 5),
            Show(46, 5),
            GoTo(P.ArmsFacingRightIdle),
        ],
        [P.ArmsFacingRightNearSwoopApex] =
        [
            Show(64, 1),
            Show(65, 1),
            Show(66, 1),
            Show(67, 64),
            Hold,
        ],
        [P.ArmsFacingRightGrab] =
        [
            Show(64, 1),
            Show(65, 1),
            Show(66, 1),
            Show(67, 8),
            Show(66, 1),
            Show(65, 1),
            Show(64, 1),
            Show(65, 1),
            Hold,
        ],
        [P.ArmsFacingRightDying] =
        [
            Show(47, 5),
            Show(48, 5),
            Show(49, 5),
            Show(50, 5),
            GoTo(P.BodyDying),
        ],
        [P.BodyFacingRightIdle] =
        [
            Cue(DraygonCue.BeginHudDraw.Instance),
            Cue(new DraygonCue.SetEyeFunction(DraygonEyeFunction.FacingRight)),
            Show(63, 1),
            Hold,
        ],
        [P.BodyFacingRightFireGoop] =
        [
            Show(51, 1),
            Show(52, 2),
            Show(53, 3),
            Cue(new DraygonCue.SpawnGoop(MovingRight: true)),
            Cue(new DraygonCue.QueueSound(SoundEffectLibrary.Library2, DraygonSoundDefinitions.GoopSpit)),
            Show(54, 3),
            Show(53, 2),
            Show(52, 2),
            Show(51, 1),
            Hold,
        ],
        [P.BodyFacingRightRoar] =
        [
            Cue(new DraygonCue.QueueSound(SoundEffectLibrary.Library2, DraygonSoundDefinitions.Roar)),
            Show(51, 6),
            Show(52, 6),
            Show(53, 6),
            Show(54, 6),
            Show(53, 6),
            Show(52, 6),
            Show(51, 6),
            Hold,
        ],
        [P.EyeFacingRightIdle] =
        [
            Show(55, 21),
            Show(56, 5),
            Show(57, 5),
            Show(56, 10),
            Show(59, 10),
            Show(59, 10),
            Show(61, 10),
            Show(61, 10),
            Show(62, 10),
            Show(57, 5),
            Show(56, 5),
            Show(55, 5),
            Cue(new DraygonCue.SetOwnEyeFunction(DraygonEyeFunction.FacingLeft)),
            Hold,
        ],
        [P.EyeFacingRightDying] =
        [
            SetCounter(4),
            FallInto(P.EyeFacingRightDyingLoop),
        ],
        [P.EyeFacingRightDyingLoop] =
        [
            Show(59, 4),
            Show(61, 4),
            Show(60, 4),
            Show(62, 4),
            RepeatWhileCounting(P.EyeFacingRightDyingLoop),
            Show(58, 32),
            Show(57, 16),
            Hold,
        ],
        [P.EyeFacingRightDead] =
        [
            Show(58, 32),
            Show(57, 32),
            Show(56, 32),
            Show(55, 1),
            Hold,
        ],
        [P.EyeFacingRightLookingRight] =
        [
            Show(59, 1),
            Hold,
        ],
        [P.EyeFacingRightLookingLeft] =
        [
            Show(60, 1),
            Hold,
        ],
        [P.EyeFacingRightLookingUp] =
        [
            Show(61, 1),
            Hold,
        ],
        [P.EyeFacingRightLookingDown] =
        [
            Show(62, 1),
            Hold,
        ],
        [P.TailFacingRightIdle] =
        [
            Show(68, 8),
            Show(69, 7),
            Show(70, 6),
            Show(71, 6),
            Show(72, 6),
            Show(73, 6),
            Show(74, 6),
            Show(73, 6),
            Show(72, 6),
            Show(71, 6),
            Show(70, 6),
            Show(69, 7),
            GoTo(P.TailFacingRightIdle),
        ],
        [P.TailFacingRightFinalWhips] =
        [
            SetCounter(4),
            FallInto(P.TailFacingRightFinalWhipsLoop),
        ],
        [P.TailFacingRightFinalWhipsLoop] =
        [
            Cue(new DraygonCue.DisplaceBodyGraphics(new(1, -1))),
            Show(70, 2),
            Cue(new DraygonCue.DisplaceBodyGraphics(new(2, -2))),
            Show(75, 6),
            Cue(new DraygonCue.DisplaceBodyGraphics(new(3, -3))),
            Show(76, 5),
            Cue(new DraygonCue.DisplaceBodyGraphics(new(4, -4))),
            Show(77, 4),
            Cue(new DraygonCue.DisplaceBodyGraphics(new(5, -5))),
            Show(78, 3),
            Cue(new DraygonCue.DisplaceBodyGraphics(new(6, -6))),
            Show(79, 2),
            Cue(new DraygonCue.DisplaceBodyGraphics(new(8, -8))),
            Show(80, 1),
            Cue(new DraygonCue.DisplaceBodyGraphics(new(0, 0))),
            Cue(DraygonCue.TailWhipHit.Instance),
            Cue(new DraygonCue.QueueSound(SoundEffectLibrary.Library2, DraygonSoundDefinitions.Explosion)),
            Show(81, 3),
            Show(80, 1),
            Show(79, 2),
            Show(78, 3),
            Show(77, 4),
            Show(76, 5),
            Show(75, 6),
            RepeatWhileCounting(P.TailFacingRightFinalWhipsLoop),
            Cue(new DraygonCue.SetBodyFunction(DraygonAiFunction.ReleaseSamus)),
            GoTo(P.TailFacingRightIdle),
        ],
        [P.TailFacingRightWhip] =
        [
            Cue(new DraygonCue.DisplaceBodyGraphics(new(1, -1))),
            Show(70, 2),
            Cue(new DraygonCue.DisplaceBodyGraphics(new(2, -2))),
            Show(75, 6),
            Cue(new DraygonCue.DisplaceBodyGraphics(new(3, -3))),
            Show(76, 5),
            Cue(new DraygonCue.DisplaceBodyGraphics(new(4, -4))),
            Show(77, 4),
            Cue(new DraygonCue.DisplaceBodyGraphics(new(5, -5))),
            Show(78, 3),
            Cue(new DraygonCue.DisplaceBodyGraphics(new(6, -6))),
            Show(79, 2),
            Cue(new DraygonCue.DisplaceBodyGraphics(new(8, -8))),
            Show(80, 1),
            Cue(new DraygonCue.DisplaceBodyGraphics(new(0, 0))),
            Cue(DraygonCue.TailWhipHit.Instance),
            Cue(new DraygonCue.QueueSound(SoundEffectLibrary.Library2, DraygonSoundDefinitions.Explosion)),
            Show(81, 3),
            Show(80, 1),
            Show(79, 2),
            Show(78, 3),
            Show(77, 4),
            Show(76, 5),
            Show(75, 6),
            GoTo(P.TailFacingRightIdle),
        ],
        [P.TailFacingRightFlail] =
        [
            Show(70, 2),
            Show(75, 6),
            Show(76, 5),
            Show(77, 4),
            Show(78, 3),
            Show(79, 2),
            Show(80, 1),
            Cue(new DraygonCue.QueueSound(SoundEffectLibrary.Library2, DraygonSoundDefinitions.Explosion)),
            Show(81, 3),
            Show(80, 1),
            Show(79, 2),
            Show(78, 3),
            Show(77, 4),
            Show(76, 5),
            Show(75, 6),
            GoTo(P.TailFacingRightIdle),
        ],    });

    /// <summary>Shows the catalog frame <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationProgramStep<DraygonProgram, DraygonFrame, DraygonCue> Show(int frame, int duration) =>
        AnimationProgramStep<DraygonProgram, DraygonFrame, DraygonCue>.Show(new(frame), duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationProgramStep<DraygonProgram, DraygonFrame, DraygonCue> Cue(DraygonCue cue) =>
        AnimationProgramStep<DraygonProgram, DraygonFrame, DraygonCue>.Publish(cue);

    /// <summary>Continues at the start of <paramref name="program"/>.</summary>
    private static AnimationProgramStep<DraygonProgram, DraygonFrame, DraygonCue> GoTo(DraygonProgram program) =>
        AnimationProgramStep<DraygonProgram, DraygonFrame, DraygonCue>.GoTo(program);

    /// <summary>Runs on into <paramref name="program"/>.</summary>
    private static AnimationProgramStep<DraygonProgram, DraygonFrame, DraygonCue> FallInto(DraygonProgram program) =>
        AnimationProgramStep<DraygonProgram, DraygonFrame, DraygonCue>.FallInto(program);

    /// <summary>Keeps the current frame for <paramref name="duration"/> updates.</summary>
    private static AnimationProgramStep<DraygonProgram, DraygonFrame, DraygonCue> Wait(int duration) =>
        AnimationProgramStep<DraygonProgram, DraygonFrame, DraygonCue>.Wait(duration);

    /// <summary>Loads the part's loop counter with <paramref name="count"/>.</summary>
    private static AnimationProgramStep<DraygonProgram, DraygonFrame, DraygonCue> SetCounter(int count) =>
        AnimationProgramStep<DraygonProgram, DraygonFrame, DraygonCue>.SetCounter(count);

    /// <summary>Repeats <paramref name="program"/> while the loop counter counts down.</summary>
    private static AnimationProgramStep<DraygonProgram, DraygonFrame, DraygonCue> RepeatWhileCounting(DraygonProgram program) =>
        AnimationProgramStep<DraygonProgram, DraygonFrame, DraygonCue>.RepeatWhileCounting(program);

    /// <summary>Holds the last frame: the native sleep.</summary>
    private static AnimationProgramStep<DraygonProgram, DraygonFrame, DraygonCue> Hold =>
        AnimationProgramStep<DraygonProgram, DraygonFrame, DraygonCue>.Hold;

    /// <summary>Stops the part and deletes it.</summary>
    private static AnimationProgramStep<DraygonProgram, DraygonFrame, DraygonCue> Stop =>
        AnimationProgramStep<DraygonProgram, DraygonFrame, DraygonCue>.Stop;
}
