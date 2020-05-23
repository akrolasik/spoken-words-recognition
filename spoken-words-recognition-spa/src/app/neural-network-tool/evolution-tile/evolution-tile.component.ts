import { Component, OnInit, Input, Pipe, PipeTransform, OnDestroy } from '@angular/core';
import { Recording } from 'src/app/services/api-client/Recording';
import { EvolutionService } from 'src/app/services/evolution.service';
import { LocalStorageService } from 'ngx-webstorage';

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
  public word: string;
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
  public verification: OutputStatistics;
  public costHistory: Aggregation[];
  public layers: LayerStatistics[];
}

export enum EvolutionState
{
    Idle = "Idle",
    Loading = "Loading",
    Saving = "Saving",
    Running = "Running"
} 

export class EvolutionConfig {
  public id: string;
  public name: string;
  public state: EvolutionState;
  public networkConfig: NetworkConfig;
  public inputConfig: InputConfig;
}

export class Title {
  text: string;
  left: string = 'center';
}

export class AxisLabel {
  rotate: number = 0;
}

export class Axis {
  type: string;
  data: any[];
  axisLabel: AxisLabel = new AxisLabel();
  max: number;
}

export class Grid {
  left: string = '10px';
  right: string = '30px';
  top: string = '40px';
  bottom: string = '20px';
  containLabel: boolean = true;
}

export class ChartOptions {

  constructor(title: string, yAxisType: string = '', xAxisType: string = '') {
    this.title.text = title;
    this.xAxis.type = xAxisType;
    this.yAxis.type = yAxisType;
  }

  name: string;
  animation: boolean = false;
  title: Title = new Title();
  grid: Grid = new Grid();
  xAxis: Axis = new Axis();
  yAxis: Axis = new Axis();
  series: any[] = [];
}

export class LineStyle {
  color: string = 'red';
  type: string;
  opacity: number = 1;
}

export class ItemStyle {
  opacity: number = 0;
}

export class AreaStyle {
  color: string = '#ccc';
  opacity: number = 0;
}

export class Serie {
  data: any[] = [];
  type: string = 'line';
  smooth: boolean = true;
  itemStyle: ItemStyle = new ItemStyle();
  lineStyle: LineStyle = new LineStyle();
  areaStyle: AreaStyle = new AreaStyle();
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

  costChartOptions: any;
  outputChartOptions: any;
  verificationChartOptions: any;
  interval: NodeJS.Timer;

  expanded: boolean;

  constructor(private storage: LocalStorageService, private evolutionService: EvolutionService) {}

  ngOnDestroy(): void {
    clearInterval(this.interval);
  }

  ngOnInit() {
    this.update();
    this.interval = setInterval(() => {
      this.update();
    }, 1000);
  }

  update() {
    if(!this.expanded) return;

    if(this.evolution.state != EvolutionState.Idle) {
      this.evolutionService.getStatistics(this.evolution.id).subscribe(statistics => {
        if(statistics != null) {
          this.statistics = statistics;
          this.storage.store(`statistics/${this.evolution.id}`, statistics);
        }
      })
    } else {
      this.statistics = this.storage.retrieve(`statistics/${this.evolution.id}`);
    }

    if(this.statistics == null) return;

    this.updateCostChart();
    this.updareOutputChart();
    this.updareVerificationChart();
  }

  updareVerificationChart() {
    this.verificationChartOptions = [];
    
    let outputs = [];

    let keys = Object.keys(this.statistics.output.words);

    keys.forEach(key => {
      let word = this.statistics.output.words[key];
      if(outputs.find(x => x == word.word) == null) {
        outputs.push(word.word);
      }
    });

    keys = Object.keys(this.statistics.verification.words);

    keys.forEach(key => {
      let stats = this.statistics.verification.words[key];
      console.log(stats);
      
      let chartOptions = new ChartOptions(`Output values for word '${stats.word}' [max/average]`, 'value', 'category');
      chartOptions.name = `${stats.word} ${stats.cost.toFixed(2)}`;
      chartOptions.xAxis.data = ['', ...outputs, ''];
      chartOptions.xAxis.axisLabel.rotate = -90;
      chartOptions.yAxis.max = 1;

      let output = new Serie();

      output.data = [0, ...stats.values, 0];

      chartOptions.series.push(output);

      this.verificationChartOptions.push(chartOptions);
    });
  }

  updareOutputChart() {
    this.outputChartOptions = [];

    let outputs = new Map<string, any[]>();

    let keys = Object.keys(this.statistics.output.words);

    keys.forEach(key => {
      let word = this.statistics.output.words[key];
      if(outputs.get(word.word) == null) {
        outputs.set(word.word, []);
      }
      outputs.get(word.word).push(word);
    });

    outputs.forEach((words, key) => {
      let chartOptions = new ChartOptions(`Output values for word '${key}' [max/average]`, 'value', 'category');
      chartOptions.name = key;
      chartOptions.xAxis.data = ['', ...outputs.keys(), ''];
      chartOptions.xAxis.axisLabel.rotate = -90;
      chartOptions.yAxis.max = 1;

      let max = new Serie();
      let avg = new Serie();

      max.areaStyle.opacity = 1;
      max.lineStyle.opacity = 0;

      max.data.push(0);
      avg.data.push(0);

      for(let i = 0; i < words[0].values.length; i++) {
        var values = words.map(x => x.values[i]);
        max.data.push(Math.max(...values));
        avg.data.push(this.average(values));
      }

      max.data.push(0);
      avg.data.push(0);

      chartOptions.series.push(max);
      chartOptions.series.push(avg);

      this.outputChartOptions.push(chartOptions);
    });
  }

  async varify() {
    await this.evolutionService.verifyEvolution(this.evolution.id);
  }

  average(array: number[]) {
    let sum = 0;
    for(let i = 0; i < array.length; i++) {
      sum += array[i];
    }
    return sum / array.length;
  }

  updateCostChart() {
    this.costChartOptions = new ChartOptions('Cost function value over iterations [max/average]', 'log', 'log');

    let max = new Serie();
    let avg = new Serie();

    max.lineStyle.type = 'dashed';

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

  async onStateChange(event: any) {
    if(event.target.checked) {
      await this.evolutionService.startEvolution(this.evolution.id);
    } else {
      await this.evolutionService.stopEvolution();
    }
  }
}
