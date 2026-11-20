using Engine.Actors;

namespace Game;

/// <summary>
/// This game's player: the engine's player controller plus what is hers. Her morph ball is the engine's low form,
/// which the morph ball switches on and the spring ball lets jump; wall jumping and crouching are the engine's,
/// switched on for her. Those switches, and her numbers, are settings in her scene and data file, not code.
/// </summary>
public partial class Samus : Player
{
}
