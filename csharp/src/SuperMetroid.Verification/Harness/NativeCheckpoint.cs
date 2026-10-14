namespace SuperMetroid.Verification.Harness;

/// <summary>One native checkpoint record: the boundary it was captured at and its WRAM.</summary>
/// <param name="SourceFrame">Source movie frame of the capture boundary.</param>
/// <param name="BoundaryPc">Long address of the capture boundary; zero for the terminal record.</param>
/// <param name="Memory">The record's WRAM, complete or limited to a layout.</param>
internal readonly record struct NativeCheckpoint(int SourceFrame, int BoundaryPc, NativeCheckpointMemory Memory);
