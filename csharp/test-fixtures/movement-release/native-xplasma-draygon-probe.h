#include "native-bounded-cpu.h"

// Original EnemyMain with Draygon's real extended map and shot/hurt callbacks.
// A stationary overlapping charged shot isolates the release/expiry boundary;
// this is not a normal firing trajectory or a whole boss encounter.
int DiagnosticXPlasmaDraygon(const char *rom, const char *output) {
  int status = ProbeLoadRetailMovementRom(rom); if (status) return status;
  FILE *f = fopen(output, "wx"); if (!f) return 4;
  fprintf(f, "hyper,entry,health,invincibility,flash,acceleration,x,y\n");
  for (int hyper = 0; hyper < 2; hyper++)
  for (int entry = 0; entry < 2; entry++) {
    cpu_reset(g_snes->cpu); memset(g_ram, 0, sizeof(g_ram));
    g_snes->cpu->e = false; g_snes->cpu->sp = 0x1ff0; g_snes->cpu->dp = 0;
    Enemy_Draygon *body = Get_Draygon(0);
    body->base.enemy_ptr = 0xde3f; body->base.bank = 0xa5;
    body->base.x_pos = body->base.y_pos = 128; body->base.health = 6000;
    body->base.spritemap_pointer = 0xa3bb; body->base.extra_properties = 4;
    body->base.flash_timer = 12; body->base.ai_handler_bits = 2;
    body->base.invincibility_timer = entry;
    first_free_enemy_index = 0x40; enemy_index_to_shake = 0xffff;
    active_enemy_indexes[0] = 0; active_enemy_indexes[1] = 0xffff;
    projectile_counter = 1;
    projectile_x_pos[0] = projectile_y_pos[0] = 128;
    projectile_x_radius[0] = projectile_y_radius[0] = 4;
    projectile_type[0] = hyper ? 0x9018 : 0x8018;
    projectile_damage[0] = hyper ? 1000 : 450;
    ProbeRunBounded(0xa08fd4);
    fprintf(f, "%d,%d,%u,%u,%u,%u,%u,%u\n", hyper, entry, body->base.health,
      body->base.invincibility_timer, body->base.flash_timer, body->draygon_var_0F,
      body->base.x_pos, body->base.y_pos);
  }
  fclose(f); return 0;
}
