import { Component, OnInit } from '@angular/core';
import { NbDialogRef } from '@nebular/theme';

@Component({
  selector: 'app-by-country',
  templateUrl: './by-country.component.html',
  styleUrls: ['./by-country.component.scss'],
})
export class ByCountryComponent implements OnInit {

  constructor(protected ref: NbDialogRef<ByCountryComponent>) { }

  ngOnInit() {
    (document.activeElement as HTMLElement).blur();
  }

  selectCountryById(countryName: string) {
    this.ref.close(countryName);
  }

}
