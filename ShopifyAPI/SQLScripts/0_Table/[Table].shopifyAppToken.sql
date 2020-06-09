/****** Object:  Table [dbo].[shopifyAppToken]    Script Date: 10/5/2020 6:07:33 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[shopifyAppToken](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[scopes] [varchar](3000) NOT NULL,
	[access_token] varchar(2000) NULL,
	[refresh_token] varchar(2000) NULL,
	[createdDate] [datetime] NOT NULL,
) ON [PRIMARY]
GO
