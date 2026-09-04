import { Component, inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { InsightDashboardPnQuestionsService } from '../../../services';
import { QUESTION_TYPES, QuestionModel } from '../../../models';

// Bruges til både at tilføje et nyt spørgsmål (data.existing er null) og
// redigere et eksisterende (data.existing er sat). Typen kan IKKE ændres
// efter oprettelse i denne version — backend'en regenererer ikke special-
// options ved typeskift (se QuestionsService.Update-kommentaren), så vi
// låser feltet i UI'et fremfor at tilbyde en handling der kan give
// inkonsistente options.
@Component({
  selector: 'app-question-edit',
  templateUrl: './question-edit.component.html',
  styleUrls: ['./question-edit.component.scss'],
  standalone: false,
})
export class QuestionEditComponent {
  private questionsService = inject(InsightDashboardPnQuestionsService);
  public dialogRef = inject(MatDialogRef<QuestionEditComponent>);
  public data = inject<{ questionSetId: number; existing: QuestionModel | null }>(MAT_DIALOG_DATA);

  questionTypes = QUESTION_TYPES;
  text = this.data.existing?.text ?? '';
  questionType = this.data.existing?.questionType ?? QUESTION_TYPES[0].value;

  get isEdit(): boolean {
    return !!this.data.existing;
  }

  hide(result: number | boolean = false) {
    this.dialogRef.close(result);
  }

  save() {
    if (this.isEdit) {
      this.questionsService
        .update({ id: this.data.existing.id, text: this.text, questionType: this.questionType })
        .subscribe((data) => {
          if (data && data.success) {
            this.hide(true);
          }
        });
    } else {
      this.questionsService
        .create({ questionSetId: this.data.questionSetId, text: this.text, questionType: this.questionType })
        .subscribe((data) => {
          if (data && data.success) {
            this.hide(data.model);
          }
        });
    }
  }
}
