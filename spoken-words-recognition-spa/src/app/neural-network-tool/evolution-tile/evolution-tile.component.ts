import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { Evolution } from "../../utilities/Evolution";

@Component({
  selector: 'app-evolution-tile',
  templateUrl: './evolution-tile.component.html',
  styleUrls: ['./evolution-tile.component.scss']
})
export class EvolutionTileComponent implements OnInit {

  @Input() evolution: Evolution;
  @Output() deleted: EventEmitter<any> = new EventEmitter();
  @Output() isProcessing: EventEmitter<boolean> = new EventEmitter();

  totalComputingTime: string;

  constructor() { }

  ngOnInit() {
    this.updateTotalTimeString();
    setInterval(() => {
      if(this.evolution.isProcessing) {
        this.evolution.totalComputingTimeInSeconds++;
        this.updateTotalTimeString();
      }
    }, 1000);
  }

  updateTotalTimeString() {
    this.totalComputingTime = new Date(this.evolution.totalComputingTimeInSeconds * 1000).toISOString().substr(11, 8);
  }

  delete() {
    this.deleted.emit();
  }

  onIsProcessingChange(event) {
    this.evolution.isProcessing = event.target.checked;
    this.isProcessing.emit(this.evolution.isProcessing);
  }
}
