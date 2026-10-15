using SuperMetroid.Core.Gameplay;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Core.Runtime;

/// <summary>
/// One published gameplay display: the latest NMI latch, the inputs the cartridge reads during
/// display (sampled after the main loop), and the PPU memory image. Immutable; capturing it
/// any number of times yields the same picture and never advances simulation.
/// </summary>
/// <param name="Latched">Values the most recent accepted NMI made PPU-visible.</param>
/// <param name="FrameEnd">IRQ- and HDMA-read inputs sampled at publication.</param>
/// <param name="Memory">VRAM, CGRAM, and displayed OAM at publication.</param>
public sealed record GameplayDisplayFrame(
    GameplayNmiDisplayLatch Latched,
    GameplayFrameEndDisplayInputs FrameEnd,
    PpuMemorySnapshot Memory);
