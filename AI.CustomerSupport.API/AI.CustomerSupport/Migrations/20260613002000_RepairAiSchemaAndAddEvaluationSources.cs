using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AI.CustomerSupport.API.Migrations
{
    public partial class RepairAiSchemaAndAddEvaluationSources : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF OBJECT_ID(N'[Products]', N'U') IS NULL
                BEGIN
                    CREATE TABLE [Products] (
                        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                        [Name] NVARCHAR(MAX) NOT NULL,
                        [Description] NVARCHAR(MAX) NOT NULL,
                        [Category] NVARCHAR(MAX) NOT NULL,
                        [Price] DECIMAL(18,2) NOT NULL,
                        [Status] NVARCHAR(MAX) NOT NULL,
                        [CreatedAt] DATETIME2 NOT NULL
                    );
                END
                """
            );

            migrationBuilder.Sql(
                """
                IF OBJECT_ID(N'[SupportPolicies]', N'U') IS NULL
                BEGIN
                    CREATE TABLE [SupportPolicies] (
                        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                        [Title] NVARCHAR(MAX) NOT NULL,
                        [PolicyType] NVARCHAR(MAX) NOT NULL,
                        [Content] NVARCHAR(MAX) NOT NULL,
                        [EffectiveFrom] DATETIME2 NOT NULL,
                        [CreatedAt] DATETIME2 NOT NULL
                    );
                END
                """
            );

            migrationBuilder.Sql(
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
                        [PrimarySourceId] NVARCHAR(MAX) NOT NULL CONSTRAINT [DF_ChatEvaluations_PrimarySourceId] DEFAULT N'',
                        [PrimarySourceType] NVARCHAR(MAX) NOT NULL CONSTRAINT [DF_ChatEvaluations_PrimarySourceType] DEFAULT N'',
                        [RetrievedSourcesJson] NVARCHAR(MAX) NOT NULL CONSTRAINT [DF_ChatEvaluations_RetrievedSourcesJson] DEFAULT N'[]',
                        [ImprovementNote] NVARCHAR(MAX) NOT NULL,
                        [CreatedAt] DATETIME2 NOT NULL,
                        CONSTRAINT [FK_ChatEvaluations_Conversations_ConversationId]
                            FOREIGN KEY ([ConversationId]) REFERENCES [Conversations]([Id]) ON DELETE CASCADE,
                        CONSTRAINT [FK_ChatEvaluations_Messages_UserMessageId]
                            FOREIGN KEY ([UserMessageId]) REFERENCES [Messages]([Id]),
                        CONSTRAINT [FK_ChatEvaluations_Messages_AssistantMessageId]
                            FOREIGN KEY ([AssistantMessageId]) REFERENCES [Messages]([Id])
                    );
                END
                """
            );

            migrationBuilder.Sql(
                """
                IF COL_LENGTH('ChatEvaluations', 'PrimarySourceId') IS NULL
                BEGIN
                    ALTER TABLE [ChatEvaluations]
                    ADD [PrimarySourceId] NVARCHAR(MAX) NOT NULL CONSTRAINT [DF_ChatEvaluations_PrimarySourceId_Repair] DEFAULT N'';
                END
                """
            );

            migrationBuilder.Sql(
                """
                IF COL_LENGTH('ChatEvaluations', 'PrimarySourceType') IS NULL
                BEGIN
                    ALTER TABLE [ChatEvaluations]
                    ADD [PrimarySourceType] NVARCHAR(MAX) NOT NULL CONSTRAINT [DF_ChatEvaluations_PrimarySourceType_Repair] DEFAULT N'';
                END
                """
            );

            migrationBuilder.Sql(
                """
                IF COL_LENGTH('ChatEvaluations', 'RetrievedSourcesJson') IS NULL
                BEGIN
                    ALTER TABLE [ChatEvaluations]
                    ADD [RetrievedSourcesJson] NVARCHAR(MAX) NOT NULL CONSTRAINT [DF_ChatEvaluations_RetrievedSourcesJson_Repair] DEFAULT N'[]';
                END
                """
            );

            migrationBuilder.Sql(
                """
                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ChatEvaluations_ConversationId' AND object_id = OBJECT_ID(N'[ChatEvaluations]'))
                BEGIN
                    CREATE INDEX [IX_ChatEvaluations_ConversationId] ON [ChatEvaluations]([ConversationId]);
                END
                """
            );

            migrationBuilder.Sql(
                """
                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ChatEvaluations_UserMessageId' AND object_id = OBJECT_ID(N'[ChatEvaluations]'))
                BEGIN
                    CREATE INDEX [IX_ChatEvaluations_UserMessageId] ON [ChatEvaluations]([UserMessageId]);
                END
                """
            );

            migrationBuilder.Sql(
                """
                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ChatEvaluations_AssistantMessageId' AND object_id = OBJECT_ID(N'[ChatEvaluations]'))
                BEGIN
                    CREATE INDEX [IX_ChatEvaluations_AssistantMessageId] ON [ChatEvaluations]([AssistantMessageId]);
                END
                """
            );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF COL_LENGTH('ChatEvaluations', 'RetrievedSourcesJson') IS NOT NULL
                BEGIN
                    ALTER TABLE [ChatEvaluations] DROP COLUMN [RetrievedSourcesJson];
                END
                """
            );

            migrationBuilder.Sql(
                """
                IF COL_LENGTH('ChatEvaluations', 'PrimarySourceType') IS NOT NULL
                BEGIN
                    ALTER TABLE [ChatEvaluations] DROP COLUMN [PrimarySourceType];
                END
                """
            );

            migrationBuilder.Sql(
                """
                IF COL_LENGTH('ChatEvaluations', 'PrimarySourceId') IS NOT NULL
                BEGIN
                    ALTER TABLE [ChatEvaluations] DROP COLUMN [PrimarySourceId];
                END
                """
            );
        }
    }
}
