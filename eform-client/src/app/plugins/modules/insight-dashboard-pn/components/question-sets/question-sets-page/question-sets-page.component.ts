import { Component, inject, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import { Overlay } from '@angular/cdk/overlay';
import { dialogConfigHelper } from 'src/app/common/helpers';
import { QuestionSetListModel, QuestionSetModel } from '../../../models';
import { InsightDashboardPnQuestionSetsService } from '../../../services';
import { QuestionSetEditComponent } from '../question-set-edit/question-set-edit.component';
import { QuestionSetDeleteComponent } from '../question-set-delete/question-set-delete.component';

// Spørgeskema-builderens landingsside — liste over spørgeskemaer. Rene
// CRUD-liste uden pagination/sortering, da backend'en (endnu) ikke
// understøtter det for question-sets (til forskel fra surveys-configs).
@Component({
  selector: 'app-question-sets-page',
  templateUrl: './question-sets-page.component.html',
  styleUrls: ['./question-sets-page.component.scss'],
  standalone: false,
})
export class QuestionSetsPageComponent implements OnInit {
  private questionSetsService = inject(InsightDashboardPnQuestionSetsService);
  private router = inject(Router);
  private dialog = inject(MatDialog);
  private overlay = inject(Overlay);

  listModel: QuestionSetListModel = new QuestionSetListModel();

  ngOnInit() {
    this.load();
  }

  load() {
    this.questionSetsService.getAll().subscribe((data) => {
      if (data && data.success) {
        this.listModel = data.model;
      }
    });
  }

  openCreateDialog() {
    this.dialog
      .open(QuestionSetEditComponent, dialogConfigHelper(this.overlay, null))
      .afterClosed()
      .subscribe((newId) => {
        if (newId) {
          this.openEditor(newId);
        }
      });
  }

  openEditor(id: number) {
    this.router.navigate(['/plugins/insight-dashboard-pn/question-sets', id]).then();
  }

  openDeleteDialog(questionSet: QuestionSetModel, event: Event) {
    event.stopPropagation();
    this.dialog
      .open(QuestionSetDeleteComponent, dialogConfigHelper(this.overlay, questionSet))
      .afterClosed()
      .subscribe((deleted) => (deleted ? this.load() : undefined));
  }
}
