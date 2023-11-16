SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Products](
	[product_id] [int] IDENTITY(1,1) NOT NULL,
	[product_name] [varchar](255) NOT NULL,
	[category_id] [int] NOT NULL,
	[price] [decimal](10, 2) NOT NULL,
	[description] [text] NULL,
	[image_url] [varchar](255) NULL,
	[date_added] [datetime] NULL,
 CONSTRAINT [PK__Products__47027DF52FDEB1CA] PRIMARY KEY CLUSTERED 
(
	[product_id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[Products] ADD  CONSTRAINT [DF_Products]  DEFAULT (getdate()) FOR [date_added]
GO

ALTER TABLE [dbo].[Products]  WITH CHECK ADD  CONSTRAINT [FK__Products__date_a__6477ECF3] FOREIGN KEY([category_id])
REFERENCES [dbo].[Categories] ([category_id])
GO

ALTER TABLE [dbo].[Products] CHECK CONSTRAINT [FK__Products__date_a__6477ECF3]
GO