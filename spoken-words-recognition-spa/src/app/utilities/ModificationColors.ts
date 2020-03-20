import { Modification } from '../services/api-client/Modification';

export const ModificationColors: Map<Modification, string> = new Map<Modification, string>([
  [Modification.None, '#2ce69b'],
  [Modification.Slower, '#42aaff'],
  [Modification.Faster, '#2ce69b'],
  [Modification.Quieter, '#ffc94d'],
  [Modification.Louder, '#ff708d'],
  [Modification.Noise, '#598bff'],
]);
