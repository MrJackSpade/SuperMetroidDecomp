using SuperMetroid.Core.Programs;
using F = SuperMetroid.Core.Rooms.Plms.BlockReactionPlmFrame;
using K = SuperMetroid.Core.Rooms.Plms.BlockReactionPlmCueKind;
using P = SuperMetroid.Core.Rooms.Plms.BlockReactionPlmProgram;
using Step = SuperMetroid.Core.Programs.AnimationProgramStep<SuperMetroid.Core.Rooms.Plms.BlockReactionPlmProgram, SuperMetroid.Core.Rooms.Plms.BlockReactionPlmFrame, SuperMetroid.Core.Rooms.Plms.BlockReactionPlmCue>;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// The block-reaction PLMs' programs (#1296), step for step their bank-$84 lists: the reaction sound, the breakup images
/// with their durations, the respawn and the restored block, and the delete.
/// </summary>
internal static class BlockReactionPlmProgramDefinitions
{
    /// <summary>Every block-reaction program.</summary>
    internal static AnimationProgramSet<P, F, BlockReactionPlmCue> Programs { get; } = new(
        new Dictionary<P, Step[]>
        {
            [P.CrumbleReveal1x1] =
            [
                Step.Show(F.CrumbleRevealSingle, 1),
                Step.Stop,
            ],
            [P.CrumbleReveal2x1] =
            [
                Step.Show(F.CrumbleRevealHorizontal, 1),
                Step.Stop,
            ],
            [P.CrumbleReveal1x2] =
            [
                Step.Show(F.CrumbleRevealVertical, 1),
                Step.Stop,
            ],
            [P.CrumbleReveal2x2] =
            [
                Step.Show(F.CrumbleRevealSquare, 1),
                Step.Stop,
            ],
            [P.BombedPowerBombBlockUnused] =
            [
                Step.Show(F.PowerBombReveal, 1),
                Step.Stop,
            ],
            [P.BombedSuperMissileBlockUnused] =
            [
                Step.Show(F.SuperMissileReveal, 1),
                Step.Stop,
            ],
            [P.ContactCrumble1x1Respawning] =
            [
                Step.Publish(new(K.QueueSoundLibrary2Maximum1, 0x0a)),
                Step.Show(F.SingleBreakup0, 8),
                Step.Show(F.SingleBreakup1, 6),
                Step.Show(F.SingleBreakup2, 4),
                Step.Show(F.SingleBreakup3, 16),
                Step.Show(F.SingleBreakup2, 4),
                Step.Show(F.SingleBreakup1, 4),
                Step.Show(F.SingleBreakup0, 4),
                Step.Show(F.RestoredBlock, 1),
                Step.Stop,
            ],
            [P.ContactCrumble2x1Respawning] =
            [
                Step.Publish(new(K.QueueSoundLibrary2Maximum1, 0x0a)),
                Step.Show(F.HorizontalBreakup0, 4),
                Step.Show(F.HorizontalBreakup1, 4),
                Step.Show(F.HorizontalBreakup2, 4),
                Step.Show(F.HorizontalBreakup3, 16),
                Step.Show(F.HorizontalBreakup2, 4),
                Step.Show(F.HorizontalBreakup1, 4),
                Step.Show(F.HorizontalBreakup0, 4),
                Step.Show(F.CrumbleRevealHorizontal, 1),
                Step.Stop,
            ],
            [P.ContactCrumble1x2Respawning] =
            [
                Step.Publish(new(K.QueueSoundLibrary2Maximum1, 0x0a)),
                Step.Show(F.VerticalBreakup0, 4),
                Step.Show(F.VerticalBreakup1, 4),
                Step.Show(F.VerticalBreakup2, 4),
                Step.Show(F.VerticalBreakup3, 32),
                Step.Show(F.VerticalBreakup2, 4),
                Step.Show(F.VerticalBreakup1, 4),
                Step.Show(F.VerticalBreakup0, 4),
                Step.Show(F.CrumbleRevealVertical, 1),
                Step.Stop,
            ],
            [P.ContactCrumble2x2Respawning] =
            [
                Step.Publish(new(K.QueueSoundLibrary2Maximum1, 0x0a)),
                Step.Show(F.SquareBreakup0, 4),
                Step.Show(F.SquareBreakup1, 4),
                Step.Show(F.SquareBreakup2, 4),
                Step.Show(F.SquareBreakup3, 32),
                Step.Show(F.SquareBreakup2, 4),
                Step.Show(F.SquareBreakup1, 4),
                Step.Show(F.SquareBreakup0, 4),
                Step.Show(F.CrumbleRevealSquare, 1),
                Step.Stop,
            ],
            [P.ContactCrumble1x1Permanent] =
            [
                Step.Publish(new(K.QueueSoundLibrary2Maximum1, 0x0a)),
                Step.Show(F.SingleBreakup0, 4),
                Step.Show(F.SingleBreakup1, 4),
                Step.Show(F.SingleBreakup2, 4),
                Step.Show(F.SingleBreakup3, 1),
                Step.Stop,
            ],
            [P.ContactCrumble2x1Permanent] =
            [
                Step.Publish(new(K.QueueSoundLibrary2Maximum1, 0x0a)),
                Step.Show(F.HorizontalBreakup0, 4),
                Step.Show(F.HorizontalBreakup1, 4),
                Step.Show(F.HorizontalBreakup2, 4),
                Step.Show(F.HorizontalBreakup3, 1),
                Step.Stop,
            ],
            [P.ContactCrumble1x2Permanent] =
            [
                Step.Publish(new(K.QueueSoundLibrary2Maximum1, 0x0a)),
                Step.Show(F.VerticalBreakup0, 4),
                Step.Show(F.VerticalBreakup1, 4),
                Step.Show(F.VerticalBreakup2, 4),
                Step.Show(F.VerticalBreakup3, 1),
                Step.Stop,
            ],
            [P.ContactCrumble2x2Permanent] =
            [
                Step.Publish(new(K.QueueSoundLibrary2Maximum1, 0x0a)),
                Step.Show(F.SquareBreakup0, 4),
                Step.Show(F.SquareBreakup1, 4),
                Step.Show(F.SquareBreakup2, 4),
                Step.Show(F.SquareBreakup3, 1),
                Step.Stop,
            ],
            [P.RespawningShotBlock1x1] =
            [
                Step.Publish(new(K.QueueSoundLibrary2Maximum1, 0x0a)),
                Step.Show(F.SingleBreakup0, 4),
                Step.Show(F.SingleBreakup1, 4),
                Step.Show(F.SingleBreakup2, 4),
                Step.Show(F.SingleBreakup3, 384),
                Step.Show(F.SingleBreakup2, 4),
                Step.Show(F.SingleBreakup1, 4),
                Step.Show(F.SingleBreakup0, 4),
                Step.Show(F.RestoredBlock, 1),
                Step.Stop,
            ],
            [P.RespawningShotBlock2x1] =
            [
                Step.Publish(new(K.QueueSoundLibrary2Maximum1, 0x0a)),
                Step.Show(F.HorizontalBreakup0, 4),
                Step.Show(F.HorizontalBreakup1, 4),
                Step.Show(F.HorizontalBreakup2, 4),
                Step.Show(F.HorizontalBreakup3, 384),
                Step.Show(F.HorizontalBreakup2, 4),
                Step.Show(F.HorizontalBreakup1, 4),
                Step.Show(F.HorizontalBreakup0, 4),
                Step.Show(F.ShotRestoreHorizontal, 1),
                Step.Stop,
            ],
            [P.RespawningShotBlock1x2] =
            [
                Step.Publish(new(K.QueueSoundLibrary2Maximum1, 0x0a)),
                Step.Show(F.VerticalBreakup0, 4),
                Step.Show(F.VerticalBreakup1, 4),
                Step.Show(F.VerticalBreakup2, 4),
                Step.Show(F.VerticalBreakup3, 384),
                Step.Show(F.VerticalBreakup2, 4),
                Step.Show(F.VerticalBreakup1, 4),
                Step.Show(F.VerticalBreakup0, 4),
                Step.Show(F.ShotRestoreVertical, 1),
                Step.Stop,
            ],
            [P.RespawningShotBlock2x2] =
            [
                Step.Publish(new(K.QueueSoundLibrary2Maximum1, 0x0a)),
                Step.Show(F.SquareBreakup0, 4),
                Step.Show(F.SquareBreakup1, 4),
                Step.Show(F.SquareBreakup2, 4),
                Step.Show(F.SquareBreakup3, 384),
                Step.Show(F.SquareBreakup2, 4),
                Step.Show(F.SquareBreakup1, 4),
                Step.Show(F.SquareBreakup0, 4),
                Step.Show(F.ShotRestoreSquare, 1),
                Step.Stop,
            ],
            [P.RespawningSuperMissileBlock] =
            [
                Step.Publish(new(K.QueueSoundLibrary2Maximum6, 0x0a)),
                Step.Show(F.SingleBreakup0, 4),
                Step.Show(F.SingleBreakup1, 4),
                Step.Show(F.SingleBreakup2, 4),
                Step.Show(F.SingleBreakup3, 384),
                Step.Show(F.SingleBreakup2, 4),
                Step.Show(F.SingleBreakup1, 4),
                Step.Show(F.SingleBreakup0, 4),
                Step.Show(F.RestoredBlock, 1),
                Step.Stop,
            ],
            [P.RespawningPowerBombBlock] =
            [
                Step.Publish(new(K.QueueSoundLibrary2Maximum1, 0x0a)),
                Step.Show(F.SingleBreakup0, 4),
                Step.Show(F.SingleBreakup1, 4),
                Step.Show(F.SingleBreakup2, 4),
                Step.Show(F.SingleBreakup3, 384),
                Step.Show(F.SingleBreakup2, 4),
                Step.Show(F.SingleBreakup1, 4),
                Step.Show(F.SingleBreakup0, 4),
                Step.Show(F.RestoredBlock, 1),
                Step.Stop,
            ],
            [P.PermanentShotBlock1x1] =
            [
                Step.Publish(new(K.QueueSoundLibrary2Maximum1, 0x0a)),
                Step.Show(F.SingleBreakup0, 4),
                Step.Show(F.SingleBreakup1, 4),
                Step.Show(F.SingleBreakup2, 4),
                Step.Show(F.SingleBreakup3, 1),
                Step.Stop,
            ],
            [P.PermanentShotBlock2x1] =
            [
                Step.Publish(new(K.QueueSoundLibrary2Maximum1, 0x0a)),
                Step.Show(F.HorizontalBreakup0, 4),
                Step.Show(F.HorizontalBreakup1, 4),
                Step.Show(F.HorizontalBreakup2, 4),
                Step.Show(F.HorizontalBreakup3, 1),
                Step.Stop,
            ],
            [P.PermanentShotBlock1x2] =
            [
                Step.Publish(new(K.QueueSoundLibrary2Maximum1, 0x0a)),
                Step.Show(F.VerticalBreakup0, 4),
                Step.Show(F.VerticalBreakup1, 4),
                Step.Show(F.VerticalBreakup2, 4),
                Step.Show(F.VerticalBreakup3, 1),
                Step.Stop,
            ],
            [P.PermanentShotBlock2x2] =
            [
                Step.Publish(new(K.QueueSoundLibrary2Maximum1, 0x0a)),
                Step.Show(F.SquareBreakup0, 4),
                Step.Show(F.SquareBreakup1, 4),
                Step.Show(F.SquareBreakup2, 4),
                Step.Show(F.SquareBreakup3, 1),
                Step.Stop,
            ],
            [P.PermanentSuperMissileBlock] =
            [
                Step.Publish(new(K.QueueSoundLibrary2Maximum6, 0x0a)),
                Step.Show(F.SingleBreakup0, 4),
                Step.Show(F.SingleBreakup1, 4),
                Step.Show(F.SingleBreakup2, 4),
                Step.Show(F.SingleBreakup3, 1),
                Step.Stop,
            ],
            [P.PermanentPowerBombBlock] =
            [
                Step.Publish(new(K.QueueSoundLibrary2Maximum1, 0x0a)),
                Step.Show(F.SingleBreakup0, 3),
                Step.Show(F.SingleBreakup1, 2),
                Step.Show(F.SingleBreakup2, 1),
                Step.Show(F.SingleBreakup3, 1),
                Step.Stop,
            ],
            [P.CollisionBombBlock1x1Respawning] =
            [
                Step.Publish(new(K.QueueSoundLibrary2Maximum3, 0x06)),
                Step.GoTo(P.BombBlock1x1RespawningBreak),
            ],
            [P.ReactionBombBlock1x1Respawning] =
            [
                Step.Publish(new(K.QueueSoundLibrary2Maximum3, 0x0a)),
                Step.FallInto(P.BombBlock1x1RespawningBreak),
            ],
            [P.BombBlock1x1RespawningBreak] =
            [
                Step.Show(F.SingleBreakup0, 4),
                Step.Show(F.SingleBreakup1, 4),
                Step.Show(F.SingleBreakup2, 4),
                Step.Show(F.SingleBreakup3, 384),
                Step.Show(F.SingleBreakup2, 4),
                Step.Show(F.SingleBreakup1, 4),
                Step.Show(F.SingleBreakup0, 4),
                Step.Show(F.RestoredBlock, 1),
                Step.Stop,
            ],
            [P.CollisionBombBlock2x1Respawning] =
            [
                Step.Publish(new(K.QueueSoundLibrary2Maximum3, 0x06)),
                Step.GoTo(P.BombBlock2x1RespawningBreak),
            ],
            [P.ReactionBombBlock2x1Respawning] =
            [
                Step.Publish(new(K.QueueSoundLibrary2Maximum3, 0x0a)),
                Step.FallInto(P.BombBlock2x1RespawningBreak),
            ],
            [P.BombBlock2x1RespawningBreak] =
            [
                Step.Show(F.HorizontalBreakup0, 4),
                Step.Show(F.HorizontalBreakup1, 4),
                Step.Show(F.HorizontalBreakup2, 4),
                Step.Show(F.HorizontalBreakup3, 384),
                Step.Show(F.HorizontalBreakup2, 4),
                Step.Show(F.HorizontalBreakup1, 4),
                Step.Show(F.HorizontalBreakup0, 4),
                Step.Show(F.BombRestoreHorizontal, 1),
                Step.Stop,
            ],
            [P.CollisionBombBlock1x2Respawning] =
            [
                Step.Publish(new(K.QueueSoundLibrary2Maximum3, 0x06)),
                Step.GoTo(P.BombBlock1x2RespawningBreak),
            ],
            [P.ReactionBombBlock1x2Respawning] =
            [
                Step.Publish(new(K.QueueSoundLibrary2Maximum3, 0x0a)),
                Step.FallInto(P.BombBlock1x2RespawningBreak),
            ],
            [P.BombBlock1x2RespawningBreak] =
            [
                Step.Show(F.VerticalBreakup0, 4),
                Step.Show(F.VerticalBreakup1, 4),
                Step.Show(F.VerticalBreakup2, 4),
                Step.Show(F.VerticalBreakup3, 384),
                Step.Show(F.VerticalBreakup2, 4),
                Step.Show(F.VerticalBreakup1, 4),
                Step.Show(F.VerticalBreakup0, 4),
                Step.Show(F.BombRestoreVertical, 1),
                Step.Stop,
            ],
            [P.CollisionBombBlock2x2Respawning] =
            [
                Step.Publish(new(K.QueueSoundLibrary2Maximum3, 0x06)),
                Step.GoTo(P.BombBlock2x2RespawningBreak),
            ],
            [P.ReactionBombBlock2x2Respawning] =
            [
                Step.Publish(new(K.QueueSoundLibrary2Maximum3, 0x0a)),
                Step.FallInto(P.BombBlock2x2RespawningBreak),
            ],
            [P.BombBlock2x2RespawningBreak] =
            [
                Step.Show(F.SquareBreakup0, 4),
                Step.Show(F.SquareBreakup1, 4),
                Step.Show(F.SquareBreakup2, 4),
                Step.Show(F.SquareBreakup3, 384),
                Step.Show(F.SquareBreakup2, 4),
                Step.Show(F.SquareBreakup1, 4),
                Step.Show(F.SquareBreakup0, 4),
                Step.Show(F.BombRestoreSquare, 1),
                Step.Stop,
            ],
            [P.CollisionBombBlock1x1Permanent] =
            [
                Step.Publish(new(K.QueueSoundLibrary2Maximum3, 0x06)),
                Step.GoTo(P.BombBlock1x1PermanentBreak),
            ],
            [P.ReactionBombBlock1x1Permanent] =
            [
                Step.Publish(new(K.QueueSoundLibrary2Maximum3, 0x0a)),
                Step.FallInto(P.BombBlock1x1PermanentBreak),
            ],
            [P.BombBlock1x1PermanentBreak] =
            [
                Step.Show(F.SingleBreakup0, 4),
                Step.Show(F.SingleBreakup1, 4),
                Step.Show(F.SingleBreakup2, 4),
                Step.Show(F.SingleBreakup3, 1),
                Step.Stop,
            ],
            [P.CollisionBombBlock2x1Permanent] =
            [
                Step.Publish(new(K.QueueSoundLibrary2Maximum3, 0x06)),
                Step.GoTo(P.BombBlock2x1PermanentBreak),
            ],
            [P.ReactionBombBlock2x1Permanent] =
            [
                Step.Publish(new(K.QueueSoundLibrary2Maximum3, 0x0a)),
                Step.FallInto(P.BombBlock2x1PermanentBreak),
            ],
            [P.BombBlock2x1PermanentBreak] =
            [
                Step.Show(F.HorizontalBreakup0, 4),
                Step.Show(F.HorizontalBreakup1, 4),
                Step.Show(F.HorizontalBreakup2, 4),
                Step.Show(F.HorizontalBreakup3, 1),
                Step.Stop,
            ],
            [P.CollisionBombBlock1x2Permanent] =
            [
                Step.Publish(new(K.QueueSoundLibrary2Maximum3, 0x06)),
                Step.GoTo(P.BombBlock1x2PermanentBreak),
            ],
            [P.ReactionBombBlock1x2Permanent] =
            [
                Step.Publish(new(K.QueueSoundLibrary2Maximum3, 0x0a)),
                Step.FallInto(P.BombBlock1x2PermanentBreak),
            ],
            [P.BombBlock1x2PermanentBreak] =
            [
                Step.Show(F.VerticalBreakup0, 4),
                Step.Show(F.VerticalBreakup1, 4),
                Step.Show(F.VerticalBreakup2, 4),
                Step.Show(F.VerticalBreakup3, 1),
                Step.Stop,
            ],
            [P.CollisionBombBlock2x2Permanent] =
            [
                Step.Publish(new(K.QueueSoundLibrary2Maximum3, 0x06)),
                Step.GoTo(P.BombBlock2x2PermanentBreak),
            ],
            [P.ReactionBombBlock2x2Permanent] =
            [
                Step.Publish(new(K.QueueSoundLibrary2Maximum3, 0x0a)),
                Step.FallInto(P.BombBlock2x2PermanentBreak),
            ],
            [P.BombBlock2x2PermanentBreak] =
            [
                Step.Show(F.SquareBreakup0, 4),
                Step.Show(F.SquareBreakup1, 4),
                Step.Show(F.SquareBreakup2, 4),
                Step.Show(F.SquareBreakup3, 1),
                Step.Stop,
            ],
            [P.RespawningBreakableGrappleBlock] =
            [
                Step.Show(F.GrappleBlock, 240),
                Step.Publish(new(K.QueueSoundLibrary2Maximum6, 0x0a)),
                Step.Show(F.GrappleBreak0, 4),
                Step.Show(F.GrappleBreak1, 4),
                Step.Show(F.GrappleBreak2, 4),
                Step.Show(F.GrappleBlank, 6),
                Step.Show(F.GrappleBreak2, 4),
                Step.Show(F.GrappleBreak1, 4),
                Step.Show(F.GrappleBreak0, 4),
                Step.Publish(new(K.SetBtsToOne, 0)),
                Step.Show(F.RestoredBlock, 1),
                Step.Stop,
            ],
            [P.PermanentBreakableGrappleBlock] =
            [
                Step.Show(F.GrappleBlock, 120),
                Step.Publish(new(K.QueueSoundLibrary2Maximum6, 0x0a)),
                Step.Show(F.GrappleBreak0, 4),
                Step.Show(F.GrappleBreak1, 4),
                Step.Show(F.GrappleBreak2, 4),
                Step.Show(F.GrappleBlank, 1),
                Step.Stop,
            ],
        });
}
