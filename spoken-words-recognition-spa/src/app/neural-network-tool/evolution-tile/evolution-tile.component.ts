import { Component, OnInit, Input, Pipe, PipeTransform, OnDestroy } from '@angular/core';
import { Recording } from 'src/app/services/api-client/Recording';
import { EvolutionService } from 'src/app/services/evolution.service';
import { map } from 'rxjs/operators';

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
  public wordSetSize: number;
  public maxCalculationTimeInMinutes?: number;
  public gradientFactor: number;
}

export class PopulationConfig {
  public unitCount: number;
}

export class Aggregation {
  public max: number;
  public min: number;
  public avg: number;
}

export class Output {
  public values: number[];
  public expectedOutputIndex: number;
  public cost: number;
  public iterationCount: number;
}

export class OutputStatistics {
  public words: Map<string, Output>;
}

export class ValuesDistribution {
  public min: number;
  public max: number;
  public buckets: number[];
}

export class LayerStatistics {
  public weight: ValuesDistribution;
  public bias: ValuesDistribution;
  public output: ValuesDistribution;
}

export class EvolutionStatistics {
  public iterationCount: number;
  public totalComputingTimeInSeconds: number;
  public output: OutputStatistics;
  public costHistory: Aggregation[];
  public layers: LayerStatistics[];
}

export class EvolutionConfig {
  public id: string;
  public name: string;
  public isRunning: boolean;
  public trainingConfig: TrainingConfig;
  public networkConfig: NetworkConfig;
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

  statistics: EvolutionStatistics;
  recordings: Recording[];
  totalComputingTime: string;
  tempIterationsPerSecond: number;

  lastUpdateTime: any;
  lastIterationCount: number;
  lastTotalComputingTimeInSeconds: number;

  costChartOptions: any;
  wordsChartOptions: any;
  interval: NodeJS.Timer;

  constructor(private evolutionService: EvolutionService) {}

  ngOnDestroy(): void {
    clearInterval(this.interval);
  }

  ngOnInit() {
    this.update();
    this.interval = setInterval(() => {
      this.update();
    }, 10000);
  }

  update() {
    if(!this.evolution.isRunning) return;

      this.evolutionService.getStatistics(this.evolution.id).subscribe(statistics => {

        console.log(statistics);

        this.statistics = statistics;

        this.updateWordsChart();
        this.updateCostChart();

        this.tempIterationsPerSecond = 
          (statistics.iterationCount - this.lastIterationCount) / 
          (statistics.totalComputingTimeInSeconds - this.lastTotalComputingTimeInSeconds);

        if(performance.now() - this.lastUpdateTime > 60000 || this.lastUpdateTime == null) {
          this.lastIterationCount = statistics.iterationCount;
          this.lastTotalComputingTimeInSeconds = statistics.totalComputingTimeInSeconds;
          this.lastUpdateTime = performance.now();
        }
      })
  }

  updateWordsChart() {
    this.wordsChartOptions = {
      grid: {
        left: '10px',
        right: '30px',
        top: '10px',
        bottom: '20px',
        containLabel: true
      },
      xAxis: {
        type: 'category',
        data: []
      },
      yAxis: {
        type: 'value'
      },
      series: []
    };

    let iterationCount = {
      data: [],
      type: 'bar'
    };

    let keys = Object.keys(this.statistics.output.words);
    for(let i = 0; i < keys.length; i++) {
      this.wordsChartOptions.xAxis.data.push(i);
      iterationCount.data.push(this.statistics.output.words[keys[i]].iterationCount);
    }

    this.wordsChartOptions.series.push(iterationCount);

    console.log(JSON.stringify(this.wordsChartOptions));
  }

  updateCostChart() {
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

    let max = {
      data: [],
      type: 'line',
      smooth: true,
      itemStyle: {
        opacity: 0
      },
      lineStyle: {
        color: 'red',
        type: 'dashed'
      }
    };

    let avg = {
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

    for(let i = 0; i < this.statistics.costHistory.length; i++) {
      let iteration = Math.pow(2, i + 1) - 1;
      avg.data.push([iteration, this.statistics.costHistory[i].avg]);
      max.data.push([iteration, this.statistics.costHistory[i].max]);
    }

    this.costChartOptions.series.push(max);
    this.costChartOptions.series.push(avg);
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
