import { Component, OnInit, Input, AfterViewInit, EventEmitter, Output } from '@angular/core';

@Component({
  selector: 'app-issue',
  templateUrl: './issue.component.html',
  styleUrls: ['./issue.component.scss'],
})
export class IssueComponent implements OnInit, AfterViewInit {

  constructor() { }

  @Input() issue: any;

  @Output() eventEmitter = new EventEmitter<string>();
  ngAfterViewInit(): void {
    const canvas = document.getElementById(this.issue.issueId) as HTMLCanvasElement;
    const canvasContext = canvas.getContext('2d');

    const width = this.issue.imageData.width;
    const height = this.issue.imageData.height;

    const imageData: ImageData = new ImageData(width, height);

    const text = (this.issue.imageData.data as string);
    const array = [];

    for (let i = 0; i < text.length; i++) {
      array.push(text.charCodeAt(i));
    }

    imageData.data.set(array);

    canvasContext.putImageData(imageData, 0, 0);
  }

  decline() {
    this.eventEmitter.emit(this.issue.issueId);
  }

  ngOnInit() {

  }

}
