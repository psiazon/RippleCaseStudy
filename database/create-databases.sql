IF DB_ID('RippleEventDb') IS NULL CREATE DATABASE RippleEventDb;
IF DB_ID('RippleTicketDb') IS NULL CREATE DATABASE RippleTicketDb;
GO
USE RippleEventDb;
GO
IF OBJECT_ID('dbo.Events') IS NULL
CREATE TABLE dbo.Events (Id uniqueidentifier NOT NULL PRIMARY KEY, Name nvarchar(200) NOT NULL, Description nvarchar(2000) NOT NULL, Venue nvarchar(300) NOT NULL, StartsAt datetimeoffset NOT NULL, TotalCapacity int NOT NULL, CreatedAtUtc datetimeoffset NOT NULL);
IF OBJECT_ID('dbo.PricingTiers') IS NULL
CREATE TABLE dbo.PricingTiers (Id uniqueidentifier NOT NULL PRIMARY KEY, EventId uniqueidentifier NOT NULL, Name nvarchar(100) NOT NULL, Price decimal(18,2) NOT NULL, CONSTRAINT FK_PricingTiers_Events FOREIGN KEY(EventId) REFERENCES dbo.Events(Id) ON DELETE CASCADE);
GO
USE RippleTicketDb;
GO
IF OBJECT_ID('dbo.TicketInventories') IS NULL
CREATE TABLE dbo.TicketInventories (EventId uniqueidentifier NOT NULL PRIMARY KEY, TotalCapacity int NOT NULL, SoldQuantity int NOT NULL, RowVersion rowversion NOT NULL);
IF OBJECT_ID('dbo.TicketOrders') IS NULL
CREATE TABLE dbo.TicketOrders (Id uniqueidentifier NOT NULL PRIMARY KEY, EventId uniqueidentifier NOT NULL, PurchaserEmail nvarchar(320) NOT NULL, Quantity int NOT NULL, UnitPrice decimal(18,2) NOT NULL, PurchasedAtUtc datetimeoffset NOT NULL);
GO
