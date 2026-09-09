// Include after native-release-probe.h and dispatch before SDL initialization.
// #472: original CPU accumulator and queue mutation for the boost sound call.
int DiagnosticSoundQueueReturn(const char *rom, const char *output) {
  int status = ProbeLoadRetailMovementRom(rom);
  if (status) return status;
  FILE *f = fopen(output, "wx");
  if (!f) return 4;
  fprintf(f, "occupancy,suppression,accumulator,writeIndex,slotValue\n");
  for (int occupancy = 0; occupancy < 16; occupancy++) {
    for (int suppression = 0; suppression < 4; suppression++) {
      cpu_reset(g_snes->cpu); memset(g_ram, 0, sizeof(g_ram));
      g_snes->cpu->e = false; g_snes->cpu->sp = 0x1ff0;
      sfx_writepos[2] = occupancy;
      debug_disable_sounds = suppression == 1;
      game_state = suppression == 2 ? 0x28 : 8;
      power_bomb_explosion_status = suppression == 3 ? 0x8000 : 0;
      RunAsmCode(0x80914d, 3, 0, 0, 0);
      fprintf(f, "%d,%d,%04X,%d,%d\n", occupancy, suppression,
        g_snes->cpu->a, sfx_writepos[2], sfx3_queue[occupancy]);
    }
  }
  fclose(f);
  return 0;
}
