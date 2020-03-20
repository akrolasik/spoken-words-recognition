import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CollectiongState } from '../../utilities/CollectiongState';
import { Modification } from '../../services/api-client/Modification';

@Component({
  selector: 'app-recording-info',
  templateUrl: './recording-info.component.html',
  styleUrls: ['./recording-info.component.scss']
})
export class RecordingInfoComponent {

  @Input() collectingState: CollectiongState;
  @Output() onStart = new EventEmitter<Modification>();

  constructor() { }
}
