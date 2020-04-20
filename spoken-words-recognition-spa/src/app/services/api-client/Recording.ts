import { Modification } from './Modification';
import { ZipEntry } from "../ZipEntry";

export class Recording {
  id: string;
  speakerId: string;
  accent: string;
  localization: string;
  modification: Modification;
  word: string;
  chunksCount: number;
  zipEntry: ZipEntry;
}
