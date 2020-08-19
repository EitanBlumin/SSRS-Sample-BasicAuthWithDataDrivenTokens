CREATE TABLE [dbo].[SSRSAuthTokens] (
    [Id]             INT            IDENTITY (1, 1) NOT NULL,
    [UserId]         NVARCHAR (128) NOT NULL,
    [HashToken]      VARCHAR (128)  CONSTRAINT [DF_SSRSAuthTokens_HashToken] DEFAULT (CONVERT([varchar](128),hashbytes('SHA2_512',CONVERT([varchar](64),newid())),(2))) NOT NULL,
    [CreateDate]     DATETIME       CONSTRAINT [DF_SSRSAuthTokens_CreateDate] DEFAULT (getutcdate()) NOT NULL,
    [ExpireDate]     DATETIME       CONSTRAINT [DF_SSRSAuthTokens_ExpireDate] DEFAULT (dateadd(hour,(1),getutcdate())) NOT NULL,
    [LastActiveDate] DATETIME       CONSTRAINT [DF_SSRSAuthTokens_LastActiveDate] DEFAULT (getutcdate()) NOT NULL,
    CONSTRAINT [PK_SSRSAuthTokens] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_SSRSAuthTokens_AspNetUsers] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE
);

