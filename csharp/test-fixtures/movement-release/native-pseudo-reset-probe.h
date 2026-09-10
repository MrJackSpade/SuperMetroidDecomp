#include "native-bounded-cpu.h"

// #421: ordinary enemy collision entry, with non-overlapping geometry so no touch
// callback runs. This isolates invulnerability reset from subsequent contact damage.
int DiagnosticPseudoReset(const char *rom, const char *output) {
  int status=ProbeLoadRetailMovementRom(rom); if(status) return status;
  FILE *f=fopen(output,"wx"); if(!f) return 4;
  EnemyDef *def=get_EnemyDef_A2(0xf000); EnemyDef saved=*def;
  fprintf(f,"contact,map,noop,invincibility,result\n");
  const uint16 contacts[]={0,3,4};
  for(int c=0;c<3;c++) for(int map=0;map<2;map++)
  for(int noop=0;noop<2;noop++) for(int inv=0;inv<2;inv++) {
    cpu_reset(g_snes->cpu); memset(g_ram,0,sizeof(g_ram));
    g_snes->cpu->e=false; g_snes->cpu->sp=0x1ff0; g_snes->cpu->dp=0;
    memset(def,0,sizeof(*def)); def->touch_ai=noop?0x804c:0xa477;
    EnemyData *enemy=gEnemyData(0); enemy->enemy_ptr=0xf000; enemy->bank=0xa3;
    enemy->spritemap_pointer=map?0x8000:0;
    enemy->x_pos=192; enemy->y_pos=128; enemy->x_width=enemy->y_height=8;
    samus_x_pos=samus_y_pos=128; samus_x_radius=5; samus_y_radius=12;
    samus_contact_damage_index=contacts[c]; samus_invincibility_timer=inv?9:0;
    ProbeRunBounded(0xa0a07a);
    fprintf(f,"%d,%d,%d,%d,%04X\n",contacts[c],map,noop,inv?9:0,samus_invincibility_timer);
  }
  *def=saved; fclose(f); return 0;
}
