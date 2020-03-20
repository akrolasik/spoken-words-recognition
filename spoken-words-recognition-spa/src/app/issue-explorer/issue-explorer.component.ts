import { Component, OnInit } from '@angular/core';
import { ApiClientService } from '../services/api-client/ApiClientService';
import { ReportedIssue } from '../services/api-client/ReportedIssue';

@Component({
  selector: 'app-issue-explorer',
  templateUrl: './issue-explorer.component.html',
  styleUrls: ['./issue-explorer.component.scss'],
})
export class IssueExplorerComponent implements OnInit {

  issues: ReportedIssue[];

  constructor(private apiClient: ApiClientService) { }

  ngOnInit() {
    this.apiClient.getReportedIssues().subscribe(issues => {
      this.issues = issues;
    }, error => {
      console.error(error);
    });
  }

  declineCallback(issueId: string) {
    this.apiClient.deleteReportedIssue(issueId);
    this.issues = this.issues.splice(1);
  }
}
