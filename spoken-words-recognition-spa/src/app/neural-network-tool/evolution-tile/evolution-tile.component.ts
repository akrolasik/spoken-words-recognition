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

export class InputConfig {
  public packageFileName: string;
  public wordsIncluded: string[];
  public accentsIncluded: string[];
  public modificationsIncluded: string[];
}

export class TrainingConfig {
  public useGpu: boolean;
  public wordSetSize: number;
  public calculationThreadCount: number;
  public populationSize: number;
  public savingThreadCount: number;
  public iterationLimit?: number;
}

export class GradientConfig {
  public gradientFactor: number;
  public weightGradientFactor: number;
  public biasGradientFactor: number;
  public inputGradientFactor: number;
}

export class PopulationConfig {
  public unitCount: number;
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
  public gradientConfig: GradientConfig;
  public inputConfig: InputConfig;
}

@Pipe({
  name: 'time'
})
export class WorkDayPipe implements PipeTransform {
 
  transform(totalSeconds: number): string {
    
    let seconds = Math.floor(totalSeconds % 60);
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

  statistics: EvolutionStatistics[];
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
          series: []
        };

        for(let s = 0; s < statistics.length; s++) {

          let serie = {
            data: [],
            type: 'line',
            smooth: true,
            itemStyle: {
              opacity: 0
            },
            lineStyle: {
              color: 'red'
            }
          };

          for(let i = 0; i < statistics[s].cost.length; i++) {
            serie.data.push([Math.pow(2, i + 1), statistics[s].cost[i]]);
          }

          this.costChartOptions.series.push(serie);
        }
      })
    }, 1000);
  }

  words() {
    return this.evolution.inputConfig.wordsIncluded.join('\n');
  }

  accents() {
    return this.evolution.inputConfig.accentsIncluded.join('\n');
  }

  modifications() {
    return this.evolution.inputConfig.modificationsIncluded.join('\n');
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
