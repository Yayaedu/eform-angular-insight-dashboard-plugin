import { Component, inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { InsightDashboardPnQuestionSetsService } from '../../../services';
import { QuestionSetModel } from '../../../models';

@Component({
  selector: 'app-question-set-delete',
  templateUrl: './question-set-delete.component.html',
  styleUrls: ['./question-set-delete.component.scss'],
  standalone: false,
})
export class QuestionSetDeleteComponent {
  private questionSetsService = inject(InsightDashboardPnQuestionSetsService);
  public dialogRef = inject(MatDialogRef<QuestionSetDeleteComponent>);
  public questionSet = inject<QuestionSetModel>(MAT_DIALOG_DATA);

  hide(result = false) {
    this.dialogRef.close(result);
  }

  delete() {
    this.questionSetsService.remove(this.questionSet.id).subscribe((data) => {
      if (data && data.success) {
        this.hide(true);
      }
    });
  }
}
