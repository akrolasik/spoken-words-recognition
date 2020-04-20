import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NeuralNetworkToolComponent } from './neural-network-tool.component';
import { Routes, RouterModule } from '@angular/router';
import { NbCardModule, NbLayoutModule, NbButtonModule, NbPopoverModule, NbCheckboxModule, NbIconModule, NbToggleModule, NbTooltipModule, NbTabsetModule } from '@nebular/theme';
import { AddNewEvolutionComponent } from './add-new-evolution/add-new-evolution.component';
import { FormsModule } from '@angular/forms';
import { EvolutionTileComponent, WorkDayPipe } from './evolution-tile/evolution-tile.component';
import { NgxEchartsModule } from 'ngx-echarts';
import { CommonComponentsModule } from '../common-components/common-components.module';
import { ChartWithScrollComponent } from './evolution-tile/chart-with-scroll/chart-with-scroll.component';

export const routes: Routes = [{ 
    path: '', 
    component: NeuralNetworkToolComponent 
  }
];

@NgModule({
  declarations: [
    NeuralNetworkToolComponent, 
    AddNewEvolutionComponent, 
    EvolutionTileComponent, 
    ChartWithScrollComponent,  
    WorkDayPipe
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
    NgxEchartsModule,
    CommonComponentsModule,
    NbTooltipModule,
    NbTabsetModule
  ]
})
export class NeuralNetworkToolModule { }
