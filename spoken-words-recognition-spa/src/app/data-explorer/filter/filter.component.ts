import { Component, OnInit, Output, EventEmitter, Input } from '@angular/core';

@Component({
  selector: 'app-filter',
  templateUrl: './filter.component.html',
  styleUrls: ['./filter.component.scss']
})
export class FilterComponent {

  @Input() words: string[];
  @Input() accents: string[];
 
  @Output() onWordSelected = new EventEmitter<string>();
  @Output() onAccentSelected = new EventEmitter<string>();

  constructor() { }

}
