import { Matrix } from "./Matrix";

export class MatrixChain {

  public matrices: Matrix[];

  constructor(dimensions: number[], value: () => number) {
    this.matrices = [];
    for (let i = 0; i < dimensions.length - 1; i++) {
      this.matrices.push(new Matrix(dimensions[i], dimensions[i + 1], value));
    }
  }

  static calculate(c: MatrixChain, io: Matrix[]) {
    for (let i = 0; i < c.matrices.length; i++) {
      Matrix.multiply(c.matrices[i], io[i], io[i + 1]);
    }
  }

  static getMinMatrices(c: MatrixChain, width: number) {
    return c.matrices.map(m => new Matrix(width, m.width, () => 0));
  }

  static getMaxMatrices(c: MatrixChain, width: number) {
    return c.matrices.map(m => new Matrix(width, m.width, () => 1));
  }

  static getResultMatrices(c: MatrixChain, width: number): Matrix[] {
    return c.matrices.map(m => new Matrix(width, m.height, () => 0));
  }

  static normalize(c: MatrixChain, mins: Matrix[], maxs: Matrix[], results: Matrix[]) {
    for (let i = 0; i < c.matrices.length; i++) {
      Matrix.normalize(c.matrices[i], mins[i], maxs[i], results[i]);
    }
  }
}
