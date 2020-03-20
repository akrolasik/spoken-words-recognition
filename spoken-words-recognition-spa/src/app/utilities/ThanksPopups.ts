import { Modification } from '../services/api-client/Modification';
import { ThanksPopup } from './ThanksPopup';

export const THANKS_POPUPS: ThanksPopup[] = [
  {
    modification: Modification.Slower,
    title: 'That was slow!',
    text: 'Flash appreciate your input',
    image: 'slower.gif'
  },
  {
    modification: Modification.Faster,
    title: 'That was fast!',
    text: 'Auctioneers appreciate your input!',
    image: 'faster.jpg'
  },
  {
    modification: Modification.Quieter,
    title: 'That was quiet!',
    text: 'Students appreciate your input!',
    image: 'quieter.jpg'
  },
  {
    modification: Modification.Louder,
    title: 'That was loud!',
    text: 'Metallica appreciate your input!',
    image: 'louder.webp'
  },
  {
    modification: Modification.Noise,
    title: 'That was noisy!',
    text: 'Your neighbour appreciate your input!',
    image: 'noise.jpg'
  }
];
