#include "native-bounded-cpu.h"

// #421: original generic touch handler with a disposable synthetic enemy header.
int DiagnosticPseudoTouch(const char *rom, const char *output) {
  int status=ProbeLoadRetailMovementRom(rom); if(status) return status;
  FILE *f=fopen(output,"wx"); if(!f) return 4;
  EnemyDef *def=get_EnemyDef_A2(0xf000); EnemyDef saved=*def;
  uint8 *vulnerability=(uint8*)RomFixedPtr(0xb4f000);
  uint8 saved_vulnerability[32]; memcpy(saved_vulnerability,vulnerability,32);
  memset(def,0,sizeof(*def)); def->vulnerability_ptr=0xf000; def->hurt_ai_time=4;
  fprintf(f,"contact,vulnerability,health,flash,ai,flare,chargePalette,flareFrame,flareTimer,invincibility,knockback\n");
  for(int contact=3;contact<=4;contact++) for(int v=0;v<4;v++) {
    cpu_reset(g_snes->cpu); memset(g_ram,0,sizeof(g_ram));
    g_snes->cpu->e=false; g_snes->cpu->sp=0x1ff0; g_snes->cpu->dp=0;
    const uint8 powers[]={0,1,2,0x82}; memset(vulnerability,powers[v],32);
    EnemyData *enemy=gEnemyData(0); enemy->enemy_ptr=0xf000; enemy->health=5000;
    samus_contact_damage_index=contact; samus_invincibility_timer=9; samus_knockback_timer=5;
    flare_counter=120; samus_charge_palette_index=6; flare_animation_frame=3; flare_animation_timer=7;
    ProbeRunBounded(0xa0a4a1);
    fprintf(f,"%d,%02X,%04X,%04X,%04X,%04X,%04X,%04X,%04X,%04X,%04X\n",contact,powers[v],
      enemy->health,enemy->flash_timer,enemy->ai_handler_bits,flare_counter,samus_charge_palette_index,
      flare_animation_frame,flare_animation_timer,samus_invincibility_timer,samus_knockback_timer);
  }
  *def=saved; memcpy(vulnerability,saved_vulnerability,32); fclose(f); return 0;
}
