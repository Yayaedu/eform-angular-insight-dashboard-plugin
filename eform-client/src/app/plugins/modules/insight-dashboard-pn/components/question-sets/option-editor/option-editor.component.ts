import { Component, inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { InsightDashboardPnOptionsService } from '../../../services';
import { OptionModel, QuestionModel } from '../../../models';

// Kun åbnet for BUTTONS/LIST/MULTI-spørgsmål — se
// questionTypeHasManualOptions i question-type.const.ts. De øvrige typer
// har deres options auto-genereret af SDK'en og har ingen "rediger
// svarmuligheder"-knap i editor-siden.
@Component({
  selector: 'app-option-editor',
  templateUrl: './option-editor.component.html',
  styleUrls: ['./option-editor.component.scss'],
  standalone: false,
})
export class OptionEditorComponent {
  private optionsService = inject(InsightDashboardPnOptionsService);
  public dialogRef = inject(MatDialogRef<OptionEditorComponent>);
  public question = inject<QuestionModel>(MAT_DIALOG_DATA);

  options: OptionModel[] = [...this.question.options].sort(
    (a, b) => a.optionIndex - b.optionIndex
  );
  newLabel = '';
  changed = false;

  hide() {
    this.dialogRef.close(this.changed);
  }

  addOption() {
    if (!this.newLabel) {
      return;
    }
    this.optionsService
      .create({ questionId: this.question.id, label: this.newLabel })
      .subscribe((data) => {
        if (data && data.success) {
          this.options.push({
            id: data.model,
            questionId: this.question.id,
            label: this.newLabel,
            optionIndex: this.options.length,
          });
          this.newLabel = '';
          this.changed = true;
        }
      });
  }

  saveLabel(option: OptionModel) {
    this.optionsService.update({ id: option.id, label: option.label }).subscribe((data) => {
      if (data && data.success) {
        this.changed = true;
      }
    });
  }

  deleteOption(option: OptionModel) {
    this.optionsService.remove(option.id).subscribe((data) => {
      if (data && data.success) {
        this.options = this.options.filter((o) => o.id !== option.id);
        this.changed = true;
      }
    });
  }
}
