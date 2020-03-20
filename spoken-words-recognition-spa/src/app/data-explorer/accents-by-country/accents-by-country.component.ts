import { Component, Input, OnInit, OnChanges, SimpleChanges } from '@angular/core';
import { Recording } from 'src/app/services/api-client/Recording';

export class AccentCount {
  public accentName: string;
  public recordingsCount: number;
}

@Component({
  selector: 'app-accents-by-country',
  templateUrl: './accents-by-country.component.html',
  styleUrls: ['./accents-by-country.component.scss']
})
export class AccentsByCountryComponent implements OnChanges {

  @Input() recordings: Recording[];

  countryName: string;
  accents: AccentCount[];

  constructor() {}

  ngOnChanges(changes: SimpleChanges): void {
    this.update();
  }

  selectCountryById(countryName: string) {
    this.countryName = countryName;
    this.update();
  }

  update() {
    if(this.recordings == null) return;

    let accents: AccentCount[] = [];

    let filteredRecordings = this.recordings;

    if(this.countryName != "World") {
      filteredRecordings = this.recordings.filter(x => 
        x.localization == this.countryName ||
        x.localization.indexOf(this.countryName) != -1 ||
        this.countryName.indexOf(x.localization) != -1);
    }

    filteredRecordings.forEach(recording => {
      let accent = accents.find(x => x.accentName == recording.accent);
      if(accent == null) {
        accents.push({
          accentName: recording.accent,
          recordingsCount: 1
        });
      } else {
        accent.recordingsCount++;
      }
    });

    let visibleCount = 9;
    let sorted = accents.sort(x => -x.recordingsCount);
    let visible = sorted.slice(0, Math.min(visibleCount, accents.length))

    if(visible.length < sorted.length) {

      let other = {
        accentName: "Other",
        recordingsCount: 0
      };

      for(let i = visibleCount; i < sorted.length; i++) {
        other.recordingsCount += sorted[i].recordingsCount;
      }

      visible.push(other);
    }

    this.accents = visible.reverse();
  }

}
