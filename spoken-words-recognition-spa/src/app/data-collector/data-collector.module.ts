import { NgModule } from '@angular/core';
import { DataCollectorComponent } from './data-collector.component';
import { TreeComponent } from './tree/tree.component';
import { UploadComponent } from './upload/upload.component';
import { ThanksComponent } from './thanks/thanks.component';
import { CommonModule } from '@angular/common';
import { NbIconModule,
  NbCardModule,
  NbMenuModule,
  NbButtonModule,
  NbLayoutModule,
  NbPopoverModule } from '@nebular/theme';
import { MatTreeModule } from '@angular/material/tree';
import { MatIconModule } from '@angular/material/icon';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { CountryOrdersMapComponent } from './map/country-orders-map.component';
import { LeafletModule } from '@asymmetrik/ngx-leaflet';
import { ByCountryComponent } from './by-country/by-country.component';
import { AssetsLoaderComponent } from './assets-loader/assets-loader.component';
import { IssuePopoverComponent } from './issue-popover/issue-popover.component';
import { WelcomeInfoComponent } from './welcome-info/welcome-info.component';
import { FooterButtonsComponent } from './footer-buttons/footer-buttons.component';
import { RecordingButtonsComponent } from './recording-buttons/recording-buttons.component';
import { RecordingInfoComponent } from './recording-info/recording-info.component';
import { Routes, RouterModule } from '@angular/router';

export const routes: Routes = [
  { path: '', component: DataCollectorComponent },
];

@NgModule({
  declarations: [
    DataCollectorComponent,
    TreeComponent,
    UploadComponent,
    ThanksComponent,
    CountryOrdersMapComponent,
    ByCountryComponent,
    AssetsLoaderComponent,
    IssuePopoverComponent,
    WelcomeInfoComponent,
    FooterButtonsComponent,
    RecordingButtonsComponent,
    RecordingInfoComponent,
  ],
  imports: [
    CommonModule,
    NbIconModule,
    NbCardModule,
    LeafletModule,
    NbMenuModule,
    NbButtonModule,
    NbLayoutModule,
    MatTreeModule,
    MatIconModule,
    NbPopoverModule,
    FormsModule,
    MatAutocompleteModule,
    ReactiveFormsModule,
    RouterModule.forChild(routes)
  ],
  providers: [],
})
export class DataCollectorModule { }
