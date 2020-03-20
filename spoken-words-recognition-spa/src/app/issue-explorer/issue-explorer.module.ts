import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { IssueExplorerComponent } from './issue-explorer.component';
import { NbLayoutModule, NbCardModule, NbButtonModule } from '@nebular/theme';
import { IssueComponent } from './issue/issue.component';
import { Routes, RouterModule } from '@angular/router';

export const routes: Routes = [
  { path: '', component: IssueExplorerComponent },
];

@NgModule({
  declarations: [IssueExplorerComponent, IssueComponent],
  imports: [
    CommonModule,
    NbLayoutModule,
    NbCardModule,
    NbButtonModule,
    RouterModule.forChild(routes)
  ],
})
export class IssueExplorerModule { }
