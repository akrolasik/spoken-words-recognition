import { Component, OnInit, ViewChild, Output, EventEmitter } from '@angular/core';
import { NbPopoverDirective, NbDialogService } from '@nebular/theme';
import { Observable, merge, BehaviorSubject } from 'rxjs';
import { FormControl } from '@angular/forms';
import { startWith, map } from 'rxjs/operators';
import { AnalyticsService } from '../../services/analytics.service';
import { ACCENTS, COUNTRY_ACCENTS_MAP } from '../../utilities/Accents';
import { ByCountryComponent } from '../by-country/by-country.component';

@Component({
  selector: 'app-upload',
  templateUrl: './upload.component.html',
  styleUrls: ['./upload.component.scss'],
})
export class UploadComponent implements OnInit {

  @ViewChild(NbPopoverDirective, { static: false }) popover;
  @Output() eventEmitter = new EventEmitter<boolean>();
  accentControl: FormControl = new FormControl();
  accents = ACCENTS;

  isAutocompleteEnabled: boolean;
  inputClicked: boolean;

  filteredAccents: Observable<string[]>;
  click: BehaviorSubject<boolean>;

  constructor(private analytics: AnalyticsService, private dialogService: NbDialogService) {
    const accent = this.analytics.getValue<string>('accent');
    this.accentControl.setValue(accent);
  }

  onInputClick() {
    this.click.next(true);
  }

  ngOnInit() {

    this.blurActiveElement();

    this.click = new BehaviorSubject(this.inputClicked);

    this.accentControl.valueChanges.subscribe(value => {
      if (value.length > 0) {
        this.popover.hide();
      } else {
        this.popover.show();
      }
    });

    const anythingChanges = merge(
      this.accentControl.valueChanges,
      this.click.asObservable(),
    );

    this.filteredAccents = anythingChanges
      .pipe(
        startWith(''),
        map(val => {
          if (this.inputClicked) {
            this.inputClicked = false;
            return [];
          } else
            return val != null && val.length >= 1 ? this.filter(val) : [];
        }),
      );

    this.isAutocompleteEnabled = true;
  }

  filter(val: string): string[] {
    return this.accents.filter(option =>
      option.toLowerCase().indexOf(val.toLowerCase()) === 0);
  }

  chooseByCountry() {
    this.dialogService.open(ByCountryComponent).onClose.subscribe(country => {
      const input = document.getElementById('accentInput') as HTMLInputElement;
      if(COUNTRY_ACCENTS_MAP.has(country)) {
        input.value = COUNTRY_ACCENTS_MAP.get(country);
      } else {
        input.value = country;
      }
      this.blurActiveElement();
    });
  }

  cancel() {
    this.eventEmitter.emit(false);
  }

  tryUpload() {
    const input = document.getElementById('accentInput') as HTMLInputElement;

    if (input.value != null && input.value.length > 0) {
      this.upload(input.value);
    } else {
      this.animate(input);
    }

    this.blurActiveElement();
  }

  upload(accent: string) {
    this.analytics.setValue('accent', accent);
    this.eventEmitter.emit(true);
  }

  animate(input: HTMLInputElement) {
    input.animate([
      { borderColor: '#edf1f7', backgroundColor: '#f7f9fc' },
      { borderColor: '#ff000088', backgroundColor: '#ff000022' },
      { borderColor: '#edf1f7', backgroundColor: '#f7f9fc' },
    ], {
      duration: 800,
      iterations: 1,
    });
  }

  blurActiveElement() {
    (document.activeElement as HTMLElement).blur();
  }
}
