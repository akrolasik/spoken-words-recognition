import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-chart-with-scroll',
  templateUrl: './chart-with-scroll.component.html',
  styleUrls: ['./chart-with-scroll.component.scss']
})
export class ChartWithScrollComponent {

  selectedIndex: any = 0;

  @Input() options: any[];
  
  constructor() { }

  selected() {
    if(this.options == null) return null; 
    return this.options[this.selectedIndex];
  }

  onMouseWheel(evt) {
    if(evt.zrDelta != null) {
      this.selectedIndex = Math.max(Math.min(this.selectedIndex - evt.zrDelta, this.options.length - 1), 0);
    }
    else if(evt.deltaY != null) {
      var delta = evt.deltaY < 0 ? 1 : -1;
      this.selectedIndex = Math.max(Math.min(this.selectedIndex - delta, this.options.length - 1), 0);
    }
  }
}
