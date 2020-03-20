import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { CollectiongState } from '../../utilities/CollectiongState';

@Component({
  selector: 'app-recording-buttons',
  templateUrl: './recording-buttons.component.html',
  styleUrls: ['./recording-buttons.component.scss']
})
export class RecordingButtonsComponent {

  @Input() collectingState: CollectiongState;
  @Output() onClick = new EventEmitter<string>();

  constructor() { }
}
