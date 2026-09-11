#include "native-bounded-cpu.h"

// Original CPU, authored vulnerable Ninja component, stationary penetrating shot.
// This isolates contact and the freeze/release boundary, not firing admission.
int DiagnosticXPlasmaSteel(const char *rom, const char *output) {
  int status = ProbeLoadRetailMovementRom(rom); if (status) return status;
  FILE *f = fopen(output, "wx"); if (!f) return 4;
  fprintf(f, "charged,frozen,stage,health,invincibility,flash,map,x,y,type,damage\n");
  for (int charged = 0; charged < 2; charged++)
  for (int frozen = 15; frozen <= 16; frozen++) {
    cpu_reset(g_snes->cpu); memset(g_ram, 0, sizeof(g_ram));
    g_snes->cpu->e = false; g_snes->cpu->sp = 0x1ff0; g_snes->cpu->dp = 0;
    EnemyData *body = gEnemyData(0);
    body->enemy_ptr = 0xf593; body->bank = 0xb2;
    body->x_pos = body->y_pos = 128; body->health = 1800;
    body->spritemap_pointer = 0x89c4; body->extra_properties = 4;
    first_free_enemy_index = 0x40; enemy_index_to_shake = 0xffff;
    active_enemy_indexes[0] = 0; active_enemy_indexes[1] = 0xffff;
    projectile_counter = 1;
    projectile_x_pos[0] = projectile_y_pos[0] = 118;
    projectile_x_radius[0] = projectile_y_radius[0] = 4;
    projectile_type[0] = charged ? 0x8018 : 0x8008;
    projectile_damage[0] = charged ? 450 : 150;
    projectile_dir[0] = 2;
    for (int stage = 0; stage <= frozen + 1; stage++) {
      if (!stage) { cur_enemy_index = 0; ProbeRunBounded(0xa09b7f); }
      else {
        time_is_frozen_flag = stage <= frozen;
        ProbeRunBounded(0xa08fd4);
      }
      fprintf(f, "%d,%d,%d,%u,%u,%u,%u,%u,%u,%u,%u\n", charged, frozen, stage,
        body->health, body->invincibility_timer, body->flash_timer,
        body->spritemap_pointer, body->x_pos, body->y_pos,
        projectile_type[0], projectile_damage[0]);
    }
  }
  fclose(f); return 0;
}
