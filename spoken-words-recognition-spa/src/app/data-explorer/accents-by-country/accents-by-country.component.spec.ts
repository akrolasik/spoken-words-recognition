import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { AccentsByCountryComponent } from './accents-by-country.component';

describe('MapComponent', () => {
  let component: AccentsByCountryComponent;
  let fixture: ComponentFixture<AccentsByCountryComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ AccentsByCountryComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(AccentsByCountryComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
