export class MouseEventsHandler {

  public mouseDownPosition: number;
  public mouseUpPosition: number;

  private onMouseDown: (mouseDownPosition: number, mouseUpPosition: number) => void;
  private isMouseDown: boolean;
  private mousePositionX: number;
  private mousePositionY: number;
  private canvas: HTMLCanvasElement;

  constructor(onMouseDown: (mouseDownPosition: number, mouseUpPosition: number) => void) {
    this.onMouseDown = onMouseDown;
  }

  setMouseEvents(layer: HTMLDivElement, canvas: HTMLCanvasElement) {
    layer.addEventListener('mousemove', (event) => this.getMousePosition(layer, event));
    layer.addEventListener('mousedown', () => this.mouseDown());
    layer.addEventListener('mouseup', () => this.mouseUp());
    this.canvas = canvas;
  }

  getMousePosition(layer, event) {
    this.mousePositionX = event.clientX - layer.getBoundingClientRect().left;
    this.mousePositionY = event.clientY - layer.getBoundingClientRect().top;
    if (this.isMouseDown) {
      this.mouseUpPosition = this.mousePositionX;
      this.onMouseDown(this.mouseDownPosition, this.mouseUpPosition);
    }
  }

  mouseDown() {
    if (this.mousePositionY > this.canvas.height / 2) {
      this.isMouseDown = true;
      this.mouseDownPosition = this.mousePositionX;
      this.mouseUpPosition = this.mousePositionX;
    }
  }

  mouseUp() {
    this.isMouseDown = false;
  }
}
