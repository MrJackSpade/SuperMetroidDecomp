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

## Regression tests

- Tests may use synthetic rooms, constructed cartridge data, fake address spaces, and other focused fixtures. A regression test does not need to drive a real retail room when a smaller fixture faithfully reproduces the reported failure.
- Prefer the smallest deterministic fixture that exercises the real production code path and makes the failure observable.
- Match the assertion to the report. Visual, positioning, animation, timing, and state-transition bugs need assertions for those exact properties; a nearby endpoint or no-crash assertion is not sufficient evidence of a fix.
- Use a real room or recorded controller sequence when the behavior depends on retail room data, interactions across systems, or a sequence that a synthetic fixture cannot reproduce faithfully.

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
