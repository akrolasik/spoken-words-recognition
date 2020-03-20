import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DataExplorerComponent } from './data-explorer.component';
import { ChartComponent } from './chart/chart.component';
import { AccentsByCountryComponent } from './accents-by-country/accents-by-country.component';
import { Routes, RouterModule } from '@angular/router';
import { NbCardModule, NbSelectModule, NbListModule, NbButtonModule, NbLayoutModule, NbIconModule } from '@nebular/theme';
import { FilterComponent } from './filter/filter.component';
import { ItemComponent } from './item/item.component';
import { PckageComponent } from './package/package.component';
import { AccentsMapComponent } from './accents-by-country/accents-map/accents-map.component';
import { BarChartComponent } from './accents-by-country/bar-chart/bar-chart.component';
import { LeafletModule } from '@asymmetrik/ngx-leaflet';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { NgxEchartsModule } from 'ngx-echarts';

export const routes: Routes = [
  { path: '', component: DataExplorerComponent },
];

@NgModule({
  declarations: [
    DataExplorerComponent,
    ChartComponent,
    AccentsByCountryComponent,
    FilterComponent,
    ItemComponent,
    PckageComponent,
    AccentsMapComponent,
    BarChartComponent
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
    NgxEchartsModule
  ]
})
export class DataExplorerModule { }
