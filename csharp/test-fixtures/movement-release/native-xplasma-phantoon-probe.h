#include "native-bounded-cpu.h"

// Original extended collision and boss callback. The optional charged Power
// Beam contact establishes Phantoon's initial non-Plasma reaction window.
int DiagnosticXPlasmaPhantoon(const char *rom, const char *output) {
  int status = ProbeLoadRetailMovementRom(rom); if (status) return status;
  FILE *f = fopen(output, "wx"); if (!f) return 4;
  const uint16 phases[] = {0xd60d, 0xd678};
  const uint16 types[] = {0x8000, 0x8008, 0x8018};
  const uint16 damage[] = {20, 150, 450};
  fprintf(f, "phase,primed,type,stage,health,invincibility,flash,function,timer,properties,accum,started,reaction,direction\n");
  for (int phase = 0; phase < 2; phase++)
  for (int primed = 0; primed < 2; primed++)
  for (int kind = 0; kind < 3; kind++) {
    cpu_reset(g_snes->cpu); memset(g_ram, 0, sizeof(g_ram));
    g_snes->cpu->e = false; g_snes->cpu->sp = 0x1ff0; g_snes->cpu->dp = 0;
    for (int i = 0; i < 4; i++) {
      EnemyData *part = gEnemyData(i * 64);
      part->enemy_ptr = 0xe4bf + i * 64; part->bank = 0xa7;
    }
    Enemy_Phantoon *body = Get_Phantoon(0), *tentacles = Get_Phantoon(128);
    body->base.x_pos = body->base.y_pos = 128; body->base.health = 2500;
    body->base.spritemap_pointer = 0xdee7; body->base.extra_properties = 4;
    body->phant_var_F = phases[phase]; body->phant_var_E = 60;
    projectile_counter = 1;
    projectile_x_pos[0] = projectile_y_pos[0] = 128;
    projectile_x_radius[0] = projectile_y_radius[0] = 4;
    for (int stage = 0; stage < 2; stage++) {
      projectile_type[0] = stage ? types[kind] : 0x8010;
      projectile_damage[0] = stage ? damage[kind] : 60;
      projectile_dir[0] = 0;
      if (stage || primed) { cur_enemy_index = 0; ProbeRunBounded(0xa09b7f); }
      fprintf(f, "%u,%d,%u,%d,%u,%u,%u,%u,%u,%u,%u,%u,%u,%u\n",
        phases[phase], primed, types[kind], stage, body->base.health,
        body->base.invincibility_timer, body->base.flash_timer, body->phant_var_F,
        body->phant_var_E, body->base.properties, tentacles->phant_var_B,
        tentacles->phant_var_A, tentacles->phant_parameter_2, projectile_dir[0]);
    }
  }
  fclose(f); return 0;
}
