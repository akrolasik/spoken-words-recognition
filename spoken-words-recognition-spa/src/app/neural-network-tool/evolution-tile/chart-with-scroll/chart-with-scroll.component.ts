import { Component, OnInit, Input } from '@angular/core';

@Component({
  selector: 'app-chart-with-scroll',
  templateUrl: './chart-with-scroll.component.html',
  styleUrls: ['./chart-with-scroll.component.scss']
})
export class ChartWithScrollComponent implements OnInit {

  selected: any;

  @Input() options: any[];
  
  constructor() { }

  ngOnInit() {
    if(this.options != null && this.options.length > 0) {
      this.selected = this.options[0];
    }
  }

  onMouseWheel(evt) {
    if(evt.zrDelta != null) {
      var index = this.options.indexOf(this.selected);
      index = Math.max(Math.min(index - evt.zrDelta, this.options.length - 1), 0);
      this.selected = this.options[index];
    }
    else if(evt.deltaY != null) {
      var delta = evt.deltaY < 0 ? 1 : -1;
      var index = this.options.indexOf(this.selected);
      index = Math.max(Math.min(index - delta, this.options.length - 1), 0);
      this.selected = this.options[index];
    }
  }
}
