import { CurrencyPipe } from '@angular/common';
import { Component, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatTableModule } from '@angular/material/table';

import { LedgerContrato } from '../../models/proyectos.models';

@Component({
  selector: 'app-contrato-ledger-dialog',
  standalone: true,
  imports: [MatDialogModule, MatTableModule, CurrencyPipe],
  templateUrl: './contrato-ledger-dialog.component.html',
  styleUrl: './contrato-ledger-dialog.component.scss'
})
export class ContratoLedgerDialogComponent {
  readonly columns = ['month', 'income', 'due', 'paid', 'balance'];

  constructor(@Inject(MAT_DIALOG_DATA) public readonly data: LedgerContrato) {}
}
