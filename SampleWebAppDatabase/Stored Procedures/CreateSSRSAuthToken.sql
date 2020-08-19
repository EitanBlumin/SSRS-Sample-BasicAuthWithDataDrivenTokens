CREATE PROCEDURE [dbo].[CreateSSRSAuthToken]
	@UserId NVARCHAR(128)
AS
SET NOCOUNT ON;

WITH Trgt
AS
(
	SELECT *
	FROM [dbo].[SSRSAuthTokens]
	WHERE [ExpireDate] > GETUTCDATE()
)
MERGE INTO Trgt
USING (SELECT [UserId] = @UserId) AS Src
ON Trgt.[UserId] = Src.[UserId]
WHEN MATCHED THEN
	UPDATE
	SET [ExpireDate] = DATEADD(hour, 1, GETUTCDATE()),
		[LastActiveDate] = GETUTCDATE()
WHEN NOT MATCHED BY TARGET THEN
	INSERT ([UserId])
	VALUES ([UserId])
OUTPUT
	inserted.[Id], inserted.[UserId], inserted.[HashToken], inserted.[ExpireDate];