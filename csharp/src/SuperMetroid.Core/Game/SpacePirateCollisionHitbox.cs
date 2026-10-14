namespace SuperMetroid.Core.Game;

/// <summary>Engine-owned signed collision bounds and touch/shot callbacks.</summary>
/// <param name="Left">Left contact extent relative to its component origin.</param>
/// <param name="Top">Upper contact extent relative to its component origin.</param>
/// <param name="Right">Right contact extent relative to its component origin.</param>
/// <param name="Bottom">Lower contact extent relative to its component origin.</param>
/// <param name="TouchAi">Native Pirate contact callback identity.</param>
/// <param name="ShotAi">Native Pirate projectile-hit callback identity.</param>
internal readonly record struct SpacePirateCollisionHitbox(short Left, short Top, short Right, short Bottom, ushort TouchAi, ushort ShotAi);
