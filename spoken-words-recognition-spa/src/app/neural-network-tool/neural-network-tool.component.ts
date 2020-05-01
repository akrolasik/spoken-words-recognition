import { Component, OnInit, OnDestroy } from '@angular/core';
import { EvolutionConfig } from './evolution-tile/evolution-tile.component';
import { EvolutionService } from '../services/evolution.service';
import { Matrix } from '../utilities/Matrix';

@Component({
  selector: 'app-neural-network-tool',
  templateUrl: './neural-network-tool.component.html',
  styleUrls: ['./neural-network-tool.component.scss']
})
export class NeuralNetworkToolComponent implements OnInit, OnDestroy {

  evolutions: EvolutionConfig[] = [];
  interval: NodeJS.Timer;

  constructor(private evolutionService: EvolutionService) {}

  ngOnDestroy(): void {
    clearInterval(this.interval);
  }

  ngOnInit() {
    this.update();
    this.interval = setInterval(() => {
      this.update();
    }, 1000)
  }

  update() {
    this.evolutionService.getEvolutions().subscribe(evolutions => {
      this.evolutions = evolutions;
    }, error => {});
  }

  trackById(index, item) {
    return item.id;
  }
}
