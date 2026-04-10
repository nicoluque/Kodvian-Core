using Kodvian.Core.Domain.Enums;
using DomainTaskStatus = Kodvian.Core.Domain.Enums.TaskStatus;

namespace Kodvian.Core.Domain.Entities;

public class TaskItem : BaseEntity
{
    public string Titulo { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public Guid ProyectoId { get; set; }
    public Guid? ResponsableId { get; set; }
    public Guid CreadoPorId { get; set; }
    public DomainTaskStatus Estado { get; set; } = DomainTaskStatus.Pendiente;
    public TaskPriority Prioridad { get; set; } = TaskPriority.Media;
    public DateOnly? FechaInicio { get; set; }
    public DateOnly? FechaVencimiento { get; set; }
    public DateOnly? FechaFinalizacion { get; set; }
    public decimal? HorasEstimadas { get; set; }
    public decimal? HorasReales { get; set; }
    public int OrdenKanban { get; set; }

    public Project? Proyecto { get; set; }
    public User? Responsable { get; set; }
    public User? CreadoPor { get; set; }
}
