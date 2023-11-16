SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[CdcTableOffset](
	[table_id] [int] IDENTITY(1,1) NOT NULL,
	[table_name] [nvarchar](100) NOT NULL,
	[table_offset] [nvarchar](100) NULL,
	[batch_size] [int] NOT NULL,
	[last_checked] [datetime] NULL,
 CONSTRAINT [PK_CdcTableOffset] PRIMARY KEY CLUSTERED 
(
	[table_id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO