using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AI.CustomerSupport.API.Migrations
{
    public partial class ImportLegacyCsvIntoTrainingExamples : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF OBJECT_ID(N'[ModelTrainingExamples]', N'U') IS NOT NULL
                BEGIN
                    IF COL_LENGTH('ModelTrainingExamples', 'Source') IS NULL
                    BEGIN
                        ALTER TABLE [ModelTrainingExamples]
                        ADD [Source] NVARCHAR(MAX) NOT NULL CONSTRAINT [DF_ModelTrainingExamples_Source] DEFAULT N'ReviewWorkspace';
                    END

                    IF COL_LENGTH('ModelTrainingExamples', 'SourceReference') IS NULL
                    BEGIN
                        ALTER TABLE [ModelTrainingExamples]
                        ADD [SourceReference] NVARCHAR(MAX) NOT NULL CONSTRAINT [DF_ModelTrainingExamples_SourceReference] DEFAULT N'';
                    END

                    IF COL_LENGTH('ModelTrainingExamples', 'ImportedAt') IS NULL
                    BEGIN
                        ALTER TABLE [ModelTrainingExamples]
                        ADD [ImportedAt] DATETIME2 NULL;
                    END

                    IF EXISTS (
                        SELECT 1
                        FROM sys.columns
                        WHERE object_id = OBJECT_ID(N'[ModelTrainingExamples]')
                          AND name = 'ChatEvaluationId'
                          AND is_nullable = 0
                    )
                    BEGIN
                        ALTER TABLE [ModelTrainingExamples]
                        ALTER COLUMN [ChatEvaluationId] INT NULL;
                    END

                    IF EXISTS (
                        SELECT 1
                        FROM sys.indexes
                        WHERE name = 'IX_ModelTrainingExamples_ChatEvaluationId'
                          AND object_id = OBJECT_ID(N'[ModelTrainingExamples]')
                    )
                    BEGIN
                        DROP INDEX [IX_ModelTrainingExamples_ChatEvaluationId]
                        ON [ModelTrainingExamples];
                    END

                    CREATE UNIQUE INDEX [IX_ModelTrainingExamples_ChatEvaluationId]
                    ON [ModelTrainingExamples]([ChatEvaluationId])
                    WHERE [ChatEvaluationId] IS NOT NULL;
                END
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF OBJECT_ID(N'[ModelTrainingExamples]', N'U') IS NOT NULL
                BEGIN
                    IF EXISTS (
                        SELECT 1
                        FROM sys.indexes
                        WHERE name = 'IX_ModelTrainingExamples_ChatEvaluationId'
                          AND object_id = OBJECT_ID(N'[ModelTrainingExamples]')
                    )
                    BEGIN
                        DROP INDEX [IX_ModelTrainingExamples_ChatEvaluationId]
                        ON [ModelTrainingExamples];
                    END

                    IF COL_LENGTH('ModelTrainingExamples', 'ChatEvaluationId') IS NOT NULL
                    BEGIN
                        DELETE FROM [ModelTrainingExamples]
                        WHERE [ChatEvaluationId] IS NULL;

                        ALTER TABLE [ModelTrainingExamples]
                        ALTER COLUMN [ChatEvaluationId] INT NOT NULL;
                    END

                    CREATE UNIQUE INDEX [IX_ModelTrainingExamples_ChatEvaluationId]
                    ON [ModelTrainingExamples]([ChatEvaluationId]);

                    IF COL_LENGTH('ModelTrainingExamples', 'ImportedAt') IS NOT NULL
                    BEGIN
                        ALTER TABLE [ModelTrainingExamples]
                        DROP COLUMN [ImportedAt];
                    END

                    IF COL_LENGTH('ModelTrainingExamples', 'SourceReference') IS NOT NULL
                    BEGIN
                        ALTER TABLE [ModelTrainingExamples]
                        DROP CONSTRAINT [DF_ModelTrainingExamples_SourceReference];
                        ALTER TABLE [ModelTrainingExamples]
                        DROP COLUMN [SourceReference];
                    END

                    IF COL_LENGTH('ModelTrainingExamples', 'Source') IS NOT NULL
                    BEGIN
                        ALTER TABLE [ModelTrainingExamples]
                        DROP CONSTRAINT [DF_ModelTrainingExamples_Source];
                        ALTER TABLE [ModelTrainingExamples]
                        DROP COLUMN [Source];
                    END
                END
                """);
        }
    }
}
