import { Component } from "@angular/core";
import { ReportedIssue } from "../../services/api-client/ReportedIssue";
import { Guid } from "guid-typescript";
import { AnalyticsService } from "../../services/analytics.service";
import { ApiClientService } from "../../services/api-client/ApiClientService";
import { Image as IssueImage } from "../../services/api-client/Image";
import {
  NbToastrService,
  NbComponentStatus,
  NbGlobalLogicalPosition
} from "@nebular/theme";

@Component({
  selector: "app-issue-popover",
  templateUrl: "./issue-popover.component.html",
  styleUrls: ["./issue-popover.component.scss"]
})
export class IssuePopoverComponent {
  image: string;
  imageData: ImageData;

  constructor(
    private analytics: AnalyticsService,
    private apiClient: ApiClientService,
    private toastrService: NbToastrService
  ) {}

  blurActiveElement() {
    (document.activeElement as HTMLElement).blur();
  }

  showImage() {
    let screenshot = document.getElementById("screenshot") as HTMLCanvasElement;
    let screenshotContext = screenshot.getContext("2d");

    let img = new Image();
    img.onload = function() {
      screenshotContext.drawImage(
        img,
        0,
        0,
        screenshot.width,
        screenshot.height
      );
    };
    img.src = this.image;
  }

  onFileChanged(files: File[]) {
    if (files.length === 0) return;

    let fileReader = new FileReader();
    fileReader.onload = event => {
      this.image = fileReader.result as string;
      this.showImage();
    };

    fileReader.readAsDataURL(files[0]);
  }

  animateMessageInput(input: HTMLTextAreaElement) {
    input.animate(
      [
        { borderColor: "#edf1f7", backgroundColor: "#f7f9fc" },
        { borderColor: "#ff000088", backgroundColor: "#ff000022" },
        { borderColor: "#edf1f7", backgroundColor: "#f7f9fc" }
      ],
      {
        duration: 800,
        iterations: 1
      }
    );
  }

  reportIssue() {
    let input = document.getElementById("message") as HTMLTextAreaElement;

    if (input.value == null || input.value.length === 0) {
      this.animateMessageInput(input);
      return;
    }

    let issueId = this.uploadIssue(input.value);
    this.uploadImage(issueId);
    this.showToastr();

    this.closePopover();
  }

  closePopover() {
    (document.getElementById("background") as HTMLElement).click();
  }

  uploadIssue(message: string): string {
    let reportedIssue: ReportedIssue = {
      id: Guid.create().toString(),
      speakerId: this.analytics.getUser(),
      message: message,
      browser:
        navigator.vendor +
        ", " +
        navigator.appName +
        ", " +
        navigator.appCodeName +
        ", " +
        navigator.product,
      userAgent: navigator.userAgent,
      localization: navigator.language,
      isImageAttached: this.image != null
    };

    this.apiClient.putReportedIssue(reportedIssue);

    return reportedIssue.id;
  }

  uploadImage(issueId: string) {
    if (this.image == null) return;
    let issueImage = this.getIssueImage();
    this.apiClient.putReportedIssueImage(issueId, issueImage);
  }

  getIssueImage(): IssueImage {
    let screenshot = document.getElementById("screenshot") as HTMLCanvasElement;
    let screenshotContext = screenshot.getContext("2d");
    let imageData = screenshotContext.getImageData(
      0,
      0,
      screenshot.width,
      screenshot.height
    );
    let data = "";

    imageData.data.forEach(uint => {
      data += String.fromCharCode(uint);
    });

    return {
      width: imageData.width,
      height: imageData.height,
      data: data
    };
  }

  showToastr() {
    let config = {
      status: "success" as NbComponentStatus,
      duration: 5000,
      position: NbGlobalLogicalPosition.TOP_END,
      destroyByClick: true,
      hasIcon: true,
      preventDuplicates: true
    };

    this.toastrService.success(
      "Tool will be better thanks to Your help!",
      "You are awesome!",
      config
    );
  }
}
