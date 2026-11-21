using System.Runtime.CompilerServices;
using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// The stage a check puts Kraid's body in, by the phase it names (#2554): production holds each stage's phase in the stage
/// (<see cref="KraidStageRun"/>), and the stages after his rise hold his head's animation. A check that names a phase of
/// another stage enters that stage; one entering a stage after his rise hands it the head the stage before held, else the
/// head the check named while he had not risen, else the roar his rise's last update selects.
/// </summary>
internal static class KraidStageAccess
{
    /// <summary>The head a check names for each body before he has risen, by body.</summary>
    private static readonly ConditionalWeakTable<KraidBodyState, KraidRisenHead> namedHeads = new();

    /// <summary>The body's stages by phase.</summary>
    /// <param name="body">Kraid's body.</param>
    extension(KraidBodyState body)
    {
        /// <summary>The phase of his rise, entering it.</summary>
        /// <exception cref="InvalidOperationException">Read while he is not rising.</exception>
        internal KraidRisePhase RisePhase
        {
            get => body.Run is KraidStageRun.Rising rising ? rising.Phase : throw NotIn(body, KraidBodyStage.Rising);
            set
            {
                if (body.Run is KraidStageRun.Rising rising)
                    rising.Phase = value;
                else
                    body.Run = new KraidStageRun.Rising(value);
            }
        }

        /// <summary>The phase of his fight, entering it.</summary>
        /// <exception cref="InvalidOperationException">Read while he is not fighting.</exception>
        internal KraidFightPhase FightPhase
        {
            get => body.Run is KraidStageRun.Fighting fight ? fight.Phase : throw NotIn(body, KraidBodyStage.Fighting);
            set
            {
                if (body.Run is KraidStageRun.Fighting fight)
                    fight.Phase = value;
                else
                    body.Run = new KraidStageRun.Fighting(value, HeadFor(body));
            }
        }

        /// <summary>The phase of his growth, entering it.</summary>
        /// <exception cref="InvalidOperationException">Read while he is not growing.</exception>
        internal KraidGrowthPhase GrowthPhase
        {
            get => body.Run is KraidStageRun.Growing growth ? growth.Phase : throw NotIn(body, KraidBodyStage.Growing);
            set
            {
                if (body.Run is KraidStageRun.Growing growth)
                    growth.Phase = value;
                else
                    body.Run = new KraidStageRun.Growing(value, HeadFor(body));
            }
        }

        /// <summary>The phase of his death up to his sinking, entering it.</summary>
        /// <exception cref="InvalidOperationException">Read while he is not dying.</exception>
        internal KraidDeathPhase DeathPhase
        {
            get => body.Run is KraidStageRun.Dying dying ? dying.Phase : throw NotIn(body, KraidBodyStage.Dying);
            set
            {
                if (body.Run is KraidStageRun.Dying dying)
                    dying.Phase = value;
                else
                    body.Run = new KraidStageRun.Dying(value, HeadFor(body));
            }
        }

        /// <summary>The phase of his sunk body, entering it.</summary>
        /// <exception cref="InvalidOperationException">Read while his body has not sunk.</exception>
        internal KraidSunkPhase SunkPhase
        {
            get => body.Run is KraidStageRun.Sunk sunk ? sunk.Phase
                : throw new InvalidOperationException($"Kraid's body is {body.Run.GetType().Name}, not sunk.");
            set
            {
                if (body.Run is KraidStageRun.Sunk sunk)
                    sunk.Phase = value;
                else
                    body.Run = new KraidStageRun.Sunk(value);
            }
        }
    }

    /// <summary>His head's animation as a check names it.</summary>
    /// <param name="state">His encounter.</param>
    extension(KraidEnemyState state)
    {
        /// <summary>
        /// The playing head animation and where it stands once he has risen, or the one the check named before; none while
        /// he has not risen and the check named none. Naming it before he has risen hands it to the stage he enters next.
        /// </summary>
        /// <exception cref="InvalidOperationException">Set to none once he has risen, when his head always plays one.</exception>
        internal AnimationPlayback<KraidHeadSegment>? HeadPlayback
        {
            get => state.BodyState.Run is KraidStageRun.Risen risen ? risen.Head.Playback
                : namedHeads.TryGetValue(state.BodyState, out KraidRisenHead? named) ? named.Playback
                : null;
            set
            {
                if (state.BodyState.Run is KraidStageRun.Risen risen)
                    risen.Head.Playback = value ?? throw new InvalidOperationException("A risen Kraid's head always plays an animation.");
                else if (value is { } playback)
                    namedHeads.AddOrUpdate(state.BodyState, new KraidRisenHead(playback));
                else
                    namedHeads.Remove(state.BodyState);
            }
        }

        /// <summary>His head's animation, which the stages after his rise hold.</summary>
        /// <exception cref="InvalidOperationException">He has not risen.</exception>
        internal KraidRisenHead RisenHead => state.BodyState.Run is KraidStageRun.Risen risen ? risen.Head
            : throw new InvalidOperationException($"Kraid's body is {state.BodyState.Run.GetType().Name}, which holds no head animation.");
    }

    /// <summary>The head a stage after his rise starts with.</summary>
    private static KraidRisenHead HeadFor(KraidBodyState body) =>
        body.Run is KraidStageRun.Risen risen ? risen.Head
        : namedHeads.TryGetValue(body, out KraidRisenHead? named) ? named
        : KraidRisenHead.SelectingRoar();

    /// <summary>The failure of a read of a stage's phase while the body is in another.</summary>
    private static InvalidOperationException NotIn(KraidBodyState body, KraidBodyStage stage) =>
        new($"Kraid's body is {body.Run.GetType().Name}, not {stage}.");

    /// <summary>The stages' phase runs a check drives with the stage the body is in.</summary>
    /// <param name="rise">His rise.</param>
    extension(KraidRise rise)
    {
        /// <summary>Runs his growth's phase in the growth he is in.</summary>
        internal void RunKraidGrowthPhase(Enemies.RoomEnemyPlace body, KraidEnemyState state) =>
            rise.RunKraidGrowthPhase(body, state, (KraidStageRun.Growing)state.BodyState.Run);

        /// <summary>Runs his rise's phase in the rise he is in.</summary>
        internal void RunKraidRisePhase(Enemies.RoomEnemyPlace body, KraidEnemyState state, Samus.SamusState samus) =>
            rise.RunKraidRisePhase(body, state, (KraidStageRun.Rising)state.BodyState.Run, samus);
    }
}
