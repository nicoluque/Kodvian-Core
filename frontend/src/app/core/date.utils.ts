export function parseIsoDate(value: string | null | undefined): Date | null {
  if (!value) {
    return null;
  }

  const parts = value.split('-');
  if (parts.length !== 3) {
    return null;
  }

  const year = Number(parts[0]);
  const month = Number(parts[1]);
  const day = Number(parts[2]);

  if (!Number.isInteger(year) || !Number.isInteger(month) || !Number.isInteger(day)) {
    return null;
  }

  if (month < 1 || month > 12 || day < 1 || day > 31) {
    return null;
  }

  const date = new Date(year, month - 1, day);
  if (Number.isNaN(date.getTime())) {
    return null;
  }

  return date;
}

export function formatDateToIso(value: Date | string | null | undefined): string | null {
  if (!value) {
    return null;
  }

  if (typeof value === 'string') {
    return value || null;
  }

  if (Number.isNaN(value.getTime())) {
    return null;
  }

  const year = value.getFullYear();
  const month = `${value.getMonth() + 1}`.padStart(2, '0');
  const day = `${value.getDate()}`.padStart(2, '0');

  return `${year}-${month}-${day}`;
}

export function compareDates(left: Date | null | undefined, right: Date | null | undefined): number {
  if (!left || !right) {
    return 0;
  }

  const leftValue = new Date(left.getFullYear(), left.getMonth(), left.getDate()).getTime();
  const rightValue = new Date(right.getFullYear(), right.getMonth(), right.getDate()).getTime();

  if (leftValue === rightValue) {
    return 0;
  }

  return leftValue < rightValue ? -1 : 1;
}
