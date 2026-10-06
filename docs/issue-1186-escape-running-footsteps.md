# Escape running-animation crash (#1186)

Affected version: 0.4.8-smoke+83291286e13a1fb32c9551f996220b45760fd7df.
Player was jumping under acid to break a wall during escape. Failure:
`Running animation frame 18 is outside ten foot-contact selectors.`

Mother Brain's low-health rainbow-beam route installs the failed-stand callback
at $90:E0C5. It deliberately has no pose check: an Up edge on frames 8..11 writes
frame 18/timer 1. This callback belongs to persistent Samus state and survives
ordinary door transitions. It must be replaced when the native shared hack-handler
word is replaced, not merely when a room or pose changes.

Native Mother Brain death at $A9:B302/B305 calls Samus command $0F, then starts the
timer. Command $90:F2D8/F2DB writes the timer callback over the hack-handler word.
The port's enemy sequence already emitted `TimerHandlingEnableRequested`, but
the runtime ignored it while consuming the separate timer-start flag. Thus the
old failed-stand callback could later overwrite a running frame during escape.

The runtime now consumes that request and relinquishes the drained callback.
The ten-entry footstep table and its strict bounds check are unchanged. Sources:
J/U NTSC 1.0 ROM, pinned InsaneFirebat revision
362be646929cf8e483f692b73a6561cfc2dc1d0d, and
[Patrick Johnston's command $0F reference](https://patrickjohnston.org/bank/90#F2D8).

`--escape-running-footsteps` starts at the identified Mother Brain death phase,
obtains its real request, applies the production runtime request consumer, then
presses Up/jump on running frame eight under acid. Before the fix it throws the
exact frame-18 exception. After the fix both facings retain frame nine, with a
valid delay and no drained handler. Both drained handler variants are checked;
the genuine failed-stand cutscene still selects frame 18 before replacement.

The newest available local recording (20261002-105547-199) replayed without this
failure, so it is not claimed as an exact player-recording reproduction. The
faithful focused fixture establishes the faulty handoff and confirms the fix.
DebugRunner's `--reported-running-footsteps RECORDING INSTALL_ROOT TRACE` keeps
the final 301 input frames, and replay traces now include animation frame/timer.
Player confirmation remains pending.
