using Kodvian.Core.Domain.Enums;

namespace Kodvian.Core.Domain.Entities;

public class Project : BaseEntity
{
    public Guid ClienteId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public Guid? ResponsableId { get; set; }
    public ProjectStatus Estado { get; set; } = ProjectStatus.Planificacion;
    public ProjectPriority Prioridad { get; set; } = ProjectPriority.Media;
    public DateOnly? FechaInicio { get; set; }
    public DateOnly? FechaEntregaEstimada { get; set; }
    public DateOnly? FechaCierre { get; set; }
    public decimal? Presupuesto { get; set; }
    public int PorcentajeAvance { get; set; }

    public Client? Cliente { get; set; }
    public User? Responsable { get; set; }
    public ICollection<TaskItem> Tareas { get; set; } = new List<TaskItem>();
}
