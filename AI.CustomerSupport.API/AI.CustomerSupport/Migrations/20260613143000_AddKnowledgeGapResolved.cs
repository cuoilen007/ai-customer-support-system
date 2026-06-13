using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AI.CustomerSupport.API.Migrations
{
    public partial class AddKnowledgeGapResolved : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF OBJECT_ID(N'[ChatEvaluations]', N'U') IS NOT NULL
                   AND COL_LENGTH('ChatEvaluations', 'KnowledgeGapResolved') IS NULL
                BEGIN
                    ALTER TABLE [ChatEvaluations]
                    ADD [KnowledgeGapResolved] BIT NOT NULL CONSTRAINT [DF_ChatEvaluations_KnowledgeGapResolved] DEFAULT 0;
                END
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF OBJECT_ID(N'[ChatEvaluations]', N'U') IS NOT NULL
                   AND COL_LENGTH('ChatEvaluations', 'KnowledgeGapResolved') IS NOT NULL
                BEGIN
                    ALTER TABLE [ChatEvaluations]
                    DROP COLUMN [KnowledgeGapResolved];
                END
                """);
        }
    }
}
