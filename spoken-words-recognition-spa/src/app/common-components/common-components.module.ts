import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { PckageComponent } from './package/package.component';
import { NbButtonModule } from '@nebular/theme';

@NgModule({
  declarations: [
    PckageComponent
  ],
  imports: [
    CommonModule,
    NbButtonModule
  ],
  exports: [
    PckageComponent
  ]
})
export class CommonComponentsModule { }
