using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace OI.Web.Services.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "hangfire");

            migrationBuilder.CreateTable(
                name: "OIJobs",
                columns: table => new
                {
                    JobId = table.Column<int>(type: "integer", nullable: false),
                    OriginalString = table.Column<string>(type: "text", nullable: false),
                    EncodedString = table.Column<string>(type: "text", nullable: false),
                    ReturnedData = table.Column<string>(type: "text", nullable: false),
                    CreatedDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    JobStateId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("JobState_pkey", x => x.JobId);
                });

            migrationBuilder.CreateTable(
                name: "OIJobStates",
                columns: table => new
                {
                    JobStateId = table.Column<int>(type: "integer", nullable: false),
                    JobStateName = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("JobStates_pkey", x => x.JobStateId);
                });

            migrationBuilder.Sql("Insert into \"OIJobStates\" (\"JobStateId\", \"JobStateName\") Select 0, 'Ready'");
            migrationBuilder.Sql("Insert into \"OIJobStates\" (\"JobStateId\", \"JobStateName\") Select 1, 'Running'");
            migrationBuilder.Sql("Insert into \"OIJobStates\" (\"JobStateId\", \"JobStateName\") Select 2, 'Complete'");
            migrationBuilder.Sql("Insert into \"OIJobStates\" (\"JobStateId\", \"JobStateName\") Select 3, 'Cancelled'");
            migrationBuilder.Sql("Insert into \"OIJobStates\" (\"JobStateId\", \"JobStateName\") Select 4, 'Error'");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "aggregatedcounter",
                schema: "hangfire");

            migrationBuilder.DropTable(
                name: "counter",
                schema: "hangfire");

            migrationBuilder.DropTable(
                name: "hash",
                schema: "hangfire");

            migrationBuilder.DropTable(
                name: "jobparameter",
                schema: "hangfire");

            migrationBuilder.DropTable(
                name: "jobqueue",
                schema: "hangfire");

            migrationBuilder.DropTable(
                name: "list",
                schema: "hangfire");

            migrationBuilder.DropTable(
                name: "lock",
                schema: "hangfire");

            migrationBuilder.DropTable(
                name: "OIJobs");

            migrationBuilder.DropTable(
                name: "OIJobStates");

            migrationBuilder.DropTable(
                name: "schema",
                schema: "hangfire");

            migrationBuilder.DropTable(
                name: "server",
                schema: "hangfire");

            migrationBuilder.DropTable(
                name: "set",
                schema: "hangfire");

            migrationBuilder.DropTable(
                name: "state",
                schema: "hangfire");

            migrationBuilder.DropTable(
                name: "job",
                schema: "hangfire");
        }
    }
}
