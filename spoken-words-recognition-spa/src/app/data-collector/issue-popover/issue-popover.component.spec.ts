import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { IssuePopoverComponent } from './issue-popover.component';

describe('IssuePopoverComponent', () => {
  let component: IssuePopoverComponent;
  let fixture: ComponentFixture<IssuePopoverComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ IssuePopoverComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(IssuePopoverComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
