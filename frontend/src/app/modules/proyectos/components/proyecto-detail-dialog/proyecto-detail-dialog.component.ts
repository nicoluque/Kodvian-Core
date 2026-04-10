import { CommonModule, CurrencyPipe } from '@angular/common';
import { Component, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';

import { ProyectoDetalle } from '../../models/proyectos.models';

@Component({
  selector: 'app-proyecto-detail-dialog',
  standalone: true,
  imports: [CommonModule, CurrencyPipe, MatDialogModule, MatButtonModule],
  templateUrl: './proyecto-detail-dialog.component.html',
  styleUrl: './proyecto-detail-dialog.component.scss'
})
export class ProyectoDetailDialogComponent {
  constructor(@Inject(MAT_DIALOG_DATA) public readonly proyecto: ProyectoDetalle) {}

  valor(texto?: string | number | null): string {
    return texto === null || texto === undefined || texto === '' ? '-' : String(texto);
  }

  mostrarEstado(estado: string): string {
    if (estado === 'Planificacion') return 'Planificación';
    if (estado === 'EnCurso') return 'En curso';
    return estado;
  }
}
