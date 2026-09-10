#include "native-bounded-cpu.h"

// Full EnemyMain after a real Ice Shield hit. Ordinary no-op hurt/main callbacks
// isolate dispatcher ordering; frozen AI remains the cartridge's common callback.
int DiagnosticIceThaw(const char *rom, const char *output) {
  int status=ProbeLoadRetailMovementRom(rom); if(status) return status;
  FILE *f=fopen(output,"wx"); if(!f) return 4;
  EnemyDef *def=get_EnemyDef_A2(0xf000); EnemyDef saved=*def;
  uint8 *v=(uint8*)RomPtr(0xb4f000), savedv[32]; memcpy(savedv,v,32);
  const int removal[]={-1,0,6,20};
  fprintf(f,"vuln,remove,frame,health,freeze,ai,flash,inv,counter\n");
  for(int b=0;b<2;b++) for(int script=0;script<4;script++) {
    cpu_reset(g_snes->cpu); memset(g_ram,0,sizeof(g_ram));
    g_snes->cpu->e=false; g_snes->cpu->sp=0x1ff0; g_snes->cpu->dp=0;
    equipped_beams=0x1002; samus_power_bombs=2; hud_item_index=3;
    samus_x_pos=samus_y_pos=128; samus_pose_x_dir=8;
    ProbeRunBounded(0x90ccc0);
    for(int i=0;i<4;i++) projectile_x_pos[i]=projectile_y_pos[i]=1024;
    projectile_x_pos[0]=projectile_y_pos[0]=128;
    projectile_index=0; ProbeRunBoundedRegisters(0x9381e9,0,0,0);
    memset(def,0,sizeof(*def)); def->shot_ai=0x802d; def->vulnerability_ptr=0xf000;
    def->main_ai=def->hurt_ai=0x804c; def->frozen_ai=0x8041;
    memset(v,2,32); v[2]=b?0xff:2;
    EnemyData *e=gEnemyData(0); e->enemy_ptr=0xf000; e->bank=0xa3;
    e->spritemap_pointer=0x8000; e->x_pos=e->y_pos=128;
    e->x_width=e->y_height=16; e->health=90;
    ProbeRunBounded(0xa0a143);
    samus_x_pos=samus_y_pos=1024;
    for(int i=0;i<4;i++) projectile_x_pos[i]=projectile_y_pos[i]=1024;
    first_free_enemy_index=64; enemy_index_to_shake=0xffff;
    active_enemy_indexes[0]=0; active_enemy_indexes[1]=0xffff;
    for(int frame=0;frame<420;frame++) {
      if(frame==removal[script]) equipped_beams=0x1000;
      ProbeRunBounded(0xa08fd4);
      fprintf(f,"%d,%d,%d,%04X,%04X,%04X,%04X,%04X,%04X\n",b?255:2,
        removal[script],frame,e->health,e->frozen_timer,e->ai_handler_bits,
        e->flash_timer,e->invincibility_timer,e->frame_counter);
    }
  }
  *def=saved; memcpy(v,savedv,32); fclose(f); return 0;
}
