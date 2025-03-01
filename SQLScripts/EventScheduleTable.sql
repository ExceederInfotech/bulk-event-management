USE [EventMgmtDB]
GO

/****** Object:  Table [dbo].[EventSchedule]    Script Date: 27-02-2025 10:22:53 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[EventSchedule](
	[EventScheduleID] [int] IDENTITY(1,1) NOT NULL,
	[EventID] [int] NULL,
	[ScheduleDate] [datetime] NULL,
	[StartTime] [varchar](20) NULL,
	[EndTime] [varchar](20) NULL,
	[CreatedOn] [datetime] NULL,
 CONSTRAINT [PK_EventSchedule] PRIMARY KEY CLUSTERED 
(
	[EventScheduleID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[EventSchedule]  WITH CHECK ADD  CONSTRAINT [fk_EventID] FOREIGN KEY([EventID])
REFERENCES [dbo].[Event] ([EventID])
GO

ALTER TABLE [dbo].[EventSchedule] CHECK CONSTRAINT [fk_EventID]
GO


