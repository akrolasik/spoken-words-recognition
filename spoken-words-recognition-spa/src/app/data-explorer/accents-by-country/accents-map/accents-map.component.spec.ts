import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { AccentsMapComponent } from './accents-map.component';

describe('MapComponent', () => {
  let component: AccentsMapComponent;
  let fixture: ComponentFixture<AccentsMapComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ AccentsMapComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(AccentsMapComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
