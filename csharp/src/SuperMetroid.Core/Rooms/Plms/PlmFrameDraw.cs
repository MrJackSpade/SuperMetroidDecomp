namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>One frame of a PLM draw family; two draws are the same draw when their frames are equal.</summary>
/// <typeparam name="TFrame">The family's frame: an enum of its frames, or a record of what varies between them.</typeparam>
/// <param name="Frame">Which of the family's draws this is.</param>
internal sealed record PlmFrameDraw<TFrame>(TFrame Frame) : PlmDraw where TFrame : struct;
