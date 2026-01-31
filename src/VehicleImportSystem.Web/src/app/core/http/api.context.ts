import { HttpContextToken } from '@angular/common/http';

export const USE_DEVICE_ID = new HttpContextToken<boolean>(() => false);