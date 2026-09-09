// #478: original SPC700 sequencer stress with Ridley roar/explosion overlap.
// Include after native-spc-probe.h. No SDL, audio device, ROM data or save writes.
int DiagnosticRidleySpcTicks(const char *engine, const char *music, const char *output) {
#ifdef _WIN32
  SetErrorMode(1 | 2);
#endif
  FILE *trace = fopen(output, "wx");
  if (!trace) return 2;
  Apu *apu = apu_init();
  apu_reset(apu);
  if (!ProbeLoadSpcStream(apu, engine) || !ProbeLoadSpcStream(apu, music)) {
    fclose(trace); apu_free(apu); return 3;
  }
  apu->spc->pc = 0x1500;
  int target = 0x15c4, tick = 0;
  uint8 elapsed = 0;
  for (unsigned cycle = 0; cycle < 40000000 && tick < 6000; cycle++) {
    apu_cycle(apu);
    if (apu->dsp->sampleOffset == 534) apu->dsp->sampleOffset = 0;
    if (apu->spc->pc != target) continue;
    target ^= 0x15c4 ^ 0x15c5;
    if (target != 0x15c4) continue;
    fprintf(trace, "T %d %u ", tick, elapsed);
    for (int reg = 0; reg < 128; reg++) fprintf(trace, "%02X", apu->dsp->ram[reg]);
    fprintf(trace, " ");
    for (int port = 0; port < 4; port++) fprintf(trace, "%02X", apu->ram[port]);
    fprintf(trace, "\n");
    elapsed = apu->spc->y;
    if (tick == 500) apu->inPorts[0] = 5;
    if (tick == 1500) apu->inPorts[2] = 0x59;
    if (tick == 1520) apu->inPorts[2] = 0;
    // Driver ticks are not video frames. This is intentionally a sequencer
    // overlap stress, not the player's exact 896-frame battle timing.
    if (tick >= 1700 && tick <= 2980 && (tick - 1700) % 40 == 0) apu->inPorts[2] = 0x24;
    if (tick >= 1720 && tick <= 3000 && (tick - 1720) % 40 == 0) apu->inPorts[2] = 0;
    if (tick == 3600) apu->inPorts[0] = 3;
    tick++;
    apu->hist.count = 0;
  }
  fclose(trace);
  apu_free(apu);
  if (tick != 6000) { fprintf(stderr, "Ridley SPC timeout at %d\n", tick); return 4; }
  return 0;
}
