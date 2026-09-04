import { Component, inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { InsightDashboardPnQuestionsService } from '../../../services';
import { QuestionModel } from '../../../models';

@Component({
  selector: 'app-question-delete',
  templateUrl: './question-delete.component.html',
  styleUrls: ['./question-delete.component.scss'],
  standalone: false,
})
export class QuestionDeleteComponent {
  private questionsService = inject(InsightDashboardPnQuestionsService);
  public dialogRef = inject(MatDialogRef<QuestionDeleteComponent>);
  public question = inject<QuestionModel>(MAT_DIALOG_DATA);

  hide(result = false) {
    this.dialogRef.close(result);
  }

  delete() {
    this.questionsService.remove(this.question.id).subscribe((data) => {
      if (data && data.success) {
        this.hide(true);
      }
    });
  }
}
