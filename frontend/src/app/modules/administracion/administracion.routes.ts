import { Routes } from '@angular/router';

import { administrationGuard } from '../../core/guards/administration.guard';
import { AdministracionPageComponent } from './administracion-page.component';

export const ADMINISTRACION_ROUTES: Routes = [{ path: '', component: AdministracionPageComponent, canActivate: [administrationGuard] }];
