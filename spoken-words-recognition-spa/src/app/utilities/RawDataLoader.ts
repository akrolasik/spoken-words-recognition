import { ApiClientService } from '../services/api-client/ApiClientService';
import { RawData } from './RawData';

export class RawDataLoader {

  rawData: RawData[] = [];
  dataLeftToValidate: number;

  onFirstLoad: () => void;

  constructor(private apiClient: ApiClientService, prepareNewData: () => void) {
    this.onFirstLoad = prepareNewData;
    this.apiClient.getRawRecordings().subscribe(rawRecordings => {
      this.dataLeftToValidate = rawRecordings.length;
      if (this.dataLeftToValidate > 0) {
        rawRecordings.forEach(rawRecodring => {
          this.rawData.push({
            rawRecording: rawRecodring,
          });
        });
        this.loadData();
      }
    }, error => console.error(error));
  }

  getNextRawDataToLoad() {
    const data = this.rawData.filter(x => x.audio == null);
    if (data != null && data.length > 0) {
      return data[0];
    }
  }

  async loadData() {

    await this.loadOne();
    this.onFirstLoad();

    while (await this.loadOne()) {}
  }

  async loadOne() {
    const data = this.getNextRawDataToLoad();

    if (data == null) { return false; }

    data.audio = await this.apiClient.getRawRecordingAudio(data.rawRecording.id);
    const chunks = await this.apiClient.getRawRecordingFrequencies(data.rawRecording.id).toPromise();
    data.frequencies = { frequenciesChunks: chunks };

    return true;
  }

  removeFirst() {
    this.rawData = this.rawData.splice(1);
    this.dataLeftToValidate = this.rawData.length;
  }
}
