import { MatPaginatorIntl } from '@angular/material/paginator';

export function createMatPaginatorEs(): MatPaginatorIntl {
  const paginator = new MatPaginatorIntl();

  paginator.itemsPerPageLabel = 'Elementos por pagina';
  paginator.nextPageLabel = 'Pagina siguiente';
  paginator.previousPageLabel = 'Pagina anterior';
  paginator.firstPageLabel = 'Primera pagina';
  paginator.lastPageLabel = 'Ultima pagina';
  paginator.getRangeLabel = (page: number, pageSize: number, length: number): string => {
    if (length === 0 || pageSize === 0) {
      return `0 de ${length}`;
    }

    const start = page * pageSize + 1;
    const end = Math.min(start + pageSize - 1, length);
    return `${start}-${end} de ${length}`;
  };

  return paginator;
}
