import { Component } from '@angular/core';
import { MatCardModule } from '@angular/material/card';

@Component({
  selector: 'app-administracion-page',
  standalone: true,
  imports: [MatCardModule],
  templateUrl: './administracion-page.component.html',
  styleUrl: './administracion-page.component.scss'
})
export class AdministracionPageComponent {}
