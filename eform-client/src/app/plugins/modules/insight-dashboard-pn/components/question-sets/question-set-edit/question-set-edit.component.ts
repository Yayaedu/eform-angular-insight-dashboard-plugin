import { Component, inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { InsightDashboardPnQuestionSetsService } from '../../../services';
import { QuestionSetModel } from '../../../models';

// Bruges til både oprettelse (data === null) og omdøbning (data er det
// eksisterende spørgeskema) — ét dialog-komponent for begge for at holde
// filantallet nede, jf. surveys-modulets separate New/Edit-mønster som
// bevidst forenklet her.
@Component({
  selector: 'app-question-set-edit',
  templateUrl: './question-set-edit.component.html',
  styleUrls: ['./question-set-edit.component.scss'],
  standalone: false,
})
export class QuestionSetEditComponent {
  private questionSetsService = inject(InsightDashboardPnQuestionSetsService);
  public dialogRef = inject(MatDialogRef<QuestionSetEditComponent>);
  public existing = inject<QuestionSetModel | null>(MAT_DIALOG_DATA);

  name = this.existing?.name ?? '';

  get isEdit(): boolean {
    return !!this.existing;
  }

  hide(result: number | boolean = false) {
    this.dialogRef.close(result);
  }

  save() {
    if (this.isEdit) {
      this.questionSetsService
        .update({ id: this.existing.id, name: this.name })
        .subscribe((data) => {
          if (data && data.success) {
            this.hide(true);
          }
        });
    } else {
      this.questionSetsService.create({ name: this.name }).subscribe((data) => {
        if (data && data.success) {
          this.hide(data.model);
        }
      });
    }
  }
}
