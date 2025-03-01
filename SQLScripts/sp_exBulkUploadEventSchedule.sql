USE [EventMgmtDB]
GO

/****** Object:  StoredProcedure [dbo].[sp_exBulkUploadEventSchedule]    Script Date: 27-02-2025 10:24:20 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[sp_exBulkUploadEventSchedule]
@Data BulkUploadEvent READONLY
AS
BEGIN
	 DECLARE @EventID INT, @StartDate DATETIME, @EndDate DATETIME, @StartTime VARCHAR(10) = null,
	 @EndTime VARCHAR(10) = null, @EventTitle VARCHAR(200)= null,
     @NOTES VARCHAR(1000) = null,
     @EventAddress VARCHAR(1000) = null;

	 DECLARE emp_cursor CURSOR FOR
	 SELECT EventID, StartDate, EndDate,  StartTime, EndTime, EventTitle, NOTES , EventAddress
     FROM @Data;  

	 OPEN emp_cursor;
	 FETCH NEXT FROM emp_cursor
	 INTO @EventID, @StartDate, @EndDate, @StartTime, @EndTime, 
     @EventTitle, @NOTES, @EventAddress;

	 WHILE @@FETCH_STATUS = 0
	 BEGIN
			DECLARE @SavedEventID INT = 0;
			SET @SavedEventID = @EventID;
			EXEC [dbo].[sp_CreateEvent] @SavedEventID OUTPUT,@EventTitle,@NOTES,null,null,@EventAddress;

			IF (@StartDate IS NOT NULL AND @EndDate IS NOT NULL)
			BEGIN
			DECLARE @StartDate1 DATE = null;
			DECLARE @EndDate1 DATE = null;
			DECLARE @CurrentDate DATE = null; 

			SET @StartDate1  = CAST(@StartDate AS DATE);
			SET @EndDate1  = CAST(@EndDate AS DATE);
			SET @CurrentDate = @StartDate1;
			WHILE @CurrentDate <= @EndDate1
			BEGIN
				EXEC sp_CreateEventSchedule @SavedEventID, @CurrentDate, @StartTime, @EndTime
				SET @CurrentDate = DATEADD(DAY, 1, @CurrentDate);
			END		
			END

			FETCH NEXT FROM emp_cursor
			INTO @EventID, @StartDate, @EndDate, @StartTime, @EndTime, 
			@EventTitle, @NOTES, @EventAddress;
	 END
	 CLOSE emp_cursor;
	 DEALLOCATE emp_cursor;	
END
GO


