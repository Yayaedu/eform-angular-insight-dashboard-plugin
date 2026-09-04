import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { OperationDataResult, OperationResult } from '../../../../common/models';
import { OptionCreateModel, OptionUpdateModel } from '../models';
import { ApiBaseService } from 'src/app/common/services';

export const OptionsMethods = {
  Create: 'api/insight-dashboard-pn/options/create',
  Update: 'api/insight-dashboard-pn/options/update',
  Delete: 'api/insight-dashboard-pn/options/',
};

@Injectable({ providedIn: 'root' })
export class InsightDashboardPnOptionsService {
  private apiBaseService = inject(ApiBaseService);

  create(model: OptionCreateModel): Observable<OperationDataResult<number>> {
    return this.apiBaseService.post(OptionsMethods.Create, model);
  }

  update(model: OptionUpdateModel): Observable<OperationResult> {
    return this.apiBaseService.post(OptionsMethods.Update, model);
  }

  remove(id: number): Observable<OperationResult> {
    return this.apiBaseService.delete(OptionsMethods.Delete + id);
  }
}
