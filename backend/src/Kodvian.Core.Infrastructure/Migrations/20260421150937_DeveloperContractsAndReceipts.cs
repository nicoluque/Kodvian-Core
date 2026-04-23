using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kodvian.Core.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DeveloperContractsAndReceipts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Developers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FullName = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    Email = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    Phone = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    TaxId = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Developers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProjectDeveloperContracts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    DeveloperId = table.Column<Guid>(type: "uuid", nullable: false),
                    PaymentMode = table.Column<int>(type: "integer", nullable: false),
                    Percentage = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    AgreedAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectDeveloperContracts", x => x.Id);
                    table.CheckConstraint("CK_ProjectDeveloperContracts_FixedAmount", "\"AgreedAmount\" IS NULL OR \"AgreedAmount\" > 0");
                    table.CheckConstraint("CK_ProjectDeveloperContracts_Percentage", "\"Percentage\" IS NULL OR (\"Percentage\" >= 0 AND \"Percentage\" <= 100)");
                    table.ForeignKey(
                        name: "FK_ProjectDeveloperContracts_Developers_DeveloperId",
                        column: x => x.DeveloperId,
                        principalTable: "Developers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProjectDeveloperContracts_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DeveloperPayments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ContractId = table.Column<Guid>(type: "uuid", nullable: false),
                    PaymentDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    PeriodYear = table.Column<int>(type: "integer", nullable: false),
                    PeriodMonth = table.Column<int>(type: "integer", nullable: false),
                    Reference = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeveloperPayments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeveloperPayments_ProjectDeveloperContracts_ContractId",
                        column: x => x.ContractId,
                        principalTable: "ProjectDeveloperContracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DocumentFiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FinancialMovementId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeveloperPaymentId = table.Column<Guid>(type: "uuid", nullable: true),
                    UploadedById = table.Column<Guid>(type: "uuid", nullable: false),
                    OriginalFileName = table.Column<string>(type: "character varying(260)", maxLength: 260, nullable: false),
                    StoredFileName = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    StoragePath = table.Column<string>(type: "character varying(260)", maxLength: 260, nullable: false),
                    Sha256 = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentFiles", x => x.Id);
                    table.CheckConstraint("CK_DocumentFiles_Owner", "(\"FinancialMovementId\" IS NOT NULL AND \"DeveloperPaymentId\" IS NULL) OR (\"FinancialMovementId\" IS NULL AND \"DeveloperPaymentId\" IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_DocumentFiles_DeveloperPayments_DeveloperPaymentId",
                        column: x => x.DeveloperPaymentId,
                        principalTable: "DeveloperPayments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DocumentFiles_FinancialMovements_FinancialMovementId",
                        column: x => x.FinancialMovementId,
                        principalTable: "FinancialMovements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DocumentFiles_Users_UploadedById",
                        column: x => x.UploadedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DeveloperPayments_ContractId",
                table: "DeveloperPayments",
                column: "ContractId");

            migrationBuilder.CreateIndex(
                name: "IX_DeveloperPayments_ContractId_PeriodYear_PeriodMonth",
                table: "DeveloperPayments",
                columns: new[] { "ContractId", "PeriodYear", "PeriodMonth" });

            migrationBuilder.CreateIndex(
                name: "IX_DeveloperPayments_PaymentDate",
                table: "DeveloperPayments",
                column: "PaymentDate");

            migrationBuilder.CreateIndex(
                name: "IX_Developers_Email",
                table: "Developers",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_Developers_FullName",
                table: "Developers",
                column: "FullName");

            migrationBuilder.CreateIndex(
                name: "IX_Developers_TaxId",
                table: "Developers",
                column: "TaxId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentFiles_DeveloperPaymentId",
                table: "DocumentFiles",
                column: "DeveloperPaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentFiles_FinancialMovementId",
                table: "DocumentFiles",
                column: "FinancialMovementId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentFiles_Sha256",
                table: "DocumentFiles",
                column: "Sha256");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentFiles_UploadedById",
                table: "DocumentFiles",
                column: "UploadedById");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectDeveloperContracts_DeveloperId",
                table: "ProjectDeveloperContracts",
                column: "DeveloperId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectDeveloperContracts_DeveloperId_FechaCreacion",
                table: "ProjectDeveloperContracts",
                columns: new[] { "DeveloperId", "FechaCreacion" });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectDeveloperContracts_ProjectId",
                table: "ProjectDeveloperContracts",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectDeveloperContracts_ProjectId_DeveloperId_Activo",
                table: "ProjectDeveloperContracts",
                columns: new[] { "ProjectId", "DeveloperId", "Activo" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DocumentFiles");

            migrationBuilder.DropTable(
                name: "DeveloperPayments");

            migrationBuilder.DropTable(
                name: "ProjectDeveloperContracts");

            migrationBuilder.DropTable(
                name: "Developers");
        }
    }
}
