#include "native-bounded-cpu.h"

// Synthetic ordinary shot target; only its definition data is constructed. The
// allocator, collision dispatcher and shot callback execute original ROM CPU code.
int DiagnosticSpazerContact(const char *rom, const char *output) {
  int status=ProbeLoadRetailMovementRom(rom); if(status) return status;
  FILE *f=fopen(output,"wx"); if(!f) return 4;
  EnemyDef *def=get_EnemyDef_A2(0xf000); EnemyDef saved=*def;
  fprintf(f,"age,blocking,overlap,slot,health,flash,inv,type,dir,pre,x,y,damage,next_count,next_types\n");
  const int ages[]={0,32,48,64};
  for(int b=0;b<4;b++) for(int blocking=0;blocking<1;blocking++)
  for(int overlap=0;overlap<2;overlap++) for(int p=0;p<4;p++) {
    cpu_reset(g_snes->cpu); memset(g_ram,0,sizeof(g_ram));
    g_snes->cpu->e=false; g_snes->cpu->sp=0x1ff0; g_snes->cpu->dp=0;
    equipped_beams=0x1004; samus_power_bombs=2; hud_item_index=3;
    samus_x_pos=samus_y_pos=128; samus_pose_x_dir=8;
    ProbeRunBounded(0x90ccc0);
    for(int frame=0;frame<ages[b];frame++) for(int i=3;i>=0;i--) if(projectile_bomb_instruction_ptr[i]) {
      projectile_index=i*2;
      ProbeRunBoundedRegisters(0x900000|projectile_bomb_pre_instructions[i],0,i*2,0);
      if(projectile_bomb_instruction_ptr[i]) ProbeRunBoundedRegisters(0x9381e9,0,i*2,0);
    }
    for(int i=0;i<4;i++) { projectile_x_pos[i]=1024; projectile_y_pos[i]=1024; }
    projectile_x_pos[p]=overlap?128:192; projectile_y_pos[p]=128;
    // Animation installs the native first-record radii before enemy contact.
    projectile_index=p*2; if(projectile_bomb_instruction_ptr[p]) ProbeRunBoundedRegisters(0x9381e9,0,p*2,0);
    memset(def,0,sizeof(*def)); def->shot_ai=0xa63d;
    EnemyData *e=gEnemyData(0); e->enemy_ptr=0xf000; e->bank=0xa0;
    e->spritemap_pointer=0x8000; e->x_pos=e->y_pos=128;
    e->x_width=e->y_height=16; e->health=10000; e->properties=blocking?0x1000:0;
    ProbeRunBounded(0xa0a143);
    fprintf(f,"%d,%d,%d,%d,%04X,%04X,%04X,%04X,%04X,%04X,%04X,%04X,%04X,",
      ages[b],blocking,overlap,p,e->health,e->flash_timer,e->invincibility_timer,
      projectile_type[p],projectile_dir[p],projectile_bomb_pre_instructions[p],
      projectile_x_pos[p],projectile_y_pos[p],projectile_damage[p]);
    projectile_index=p*2;
    if(projectile_bomb_instruction_ptr[p]) ProbeRunBoundedRegisters(0x900000|projectile_bomb_pre_instructions[p],0,p*2,0);
    fprintf(f,"%04X,%04X%04X%04X%04X\n",projectile_counter,
      projectile_type[0],projectile_type[1],projectile_type[2],projectile_type[3]);
  }
  *def=saved; fclose(f); return 0;
}
