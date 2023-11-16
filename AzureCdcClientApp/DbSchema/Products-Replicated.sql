SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Products](
	[product_id] [int] NOT NULL,
	[product_name] [varchar](255) NOT NULL,
	[category_id] [int] NOT NULL,
	[price] [decimal](10, 2) NOT NULL,
	[description] [text] NULL,
	[image_url] [varchar](255) NULL,
	[date_added] [datetime] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO