import { Injectable, PLATFORM_ID, inject } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';

@Injectable({ providedIn: 'root' })
export class DeviceService {
  private readonly platformId = inject(PLATFORM_ID);
  private readonly storageKey = 'device_id';

  getDeviceId(): string {
    if (!isPlatformBrowser(this.platformId)) return '';

    let id = localStorage.getItem(this.storageKey);
    if (!id) {
      id = crypto.randomUUID();
      localStorage.setItem(this.storageKey, id);
    }
    return id;
  }
}