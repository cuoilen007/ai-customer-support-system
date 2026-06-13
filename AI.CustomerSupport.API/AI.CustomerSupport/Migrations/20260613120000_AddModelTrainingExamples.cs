using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AI.CustomerSupport.API.Migrations
{
    public partial class AddModelTrainingExamples : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF OBJECT_ID(N'[ModelTrainingExamples]', N'U') IS NULL
                BEGIN
                    CREATE TABLE [ModelTrainingExamples] (
                        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                        [ChatEvaluationId] INT NOT NULL,
                        [Input] NVARCHAR(MAX) NOT NULL,
                        [ExpectedOutput] NVARCHAR(MAX) NOT NULL,
                        [OriginalAnswer] NVARCHAR(MAX) NOT NULL,
                        [Category] NVARCHAR(MAX) NOT NULL,
                        [Intent] NVARCHAR(MAX) NOT NULL,
                        [PrimarySourceId] NVARCHAR(MAX) NOT NULL,
                        [PrimarySourceType] NVARCHAR(MAX) NOT NULL,
                        [Status] NVARCHAR(MAX) NOT NULL,
                        [IsActive] BIT NOT NULL CONSTRAINT [DF_ModelTrainingExamples_IsActive] DEFAULT 1,
                        [CreatedAt] DATETIME2 NOT NULL,
                        [UpdatedAt] DATETIME2 NOT NULL,
                        CONSTRAINT [FK_ModelTrainingExamples_ChatEvaluations_ChatEvaluationId]
                            FOREIGN KEY ([ChatEvaluationId]) REFERENCES [ChatEvaluations]([Id]) ON DELETE CASCADE
                    );
                END
                """);

            migrationBuilder.Sql(
                """
                IF OBJECT_ID(N'[ModelTrainingExamples]', N'U') IS NOT NULL
                   AND NOT EXISTS (
                       SELECT 1
                       FROM sys.indexes
                       WHERE name = 'IX_ModelTrainingExamples_ChatEvaluationId'
                         AND object_id = OBJECT_ID(N'[ModelTrainingExamples]')
                   )
                BEGIN
                    CREATE UNIQUE INDEX [IX_ModelTrainingExamples_ChatEvaluationId]
                    ON [ModelTrainingExamples]([ChatEvaluationId]);
                END
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF OBJECT_ID(N'[ModelTrainingExamples]', N'U') IS NOT NULL
                BEGIN
                    DROP TABLE [ModelTrainingExamples];
                END
                """);
        }
    }
}
