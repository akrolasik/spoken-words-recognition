//import { GPU } from 'gpu.js';

export class Matrix {
    

    public width: number;
    public height: number;
    public params: number[];

    constructor(height: number, width: number, value: (y: number, x: number) => number) {
      this.width = width;
      this.height = height;
      this.params = [];
      for (let y = 0; y < height; y++) {
        for (let x = 0; x < width; x++) {
          Matrix.setValue(this, y, x, value(y, x));
        }
      }
    }
  
    static getValue(m: Matrix, y: number, x: number): number {
      return m.params[y * m.width + x];
    }
  
    static setValue(m: Matrix, y: number, x: number, value: number) {
      m.params[y * m.width + x] = value;
    }

    static getMinParam(m: Matrix) {
      return Math.min(...m.params);
    }

    static getMaxParam(m: Matrix) {
      return Math.max(...m.params);
    }

    static getMinMatrix(m: Matrix, width: number) {
      return new Matrix(width, m.width, () => 0);
    }

    static getMaxMatrix(m: Matrix, width: number) {
      return new Matrix(width, m.width, () => 1);
    }
  
    static normalizeValue(m: Matrix, y: number, x: number, min: number, range: number) {
      Matrix.setValue(m, y, x, (Matrix.getValue(m, y, x) - min) / range);
    }

    static getResultMatrix(m: Matrix, width: number): Matrix {
      return new Matrix(width, m.height, () => 0);
    }
  
    static multiply(a: Matrix, b: Matrix, result: Matrix) {
      for (let y = 0; y < a.height; y++) {
        for (let x = 0; x < b.width; x++) {
          let sum = 0;
          for (let i = 0; i < a.width; i++) {
            sum += Matrix.getValue(a, y, i) * Matrix.getValue(b, i, x);
          }
          Matrix.setValue(result, y, x, sum);
        }
      }
    }

    static add(a: Matrix, b: Matrix, result: Matrix) {
      for (let y = 0; y < a.height; y++) {
        for (let x = 0; x < a.width; x++) {
          Matrix.setValue(result, y, x, Matrix.getValue(a, y, x) + Matrix.getValue(b, y, x));
        }
      }
    }

    static multiplyByNumber(a: Matrix, num: number, result: Matrix) {
      for (let y = 0; y < a.height; y++) {
        for (let x = 0; x < a.width; x++) {
          Matrix.setValue(result, y, x, Matrix.getValue(a, y, x) * num);
        }
      }
    }

    static sigmoid(matrix: Matrix) {
      for (let y = 0; y < matrix.height; y++) {
        for (let x = 0; x < matrix.width; x++) {
          let newValue = 1 / (1 + Math.exp(-Matrix.getValue(matrix, y, x)));
          Matrix.setValue(matrix, y, x, newValue);
        }
      }
    }

    static subtract(a: Matrix, b: Matrix, result: Matrix) {
      for (let y = 0; y < a.height; y++) {
        for (let x = 0; x < a.width; x++) {
          Matrix.setValue(result, y, x, Matrix.getValue(a, y, x) - Matrix.getValue(b, y, x));
        }
      }
    }

    static normalize(m: Matrix, min: Matrix, max: Matrix, result: Matrix) {

      Matrix.multiply(m, min, result);
      let minValue = Matrix.getMinParam(result);

      Matrix.multiply(m, max, result);
      let maxValue = Matrix.getMaxParam(result);

      let range = maxValue - minValue;

      for (let y = 0; y < m.height; y++) {
        for (let x = 0; x < m.width; x++) {
          Matrix.normalizeValue(m, y, x, minValue, range);
        }
      }
    }
      
    // public static createFromParams(params: number[]): Matrix {
    //   let matrix = new Matrix();
    //   matrix.params = params;
    //   return matrix;
    // }
  
    // public static gpuMultiply(a: Matrix, b: Matrix): Matrix {
    //   const gpu = new GPU();
    //   const multiplyMatrix = gpu.createKernel(function(a: number[][], b: number[][], size: number) {
    //     let sum = 0;
    //     for (let i = 0; i < size; i++) {
    //       sum += a[this.thread.y][i] * b[i][this.thread.x];
    //     }
    //     return sum;
    //   }).setOutput([b.width(), a.height()]);
      
    //   var params = multiplyMatrix(a.params, b.params, b.height()) as number[][];
    //   return this.createFromParams(params);
    // }
  } 