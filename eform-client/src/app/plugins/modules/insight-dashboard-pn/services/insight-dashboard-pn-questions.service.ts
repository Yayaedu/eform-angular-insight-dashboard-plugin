import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { OperationDataResult, OperationResult } from '../../../../common/models';
import { QuestionCreateModel, QuestionReorderModel, QuestionUpdateModel } from '../models';
import { ApiBaseService } from 'src/app/common/services';

export const QuestionsMethods = {
  Create: 'api/insight-dashboard-pn/questions/create',
  Update: 'api/insight-dashboard-pn/questions/update',
  Reorder: 'api/insight-dashboard-pn/questions/reorder',
  Delete: 'api/insight-dashboard-pn/questions/',
};

@Injectable({ providedIn: 'root' })
export class InsightDashboardPnQuestionsService {
  private apiBaseService = inject(ApiBaseService);

  create(model: QuestionCreateModel): Observable<OperationDataResult<number>> {
    return this.apiBaseService.post(QuestionsMethods.Create, model);
  }

  update(model: QuestionUpdateModel): Observable<OperationResult> {
    return this.apiBaseService.post(QuestionsMethods.Update, model);
  }

  reorder(model: QuestionReorderModel): Observable<OperationResult> {
    return this.apiBaseService.post(QuestionsMethods.Reorder, model);
  }

  remove(id: number): Observable<OperationResult> {
    return this.apiBaseService.delete(QuestionsMethods.Delete + id);
  }
}
