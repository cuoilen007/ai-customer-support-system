using Microsoft.EntityFrameworkCore;

namespace AI.CustomerSupport.API.Data
{
    public static class DatabaseSchemaRepairer
    {
        public static async Task RepairAsync(AppDbContext context)
        {
            await context.Database.ExecuteSqlRawAsync(
                """
                IF OBJECT_ID(N'[ChatEvaluations]', N'U') IS NULL
                BEGIN
                    CREATE TABLE [ChatEvaluations] (
                        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                        [ConversationId] INT NOT NULL,
                        [UserMessageId] INT NOT NULL,
                        [AssistantMessageId] INT NOT NULL,
                        [Category] NVARCHAR(MAX) NOT NULL,
                        [Sentiment] NVARCHAR(MAX) NOT NULL,
                        [Intent] NVARCHAR(MAX) NOT NULL,
                        [ConfidenceScore] INT NOT NULL,
                        [NeedsHumanReview] BIT NOT NULL,
                        [RetrievedContext] NVARCHAR(MAX) NOT NULL,
                        [PrimarySourceId] NVARCHAR(MAX) NOT NULL CONSTRAINT [DF_ChatEvaluations_PrimarySourceId_StartupRepair] DEFAULT N'',
                        [PrimarySourceType] NVARCHAR(MAX) NOT NULL CONSTRAINT [DF_ChatEvaluations_PrimarySourceType_StartupRepair] DEFAULT N'',
                        [RetrievedSourcesJson] NVARCHAR(MAX) NOT NULL CONSTRAINT [DF_ChatEvaluations_RetrievedSourcesJson_StartupRepair] DEFAULT N'[]',
                        [ImprovementNote] NVARCHAR(MAX) NOT NULL,
                        [ApprovedForTraining] BIT NOT NULL CONSTRAINT [DF_ChatEvaluations_ApprovedForTraining_StartupRepair_Create] DEFAULT 0,
                        [KnowledgeGap] BIT NOT NULL CONSTRAINT [DF_ChatEvaluations_KnowledgeGap_StartupRepair_Create] DEFAULT 0,
                        [KnowledgeGapResolved] BIT NOT NULL CONSTRAINT [DF_ChatEvaluations_KnowledgeGapResolved_StartupRepair_Create] DEFAULT 0,
                        [HumanCorrectedAnswer] NVARCHAR(MAX) NOT NULL CONSTRAINT [DF_ChatEvaluations_HumanCorrectedAnswer_StartupRepair_Create] DEFAULT N'',
                        [IsDeleted] BIT NOT NULL CONSTRAINT [DF_ChatEvaluations_IsDeleted_StartupRepair_Create] DEFAULT 0,
                        [CreatedAt] DATETIME2 NOT NULL,
                        CONSTRAINT [FK_ChatEvaluations_Conversations_ConversationId]
                            FOREIGN KEY ([ConversationId]) REFERENCES [Conversations]([Id]) ON DELETE CASCADE,
                        CONSTRAINT [FK_ChatEvaluations_Messages_UserMessageId]
                            FOREIGN KEY ([UserMessageId]) REFERENCES [Messages]([Id]),
                        CONSTRAINT [FK_ChatEvaluations_Messages_AssistantMessageId]
                            FOREIGN KEY ([AssistantMessageId]) REFERENCES [Messages]([Id])
                    );
                END
                """);

            await context.Database.ExecuteSqlRawAsync(
                """
                IF OBJECT_ID(N'[ChatEvaluations]', N'U') IS NOT NULL
                   AND COL_LENGTH('ChatEvaluations', 'PrimarySourceId') IS NULL
                BEGIN
                    ALTER TABLE [ChatEvaluations]
                    ADD [PrimarySourceId] NVARCHAR(MAX) NOT NULL CONSTRAINT [DF_ChatEvaluations_PrimarySourceId_StartupRepair_Add] DEFAULT N'';
                END
                """);

            await context.Database.ExecuteSqlRawAsync(
                """
                IF OBJECT_ID(N'[ChatEvaluations]', N'U') IS NOT NULL
                   AND COL_LENGTH('ChatEvaluations', 'PrimarySourceType') IS NULL
                BEGIN
                    ALTER TABLE [ChatEvaluations]
                    ADD [PrimarySourceType] NVARCHAR(MAX) NOT NULL CONSTRAINT [DF_ChatEvaluations_PrimarySourceType_StartupRepair_Add] DEFAULT N'';
                END
                """);

            await context.Database.ExecuteSqlRawAsync(
                """
                IF OBJECT_ID(N'[ChatEvaluations]', N'U') IS NOT NULL
                   AND COL_LENGTH('ChatEvaluations', 'RetrievedSourcesJson') IS NULL
                BEGIN
                    ALTER TABLE [ChatEvaluations]
                    ADD [RetrievedSourcesJson] NVARCHAR(MAX) NOT NULL CONSTRAINT [DF_ChatEvaluations_RetrievedSourcesJson_StartupRepair_Add] DEFAULT N'[]';
                END
                """);

            await context.Database.ExecuteSqlRawAsync(
                """
                IF OBJECT_ID(N'[ChatEvaluations]', N'U') IS NOT NULL
                   AND COL_LENGTH('ChatEvaluations', 'ApprovedForTraining') IS NULL
                BEGIN
                    ALTER TABLE [ChatEvaluations]
                    ADD [ApprovedForTraining] BIT NOT NULL CONSTRAINT [DF_ChatEvaluations_ApprovedForTraining_StartupRepair] DEFAULT 0;
                END
                """);

            await context.Database.ExecuteSqlRawAsync(
                """
                IF OBJECT_ID(N'[ChatEvaluations]', N'U') IS NOT NULL
                   AND COL_LENGTH('ChatEvaluations', 'HumanCorrectedAnswer') IS NULL
                BEGIN
                    ALTER TABLE [ChatEvaluations]
                    ADD [HumanCorrectedAnswer] NVARCHAR(MAX) NOT NULL CONSTRAINT [DF_ChatEvaluations_HumanCorrectedAnswer_StartupRepair] DEFAULT N'';
                END
                """);

            await context.Database.ExecuteSqlRawAsync(
                """
                IF OBJECT_ID(N'[ChatEvaluations]', N'U') IS NOT NULL
                   AND COL_LENGTH('ChatEvaluations', 'KnowledgeGap') IS NULL
                BEGIN
                    ALTER TABLE [ChatEvaluations]
                    ADD [KnowledgeGap] BIT NOT NULL CONSTRAINT [DF_ChatEvaluations_KnowledgeGap_StartupRepair] DEFAULT 0;
                END
                """);

            await context.Database.ExecuteSqlRawAsync(
                """
                IF OBJECT_ID(N'[ChatEvaluations]', N'U') IS NOT NULL
                   AND COL_LENGTH('ChatEvaluations', 'KnowledgeGapResolved') IS NULL
                BEGIN
                    ALTER TABLE [ChatEvaluations]
                    ADD [KnowledgeGapResolved] BIT NOT NULL CONSTRAINT [DF_ChatEvaluations_KnowledgeGapResolved_StartupRepair] DEFAULT 0;
                END
                """);

            await context.Database.ExecuteSqlRawAsync(
                """
                IF OBJECT_ID(N'[ChatEvaluations]', N'U') IS NOT NULL
                   AND COL_LENGTH('ChatEvaluations', 'IsDeleted') IS NULL
                BEGIN
                    ALTER TABLE [ChatEvaluations]
                    ADD [IsDeleted] BIT NOT NULL CONSTRAINT [DF_ChatEvaluations_IsDeleted_StartupRepair] DEFAULT 0;
                END
                """);

            await context.Database.ExecuteSqlRawAsync(
                """
                IF OBJECT_ID(N'[ModelTrainingExamples]', N'U') IS NULL
                BEGIN
                    CREATE TABLE [ModelTrainingExamples] (
                        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                        [ChatEvaluationId] INT NULL,
                        [Input] NVARCHAR(MAX) NOT NULL,
                        [ExpectedOutput] NVARCHAR(MAX) NOT NULL,
                        [OriginalAnswer] NVARCHAR(MAX) NOT NULL,
                        [Category] NVARCHAR(MAX) NOT NULL,
                        [Intent] NVARCHAR(MAX) NOT NULL,
                        [PrimarySourceId] NVARCHAR(MAX) NOT NULL,
                        [PrimarySourceType] NVARCHAR(MAX) NOT NULL,
                        [Status] NVARCHAR(MAX) NOT NULL,
                        [IsActive] BIT NOT NULL CONSTRAINT [DF_ModelTrainingExamples_IsActive_StartupRepair] DEFAULT 1,
                        [Source] NVARCHAR(MAX) NOT NULL CONSTRAINT [DF_ModelTrainingExamples_Source_StartupRepair] DEFAULT N'ReviewWorkspace',
                        [SourceReference] NVARCHAR(MAX) NOT NULL CONSTRAINT [DF_ModelTrainingExamples_SourceReference_StartupRepair] DEFAULT N'',
                        [ImportedAt] DATETIME2 NULL,
                        [CreatedAt] DATETIME2 NOT NULL,
                        [UpdatedAt] DATETIME2 NOT NULL,
                        CONSTRAINT [FK_ModelTrainingExamples_ChatEvaluations_ChatEvaluationId]
                            FOREIGN KEY ([ChatEvaluationId]) REFERENCES [ChatEvaluations]([Id]) ON DELETE CASCADE
                    );
                END
                """);

            await context.Database.ExecuteSqlRawAsync(
                """
                IF OBJECT_ID(N'[ModelTrainingExamples]', N'U') IS NOT NULL
                   AND COL_LENGTH('ModelTrainingExamples', 'Source') IS NULL
                BEGIN
                    ALTER TABLE [ModelTrainingExamples]
                    ADD [Source] NVARCHAR(MAX) NOT NULL CONSTRAINT [DF_ModelTrainingExamples_Source_StartupRepair_Add] DEFAULT N'ReviewWorkspace';
                END
                """);

            await context.Database.ExecuteSqlRawAsync(
                """
                IF OBJECT_ID(N'[ModelTrainingExamples]', N'U') IS NOT NULL
                   AND COL_LENGTH('ModelTrainingExamples', 'SourceReference') IS NULL
                BEGIN
                    ALTER TABLE [ModelTrainingExamples]
                    ADD [SourceReference] NVARCHAR(MAX) NOT NULL CONSTRAINT [DF_ModelTrainingExamples_SourceReference_StartupRepair_Add] DEFAULT N'';
                END
                """);

            await context.Database.ExecuteSqlRawAsync(
                """
                IF OBJECT_ID(N'[ModelTrainingExamples]', N'U') IS NOT NULL
                   AND COL_LENGTH('ModelTrainingExamples', 'ImportedAt') IS NULL
                BEGIN
                    ALTER TABLE [ModelTrainingExamples]
                    ADD [ImportedAt] DATETIME2 NULL;
                END
                """);

            await context.Database.ExecuteSqlRawAsync(
                """
                IF OBJECT_ID(N'[ModelTrainingExamples]', N'U') IS NOT NULL
                   AND EXISTS (
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
                """);

            await context.Database.ExecuteSqlRawAsync(
                """
                IF OBJECT_ID(N'[ModelTrainingExamples]', N'U') IS NOT NULL
                   AND EXISTS (
                       SELECT 1
                       FROM sys.indexes
                       WHERE name = 'IX_ModelTrainingExamples_ChatEvaluationId'
                         AND object_id = OBJECT_ID(N'[ModelTrainingExamples]')
                   )
                BEGIN
                    DROP INDEX [IX_ModelTrainingExamples_ChatEvaluationId]
                    ON [ModelTrainingExamples];
                END
                """);

            await context.Database.ExecuteSqlRawAsync(
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
                    ON [ModelTrainingExamples]([ChatEvaluationId])
                    WHERE [ChatEvaluationId] IS NOT NULL;
                END
                """);

            await context.Database.ExecuteSqlRawAsync(
                """
                IF OBJECT_ID(N'[TrainingRuns]', N'U') IS NULL
                BEGIN
                    CREATE TABLE [TrainingRuns] (
                        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                        [UserId] INT NULL,
                        [Status] NVARCHAR(MAX) NOT NULL,
                        [Message] NVARCHAR(MAX) NOT NULL,
                        [ReviewedExampleCount] INT NOT NULL,
                        [DatasetSize] INT NOT NULL,
                        [ClassCount] INT NOT NULL,
                        [BestModelName] NVARCHAR(MAX) NOT NULL,
                        [Accuracy] FLOAT NOT NULL,
                        [ModelVersion] INT NOT NULL,
                        [Error] NVARCHAR(MAX) NOT NULL,
                        [StartedAt] DATETIMEOFFSET NULL,
                        [CompletedAt] DATETIMEOFFSET NULL,
                        [CreatedAt] DATETIME2 NOT NULL,
                        [UpdatedAt] DATETIME2 NOT NULL
                    );
                END
                """);
        }
    }
}
