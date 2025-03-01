USE [EventMgmtDB]
GO

/****** Object:  Table [dbo].[Event]    Script Date: 27-02-2025 10:22:09 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Event](
	[EventID] [int] IDENTITY(1,1) NOT NULL,
	[EventName] [nvarchar](250) NOT NULL,
	[Note] [nvarchar](500) NULL,
	[StartDate] [datetime] NULL,
	[EndDate] [datetime] NULL,
	[Address] [nvarchar](500) NULL,
	[Pincode] [int] NULL,
	[CreatedOn] [datetime] NULL,
	[CreatedBy] [nchar](10) NULL,
 CONSTRAINT [PK_Event] PRIMARY KEY CLUSTERED 
(
	[EventID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO


