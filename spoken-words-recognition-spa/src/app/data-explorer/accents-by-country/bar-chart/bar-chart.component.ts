import { AfterViewInit, Component, Input, OnChanges, OnDestroy, SimpleChanges } from '@angular/core';
import { NbThemeService } from '@nebular/theme';
import { takeWhile } from 'rxjs/operators';
import { AccentCount } from '../accents-by-country.component';

@Component({
  selector: 'app-bar-chart',
  templateUrl: './bar-chart.component.html',
  styleUrls: ['./bar-chart.component.scss']
})
export class BarChartComponent implements AfterViewInit, OnChanges {

  @Input() countryName: string;
  @Input() accents: AccentCount[];

  option: any = {};
  echartsInstance;

  constructor(private theme: NbThemeService) {}

  ngOnChanges(changes: SimpleChanges): void {
    if (changes.accents && !changes.accents.isFirstChange()) {
      this.echartsInstance.setOption({
        yAxis: {
          data: this.accents.map(x => x.accentName)
        },
        series: [
          {
            data: this.accents.map(v => v.recordingsCount),
          },
        ],
      });
    }
  }

  ngAfterViewInit() {
    this.theme.getJsTheme()
      .subscribe(config => {
        this.option = {
          grid: {
            left: '3%',
            right: '5%',
            bottom: '3%',
            top: '3%',
            containLabel: true,
          },
          xAxis: {
            axisLabel: {
              color: config.variables.fg,
              fontSize: '16',
            },
            axisLine: {
              lineStyle: {
                color: config.variables.border4,
                width: '2',
              },
            },
            axisTick: {
              show: false,
            },
            splitLine: {
              lineStyle: {
                color: config.variables.separator,
                width: '1',
              },
            },
          },
          yAxis: {
            data: [],
            axisLabel: {
              color: config.variables.fg,
              fontSize: '16',
            },
            axisLine: {
              lineStyle: {
                color: config.variables.border4,
                width: '2',
              },
            },
            axisTick: {
              show: false,
            },
          },
          series: [
            {
              type: 'bar',
              barWidth: '30%',
              data: [],
              cursor: 'default',
              itemStyle: {
                normal: {
                  color: config.variables.primary,
                },
              }
            },
          ],
        };
      });
  }

  onChartInit(echartsInstance) {
    this.echartsInstance = echartsInstance;
  }

  resizeChart() {
    if (this.echartsInstance) {
      this.echartsInstance.resize();
    }
  }
}
