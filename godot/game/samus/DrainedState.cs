using Engine.Actors;
using Engine.Actors.Components;
using Engine.Input;
using Engine.Shared;
using Game.Data;

namespace Game;

/// <summary>
/// Drained of her strength, as Mother Brain's beam and the giant Metroid leave her. Whatever drains her says how she
/// is: pinned in the beam, fallen to her knees, stood up, or frozen as she takes in the hyper beam. On her knees she
/// tries to rise when Up is pressed: able to, she gets to her feet and is her own again; unable, she sinks back.
/// She cannot move or fire until she is.
/// </summary>
/// <remarks>
/// Her looks are "drained_pinned", "drained", "drained_struggling", "drained_getting_up", "drained_standing" and
/// "drained_frozen". Rising and struggling last as long as their timelines.
/// </remarks>
public sealed class DrainedState : PlayerState, ISamusLook
{
    /// <summary>How she is.</summary>
    public enum Pose
    {
        /// <summary>Held in a beam, thrown back.</summary>
        Pinned,

        /// <summary>On her knees.</summary>
        Kneeling,

        /// <summary>Trying to rise, and failing.</summary>
        Struggling,

        /// <summary>Getting to her feet, after which she is her own again.</summary>
        GettingUp,

        /// <summary>On her feet, still drained.</summary>
        Standing,

        /// <summary>Held still as she takes in the hyper beam.</summary>
        Frozen,
    }

    private TimelinePlayer? _passing;
    private int _glowWait;

    /// <summary>A drained Samus in the given pose.</summary>
    /// <param name="pose">How she starts.</param>
    /// <param name="canStand">Whether she has the strength to get up when Up is pressed.</param>
    public DrainedState(Pose pose, bool canStand)
    {
        Posed = pose;
        CanStand = canStand;
    }

    /// <summary>How she is.</summary>
    public Pose Posed { get; private set; }

    /// <summary>Whether she has the strength to get up when Up is pressed.</summary>
    public bool CanStand { get; set; }

    /// <summary>Whether something else is moving her; she does not then fall by herself.</summary>
    public bool IsCarried { get; set; }

    /// <summary>Whether she glows with every colour in turn.</summary>
    public bool Glows { get; set; }

    /// <summary>How many updates each colour of her glow is held.</summary>
    public int GlowHold { get; set; } = 1;

    /// <summary>Which colour of her glow shows.</summary>
    public int GlowStep { get; private set; }

    /// <inheritdoc/>
    public string Look => Posed switch
    {
        Pose.Pinned => "drained_pinned",
        Pose.Kneeling => "drained",
        Pose.Struggling => "drained_struggling",
        Pose.GettingUp => "drained_getting_up",
        Pose.Standing => "drained_standing",
        Pose.Frozen => "drained_frozen",
        _ => throw new InvalidOperationException($"A drained Samus has no pose {Posed}."),
    };

    /// <inheritdoc/>
    public override bool CanAttack => false;

    /// <inheritdoc/>
    public override bool InControl => false;

    /// <summary>Pinned in a beam she is in its hands; on her knees or trying to rise she is in her own.</summary>
    public override bool InAnothersHands => Posed == Pose.Pinned;

    /// <summary>A knockback waits while she is in this state, as the cartridge refuses it here and tries again each update of its time.</summary>
    public override bool HoldsOffInterruptions => true;

    /// <summary>Puts her in a pose.</summary>
    public void Put(Pose pose)
    {
        Posed = pose;
        _passing = null;
    }

    /// <summary>
    /// Gives her the hyper beam: her beam becomes it, with the charge, wave and plasma beams on and the others off. Her
    /// charge is kept as it stands, as the cartridge keeps it, until her flare, which the hyper beam counts back from
    /// where it stands, has counted its fast ring of sparks back onto its first picture (<see cref="SamusFlare"/>).
    /// </summary>
    public static void GiveHyperBeam(Player samus)
    {
        ArgumentNullException.ThrowIfNull(samus);
        Inventory inventory = samus.Inventory;
        foreach (string beam in new[] { Equipment.ChargeBeam, Equipment.WaveBeam, Equipment.PlasmaBeam, Equipment.HyperBeam })
        {
            inventory.Grant(beam);
            inventory.Switch(beam, true);
        }

        foreach (string beam in new[] { Equipment.IceBeam, Equipment.Spazer })
        {
            if (inventory.Owns(beam))
                inventory.Switch(beam, false);
        }

        // The gift takes back the asking again of a charge's sound, as a beam shot does.
        (samus as Samus)?.ForgetChargingSoundResume();
    }

    /// <inheritdoc/>
    public override void Enter(Player owner)
    {
        owner.StopAcross();
        // Her charge is kept as it stands, as the cartridge keeps it: the drain only takes her control.
        owner.LoseMomentum();
        owner.TrySetForm(Player.Standing);
        owner.Velocity = PixelVelocity.Zero;
        if (Posed == Pose.Pinned)
            owner.Facing = Facing.Left;
    }

    /// <inheritdoc/>
    public override PlayerState? Move(Player owner)
    {
        if (Glows && --_glowWait <= 0)
        {
            _glowWait = Math.Max(1, GlowHold);
            GlowStep++;
        }

        // A pose that passes lasts as long as its timeline.
        if (Posed is Pose.Struggling or Pose.GettingUp)
        {
            _passing ??= new TimelinePlayer(((Samus)owner).Timelines.Get(PassingTimeline(owner)));
            _passing.Step();
            if (_passing.IsOver)
            {
                if (Posed == Pose.GettingUp)
                    return owner.StateAtRest();
                Put(Pose.Kneeling);
            }
        }
        else if (Posed == Pose.Kneeling && owner.Input.WasPressed(GameAction.Up) && owner.IsOnGround)
        {
            Put(CanStand ? Pose.GettingUp : Pose.Struggling);
        }

        owner.Velocity = owner.Velocity.WithX(Pixels.Zero);
        if (IsCarried || Posed is Pose.Pinned or Pose.Frozen)
        {
            owner.Velocity = PixelVelocity.Zero;
            return null;
        }

        owner.MoveDown();
        owner.Fall();
        return null;
    }

    /// <summary>The name of the timeline of the passing pose she is in, as her picture chooses it.</summary>
    private string PassingTimeline(Player owner)
    {
        var samus = (Samus)owner;
        string facing = owner.Facing == Facing.Right ? "right" : "left";
        string[] candidates = [$"{Look}_{facing}", Look];
        return candidates.FirstOrDefault(samus.Timelines.Has)
            ?? throw new InvalidOperationException($"Samus's data has no timeline for the look \"{Look}\".");
    }
}
