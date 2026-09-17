# Project Working Instructions

## Discord version updates

- Use `pwsh -File tools/discord-updates.ps1 next` to obtain the single next
  unposted published commit. Follow `tools/discord-updates.md` for connection and
  recovery. The queue starts at the first commit and targets server
  `1547270817312809112`, channel `1547271407476408411`.
- After every successful push, drain all unposted published commits before
  handing control back to the user. The user has authorized this recurring
  posting workflow and repo text other than API keys; do not ask again.
- Run `next`, inspect only the exposed commit, write a concise plain-language
  summary of that version, preview it, and post with its full SHA. While historical
  commits remain, announce the oldest unposted commit, even when it differs from
  the commit just pushed. After each confirmed post, run `next` again and repeat
  this loop until it returns `caught-up`. Do not stop after one update. Catch-up
  requests resume the existing tracker; never restart from the first commit.
  If already caught up, no post is needed.
- Install the tracked push reminder with `git config core.hooksPath .githooks`
  as described in `tools/discord-updates.md`. Its `pre-push` output reminds the
  model to perform this loop AFTER the push succeeds; it does not post itself.
- Do not enumerate later commits, skip ahead, or use raw webhooks to bypass the
  tracker. Verify the delivery receipt and report any posting failure at handoff;
  do not treat a failed or uncertain send as a completed announcement.
- Keep webhook credentials in `DISCORD_WEBHOOK_URL` and tracker state in the Git
  common directory. Never commit either. If delivery is uncertain, reconcile the
  existing attempt before posting again.

## Diagnostic references

- [InsaneFirebat Super Metroid disassembly](https://github.com/InsaneFirebat/sm_disassembly)
- [Patrick Johnston's bank reference](https://patrickjohnston.org/bank/index.html)

Use these references during cartridge diagnostics to cross-check routines, symbols,
addresses, and behavior. Confirm findings against the project's ROM revision and
pinned local sources; reference annotations do not replace reproducing a reported bug.

## Process-boundary error handling

- No repository executable, verifier, diagnostic, replay tool, or temporary probe may
  allow an exception to escape to the CLR or Windows error reporter. This applies to
  one-off investigation programs under `test-temp` just as strictly as shipped hosts.
- Every console entry point must install the existing Windows no-dialog process policy
  before doing fallible work, wrap its outermost process boundary in `try`/`catch`, write
  `exception.ToString()` to standard error, and return a nonzero exit code. Follow
  `SuperMetroid.Verification` or `SuperMetroid.DebugRunner`; do not create unguarded
  top-level statements that can throw.
- GUI entry points must use the established `UnhandledExceptionConsole`/dispatcher
  boundary so failures remain visible in the console or diagnostic log without a modal
  dialog. Do not add `MessageBox`, Windows Error Reporting, or debugger-only UI as an
  error path.
- Failing loudly means a full exception and failing exit code or the configured GitHub
  recoverable-error report. It never means a focus-stealing dialog. Tests for expected
  failures must catch them inside the test harness rather than leaking them from the
  process.

## Regression tests

- Tests may use synthetic rooms, constructed cartridge data, fake address spaces, and other focused fixtures. A regression test does not need to drive a real retail room when a smaller fixture faithfully reproduces the reported failure.
- Prefer the smallest deterministic fixture that exercises the real production code path and makes the failure observable.
- Match the assertion to the report. Visual, positioning, animation, timing, and state-transition bugs need assertions for those exact properties; a nearby endpoint or no-crash assertion is not sufficient evidence of a fix.
- Use a real room or recorded controller sequence when the behavior depends on retail room data, interactions across systems, or a sequence that a synthetic fixture cannot reproduce faithfully.

## Issue report versions

- Every player bug report must record the affected game version in the GitHub
  issue body under `Affected version`. Include it when creating or reopening an
  issue, or when adding a new report to an existing issue.
- Use the version supplied by the player; carry it forward for subsequent reports
  in the same testing session until the player specifies a different version.
  Never infer the affected version from the current repository or latest release.
- If the version is unknown, record `Unknown (awaiting player version)` and ask
  for it. Update the issue when supplied, preserving earlier reported versions.

## Issue-fix workflow

- On the first attempt to fix an issue, exact reproduction is optional when the report is clear and the defect is small or mechanically obvious. Reproduce first whenever diagnosis is uncertain, the change is risky, or the reported property cannot otherwise be verified confidently.
- If the user reports that a first fix failed, exact reproduction becomes mandatory before making another fix attempt. Do not apply a second speculative fix. Capture the failing behavior with a real-room sequence or a faithful synthetic fixture, make the relevant assertion fail, and only then change production code.
- Treat each repeated report as evidence that the earlier test or diagnosis was insufficient. Expand the reproduction to cover the user's exact trigger, trajectory, timing, or visual property rather than rerunning a nearby test.
- Do not make the user report the same defect a third time because the second attempt was not tested against the actual failure.
- Do not close a reported issue solely because a synthetic test passes. After reproducing as required, add an appropriate regression test, verify the fix against that reproduction, and leave the issue open for player confirmation unless explicitly instructed otherwise.
- As soon as an implemented fix is ready for player confirmation, add the
  `awaiting-player-validation` label to its GitHub issue. Remove that label if
  player validation fails, and close the issue when the player confirms the fix.
  An open issue without this label must not be assumed to be awaiting validation.

## Emulation scope limits

- Techniques and reported behavior that require unbounded memory corruption are
  outside the project's implementation scope. This exemption applies specifically
  when the behavior cannot be reproduced with bounded modeled state or a faithful
  deterministic fixture and would instead require full cartridge-compatible memory,
  CPU-state, and native-side-effect emulation.
- Do not approximate, hard-code, or otherwise implement the observed outcome of an
  exempt technique. Once investigation establishes this boundary, stop implementation,
  document the architectural reason on the GitHub issue, and close it as `not planned`.
- Do not treat memory corruption by itself as sufficient grounds for exemption. If its
  relevant effects can be bounded and reproduced faithfully without full memory
  emulation, follow the normal issue-fix and regression-test workflow. Keep any
  separable bounded behavior in scope, using a separate issue when appropriate.

## Batch handoff summaries

- At the end of every batch containing multiple fixes, provide a self-contained summary before returning control to the user. The user must not need to reconstruct the results from intermediate progress messages or earlier conversation history.
- Include one entry for every fix in the batch. For each entry, state the reported problem, the diagnosed root cause, the implemented solution, and how the result was verified.
- Clearly distinguish fixes that were reproduced and verified from first-pass fixes that did not require reproduction, and identify anything still awaiting player confirmation.
- Also summarize relevant tests, commits, pushes, and remaining open or deferred work once for the batch.

## Commit and push discipline

- Commit and push every completed change as soon as it has passed its proportionate
  verification. Do not leave recoverable work only in the local working tree between
  handoffs.
- Keep commits logically sized and scoped. A focused fix and its regression test belong
  together. Prefer one commit per reported ticket, containing that ticket's production fix
  and regression test. Use a shared commit only when multiple tickets have the same root
  cause or cannot be separated without leaving a broken intermediate state, and identify
  every covered ticket in the commit message or body.
- Never rewrite or discard the user's history to accomplish this. A committed regression
  can be reverted, while uncommitted lost work cannot be recovered.

## Cartridge constants and definition data

- Put ROM addresses, native dispatcher identifiers, instruction-list pointers, phase IDs,
  table offsets, and similar definition sets in dedicated, domain-named catalog types. Do
  not park them as constants at the top of a functional runtime, renderer, or state class.
- Call sites should read as named domain operations, for example
  `DoorCodes.DoorCode_Scroll6_Green`, rather than raw hexadecimal values with explanatory
  comments beside the behavior.
- Put the address, native symbol, and identity explanation on the catalog member's XML
  summary. Keep comments in functional code for behavior, ordering, and non-obvious side
  effects instead of repeating what a numeric value denotes.
- Use an enum only when values are a proven mutually exclusive domain. Use `[Flags]` only
  when the cartridge demonstrably treats the values as composable bits; do not infer flag
  semantics from suggestive values alone.
