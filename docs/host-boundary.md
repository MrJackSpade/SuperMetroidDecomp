# Host boundary (SME6448)

`SuperMetroid.Core` is the game. It publishes a display packet and an audio frame each admitted step and consumes typed
controller input through the frame input latch. Everything that schedules, presents, times, configures or reads a
device is the host's: frame mailboxes, presentation gates, wall-clock deadlines and catch-up policy, renderer selection,
aspect correction and viewport fitting, options files, device input, legacy save-file import and recording formats
(v0.9.0 manifesto, section 3, "Hosts"). The Godot port replaces the host and carries Core over unchanged, so none of the
host's concerns may live in Core.

## Target shape

- Core holds the typed button set, the frame input latch, the display packet, the audio frame, the session composer and
  a typed record of the option values the game reads. It holds no device type, no thread hand-off for presentation, no
  clock, no file format for configuration and no renderer name.
- A host supplies what the session needs through narrow interfaces at the composition root. The Desktop host composes a
  Desktop session in `SuperMetroid.Desktop`; a type used only by the Direct3D 11 backend lives in
  `SuperMetroid.Rendering.Direct3D11`.
- Host timing (a deadline after each completed step, the catch-up limit, rebasing after a long stall) is the host's
  policy. The simulation advances one admitted step at a time and never reads the wall clock.

## The rule

`HostConcernAnalyzer` reports, in the `SuperMetroid.Core` assembly only:

- a type or member whose camel-case words name the Windows API: an identifier that begins with `Windows` followed by
  more words, or one that contains `Win32`, `WinMM`, `Hwnd`, `JoyCaps`, `XInput`, `DirectInput` or `RawInput`;
- a method declared as a platform-invoke entry point (`DllImport`, `LibraryImport`);
- a type or member that names an input device or driver: `Usb`, `Hid`, `Gamepad`, `Joystick`, `VendorId`, `ProductId`;
- a thread-synchronization primitive (`lock`, `SemaphoreSlim`, `Monitor`, `Interlocked`, `Volatile` and the other
  `System.Threading` primitives) used inside a type or namespace whose name puts it in presentation (`Render`,
  `Rendering`, `Renderer`, `Present`, `Presentation`, `Presenter`, `Mailbox`, `Display`, `Publication`, `Publisher`),
  and any type or member named as a `Mailbox`;
- a wall-clock read (`Stopwatch`, `TimeProvider`, timers, `DateTime.Now`, `DateTime.UtcNow`, `Environment.TickCount`,
  `Thread.Sleep`, `Task.Delay`) and any type or member named for a `Deadline`, the `WallClock` or `CatchUp`;
- a type or member named for an `Ini` file, and a string naming an `.ini` file;
- a type or member naming a renderer: `Direct3D`, `Direct3D11`, `D3D11`, `DirectX`, `Dxgi`, `OpenGL`, `Vulkan`,
  `SwapChain`.

Words match whole camel-case words or runs of adjacent words, ignoring case, with a plural `s`. `Windows` alone or after
another word (`GameplayWindows`) is the plural of a display window and is not reported; `Hidden` is not `Hid`; a game
timer is not the wall clock; the input latch's lock hands buttons between threads and is not presentation.

## Exemplar

```csharp
// Before, in Core: a host's frame pacing and its backend choice.
public readonly record struct HostFrameDeadline(double NextDeadline, double WaitSeconds, bool Rebased);
public enum RendererSelection { Software, Direct3D11, Auto }

// After: each lives with the host that uses it. The Android host's pacing is AndroidFrameDeadline in its host
// support (SuperMetroid.Diagnostics/Android); renderer choice is the Desktop host's. Core's options record holds
// only the values the game reads, and each host reads its own file and hands Core that record.
```

## Allowance

There is none. A host concern found in Core moves to the host that owns it, and the game takes the value it needs through
a narrow typed interface. No baseline, allowlist, count, severity change or scope change (project manifesto, section 6).
