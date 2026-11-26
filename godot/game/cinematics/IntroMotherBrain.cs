using Engine.Actors;
using Engine.Collision;
using Engine.Content;
using Engine.Shared;
using Engine.World;
using Game.Data;
using Godot;

namespace Game.Cinematics;

/// <summary>
/// Mother Brain as Samus remembers her, in the opening's flashback. She pulses in her tank while rings appear and
/// drift down at Samus, one of them striking her. The fourth missile to reach her sets her blowing up, the room
/// shaking; when that has gone on long enough she asks for the next page, and she is gone once it has come.
/// </summary>
/// <remarks>
/// Her data names her timeline "pulsing" and, by the data files of their timelines, the effects "ring" (which shows
/// itself appearing and then loops), "small_explosion" and "big_explosion" (which loop).
/// </remarks>
public partial class IntroMotherBrain : Enemy
{
    /// <summary>The effect of a ring, by the data file of its timeline.</summary>
    [Export, DataValue("ring")]
    public string RingEffect { get; set; } = "";

    /// <summary>The effect of a small blast.</summary>
    [Export, DataValue("small_explosion")]
    public string SmallExplosionEffect { get; set; } = "";

    /// <summary>The effect of a big blast.</summary>
    [Export, DataValue("big_explosion")]
    public string BigExplosionEffect { get; set; } = "";

    private enum Phase
    {
        TakingHits,
        Exploding,
        AwaitingNextPage,
    }

    /// <summary>How many missiles it takes, how long she flashes at each, and how long she blows up for.</summary>
    private const int HitsToExplode = 4, FlashUpdates = 8, ExplodingUpdates = 128;

    /// <summary>How long a ring takes to appear before it moves.</summary>
    private const int RingAppearingUpdates = 30;

    /// <summary>
    /// How many updates the ring that strikes Samus throws her back for: longer than a hit of play's, far enough to
    /// carry her from the pillar onto the ledge behind her.
    /// </summary>
    private const int RingKnockbackUpdates = 11;

    /// <summary>The waves of rings: how long each waits after the one before, and each ring's place, drift across and whether it strikes Samus.</summary>
    private static readonly (int Wait, (int X, int Y, double Drift, bool Strikes)[] Rings)[] Waves =
    [
        (75, [(112, 72, 0.5, true), (192, 56, -0.5, false)]),
        (128, [(128, 48, 0.5, false), (232, 80, -0.5, false)]),
    ];

    /// <summary>The small blasts, then the big ones: where each goes off from her middle and how long it waits.</summary>
    private static readonly (int X, int Y, int Delay)[] SmallBlasts = [(16, 0, 1), (-16, 4, 8), (-16, -8, 16)];
    private static readonly (int X, int Y, int Delay)[] BigBlasts = [(0, 0, 1), (16, -16, 16), (-16, 8, 32), (-8, -16, 48), (8, 8, 64)];

    private readonly List<(Effect Ring, bool Strikes, double Drift, int Age)> _rings = [];
    private readonly List<(string File, PixelPosition At, int Wait)> _blastsToCome = [];
    private readonly List<Effect> _blasts = [];
    private Phase _phase;
    private int _hits;

    /// <summary>How far the room's picture jumps as she blows up, in pixels.</summary>
    private const int ShakePixels = 4;

    /// <summary>Whether the last missile struck on this update: her blowing up begins on the next.</summary>
    private bool _struckThisUpdate;

    /// <summary>How many updates the room's picture has been shaken.</summary>
    private int _shaken;
    private int _wave;
    private int _waveWait;
    private int _left;

    /// <summary>Whether she has blown up for long enough that the page should turn.</summary>
    public bool WantsNextPage { get; private set; }

    /// <summary>Whether she has started blowing up.</summary>
    public bool IsExploding => _phase != Phase.TakingHits;

    /// <inheritdoc/>
    protected override void OnEnterRoom()
    {
        (_phase, _hits, _wave, _waveWait, WantsNextPage) = (Phase.TakingHits, 0, 0, Waves[0].Wait, false);
        _rings.Clear();
        _blastsToCome.Clear();
        _blasts.Clear();
        Play("pulsing");
    }

    /// <inheritdoc/>
    public override bool IsHitBy(HitSide side) => side != HitSide.Enemies && _phase == Phase.TakingHits;

    /// <inheritdoc/>
    public override HitResult TakeHit(Hit hit, Collider part)
    {
        if (_phase != Phase.TakingHits)
            return HitResult.Ignored;
        if (!hit.Carries(new HitKind(HitKinds.Missile)))
            return HitResult.Absorbed;
        FlashFor(FlashUpdates);
        if (++_hits < HitsToExplode)
            return HitResult.Hurt;
        (_phase, _left) = (Phase.Exploding, ExplodingUpdates);
        _struckThisUpdate = true;
        foreach ((int x, int y, int delay) in SmallBlasts)
            _blastsToCome.Add((SmallExplosionEffect, Position + PixelOffset.FromWhole(x, y), delay));
        foreach ((int x, int y, int delay) in BigBlasts)
            _blastsToCome.Add((BigExplosionEffect, Position + PixelOffset.FromWhole(x, y), delay));
        return HitResult.Hurt;
    }

    /// <inheritdoc/>
    protected override void Behave()
    {
        if (_struckThisUpdate)
        {
            // The missile struck in her own turn; her blowing up begins on the update after.
            _struckThisUpdate = false;
        }
        else if (_phase != Phase.TakingHits)
        {
            // For as long as she is blowing up the room's picture, and nothing else, jumps up on every other update
            // and is back on the one between: she, her blasts and Samus stay where they are.
            Room.Terrain.ShownShift = new Godot.Vector2I(0, _shaken++ % 2 == 0 ? -ShakePixels : 0);
            StepBlasts();
            if (_phase == Phase.Exploding && --_left <= 0)
            {
                _phase = Phase.AwaitingNextPage;
                WantsNextPage = true;
            }
        }

        StepRings();
    }

    /// <summary>Takes her, her blasts and her rings out of the room, once the page has turned.</summary>
    public void Leave()
    {
        foreach (Effect blast in _blasts)
            Room.Remove(blast);
        foreach ((Effect ring, _, _, _) in _rings)
            Room.Remove(ring);
        _blasts.Clear();
        _rings.Clear();
        _blastsToCome.Clear();
        Room.Terrain.ShownShift = Godot.Vector2I.Zero;
        Vanish();
    }

    private void StepBlasts()
    {
        for (int index = _blastsToCome.Count - 1; index >= 0; index--)
        {
            (string file, PixelPosition at, int wait) = _blastsToCome[index];
            if (--wait > 0)
            {
                _blastsToCome[index] = (file, at, wait);
                continue;
            }

            _blastsToCome.RemoveAt(index);
            if (file.Length > 0)
                _blasts.Add(LeaveEffect(file, at));
        }
    }

    private void StepRings()
    {
        if (_wave < Waves.Length && --_waveWait <= 0)
        {
            foreach ((int x, int y, double drift, bool strikes) in Waves[_wave].Rings)
            {
                if (RingEffect.Length > 0)
                    _rings.Add((LeaveEffect(RingEffect, PixelPosition.FromWhole(x, y)), strikes, drift, 0));
            }

            if (++_wave < Waves.Length)
                _waveWait = Waves[_wave].Wait;
        }

        Player samus = Room.Player;
        for (int index = _rings.Count - 1; index >= 0; index--)
        {
            (Effect ring, bool strikes, double drift, int age) = _rings[index];
            age++;
            _rings[index] = (ring, strikes, drift, age);
            if (age < RingAppearingUpdates)
                continue;

            // Once it has appeared it drifts down and across.
            ring.Velocity = new PixelVelocity(Pixels.FromNumber(drift), Pixels.FromNumber(0.5));
            bool gone;
            if (strikes)
            {
                bool reached = ring.Position.X.Whole + 8 >= samus.Position.X.Whole - 5;
                if (reached)
                    samus.KnockBack(Facing.Left, RingKnockbackUpdates);
                gone = reached || IsExploding;
            }
            else
            {
                int y = ring.Position.Y.Whole;
                gone = y < 16 || y >= 208 || IsExploding;
            }

            if (!gone)
                continue;
            Room.Remove(ring);
            _rings.RemoveAt(index);
        }
    }
}
