#include "native-bounded-cpu.h"

int DiagnosticPlasmaRepeat(const char *rom, const char *output) {
  int status=ProbeLoadRetailMovementRom(rom); if(status) return status;
  FILE *f=fopen(output,"wx"); if(!f) return 4;
  EnemyDef *def=get_EnemyDef_A2(0xf000); EnemyDef saved=*def;
  fprintf(f,"blocking,slot,pass,health0,inv0,health1,inv1,count,types\n");
  for(int blocking=0;blocking<2;blocking++) for(int p=0;p<4;p++) {
    cpu_reset(g_snes->cpu); memset(g_ram,0,sizeof(g_ram));
    g_snes->cpu->e=false; g_snes->cpu->sp=0x1ff0; g_snes->cpu->dp=0;
    equipped_beams=0x1008; samus_power_bombs=2; hud_item_index=3;
    samus_x_pos=samus_y_pos=128; samus_pose_x_dir=8;
    ProbeRunBounded(0x90ccc0);
    for(int i=0;i<4;i++) projectile_x_pos[i]=projectile_y_pos[i]=1024;
    projectile_x_pos[p]=projectile_y_pos[p]=128;
    projectile_index=p*2; ProbeRunBoundedRegisters(0x9381e9,0,p*2,0);
    memset(def,0,sizeof(*def)); def->shot_ai=0xa63d;
    for(int i=0;i<2;i++) {
      EnemyData *e=gEnemyData(i*64); e->enemy_ptr=0xf000; e->bank=0xa0;
      e->spritemap_pointer=0x8000; e->x_pos=e->y_pos=128;
      e->x_width=e->y_height=16; e->health=10000;
      e->properties=blocking && i==0?0x1000:0;
    }
    for(int pass=0;pass<4;pass++) {
      // Boundary controls, not simulated timer progression: retain the initial
      // hit's timer, test one remaining tick, then make the targets eligible again.
      if(pass>=2) for(int i=0;i<2;i++) gEnemyData(i*64)->invincibility_timer=pass==2?1:0;
      for(int i=0;i<2;i++) { cur_enemy_index=i*64; ProbeRunBounded(0xa0a143); }
      if(projectile_type[p] && (projectile_dir[p]&0xf0)) {
        projectile_index=p*2; ProbeRunBoundedRegisters(0x90d793,0,p*2,0);
      }
      fprintf(f,"%d,%d,%d,%04X,%04X,%04X,%04X,%04X,%04X%04X%04X%04X\n",
        blocking,p,pass,gEnemyData(0)->health,gEnemyData(0)->invincibility_timer,
        gEnemyData(64)->health,gEnemyData(64)->invincibility_timer,projectile_counter,
        projectile_type[0],projectile_type[1],projectile_type[2],projectile_type[3]);
    }
  }
  *def=saved; fclose(f); return 0;
}
