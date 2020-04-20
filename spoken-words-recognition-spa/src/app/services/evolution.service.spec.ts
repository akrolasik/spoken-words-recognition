import { TestBed } from '@angular/core/testing';

import { EvolutionService } from './evolution.service';

describe('EvolutionService', () => {
  beforeEach(() => TestBed.configureTestingModule({}));

  it('should be created', () => {
    const service: EvolutionService = TestBed.get(EvolutionService);
    expect(service).toBeTruthy();
  });
});
