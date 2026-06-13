using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AI.CustomerSupport.API.Migrations
{
    public partial class AddTrainingRuns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
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

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF OBJECT_ID(N'[TrainingRuns]', N'U') IS NOT NULL
                BEGIN
                    DROP TABLE [TrainingRuns];
                END
                """);
        }
    }
}
