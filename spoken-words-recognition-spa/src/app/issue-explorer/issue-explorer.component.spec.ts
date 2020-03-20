import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { IssueExplorerComponent } from './issue-explorer.component';

describe('IssueExplorerComponent', () => {
  let component: IssueExplorerComponent;
  let fixture: ComponentFixture<IssueExplorerComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ IssueExplorerComponent ],
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(IssueExplorerComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
