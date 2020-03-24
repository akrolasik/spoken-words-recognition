import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { EvolutionTileComponent } from './evolution-tile.component';

describe('EvolutionTileComponent', () => {
  let component: EvolutionTileComponent;
  let fixture: ComponentFixture<EvolutionTileComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ EvolutionTileComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(EvolutionTileComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
