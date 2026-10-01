import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { OperationDataResult } from '../../../../common/models';
import {
  DevicePairingCodeRequestModel,
  DevicePairingCodeResponseModel,
} from '../models';
import { ApiBaseService } from 'src/app/common/services';

export const DevicePairingMethods = {
  RequestCode: 'api/insight-dashboard-pn/device-pairing/code',
};

@Injectable({ providedIn: 'root' })
export class InsightDashboardPnDevicePairingService {
  private apiBaseService = inject(ApiBaseService);

  requestPairingCode(
    model: DevicePairingCodeRequestModel
  ): Observable<OperationDataResult<DevicePairingCodeResponseModel>> {
    return this.apiBaseService.post(DevicePairingMethods.RequestCode, model);
  }
}
