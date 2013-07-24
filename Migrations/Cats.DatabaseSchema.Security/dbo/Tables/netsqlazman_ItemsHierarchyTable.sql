﻿CREATE TABLE [dbo].[netsqlazman_ItemsHierarchyTable] (
    [ItemId]         INT NOT NULL,
    [MemberOfItemId] INT NOT NULL,
    CONSTRAINT [PK_ItemsHierarchy] PRIMARY KEY CLUSTERED ([ItemId] ASC, [MemberOfItemId] ASC)
);


GO
CREATE NONCLUSTERED INDEX [IX_ItemsHierarchy]
    ON [dbo].[netsqlazman_ItemsHierarchyTable]([ItemId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_ItemsHierarchy_1]
    ON [dbo].[netsqlazman_ItemsHierarchyTable]([MemberOfItemId] ASC);


GO
CREATE TRIGGER [dbo].[ItemsHierarchyTrigger] ON [dbo].[netsqlazman_ItemsHierarchyTable] 
FOR INSERT, UPDATE
AS
DECLARE @INSERTEDITEMID int
DECLARE @INSERTEDMEMBEROFITEMID int

DECLARE itemhierarchy_cur CURSOR FAST_FORWARD FOR SELECT ItemId, MemberOfItemId FROM inserted
OPEN itemhierarchy_cur
FETCH NEXT from itemhierarchy_cur INTO @INSERTEDITEMID, @INSERTEDMEMBEROFITEMID
WHILE @@FETCH_STATUS = 0
BEGIN
	IF UPDATE(ItemId) AND NOT EXISTS (SELECT ItemId FROM dbo.[netsqlazman_ItemsTable] WHERE [netsqlazman_ItemsTable].ItemId = @INSERTEDITEMID) 
	 BEGIN
	  RAISERROR ('ItemId NOT FOUND into dbo.ItemsTable', 16, 1)
	  ROLLBACK TRANSACTION
	 END
	
	IF UPDATE(MemberOfItemId) AND NOT EXISTS (SELECT ItemId FROM dbo.[netsqlazman_ItemsTable] WHERE [netsqlazman_ItemsTable].ItemId = @INSERTEDMEMBEROFITEMID)
	 BEGIN
	  RAISERROR ('MemberOfItemId NOT FOUND into dbo.ItemsTable', 16, 1)
	  ROLLBACK TRANSACTION
	 END
	FETCH NEXT from itemhierarchy_cur INTO @INSERTEDITEMID, @INSERTEDMEMBEROFITEMID
END
CLOSE itemhierarchy_cur
DEALLOCATE itemhierarchy_cur
