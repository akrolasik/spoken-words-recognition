import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ChartWithScrollComponent } from './chart-with-scroll.component';

describe('ChartWithScrollComponent', () => {
  let component: ChartWithScrollComponent;
  let fixture: ComponentFixture<ChartWithScrollComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ ChartWithScrollComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ChartWithScrollComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
