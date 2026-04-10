import { Component, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';

import { TareaDetalle } from '../../models/tareas.models';

@Component({
  selector: 'app-tarea-detail-dialog',
  standalone: true,
  imports: [MatDialogModule, MatButtonModule],
  templateUrl: './tarea-detail-dialog.component.html',
  styleUrl: './tarea-detail-dialog.component.scss'
})
export class TareaDetailDialogComponent {
  constructor(@Inject(MAT_DIALOG_DATA) public readonly tarea: TareaDetalle) {}

  valor(texto?: string | number | null): string {
    return texto === null || texto === undefined || texto === '' ? '-' : String(texto);
  }

  mostrarEstado(estado: string): string {
    if (estado === 'EnCurso') return 'En curso';
    return estado;
  }
}
