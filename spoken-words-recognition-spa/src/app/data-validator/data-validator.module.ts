import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DataValidatorComponent } from './data-validator.component';
import { Routes, RouterModule } from '@angular/router';
import { NbIconModule, NbCardModule, NbMenuModule, NbButtonModule, NbLayoutModule, NbPopoverModule } from '@nebular/theme';
import { FormsModule } from '@angular/forms';

export const routes: Routes = [
  { path: '', component: DataValidatorComponent },
];

@NgModule({
  declarations: [DataValidatorComponent],
  imports: [
    CommonModule,
    NbIconModule,
    NbCardModule,
    NbMenuModule,
    NbButtonModule,
    NbLayoutModule,
    NbPopoverModule,
    FormsModule,
    RouterModule.forChild(routes)
  ]
})
export class DataValidatorModule { }
