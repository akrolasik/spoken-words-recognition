import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';

const routes: Routes = [
  {
    path: 'data-collector',
    loadChildren: () => import('./data-collector/data-collector.module')
      .then(m => m.DataCollectorModule)
  },
  {
    path: 'data-explorer',
    loadChildren: () => import('./data-explorer/data-explorer.module')
      .then(m => m.DataExplorerModule)
  },
  {
    path: 'data-validator',
    loadChildren: () => import('./data-validator/data-validator.module')
      .then(m => m.DataValidatorModule)
  },
  {
    path: 'issue-explorer',
    loadChildren: () => import('./issue-explorer/issue-explorer.module')
      .then(m => m.IssueExplorerModule)
  },
  {
    path: 'login',
    loadChildren: () => import('./login/login.module')
      .then(m => m.LoginModule)
  },
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
