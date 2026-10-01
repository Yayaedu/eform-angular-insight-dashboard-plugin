import { Component, inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { InsightDashboardPnDevicePairingService } from '../../../services';
import { DevicePairingCodeResponseModel } from '../../../models';
import { CommonDictionaryModel } from 'src/app/common/models';

// Samme to-trins-UX som eform-angular-frontends rigtige Unit-OTP
// ("New OTP" på Device Users-siden): advar om at en ny kode ugyldiggør en
// evt. udestående kode for samme site, generér den, og vis den så admin kan
// give den videre til den, der sidder med insight_app.
@Component({
  selector: 'app-device-pairing-code-modal',
  templateUrl: './device-pairing-code-modal.component.html',
  styleUrls: ['./device-pairing-code-modal.component.scss'],
  standalone: false,
})
export class DevicePairingCodeModalComponent {
  private devicePairingService = inject(InsightDashboardPnDevicePairingService);
  public dialogRef = inject(MatDialogRef<DevicePairingCodeModalComponent>);
  public site = inject<CommonDictionaryModel>(MAT_DIALOG_DATA);

  result: DevicePairingCodeResponseModel | null = null;
  requesting = false;

  hide() {
    this.dialogRef.close(!!this.result);
  }

  requestCode() {
    this.requesting = true;
    this.devicePairingService
      .requestPairingCode({ site_id: this.site.id })
      .subscribe((data) => {
        this.requesting = false;
        if (data && data.success) {
          this.result = data.model;
        }
      });
  }
}
