import { Injectable } from '@angular/core';

const colors = [
  'Black',
  'Blue',
  'Brown',
  'Cyan',
  'Fuchsia',
  'Gold',
  'Gray',
  'Green',
  'Indigo',
  'Lime',
  'Magenta',
  'Olive',
  'Orange',
  'Pink',
  'Purple',
  'Red',
  'Silver',
  'Violet',
  'White',
  'Yellow',
];

const animals = [
  'Alligator',
  'Bat',
  'Bear',
  'Beaver',
  'Camel',
  'Chameleon',
  'Crow',
  'Dolphin',
  'Duck',
  'Elephant',
  'Fox',
  'Frog',
  'Giraffe',
  'Goose',
  'Hamster',
  'Hedgehog',
  'Iguana',
  'Kangaroo',
  'Koala',
  'Lemur',
  'Lion',
  'Moose',
  'Octopus',
  'Otter',
  'Panda',
  'Penguin',
  'Rabbit',
  'Raccoon',
  'Rhino',
  'Squirrel',
  'Tiger',
  'Turtle',
  'Wolf',
];

@Injectable({
  providedIn: 'root'
})
export class RandomNamesService {

  constructor() { }

  getRandomName() {
    const randomColor = this.getRandomInt(0, colors.length);
    const randomAnimal = this.getRandomInt(0, animals.length);
    return `${colors[randomColor]} ${animals[randomAnimal]}`;
  }

  getRandomInt(min, max) {
    min = Math.ceil(min);
    max = Math.floor(max);
    return Math.floor(Math.random() * (max - min)) + min;
  }
}
