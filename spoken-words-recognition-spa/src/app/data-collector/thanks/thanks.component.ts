import { Component, OnInit, Input } from '@angular/core';
import { NbDialogRef } from '@nebular/theme';
import { Modification } from '../../services/api-client/Modification';
import { THANKS_POPUPS } from '../../utilities/ThanksPopups';
import { ThanksPopup } from '../../utilities/ThanksPopup';

@Component({
  selector: 'app-thanks',
  templateUrl: './thanks.component.html',
  styleUrls: ['./thanks.component.scss'],
})
export class ThanksComponent implements OnInit {

  popups = THANKS_POPUPS;

  @Input() modification: Modification;
  popup: ThanksPopup;

  constructor(protected ref: NbDialogRef<ThanksComponent>) {}

  ngOnInit() {
    (document.activeElement as HTMLElement).blur();
    this.popup = this.popups.find(x => x.modification === this.modification);
  }

  done() {
    this.ref.close();
  }
}
