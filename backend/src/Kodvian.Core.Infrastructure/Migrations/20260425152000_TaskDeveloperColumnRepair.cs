using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kodvian.Core.Infrastructure.Migrations
{
    public partial class TaskDeveloperColumnRepair : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE \"Tasks\" ADD COLUMN IF NOT EXISTS \"DeveloperId\" uuid NULL;");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS \"IX_Tasks_DeveloperId\" ON \"Tasks\" (\"DeveloperId\");");
            migrationBuilder.Sql(@"
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM pg_constraint
        WHERE conname = 'FK_Tasks_Developers_DeveloperId') THEN
        ALTER TABLE ""Tasks""
            ADD CONSTRAINT ""FK_Tasks_Developers_DeveloperId""
            FOREIGN KEY (""DeveloperId"")
            REFERENCES ""Developers"" (""Id"")
            ON DELETE SET NULL;
    END IF;
END
$$;");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE \"Tasks\" DROP CONSTRAINT IF EXISTS \"FK_Tasks_Developers_DeveloperId\";");
            migrationBuilder.Sql("DROP INDEX IF EXISTS \"IX_Tasks_DeveloperId\";");
            migrationBuilder.Sql("ALTER TABLE \"Tasks\" DROP COLUMN IF EXISTS \"DeveloperId\";");
        }
    }
}
