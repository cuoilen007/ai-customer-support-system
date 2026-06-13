using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AI.CustomerSupport.API.Migrations
{
    public partial class AddChatEvaluationFeedbackFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF OBJECT_ID(N'[ChatEvaluations]', N'U') IS NOT NULL
                   AND COL_LENGTH('ChatEvaluations', 'ApprovedForTraining') IS NULL
                BEGIN
                    ALTER TABLE [ChatEvaluations]
                    ADD [ApprovedForTraining] BIT NOT NULL CONSTRAINT [DF_ChatEvaluations_ApprovedForTraining] DEFAULT 0;
                END
                """);

            migrationBuilder.Sql(
                """
                IF OBJECT_ID(N'[ChatEvaluations]', N'U') IS NOT NULL
                   AND COL_LENGTH('ChatEvaluations', 'HumanCorrectedAnswer') IS NULL
                BEGIN
                    ALTER TABLE [ChatEvaluations]
                    ADD [HumanCorrectedAnswer] NVARCHAR(MAX) NOT NULL CONSTRAINT [DF_ChatEvaluations_HumanCorrectedAnswer] DEFAULT N'';
                END
                """);

            migrationBuilder.Sql(
                """
                IF OBJECT_ID(N'[ChatEvaluations]', N'U') IS NOT NULL
                   AND COL_LENGTH('ChatEvaluations', 'KnowledgeGap') IS NULL
                BEGIN
                    ALTER TABLE [ChatEvaluations]
                    ADD [KnowledgeGap] BIT NOT NULL CONSTRAINT [DF_ChatEvaluations_KnowledgeGap] DEFAULT 0;
                END
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF COL_LENGTH('ChatEvaluations', 'ApprovedForTraining') IS NOT NULL
                BEGIN
                    ALTER TABLE [ChatEvaluations] DROP COLUMN [ApprovedForTraining];
                END
                """);

            migrationBuilder.Sql(
                """
                IF COL_LENGTH('ChatEvaluations', 'HumanCorrectedAnswer') IS NOT NULL
                BEGIN
                    ALTER TABLE [ChatEvaluations] DROP COLUMN [HumanCorrectedAnswer];
                END
                """);

            migrationBuilder.Sql(
                """
                IF COL_LENGTH('ChatEvaluations', 'KnowledgeGap') IS NOT NULL
                BEGIN
                    ALTER TABLE [ChatEvaluations] DROP COLUMN [KnowledgeGap];
                END
                """);
        }
    }
}
