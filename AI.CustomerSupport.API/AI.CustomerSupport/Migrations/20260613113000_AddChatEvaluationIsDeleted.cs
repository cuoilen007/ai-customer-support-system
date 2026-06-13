using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AI.CustomerSupport.API.Migrations
{
    public partial class AddChatEvaluationIsDeleted : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF OBJECT_ID(N'[ChatEvaluations]', N'U') IS NOT NULL
                   AND COL_LENGTH('ChatEvaluations', 'IsDeleted') IS NULL
                BEGIN
                    ALTER TABLE [ChatEvaluations]
                    ADD [IsDeleted] BIT NOT NULL CONSTRAINT [DF_ChatEvaluations_IsDeleted] DEFAULT 0;
                END
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF COL_LENGTH('ChatEvaluations', 'IsDeleted') IS NOT NULL
                BEGIN
                    ALTER TABLE [ChatEvaluations] DROP COLUMN [IsDeleted];
                END
                """);
        }
    }
}
