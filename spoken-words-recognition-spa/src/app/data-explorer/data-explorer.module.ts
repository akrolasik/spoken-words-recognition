import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DataExplorerComponent } from './data-explorer.component';
import { ChartComponent } from './chart/chart.component';
import { AccentsByCountryComponent } from './accents-by-country/accents-by-country.component';
import { Routes, RouterModule } from '@angular/router';
import { NbCardModule, NbSelectModule, NbListModule, NbButtonModule, NbLayoutModule, NbIconModule, NbToggleModule } from '@nebular/theme';
import { AccentsMapComponent } from './accents-by-country/accents-map/accents-map.component';
import { BarChartComponent } from './accents-by-country/bar-chart/bar-chart.component';
import { LeafletModule } from '@asymmetrik/ngx-leaflet';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { NgxEchartsModule } from 'ngx-echarts';
import { FilterComponent } from './filter/filter.component';
import { ItemComponent } from './item/item.component';
import { FrequencyInTimeComponent } from './frequency-in-time/frequency-in-time.component';
import { CommonComponentsModule } from '../common-components/common-components.module';

export const routes: Routes = [
  { path: '', component: DataExplorerComponent },
];

@NgModule({
  declarations: [
    DataExplorerComponent,
    ChartComponent,
    AccentsByCountryComponent,
    AccentsMapComponent,
    BarChartComponent,
    FilterComponent,
    ItemComponent,
    FrequencyInTimeComponent
  ],
  imports: [
    CommonModule,
    RouterModule.forChild(routes),
    NbCardModule,
    NbSelectModule,
    NbLayoutModule,
    NbListModule,
    NbButtonModule,
    NbIconModule,
    LeafletModule,
    FormsModule,
    ReactiveFormsModule,
    NgxEchartsModule,
    CommonComponentsModule,
    NbToggleModule
  ]
})
export class DataExplorerModule { }
