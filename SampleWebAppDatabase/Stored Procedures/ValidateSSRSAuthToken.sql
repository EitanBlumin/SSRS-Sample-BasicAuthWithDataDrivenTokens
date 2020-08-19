CREATE PROCEDURE [dbo].[ValidateSSRSAuthToken]
	@UserId NVARCHAR(128),
	@HashToken VARCHAR(128),
	@FailWhenNotFound BIT = 1
AS
SET NOCOUNT ON;

DECLARE @RCount INT;

UPDATE [dbo].[SSRSAuthTokens]
	SET [ExpireDate] = DATEADD(hour, 1, GETUTCDATE()),
		[LastActiveDate] = GETUTCDATE()
WHERE
	[UserId] = @UserId
AND [HashToken] = @HashToken
AND [ExpireDate] > GETUTCDATE()

SET @RCount = @@ROWCOUNT;

SELECT @RCount AS [RowsFound]

IF @FailWhenNotFound = 1 AND @RCount = 0
BEGIN
	RAISERROR(N'Authentication token is invalid or expired.',16,1);
	RETURN -1;
END