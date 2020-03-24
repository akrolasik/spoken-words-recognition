import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NeuralNetworkToolComponent } from './neural-network-tool.component';
import { Routes, RouterModule } from '@angular/router';
import { NbCardModule, NbLayoutModule, NbButtonModule, NbPopoverModule, NbCheckboxModule, NbIconModule, NbToggleModule } from '@nebular/theme';
import { AddNewEvolutionComponent } from './add-new-evolution/add-new-evolution.component';
import { FormsModule } from '@angular/forms';
import { EvolutionTileComponent } from './evolution-tile/evolution-tile.component';

export const routes: Routes = [
  { 
    path: '', 
    component: NeuralNetworkToolComponent 
  }
];

@NgModule({
  declarations: [
    NeuralNetworkToolComponent, 
    AddNewEvolutionComponent, 
    EvolutionTileComponent,  
  ],
  imports: [
    CommonModule,
    RouterModule.forChild(routes),
    NbCardModule,
    NbLayoutModule,
    NbButtonModule,
    NbPopoverModule,
    NbCheckboxModule,
    FormsModule,
    NbIconModule,
    NbToggleModule,
  ]
})
export class NeuralNetworkToolModule { }
