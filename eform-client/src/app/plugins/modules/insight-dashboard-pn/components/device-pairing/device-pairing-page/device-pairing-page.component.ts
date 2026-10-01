import { Component, inject, OnInit } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { Overlay } from '@angular/cdk/overlay';
import { dialogConfigHelper } from 'src/app/common/helpers';
import { CommonDictionaryModel } from 'src/app/common/models';
import { SitesService } from 'src/app/common/services';
import { DevicePairingCodeModalComponent } from '../device-pairing-code-modal/device-pairing-code-modal.component';

// Landingsside for enhedsparring: én liste over sites (samme dictionary
// survey-configs-siden bruger til "Location Name"), hver med en knap der
// åbner DevicePairingCodeModalComponent til at generere/regenerere en
// engangskode til insight_app — se DeviceModels.cs for hvorfor dette
// erstatter den gamle (usikre) direkte-på-SiteId-parring.
@Component({
  selector: 'app-device-pairing-page',
  templateUrl: './device-pairing-page.component.html',
  styleUrls: ['./device-pairing-page.component.scss'],
  standalone: false,
})
export class DevicePairingPageComponent implements OnInit {
  private sitesService = inject(SitesService);
  private dialog = inject(MatDialog);
  private overlay = inject(Overlay);

  sites: CommonDictionaryModel[] = [];

  ngOnInit() {
    this.load();
  }

  load() {
    this.sitesService.getAllSitesDictionary().subscribe((data) => {
      if (data && data.success) {
        this.sites = data.model;
      }
    });
  }

  openPairingCodeDialog(site: CommonDictionaryModel) {
    this.dialog.open(DevicePairingCodeModalComponent, dialogConfigHelper(this.overlay, site));
  }
}
