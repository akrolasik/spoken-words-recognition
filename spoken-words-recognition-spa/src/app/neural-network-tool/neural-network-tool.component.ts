import { Component, OnInit, OnDestroy } from '@angular/core';
import { EvolutionConfig } from './evolution-tile/evolution-tile.component';
import { EvolutionService } from '../services/evolution.service';
import { Matrix } from '../utilities/Matrix';

@Component({
  selector: 'app-neural-network-tool',
  templateUrl: './neural-network-tool.component.html',
  styleUrls: ['./neural-network-tool.component.scss']
})
export class NeuralNetworkToolComponent implements OnInit, OnDestroy {

  evolutions: EvolutionConfig[] = [];
  interval: NodeJS.Timer;

  constructor(private evolutionService: EvolutionService) {}

  ngOnDestroy(): void {
    clearInterval(this.interval);
  }

  ngOnInit() {
    this.update();
    this.interval = setInterval(() => {
      this.update();
    }, 1000)

    //this.gradientTest();
  }

  update() {
    this.evolutionService.getEvolutions().subscribe(evolutions => {
      this.evolutions = evolutions;
    }, error => {});
  }

  trackById(index, item) {
    return item.id;
  }

  gradientTest() {

    let maxIterations = 100000;
    let inputCount = 100;
    let layers = [50, 50, 10];
    let outputCount = layers[layers.length - 1];
    let widthGradientFactor = 1;
    let biasGradientFactor = 1;
    let inputGradientFactor = 1;
    let gradientFactor = 0.001;

    // training data 

    let input: Matrix[] = [];
    let expected: Matrix[] = [];

    for(let i = 0; i < outputCount; i ++) {
      input.push(new Matrix(inputCount, 1, () => Math.random() *  2 - 1));
      expected.push(new Matrix(outputCount, 1, (y, x) => y == i ? 1 : 0));
    }

    // network

    let weight: Matrix[] = [];
    let bias: Matrix[] = [];
    
    for(let i = 0; i < layers.length; i ++) {
      weight.push(new Matrix(layers[i], i == 0 ? inputCount : layers[i - 1], () => Math.random() *  2 - 1));
      bias.push(new Matrix(layers[i], 1, () => Math.random() *  2 - 1));
    }

    // output

    let output: Matrix[][] = [];

    for(let v = 0; v < outputCount; v ++) {
      let temp: Matrix[] = [];
      for(let i = 0; i < layers.length; i ++) {
        temp.push(new Matrix(layers[i], 1, () => 0));
      }
      output.push(temp);
    }

    // temp gradient

    let outputGradient: Matrix = null;

    for(let v = 0; v < outputCount; v ++) {
      for(let i = 0; i < layers.length; i ++) {
        Matrix.multiply(weight[i], i == 0 ? input[v] : output[v][i - 1], output[v][i]);
        Matrix.add(output[v][i], bias[i], output[v][i]);
        Matrix.sigmoid(output[v][i]);
      }
    }

    //this.displayMatrix(output[0][output.length - 1].params, 'bar');

    let cost = [];

    for(let z = 0; z < maxIterations; z ++) {

      let widthGradients: Matrix[][] = [];
      let biasGradients: Matrix[][] = [];

      for(let v = 0; v < outputCount; v ++) {

        let widthGradientsTemp: Matrix[] = [];
        let biasGradientsTemp: Matrix[] = [];

        for(let i = layers.length - 1; i >= 0; i--) {

          let temp = new Matrix(output[v][i].height, 1, () => 0);
          Matrix.subtract(i == layers.length - 1 ? expected[v] : outputGradient, output[v][i], temp);
          outputGradient = temp;

          let widthGradient = new Matrix(weight[i].height, weight[i].width, (y, x) => {
            var outputValue = Matrix.getValue(outputGradient, y, 0); 
            var inputValue = Matrix.getValue(i == 0 ? input[v] : output[v][i - 1], x, 0);
            return outputValue * inputValue * widthGradientFactor;
          });

          let biasGradient = new Matrix(weight[i].height, 1, (y, x) => 
            Matrix.getValue(outputGradient, y, x) * biasGradientFactor / weight[i].height);

          outputGradient = new Matrix(weight[i].height, 1, (y, x) => {
            let inputValue = 0;
            for(let r = 0; r < weight[i].height; r++) {
              inputValue += Matrix.getValue(weight[i], r, y) * Matrix.getValue(outputGradient, r, 0);
            }
            return inputValue * inputGradientFactor;
          });

          widthGradientsTemp.push(widthGradient);
          biasGradientsTemp.push(biasGradient);
        }

        widthGradientsTemp.reverse();
        biasGradientsTemp.reverse();

        widthGradients.push(widthGradientsTemp);
        biasGradients.push(biasGradientsTemp);
      }

      let widthGradientsAvg: Matrix[] = []
      let biasGradientsAvg: Matrix[] = [];

      for(let i = 0; i < layers.length; i ++) {

        widthGradientsAvg.push(new Matrix(weight[i].height, weight[i].width, (y, x) => {
          let sum = 0;
          for(let v = 0; v < outputCount; v ++) {
            sum += Matrix.getValue(widthGradients[v][i], y, x);
          }
          return sum / outputCount;
        }));

        biasGradientsAvg.push(new Matrix(weight[i].height, 1, (y, x) => {
          let sum = 0;
          for(let v = 0; v < outputCount; v ++) {
            sum += Matrix.getValue(biasGradients[v][i], y, x);
          }
          return sum / outputCount;
        }));
      }

      for(let i = 0; i < layers.length; i ++) {
        Matrix.multiplyByNumber(widthGradientsAvg[i], gradientFactor, widthGradientsAvg[i]);
        Matrix.multiplyByNumber(biasGradientsAvg[i], gradientFactor, biasGradientsAvg[i]);
        Matrix.add(weight[i], widthGradientsAvg[i], weight[i]);
        Matrix.add(bias[i], biasGradientsAvg[i], bias[i]);
      }

      for(let v = 0; v < outputCount; v ++) {
        for(let i = 0; i < layers.length; i ++) {
          Matrix.multiply(weight[i], i == 0 ? input[v] : output[v][i - 1], output[v][i]);
          Matrix.add(output[v][i], bias[i], output[v][i]);
		      Matrix.sigmoid(output[v][i]);
        }
      }
  
      let sum = 0;

      for(let v = 0; v < outputCount; v ++) {
        for(let i = 0; i < outputCount; i++) {
          sum += Math.pow(Math.abs(output[v][output[v].length - 1].params[i] - expected[v].params[i]), 2);
        }
      }

      sum /= outputCount;

      if(cost.length % 100 == 0) {
        console.log(`${z}: ${sum}`);
      }

      // if(sum > cost[cost.length - 1]) {
      //   break;
      // }

      cost.push(sum);
    }

    console.log(output);
  }
}
