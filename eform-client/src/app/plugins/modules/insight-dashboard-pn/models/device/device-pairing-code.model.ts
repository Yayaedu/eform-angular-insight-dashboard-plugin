export class DevicePairingCodeRequestModel {
  site_id: number;
}

export class DevicePairingCodeResponseModel {
  code: string;
  site_id: number;
  site_name: string;
  expires_at: string;
}
