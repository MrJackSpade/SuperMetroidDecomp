# Issue 639: simultaneous Left+Right input

The private glitch-demo corpus contains six Snes9x movie-v5 recordings that hold
the otherwise-impossible SNES Left+Right chord through distinct gameplay owners:

| Recording | Frames | Continuous L+R run |
| --- | ---: | ---: |
| `SM ballbouncing 2 tiles leftwards with L+R held.smv` | 5,109 | 8-4,799 |
| `SM diag bombjump while holding L+R.smv` | 382 | 4-382 |
| `SM entering Kraid's Lair with L+R held.smv` | 702 | 0-652 |
| `SM Kraid fight with L+R held.smv` | 3,417 | 0-3,417 |
| `SM riding elevator up & down with L+R.smv` | 8,551 | 0-8,551 |
| `Super_Metroid_JU_(ok checksum) Red Tower climb with L+R held.smv` | 4,141 | 1-4,068 |

The movie headers name `Super_Metroid_JU_(ok checksum).smc` and the pinned local
ROM has SHA-256
`12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72`
(CRC32 `D63ED5F8`). The private movies, extracted controller streams, ROM, and
checkpoints remain under ignored `csharp/test-temp/issue-639-movies`; none is
published by this fixture.

The available Snes9x 1.51 capture build rejected both an unmodified complete
movie and a generated frame-zero checkpoint as a ROM/movie mismatch despite the
matching ROM identity. Following the issue's fallback rule, the committed tests
therefore verify the bounded cartridge behavior directly through the production
paths rather than claiming an unavailable playback comparison:

- `ControllerBindings.Normalize` and the NMI latch preserve both direction bits.
- Rising edges depend on press order; an added Left or Right is not collapsed.
- The compiled bank-$91 pose graph retains the cartridge's ordered first-match
  priority for standing poses.
- Bank-$90 wall-jump selection retains its asymmetric Left-first probe order.
- Airborne Morph Ball movement treats the chord as directional input and uses
  the pose's direction; it does not cancel horizontal movement.
- Adding Up while L+R remains held still emits the Up edge consumed by elevator
  AI.
- The complete retail door-transition fixture keeps the live chord through the
  destination OAM handoff without manufacturing a release or a second press.

Bomb-jump direction is selected from bomb-versus-Samus geometry before the
special movement owner begins; that owner takes no controller word. Kraid's
enemy AI likewise has no separate host input normalization. Both therefore use
the same latched Samus state and ordered pose rules covered above rather than a
host-specific L+R path.
