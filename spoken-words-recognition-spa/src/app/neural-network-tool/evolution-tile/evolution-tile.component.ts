import { Component, OnInit, Input, Pipe, PipeTransform, OnDestroy } from '@angular/core';
import { Recording } from 'src/app/services/api-client/Recording';
import { EvolutionService } from 'src/app/services/evolution.service';

export class InputResolution {
  public width: number;
  public height: number;
}

export class NetworkConfig {
  public inputResolution: InputResolution;
  public inputCount?(): number {
    return this.inputResolution.width * this.inputResolution.height;
  };
  public hiddenLayersNeuronCount: number[];
  public outputCount: number;
}

export class TrainingConfig {
  public wordSetSize: number;
  public packageFileName: string;
  public iterationLimit?: number;
  public wordsIncluded: string[];
  public accentsIncluded: string[];
  public modificationsIncluded: string[];
}

export class SelectionConfig {
  public gradientFactor: number;
  public weightGradientFactor: number;
  public biasGradientFactor: number;
  public inputGradientFactor: number;
}

export class Aggregation {
  public max: number;
  public min: number;
  public avg: number;
}

export class EvolutionStatistics {
  public currentIteration: number;
  public totalComputingTimeInSeconds: number;
  public output: Aggregation[];
  public cost: number[];
}

export class EvolutionConfig {
  public id: string;
  public name: string;
  public isRunning: boolean;
  public trainingConfig: TrainingConfig;
  public networkConfig: NetworkConfig;
  public gradientConfig: SelectionConfig;
}

@Pipe({
  name: 'time'
})
export class WorkDayPipe implements PipeTransform {
 
  transform(totalSeconds: number): string {
    
    let seconds = totalSeconds % 60;
    let minutes = Math.floor(totalSeconds / 60);
    let hours = Math.floor(totalSeconds / 3600);
    
    let text = `${seconds}s`;

    if(minutes > 0) {
      text = `${minutes}m${text}`
    }

    if(hours > 0) {
      text = `${hours}h${text}`
    }

    return text;
  }
}

@Component({
  selector: 'app-evolution-tile',
  templateUrl: './evolution-tile.component.html',
  styleUrls: ['./evolution-tile.component.scss']
})
export class EvolutionTileComponent implements OnInit, OnDestroy {

  @Input() evolution: EvolutionConfig;

  statistics: EvolutionStatistics;
  recordings: Recording[];
  totalComputingTime: string;

  costChartOptions: any;
  interval: NodeJS.Timer;

  constructor(private evolutionService: EvolutionService) {}

  ngOnDestroy(): void {
    clearInterval(this.interval);
  }

  ngOnInit() {

    this.interval = setInterval(() => {

      if(!this.evolution.isRunning) return;

      this.evolutionService.getStatistics(this.evolution.id).subscribe(statistics => {
        this.statistics = statistics;

        this.costChartOptions = {
          grid: {
            left: '10px',
            right: '30px',
            top: '10px',
            bottom: '20px',
            containLabel: true
          },
          xAxis: {
              type: 'log'
          },
          yAxis: {
              type: 'log'
          },
          series: [{
              data: [],
              type: 'line',
              smooth: true,
              itemStyle: {
                opacity: 0
              },
              lineStyle: {
                color: 'red'
              }
          }]
        };

        for(let i = 0; i < statistics.cost.length; i++) {
          this.costChartOptions.series[0].data.push([Math.pow(2, i), statistics.cost[i]]);
        }

      })
    }, 1000);
  }

  words() {
    return this.evolution.trainingConfig.wordsIncluded.join('\n');
  }

  accents() {
    return this.evolution.trainingConfig.accentsIncluded.join('\n');
  }

  modifications() {
    return this.evolution.trainingConfig.modificationsIncluded.join('\n');
  }

  async delete() {
    document.getElementById('background').click();
    await this.evolutionService.deleteEvolution(this.evolution.id);
  }

  async onStateChange(enabled: boolean) {
    if(enabled) {
      await this.evolutionService.startEvolution(this.evolution.id);
    } else {
      await this.evolutionService.stopEvolution();
    }
  }
}
