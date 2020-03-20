import { Component } from '@angular/core';

@Component({
  selector: 'app-footer-buttons',
  templateUrl: './footer-buttons.component.html',
  styleUrls: ['./footer-buttons.component.scss']
})
export class FooterButtonsComponent {

  constructor() { }

  blurActive() {
    (document.activeElement as HTMLElement).blur();
  }

  link(link: string) {
    switch (link) {
      case 'github':
        window.open('https://github.com/akrolasik');
        break;
      case 'data-explorer':
        window.open('/data-explorer');
        break;
    }

    this.blurActive();
  }
}
