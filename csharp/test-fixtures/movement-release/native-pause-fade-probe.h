// #471: unpatched cartridge CPU fade recurrence, independent of rasterization.
// Include after native-release-probe.h; dispatch before SDL initialization.
int DiagnosticPauseFade(const char *rom, const char *output) {
  int status = ProbeLoadRetailMovementRom(rom); if (status) return status;
  FILE *f = fopen(output, "wx"); if (!f) return 4;
  fprintf(f, "stage,frame,brightness,delay,counter\n");
  for (int stage = 0; stage < 4; stage++) {
    cpu_reset(g_snes->cpu); memset(g_ram, 0, sizeof(g_ram));
    g_snes->cpu->e = false; g_snes->cpu->sp = 0x1ff0; g_snes->cpu->dp = 0;
    // Gameplay -> pause obtains the actual caller's initialization. The other
    // three callers ($828D2F, $82A5C9, $82937C) seed the identical two words.
    game_state = 8; joypad1_newkeys = 0x1000;
    RunAsmCode(0x90ea45, 0, 0, 0, 0);
    if (game_state != 12 || screen_fade_delay != 1 || screen_fade_counter != 1) {
      fclose(f); return 5;
    }
    reg_INIDISP = stage & 1 ? 0 : 15;
    for (int frame = 0; frame < 30; frame++) {
      RunAsmCode(stage & 1 ? 0x80894d : 0x808924, 0, 0, 0, 0);
      fprintf(f, "%d,%d,%02X,%04X,%04X\n", stage, frame,
        reg_INIDISP, screen_fade_delay, screen_fade_counter);
    }
  }
  fclose(f); return 0;
}
