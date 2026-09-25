import { bootstrapApplication } from '@angular/platform-browser';
import { registerLocaleData } from '@angular/common';
import localeVi from '@angular/common/locales/vi';
import { appConfig } from './app/app.config';
import { App } from './app/app';

// Bắt buộc trước khi dùng DatePipe với locale 'vi' (vd date:'EEEE, dd MMMM yyyy':'':'vi'
// ở dashboard) — thiếu dòng này Angular ném NG0701 "Missing locale data for the locale vi"
// và crash render, không liên quan gì tới dữ liệu/cache.
registerLocaleData(localeVi);

bootstrapApplication(App, appConfig)
  .catch((err) => console.error(err));
