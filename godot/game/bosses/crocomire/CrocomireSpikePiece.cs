using Engine.Actors;
using Engine.Shared;
using Engine.World;

namespace Game.Enemies;

/// <summary>
/// One piece of the spike wall Crocomire's skeleton breaks through. It is thrown up and falls, gathering speed across
/// faster and faster up to its own limit, and bursts into dust on the floor. It harms nothing.
/// </summary>
public partial class CrocomireSpikePiece : Projectile
{
    private Pixels _growth;
    private Pixels _mostQuickening;
    private int _fastest;
    private Pixels _quickening;
    private Pixels _across;
    private Pixels _fall;
    private Pixels _gravity;
    private int _landingY;
    private string _dust = "";
    private string _crumbleSound = "";
    private string _landingSound = "";

    /// <summary>Sets the piece off.</summary>
    /// <param name="growth">How much its quickening across grows each update.</param>
    /// <param name="mostQuickening">The most its quickening across grows to.</param>
    /// <param name="fastest">The most whole pixels an update it moves across.</param>
    /// <param name="launchSpeed">The speed down it starts with; up, when less than nothing.</param>
    /// <param name="gravity">The speed down it gains each update.</param>
    /// <param name="landingY">How far down the room it lands, in pixels.</param>
    /// <param name="dust">The effect it leaves where it lands, by the data file of its timeline; empty for none.</param>
    /// <param name="crumbleSound">The sound of it crumbling as it lands, by name; empty for none.</param>
    /// <param name="landingSound">The sound of its landing, by name; empty for none.</param>
    public void Begin(Pixels growth, Pixels mostQuickening, int fastest, Pixels launchSpeed, Pixels gravity, int landingY, string dust, string crumbleSound, string landingSound)
    {
        (_growth, _mostQuickening, _fastest) = (growth, mostQuickening, fastest);
        (_fall, _gravity, _landingY) = (launchSpeed, gravity, landingY);
        (_dust, _crumbleSound, _landingSound) = (dust, crumbleSound, landingSound);
    }

    /// <inheritdoc/>
    protected override void BeforeTravel(Room room)
    {
        if (_quickening != _mostQuickening)
        {
            Pixels quicker = _quickening + _growth;
            _quickening = quicker < _mostQuickening ? quicker : _mostQuickening;
        }

        // Its speed takes only the whole 256ths of what quickens it, as the body's does.
        Pixels faster = _across + Pixels.FromParts(_quickening.Whole, _quickening.Fraction / 256 * 256);
        _across = faster.Whole > _fastest ? faster.WithWhole(_fastest) : faster;
        _fall += _gravity;
        Position += new PixelOffset(_across, _fall);
        if (Position.Y.Whole < _landingY)
            return;
        if (_crumbleSound.Length > 0)
            room.Session.Sounds.Request(_crumbleSound, channel: "enemies");

        // It is gone before its dust is raised: the dust, one of the enemies' shots on the cartridge as the piece is,
        // takes the highest place free, and is not raised with none.
        Leave(room);
        if (_dust.Length > 0)
            room.Spawn(Effect.Of(FrameTimeline.Read(room.Content.ReadData(_dust)), Position, holdsPlace: true));
        if (_landingSound.Length > 0)
            room.Session.Sounds.Request(_landingSound, channel: "enemies");
    }
}
