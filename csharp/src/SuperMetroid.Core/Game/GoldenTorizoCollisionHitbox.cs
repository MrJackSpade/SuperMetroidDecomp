namespace SuperMetroid.Core.Game;

/// <summary>Native ordered touch/shot rectangle from a bank-$AA hitbox list.</summary>
/// <param name="Left">Left extent relative to the component origin.</param>
/// <param name="Top">Upper extent relative to the component origin.</param>
/// <param name="Right">Right extent relative to the component origin.</param>
/// <param name="Bottom">Lower extent relative to the component origin.</param>
/// <param name="TouchAi">Native callback pointer for Samus contact.</param>
/// <param name="ShotAi">Native callback pointer for projectile contact.</param>
internal readonly record struct GoldenTorizoCollisionHitbox(
    short Left, short Top, short Right, short Bottom,
    ushort TouchAi, ushort ShotAi);
