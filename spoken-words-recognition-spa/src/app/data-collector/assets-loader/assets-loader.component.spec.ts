import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { AssetsLoaderComponent } from './assets-loader.component';

describe('AssetsLoaderComponent', () => {
  let component: AssetsLoaderComponent;
  let fixture: ComponentFixture<AssetsLoaderComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ AssetsLoaderComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(AssetsLoaderComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
