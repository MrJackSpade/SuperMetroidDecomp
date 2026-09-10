#include "native-bounded-cpu.h"

// Original collision + normal shot callback, with authored target vulnerabilities
// patched only in memory. No gameplay cheats, custom shot logic or host emulation.
int DiagnosticIceContact(const char *rom, const char *output) {
  int status=ProbeLoadRetailMovementRom(rom); if(status) return status;
  FILE *f=fopen(output,"wx"); if(!f) return 4;
  EnemyDef *def=get_EnemyDef_A2(0xf000); EnemyDef saved=*def;
  uint8 *v=(uint8*)RomPtr(0xb4f000), savedv[32]; memcpy(savedv,v,32);
  const int healths[]={45,90,91,180}, vulns[]={2,0x82,0xff};
  fprintf(f,"health,vuln,ice,area,frozen,alive,next_health,freeze,ai,inv,damage,dir,sfx3,next_count\n");
  for(int h=0;h<4;h++) for(int b=0;b<3;b++) for(int ice=0;ice<2;ice++)
  for(int area=0;area<=2;area+=2) for(int frozen=0;frozen<2;frozen++) {
    cpu_reset(g_snes->cpu); memset(g_ram,0,sizeof(g_ram));
    g_snes->cpu->e=false; g_snes->cpu->sp=0x1ff0; g_snes->cpu->dp=0;
    equipped_beams=0x1002; samus_power_bombs=2; hud_item_index=3;
    samus_x_pos=samus_y_pos=128; samus_pose_x_dir=8;
    ProbeRunBounded(0x90ccc0);
    equipped_beams=ice?0x1002:0x1000; area_index=area;
    for(int i=0;i<4;i++) projectile_x_pos[i]=projectile_y_pos[i]=1024;
    projectile_x_pos[0]=projectile_y_pos[0]=128;
    projectile_index=0; ProbeRunBoundedRegisters(0x9381e9,0,0,0);
    memset(def,0,sizeof(*def)); def->shot_ai=0xa63d; def->vulnerability_ptr=0xf000;
    memset(v,2,32); v[2]=vulns[b];
    EnemyData *e=gEnemyData(0); e->enemy_ptr=0xf000; e->bank=0xa0;
    e->spritemap_pointer=0x8000; e->x_pos=e->y_pos=128;
    e->x_width=e->y_height=16; e->health=healths[h]; e->frozen_timer=frozen?20:0;
    ProbeRunBounded(0xa0a143);
    int alive=e->enemy_ptr!=0;
    fprintf(f,"%d,%d,%d,%d,%d,%d,%04X,%04X,%04X,%04X,%04X,%04X,%02X,",
      healths[h],vulns[b],ice,area,frozen,alive,alive?e->health:0,
      alive?e->frozen_timer:0,alive?e->ai_handler_bits:0,alive?e->invincibility_timer:0,
      projectile_damage[0],projectile_dir[0],sfx3_queue[0]);
    projectile_index=0; ProbeRunBoundedRegisters(0x90cf09,0,0,0);
    fprintf(f,"%04X\n",projectile_counter);
  }
  *def=saved; memcpy(v,savedv,32); fclose(f); return 0;
}
