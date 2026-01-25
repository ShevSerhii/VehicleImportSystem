import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { DeviceService } from '../../services/device.service';
import { USE_DEVICE_ID } from './api.context';

export const deviceInterceptor: HttpInterceptorFn = (req, next) => {
  const deviceService = inject(DeviceService);

  if (req.context.get(USE_DEVICE_ID)) {
    const deviceId = deviceService.getDeviceId();
    
    const clonedReq = req.clone({
      setHeaders: { 'X-Device-Id': deviceId }
    });
    
    return next(clonedReq);
  }

  return next(req);
};