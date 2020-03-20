export class SoundAnimation {

    private canvas: HTMLCanvasElement;
    private canvasContext: CanvasRenderingContext2D;
    private minimalBarSize = 3;
    private interval: NodeJS.Timer;

    constructor(canvas?: HTMLCanvasElement) {
        this.canvas = canvas;
        this.canvasContext = this.canvas.getContext('2d');
    }

    start() {
        if (this.interval == null) {
            this.interval = setInterval(() => this.displayAnimation(), 0);
        }
    }

    stop() {
        clearInterval(this.interval);
        this.interval = null;
    }

    private displayAnimation(): void {

        window.requestAnimationFrame(() => {

            this.canvasContext.fillStyle = '#ffffff';
            this.canvasContext.fillRect(0, 0, this.canvas.width, this.canvas.height);

            const animationTime = 1500;
            const barsCount = 80;
            const barDistance = this.canvas.width / barsCount;
            const barWidth = barDistance - this.minimalBarSize;
            const height = this.canvas.height * 3 / 4;

            this.canvasContext.fillStyle = '#2ce69b';
            let time = performance.now() % (animationTime * 2);

            if (time > animationTime) {
            time = animationTime * 2 - time;
            }

            time /= animationTime;

            for (let b = 0; b < barsCount; b++) {

            const distance1 = Math.max(barsCount - Math.pow(Math.abs(b - time * barsCount), 2), 0);
            const distance2 = this.canvas.height / 4 * barsCount / (barsCount - b) / (b + 1);
            let h = (distance1 + distance2) / 2;

            if (distance1 === 0) { h = this.minimalBarSize; }

            const x = b * barDistance;
            const y = height - h / 2;

            this.canvasContext.fillRect(x, y, barWidth, h);
            }
        });
    }
}

