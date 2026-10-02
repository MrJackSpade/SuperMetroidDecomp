# Before/failed-frame diagnostics (#1187)

Affected version: 0.4.8-smoke+83291286e13a1fb32c9551f996220b45760fd7df.
The #1186 console stack named the footstep bounds check but omitted the live
handler and animation state needed to explain the invalid frame.

The playable host now copies a side-effect-free gameplay snapshot before each
frame. On failure, after flushing the input recorder, it captures the partially
mutated state and writes both snapshots to stderr before fatal rethrow or
recoverable reporting. The session log and its diagnostic ZIP receive the same
console text. Opt-in GitHub issues include the identical diagnostic payload.

The payload includes:

- Informational build version (including commit), Core and host module build IDs.
- Attempted controller input, recording path, and zero-based input index. The
  index comes from the recording count, not the wrapping 16-bit game frame.
- Before/failed-frame game state, room/state/door, pose, animation frame, timer,
  delay-list address, position, current/new input, and input lock.
- Persistent drained phase and get-up handler, escape timer state, FX/liquid
  medium, and water/acid surfaces.

For an in-memory replay the original path is unavailable to this control; the
payload says so explicitly and still reports the exact replay input index.
The exception type/message/stack remain unchanged, so live diagnostic values do
not multiply automatic issue fingerprints. A console-write failure retains the
original exception inside an aggregate instead of hiding it.

`--github-error-reporter-audit` verifies snapshot immutability across the exact
8-to-18 animation mutation, the stale `UnableToStand` handler, acid/pose context,
build identifiers, a non-wrapping recording index of 70000, propagation into the
GitHub body, preservation of the original exception, and stable deduplication
when diagnostic state differs. The fixture uses a fake issue client and sends
no test reports to GitHub.
