#include "native-bounded-cpu.h"

// #402: execute original EnemyMain, not its C translation. Keep actor AI frozen
// and invisible so the experiment isolates timer/dispatch semantics, not pixels.
int DiagnosticXPlasmaTimers(const char *rom, const char *output) {
  int status = ProbeLoadRetailMovementRom(rom); if (status) return status;
  FILE *f = fopen(output, "wx"); if (!f) return 4;
  fprintf(f, "intangible,initial,frame,invincibility,flash,frozen,x,instruction\n");
  const uint16 initial[] = {0, 1, 2, 10, 0xffff};
  for (int intangible = 0; intangible < 2; intangible++)
  for (int sample = 0; sample < 5; sample++) {
    cpu_reset(g_snes->cpu); memset(g_ram, 0, sizeof(g_ram));
    g_snes->cpu->e = false; g_snes->cpu->sp = 0x1ff0; g_snes->cpu->dp = 0;
    first_free_enemy_index = 0x40; enemy_index_to_shake = 0xffff;
    active_enemy_indexes[0] = 0; active_enemy_indexes[1] = 0xffff;
    time_is_frozen_flag = 1;
    EnemyData *e = gEnemyData(0);
    e->enemy_ptr = 0xd47f; e->bank = 0xa2;
    e->properties = 0x0900 | (intangible ? 0x0400 : 0);
    e->x_pos = 80; e->y_pos = 64;
    e->current_instruction = 0xe477;
    e->invincibility_timer = initial[sample];
    e->flash_timer = 16; e->frozen_timer = 400;
    for (int frame = 1; frame <= 12; frame++) {
      ProbeRunBounded(0xa08fd4);
      fprintf(f, "%d,%u,%d,%u,%u,%u,%u,%u\n", intangible, initial[sample], frame,
        e->invincibility_timer, e->flash_timer, e->frozen_timer, e->x_pos, e->current_instruction);
    }
  }
  fclose(f); return 0;
}
