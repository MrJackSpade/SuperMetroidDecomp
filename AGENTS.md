# Project Working Instructions

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
