USE [EventMgmtDB]
GO

/****** Object:  StoredProcedure [dbo].[sp_CreateEvent]    Script Date: 27-02-2025 10:23:18 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[sp_CreateEvent]
@EventID INT OUTPUT,
@EventName NVARCHAR(250),
@Note NVARCHAR(500),
@StartDate DATETIME,
@EndDate DATETIME,
@Address NVARCHAR(500)
AS
BEGIN
	 IF(@EventID != 0 OR @EventID IS NOT NULL )
	 BEGIN
	      UPDATE [dbo].[Event]
		  SET
		  EventName = @EventName,
		  Note = @Note,
		  StartDate = @StartDate,
		  EndDate = @EndDate,
		  Address = @Address
		  WHERE [EventID] = @EventID

		  SET @EventID = @EventID
	 END
	 ELSE
	 BEGIN
	      INSERT INTO [dbo].[Event]
		  ([EventName], [Note], [StartDate], [EndDate], [Address], [CreatedOn])
		  VALUES
		  (@EventName,@Note,@StartDate,@EndDate,@Address, GETDATE())

		  SET @EventID =  @@IDENTITY;
	 END
END
GO


