import { Component, inject, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import { Overlay } from '@angular/cdk/overlay';
import { dialogConfigHelper } from 'src/app/common/helpers';
import {
  InsightDashboardPnQuestionsService,
  InsightDashboardPnQuestionSetsService,
} from '../../../services';
import { QuestionModel, QuestionSetModel, questionTypeHasManualOptions, questionTypeLabel } from '../../../models';
import { QuestionEditComponent } from '../question-edit/question-edit.component';
import { QuestionDeleteComponent } from '../question-delete/question-delete.component';
import { OptionEditorComponent } from '../option-editor/option-editor.component';

// Spørgeskema-builderens hovedside: spørgsmålslisten er LINEÆR (bekræftet
// af René) — rækkefølgen er simpelthen questionIndex, ingen forgrening.
// Rykkes med op/ned-knapper fremfor drag-and-drop, for at holde
// implementeringen simpel i denne første version.
@Component({
  selector: 'app-question-set-editor-page',
  templateUrl: './question-set-editor-page.component.html',
  styleUrls: ['./question-set-editor-page.component.scss'],
  standalone: false,
})
export class QuestionSetEditorPageComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private questionSetsService = inject(InsightDashboardPnQuestionSetsService);
  private questionsService = inject(InsightDashboardPnQuestionsService);
  private dialog = inject(MatDialog);
  private overlay = inject(Overlay);

  questionSet: QuestionSetModel = new QuestionSetModel();
  loaded = false;

  questionTypeLabel = questionTypeLabel;
  questionTypeHasManualOptions = questionTypeHasManualOptions;

  ngOnInit() {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    this.load(id);
  }

  load(id: number) {
    this.questionSetsService.get(id).subscribe((data) => {
      if (data && data.success) {
        this.questionSet = data.model;
        this.questionSet.questions.sort((a, b) => a.questionIndex - b.questionIndex);
      }
      this.loaded = true;
    });
  }

  backToList() {
    this.router.navigate(['/plugins/insight-dashboard-pn/question-sets']).then();
  }

  openAddQuestionDialog() {
    this.dialog
      .open(
        QuestionEditComponent,
        dialogConfigHelper(this.overlay, { questionSetId: this.questionSet.id, existing: null })
      )
      .afterClosed()
      .subscribe((created) => (created ? this.load(this.questionSet.id) : undefined));
  }

  openEditQuestionDialog(question: QuestionModel) {
    this.dialog
      .open(
        QuestionEditComponent,
        dialogConfigHelper(this.overlay, { questionSetId: this.questionSet.id, existing: question })
      )
      .afterClosed()
      .subscribe((saved) => (saved ? this.load(this.questionSet.id) : undefined));
  }

  openDeleteQuestionDialog(question: QuestionModel) {
    this.dialog
      .open(QuestionDeleteComponent, dialogConfigHelper(this.overlay, question))
      .afterClosed()
      .subscribe((deleted) => (deleted ? this.load(this.questionSet.id) : undefined));
  }

  openOptionEditor(question: QuestionModel) {
    this.dialog
      .open(OptionEditorComponent, dialogConfigHelper(this.overlay, question))
      .afterClosed()
      .subscribe((changed) => (changed ? this.load(this.questionSet.id) : undefined));
  }

  moveUp(question: QuestionModel) {
    if (question.questionIndex === 0) {
      return;
    }
    this.questionsService
      .reorder({ id: question.id, newIndex: question.questionIndex - 1 })
      .subscribe((data) => (data && data.success ? this.load(this.questionSet.id) : undefined));
  }

  moveDown(question: QuestionModel) {
    if (question.questionIndex === this.questionSet.questions.length - 1) {
      return;
    }
    this.questionsService
      .reorder({ id: question.id, newIndex: question.questionIndex + 1 })
      .subscribe((data) => (data && data.success ? this.load(this.questionSet.id) : undefined));
  }
}
