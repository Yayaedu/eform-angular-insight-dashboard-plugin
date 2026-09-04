import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { OperationDataResult, OperationResult } from '../../../../common/models';
import {
  QuestionSetCreateModel,
  QuestionSetListModel,
  QuestionSetModel,
  QuestionSetUpdateModel,
} from '../models';
import { ApiBaseService } from 'src/app/common/services';

export const QuestionSetsMethods = {
  Index: 'api/insight-dashboard-pn/question-sets',
  Get: 'api/insight-dashboard-pn/question-sets/',
  Create: 'api/insight-dashboard-pn/question-sets/create',
  Update: 'api/insight-dashboard-pn/question-sets/update',
  Delete: 'api/insight-dashboard-pn/question-sets/',
};

@Injectable({ providedIn: 'root' })
export class InsightDashboardPnQuestionSetsService {
  private apiBaseService = inject(ApiBaseService);

  getAll(): Observable<OperationDataResult<QuestionSetListModel>> {
    return this.apiBaseService.get(QuestionSetsMethods.Index);
  }

  get(id: number): Observable<OperationDataResult<QuestionSetModel>> {
    return this.apiBaseService.get(QuestionSetsMethods.Get + id);
  }

  create(model: QuestionSetCreateModel): Observable<OperationDataResult<number>> {
    return this.apiBaseService.post(QuestionSetsMethods.Create, model);
  }

  update(model: QuestionSetUpdateModel): Observable<OperationResult> {
    return this.apiBaseService.post(QuestionSetsMethods.Update, model);
  }

  remove(id: number): Observable<OperationResult> {
    return this.apiBaseService.delete(QuestionSetsMethods.Delete + id);
  }
}
