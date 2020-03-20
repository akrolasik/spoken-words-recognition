import { Component, EventEmitter, Input, OnDestroy, Output } from '@angular/core';

import * as L from 'leaflet';

import { NbThemeService } from '@nebular/theme';
import { combineLatest } from 'rxjs';
import { takeWhile } from 'rxjs/operators';
import { HttpClient } from '@angular/common/http';


@Component({
  selector: 'app-country-orders-map',
  styleUrls: ['./country-orders-map.component.scss'],
  template: `
    <div leaflet [leafletOptions]="options" [leafletLayers]="layers" (leafletMapReady)="mapReady($event)"></div>
  `,
})
export class CountryOrdersMapComponent implements OnDestroy {

  @Input() countryId: string;

  @Output() select: EventEmitter<any> = new EventEmitter();

  layers = [];
  currentTheme: any;
  alive = true;
  selectedCountry;

  options = {
    zoom: 2,
    minZoom: 2,
    maxZoom: 6,
    zoomControl: false,
    center: L.latLng({lat: 38.991709, lng: -76.886109}),
    maxBounds: new L.LatLngBounds(
      new L.LatLng(-89.98155760646617, -180),
      new L.LatLng(89.99346179538875, 180),
    ),
    maxBoundsViscosity: 1.0,
  };

  constructor(private http: HttpClient,
              private theme: NbThemeService) {

    combineLatest([
      this.http.get('assets/leaflet-countries/countries.geo.json'),
      this.theme.getJsTheme(),
    ])
      .pipe(takeWhile(() => this.alive))
      .subscribe(([cords, config]: [any, any]) => {
        console.log(config);

        this.currentTheme = {
          countryBorderWidth: '1',
          countryFillColor: config.variables.bg4,
          countryBorderColor: 'white',
          hoveredCountryBorderWidth: '1',
          hoveredCountryFillColor: config.variables.primaryLight,
          hoveredCountryBorderColor: config.variables.primary,

          chartAxisLineColor: config.variables.border4,
          chartAxisTextColor: config.variables.fg,
          chartAxisFontSize: '16',
          chartGradientTo: config.variables.primary,
          chartGradientFrom: config.variables.primaryLight,
          chartAxisSplitLine: config.variables.separator,
          chartShadowLineColor: config.variables.primaryLight,

          chartLineBottomShadowColor: config.variables.primary,

          chartInnerLineColor: config.variables.bg2,
        };

        this.layers = [this.createGeoJsonLayer(cords)];
      });
  }

  mapReady(map: L.Map) {
    map.addControl(L.control.zoom({position: 'bottomright'}));

    // fix the map fully displaying, existing leaflet bag
    setTimeout(() => {
      map.invalidateSize();
    }, 0);
  }

  private createGeoJsonLayer(cords) {
    return L.geoJSON(
      cords as any,
      {
        style: () => ({
          weight: this.currentTheme.countryBorderWidth,
          fillColor: this.currentTheme.countryFillColor,
          fillOpacity: 1,
          color: this.currentTheme.countryBorderColor,
          opacity: 1,
        }),
        onEachFeature: (f, l) => {
          this.onEachFeature(f, l);
        },
      });
  }

  private onEachFeature(feature, layer) {
    layer.on({
      mouseover: (e) => this.highlightFeature(e.target),
      mouseout: (e) => this.moveout(e.target),
      click: (e) => this.selectFeature(e.target),
    });
  }

  private highlightFeature(featureLayer) {
    console.log(featureLayer);
    if (featureLayer) {
      featureLayer.setStyle({
        weight: this.currentTheme.hoveredCountryBorderWidth,
        fillColor: this.currentTheme.hoveredCountryFillColor,
        color: this.currentTheme.hoveredCountryBorderColor,
      });

      if (!L.Browser.ie && !L.Browser.opera12 && !L.Browser.edge) {
        featureLayer.bringToFront();
      }
    }
  }

  private moveout(featureLayer) {
    if (featureLayer !== this.selectedCountry) {
      this.resetHighlight(featureLayer);

      // When countries have common border we should highlight selected country once again
      this.highlightFeature(this.selectedCountry);
    }
  }

  private resetHighlight(featureLayer) {
    if (featureLayer) {
      const geoJsonLayer = this.layers[0];

      geoJsonLayer.resetStyle(featureLayer);
    }
  }

  private selectFeature(featureLayer) {
    if (featureLayer !== this.selectedCountry && featureLayer != null) {
      this.resetHighlight(this.selectedCountry);
      this.highlightFeature(featureLayer);
      this.selectedCountry = featureLayer;
      this.select.emit(featureLayer.feature.properties.name);
    }
  }

  private findFeatureLayerByCountryId(id) {
    const layers = this.layers[0].getLayers();
    const featureLayer = layers.find(item => {
      return item.feature.id === id;
    });

    return featureLayer ? featureLayer : null;
  }

  ngOnDestroy(): void {
    this.alive = false;
  }

}
