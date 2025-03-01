USE [EventMgmtDB]
GO

/****** Object:  StoredProcedure [dbo].[sp_CreateEventSchedule]    Script Date: 27-02-2025 10:24:00 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[sp_CreateEventSchedule]
@EventID INT,
@ScheduleDate datetime,
@StartTime varchar(20),
@EndTime varchar(20)
AS
BEGIN
	INSERT INTO [dbo].[EventSchedule]
	([EventID], [ScheduleDate], [StartTime], [EndTime], [CreatedOn])
	VALUES
	(@EventID, @ScheduleDate, @StartTime, @EndTime, GETDATE())
END
GO


