namespace SuperMetroid.Core.Frontend.GameOver;

/// <summary>What the game-over Baby Metroid shows at one moment of its animation: its pose and its colours.</summary>
/// <param name="Frame">The pose.</param>
/// <param name="Colours">The colours it shows the pose in.</param>
internal readonly record struct GameOverBabyLook(GameOverBabyFrame Frame, GameOverBabyColouring Colours);
