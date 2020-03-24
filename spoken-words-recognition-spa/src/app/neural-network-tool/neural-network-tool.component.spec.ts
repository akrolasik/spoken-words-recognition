import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { NeuralNetworkToolComponent } from './neural-network-tool.component';

describe('NeuralNetworkToolComponent', () => {
  let component: NeuralNetworkToolComponent;
  let fixture: ComponentFixture<NeuralNetworkToolComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ NeuralNetworkToolComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(NeuralNetworkToolComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
