using Kodvian.Core.Application.Common.Security;
using Kodvian.Core.Domain.Entities;
using Kodvian.Core.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Kodvian.Core.Infrastructure.Persistence;

public class KodvianDbContext : DbContext
{
    public static readonly Guid AdministratorRoleId = Guid.Parse("95e52dc4-44f5-4b0b-aabf-4044f28cc55a");
    public static readonly Guid OperativeRoleId = Guid.Parse("a77176f8-d33a-4c23-b613-f2e73093e1b7");
    public static readonly Guid ReadOnlyRoleId = Guid.Parse("24b2ab35-0c84-4fa8-9c35-eecf5f476bb8");

    public KodvianDbContext(DbContextOptions<KodvianDbContext> options) : base(options)
    {
    }

    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<TaskItem> Tasks => Set<TaskItem>();
    public DbSet<FinancialCategory> FinancialCategories => Set<FinancialCategory>();
    public DbSet<FinancialMovement> FinancialMovements => Set<FinancialMovement>();
    public DbSet<Provider> Providers => Set<Provider>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Client>(entity =>
        {
            entity.ToTable("Clients");
            entity.Property(x => x.CommercialName).IsRequired().HasMaxLength(200);
            entity.Property(x => x.LegalName).HasMaxLength(220);
            entity.Property(x => x.TaxId).HasMaxLength(20);
            entity.Property(x => x.ContactName).HasMaxLength(120);
            entity.Property(x => x.ContactEmail).HasMaxLength(120);
            entity.Property(x => x.ContactPhone).HasMaxLength(40);
            entity.Property(x => x.Address).HasMaxLength(180);
            entity.Property(x => x.City).HasMaxLength(120);
            entity.Property(x => x.Province).HasMaxLength(120);
            entity.Property(x => x.Country).HasMaxLength(120);
            entity.Property(x => x.Status).HasConversion<int>();
            entity.Property(x => x.ServiceType).HasMaxLength(120);
            entity.Property(x => x.MonthlyAmount).HasColumnType("numeric(18,2)");
            entity.Property(x => x.BillingDay);
            entity.Property(x => x.Notes).HasMaxLength(1000);
            entity.HasIndex(x => x.CommercialName);
            entity.HasIndex(x => x.Status);
        });

        modelBuilder.Entity<Project>(entity =>
        {
            entity.ToTable("Projects");
            entity.Property(x => x.Nombre).IsRequired().HasMaxLength(200);
            entity.Property(x => x.Descripcion).HasMaxLength(2000);
            entity.Property(x => x.Estado).HasConversion<int>();
            entity.Property(x => x.Prioridad).HasConversion<int>();
            entity.Property(x => x.Presupuesto).HasColumnType("numeric(18,2)");
            entity.Property(x => x.PorcentajeAvance).HasDefaultValue(0);
            entity.HasIndex(x => x.ClienteId);
            entity.HasIndex(x => x.Estado);
            entity.HasIndex(x => x.Prioridad);
            entity.HasIndex(x => new { x.ClienteId, x.FechaCreacion });
            entity.HasIndex(x => new { x.Estado, x.Prioridad, x.FechaCreacion });

            entity.HasOne(x => x.Cliente)
                .WithMany(x => x.Projects)
                .HasForeignKey(x => x.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Responsable)
                .WithMany(x => x.Projects)
                .HasForeignKey(x => x.ResponsableId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<TaskItem>(entity =>
        {
            entity.ToTable("Tasks");
            entity.Property(x => x.Titulo).IsRequired().HasMaxLength(200);
            entity.Property(x => x.Descripcion).HasMaxLength(2000);
            entity.Property(x => x.Estado).HasConversion<int>();
            entity.Property(x => x.Prioridad).HasConversion<int>();
            entity.Property(x => x.HorasEstimadas).HasColumnType("numeric(8,2)");
            entity.Property(x => x.HorasReales).HasColumnType("numeric(8,2)");
            entity.Property(x => x.OrdenKanban).HasDefaultValue(0);
            entity.HasIndex(x => x.ProyectoId);
            entity.HasIndex(x => x.ResponsableId);
            entity.HasIndex(x => x.CreadoPorId);
            entity.HasIndex(x => x.Estado);
            entity.HasIndex(x => x.Prioridad);
            entity.HasIndex(x => x.FechaVencimiento);
            entity.HasIndex(x => new { x.Estado, x.OrdenKanban, x.FechaCreacion });
            entity.HasIndex(x => new { x.ProyectoId, x.Estado });

            entity.HasOne(x => x.Proyecto)
                .WithMany(x => x.Tareas)
                .HasForeignKey(x => x.ProyectoId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Responsable)
                .WithMany(x => x.Tasks)
                .HasForeignKey(x => x.ResponsableId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(x => x.CreadoPor)
                .WithMany(x => x.CreatedTasks)
                .HasForeignKey(x => x.CreadoPorId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<FinancialCategory>(entity =>
        {
            entity.ToTable("FinancialCategories");
            entity.Property(x => x.Name).IsRequired().HasMaxLength(120);
            entity.Property(x => x.MovementType).HasConversion<int>();
            entity.HasIndex(x => x.Name);
            entity.HasIndex(x => x.MovementType);
        });

        modelBuilder.Entity<Provider>(entity =>
        {
            entity.ToTable("Providers");
            entity.Property(x => x.Name).IsRequired().HasMaxLength(160);
            entity.Property(x => x.TaxId).HasMaxLength(20);
            entity.Property(x => x.Email).HasMaxLength(120);
            entity.Property(x => x.Phone).HasMaxLength(40);
            entity.HasIndex(x => x.Name);
        });

        modelBuilder.Entity<FinancialMovement>(entity =>
        {
            entity.ToTable("FinancialMovements");
            entity.Property(x => x.MovementType).HasConversion<int>();
            entity.Property(x => x.Description).IsRequired().HasMaxLength(500);
            entity.Property(x => x.Amount).HasColumnType("numeric(18,2)");
            entity.Property(x => x.Status).HasConversion<int>();
            entity.Property(x => x.PaymentMethod).HasMaxLength(80);
            entity.Property(x => x.ReceiptNumber).HasMaxLength(80);
            entity.Property(x => x.Notes).HasMaxLength(1000);
            entity.HasIndex(x => x.MovementDate);
            entity.HasIndex(x => x.DueDate);
            entity.HasIndex(x => x.MovementType);
            entity.HasIndex(x => x.Status);
            entity.HasIndex(x => x.CategoryId);
            entity.HasIndex(x => x.ClientId);
            entity.HasIndex(x => x.ProviderId);
            entity.HasIndex(x => x.ProjectId);
            entity.HasIndex(x => new { x.MovementType, x.Status, x.DueDate });
            entity.HasIndex(x => new { x.MovementDate, x.FechaCreacion });

            entity.HasOne(x => x.Category)
                .WithMany(x => x.FinancialMovements)
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Client)
                .WithMany()
                .HasForeignKey(x => x.ClientId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(x => x.Provider)
                .WithMany(x => x.FinancialMovements)
                .HasForeignKey(x => x.ProviderId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(x => x.Project)
                .WithMany()
                .HasForeignKey(x => x.ProjectId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(x => x.CreatedBy)
                .WithMany(x => x.FinancialMovementsCreated)
                .HasForeignKey(x => x.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("Roles");
            entity.Property(x => x.Name).IsRequired().HasMaxLength(80);
            entity.Property(x => x.Description).HasMaxLength(200);
            entity.HasIndex(x => x.Name).IsUnique();

            entity.HasData(
                new Role
                {
                    Id = AdministratorRoleId,
                    Name = RoleNames.Administrator,
                    Description = "Acceso total al sistema",
                    FechaCreacion = new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc),
                    Activo = true
                },
                new Role
                {
                    Id = OperativeRoleId,
                    Name = RoleNames.Operative,
                    Description = "Acceso operativo a clientes, proyectos y tareas",
                    FechaCreacion = new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc),
                    Activo = true
                },
                new Role
                {
                    Id = ReadOnlyRoleId,
                    Name = RoleNames.ReadOnly,
                    Description = "Acceso de solo lectura",
                    FechaCreacion = new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc),
                    Activo = true
                }
            );
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.Property(x => x.FullName).IsRequired().HasMaxLength(160);
            entity.Property(x => x.Email).IsRequired().HasMaxLength(120);
            entity.Property(x => x.PasswordHash).IsRequired();
            entity.HasIndex(x => x.Email).IsUnique();

            entity.HasOne(x => x.Role)
                .WithMany(x => x.Users)
                .HasForeignKey(x => x.RoleId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
