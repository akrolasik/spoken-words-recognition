import { Component } from '@angular/core';

@Component({
  selector: 'app-assets-loader',
  templateUrl: './assets-loader.component.html',
  styleUrls: ['./assets-loader.component.scss']
})
export class AssetsLoaderComponent {

  images: string[] = [
    "slower.gif",
    "louder.webp",
    "quieter.jpg",
    "noise.jpg",
    "faster.jpg",
  ];

  constructor() { }

}
