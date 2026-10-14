/* =====================================================================================
   LegacyEcommerceDb — initial schema for SQL Server Express  (Database First / EDMX)
   Target instance : .\SQLEXPRESS      Database : LegacyEcommerceDb
   Spec            : .NET 4.7 MVC + EF6 Database First + ASP.NET Identity 2.0 (OWIN)

   SAFETY CONTRACT (read before running):
     * Creates ONLY missing objects. Every CREATE is guarded with IF NOT EXISTS / IF OBJECT_ID IS NULL.
     * Contains NO DROP, NO DELETE, NO UPDATE, NO TRUNCATE, NO ALTER of existing objects.
     * Safe to re-run (idempotent); a second run reports "already exists" and skips.
     * INSTANCE LOCK: runs ONLY against .\SQLEXPRESS — STEP 0 raises an error on any other
       instance and STEP 1 refuses to create the database there. The only database this
       script ever switches into or writes to is [LegacyEcommerceDb]; all other databases
       (StudentDB, PhonebookDB, ContactBookDb, master, ...) are untouched.
     * Preferred invocation — the -b flag aborts the whole file on the first error:
           sqlcmd -S .\SQLEXPRESS -E -b -i "database\01_Create_LegacyEcommerceDb.sql"
     * The SQL login password below is a PLACEHOLDER for local dev only.
       Replace before any real use and never commit real credentials.
     * Requires an elevated/rights context on .\SQLEXPRESS (e.g. Windows admin / sysadmin).

   AFTER RUNNING — set these initializers in code so neither context tries to (re)create schema:
       Database.SetInitializer<EcommerceDbContext>(null);        // Ecommerce.Data
       Database.SetInitializer<ApplicationDbContext>(null);      // Ecommerce.Web (OWIN/Identity)
   ===================================================================================== */

SET NOCOUNT ON;
GO

/* Required SET options for filtered indexes (Msg 1934): sqlcmd opens sessions with
   QUOTED_IDENTIFIER OFF, so set both explicitly in their own batch BEFORE the batches
   that need them. These are also the settings EF/SqlClient uses at runtime. */
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

/* -------------------------------------------------------------------------------------
   STEP 0 — INSTANCE LOCK: abort unless connected to .\SQLEXPRESS
   The server is chosen by the sqlcmd -S argument / connection string, not by this file.
   RAISERROR (severity 16) + sqlcmd -b terminates the run there; additionally
   SET NOEXEC ON makes every remaining batch compile-but-NOT-run in this session, so
   even without -b (e.g. SSMS) nothing can be written to the wrong instance.
   ------------------------------------------------------------------------------------- */
DECLARE @currentServer nvarchar(128) = @@SERVERNAME;
IF @currentServer NOT LIKE N'%\SQLEXPRESS'
BEGIN
    RAISERROR(N'ABORT — wrong instance. This script targets .\SQLEXPRESS only, but connected to [%s]. Nothing will be executed.', 16, 1, @currentServer);
    SET NOEXEC ON;   -- every later batch in this session compiles but never runs
END
GO

/* -------------------------------------------------------------------------------------
   STEP 1 — Create the database (only if RIGHT instance AND the name is free)
   ------------------------------------------------------------------------------------- */
IF @@SERVERNAME NOT LIKE N'%\SQLEXPRESS'
BEGIN
    PRINT '>> ABORT: not .\SQLEXPRESS — database creation REFUSED.';
END
ELSE IF DB_ID(N'LegacyEcommerceDb') IS NULL
BEGIN
    CREATE DATABASE [LegacyEcommerceDb];
    PRINT '>> Created database [LegacyEcommerceDb].';
END
ELSE
BEGIN
    PRINT '>> Database [LegacyEcommerceDb] already exists — creation SKIPPED (no overwrite).';
END
GO

USE [LegacyEcommerceDb];
GO

/* Tripwire — confirms the context switch actually succeeded. If USE failed, the next
   batch would otherwise run against whatever database the session defaulted to (e.g.
   master). RAISERROR reports it; SET NOEXEC ON renders every remaining batch inert. */
DECLARE @currentDb nvarchar(128);
SET @currentDb = DB_NAME();   -- RAISERROR substitution args must be constant/variable, NOT a function call

IF @currentDb <> N'LegacyEcommerceDb'
BEGIN
    RAISERROR(N'ABORT — current database is [%s], expected [LegacyEcommerceDb]. Nothing will be executed.', 16, 1, @currentDb);
    SET NOEXEC ON;   -- every later batch in this session compiles but never runs
END
GO

/* -------------------------------------------------------------------------------------
   STEP 2 — Domain tables
   ------------------------------------------------------------------------------------- */

-- ================================ Categories ========================================
IF OBJECT_ID(N'dbo.Categories', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Categories
    (
        Id              INT             IDENTITY(1,1)   NOT NULL,
        ParentCategoryId INT            NULL,               -- self FK, supports the nested category tree
        Name            NVARCHAR(100)   NOT NULL,
        Slug            NVARCHAR(100)   NOT NULL,
        Description     NVARCHAR(500)   NULL,
        IsActive        BIT             NOT NULL CONSTRAINT DF_Categories_IsActive DEFAULT (1),
        DisplayOrder    INT             NOT NULL CONSTRAINT DF_Categories_DisplayOrder DEFAULT (0),
        CONSTRAINT PK_Categories PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT FK_Categories_Parent FOREIGN KEY (ParentCategoryId) REFERENCES dbo.Categories(Id)
    );
    PRINT '>> Created dbo.Categories.';
END
ELSE PRINT '>> dbo.Categories already exists — skipped.';
GO

-- ================================= Products ========================================
IF OBJECT_ID(N'dbo.Products', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Products
    (
        Id               INT              IDENTITY(1,1)  NOT NULL,
        CategoryId       INT              NOT NULL,
        Name             NVARCHAR(200)    NOT NULL,
        Slug             NVARCHAR(200)    NOT NULL,
        Sku              NVARCHAR(50)     NOT NULL,
        ShortDescription NVARCHAR(1000)   NULL,
        Description      NVARCHAR(MAX)    NULL,
        Price            DECIMAL(18,2)    NOT NULL CONSTRAINT CK_Products_Price CHECK (Price >= 0),
        Stock            INT              NOT NULL CONSTRAINT DF_Products_Stock DEFAULT (0)
                                                   CONSTRAINT CK_Products_Stock CHECK (Stock >= 0),
        IsFeatured       BIT              NOT NULL CONSTRAINT DF_Products_IsFeatured DEFAULT (0),
        IsActive         BIT              NOT NULL CONSTRAINT DF_Products_IsActive DEFAULT (1),
        ThumbnailUrl     NVARCHAR(300)    NULL,               -- used by _ProductCard.cshtml
        CreatedUtc       DATETIME2(0)     NOT NULL CONSTRAINT DF_Products_CreatedUtc DEFAULT (SYSUTCDATETIME()),
        ModifiedUtc      DATETIME2(0)     NULL,
        CONSTRAINT PK_Products PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT FK_Products_Categories FOREIGN KEY (CategoryId) REFERENCES dbo.Categories(Id)
    );
    PRINT '>> Created dbo.Products.';
END
ELSE PRINT '>> dbo.Products already exists — skipped.';
GO

-- ============================== ProductImages ======================================
IF OBJECT_ID(N'dbo.ProductImages', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ProductImages
    (
        Id           INT            IDENTITY(1,1) NOT NULL,
        ProductId    INT            NOT NULL,
        Url          NVARCHAR(300)  NOT NULL,
        AltText      NVARCHAR(150)  NULL,
        DisplayOrder INT            NOT NULL CONSTRAINT DF_ProductImages_DisplayOrder DEFAULT (0),
        IsPrimary    BIT            NOT NULL CONSTRAINT DF_ProductImages_IsPrimary DEFAULT (0),
        CONSTRAINT PK_ProductImages PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT FK_ProductImages_Products FOREIGN KEY (ProductId) REFERENCES dbo.Products(Id) ON DELETE CASCADE
    );
    PRINT '>> Created dbo.ProductImages.';
END
ELSE PRINT '>> dbo.ProductImages already exists — skipped.';
GO

-- ============================== ProductVariants ====================================
IF OBJECT_ID(N'dbo.ProductVariants', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ProductVariants
    (
        Id              INT             IDENTITY(1,1) NOT NULL,
        ProductId       INT             NOT NULL,
        Name            NVARCHAR(100)   NOT NULL,           -- e.g. "16 GB / 512 GB"
        Sku             NVARCHAR(50)    NOT NULL,
        PriceAdjustment DECIMAL(18,2)   NOT NULL CONSTRAINT DF_ProductVariants_PriceAdjustment DEFAULT (0),
        Stock           INT             NOT NULL CONSTRAINT DF_ProductVariants_Stock DEFAULT (0),
        IsActive        BIT             NOT NULL CONSTRAINT DF_ProductVariants_IsActive DEFAULT (1),
        CONSTRAINT PK_ProductVariants PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT FK_ProductVariants_Products FOREIGN KEY (ProductId) REFERENCES dbo.Products(Id) ON DELETE CASCADE
    );
    PRINT '>> Created dbo.ProductVariants.';
END
ELSE PRINT '>> dbo.ProductVariants already exists — skipped.';
GO

/* -------------------------------------------------------------------------------------
   STEP 3 — ASP.NET Identity 2.0 tables (OWIN cookie auth) — same database
   (schema matches the default Identity 2.0 model; initializer must be disabled in code)
   ------------------------------------------------------------------------------------- */

-- ================================= AspNetRoles =====================================
IF OBJECT_ID(N'dbo.AspNetRoles', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.AspNetRoles
    (
        Id   NVARCHAR(128) NOT NULL,
        Name NVARCHAR(256) NOT NULL,
        CONSTRAINT PK_AspNetRoles PRIMARY KEY CLUSTERED (Id)
    );
    PRINT '>> Created dbo.AspNetRoles.';
END
ELSE PRINT '>> dbo.AspNetRoles already exists — skipped.';
GO

-- ================================= AspNetUsers =====================================
IF OBJECT_ID(N'dbo.AspNetUsers', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.AspNetUsers
    (
        Id                   NVARCHAR(128)  NOT NULL,
        UserName             NVARCHAR(256)  NOT NULL,
        Email                NVARCHAR(256)  NULL,
        EmailConfirmed       BIT            NOT NULL CONSTRAINT DF_AspNetUsers_EmailConfirmed DEFAULT (0),
        PasswordHash         NVARCHAR(MAX)  NULL,
        SecurityStamp        NVARCHAR(MAX)  NULL,
        PhoneNumber          NVARCHAR(MAX)  NULL,
        PhoneNumberConfirmed BIT            NOT NULL CONSTRAINT DF_AspNetUsers_PhoneNumberConfirmed DEFAULT (0),
        TwoFactorEnabled     BIT            NOT NULL CONSTRAINT DF_AspNetUsers_TwoFactorEnabled DEFAULT (0),
        LockoutEndDateUtc    DATETIMEOFFSET(7) NULL,
        LockoutEnabled       BIT            NOT NULL CONSTRAINT DF_AspNetUsers_LockoutEnabled DEFAULT (0),
        AccessFailedCount    INT            NOT NULL CONSTRAINT DF_AspNetUsers_AccessFailedCount DEFAULT (0),
        CONSTRAINT PK_AspNetUsers PRIMARY KEY CLUSTERED (Id)
    );
    PRINT '>> Created dbo.AspNetUsers.';
END
ELSE PRINT '>> dbo.AspNetUsers already exists — skipped.';
GO

IF OBJECT_ID(N'dbo.AspNetUserClaims', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.AspNetUserClaims
    (
        Id        INT            IDENTITY(1,1) NOT NULL,
        UserId    NVARCHAR(128)  NOT NULL,
        ClaimType NVARCHAR(MAX)  NULL,
        ClaimValue NVARCHAR(MAX) NULL,
        CONSTRAINT PK_AspNetUserClaims PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT FK_AspNetUserClaims_Users FOREIGN KEY (UserId) REFERENCES dbo.AspNetUsers(Id) ON DELETE CASCADE
    );
    PRINT '>> Created dbo.AspNetUserClaims.';
END
ELSE PRINT '>> dbo.AspNetUserClaims already exists — skipped.';
GO

IF OBJECT_ID(N'dbo.AspNetUserLogins', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.AspNetUserLogins
    (
        LoginProvider NVARCHAR(128) NOT NULL,
        ProviderKey   NVARCHAR(128) NOT NULL,
        UserId        NVARCHAR(128) NOT NULL,
        CONSTRAINT PK_AspNetUserLogins PRIMARY KEY CLUSTERED (LoginProvider, ProviderKey, UserId),
        CONSTRAINT FK_AspNetUserLogins_Users FOREIGN KEY (UserId) REFERENCES dbo.AspNetUsers(Id) ON DELETE CASCADE
    );
    PRINT '>> Created dbo.AspNetUserLogins.';
END
ELSE PRINT '>> dbo.AspNetUserLogins already exists — skipped.';
GO

IF OBJECT_ID(N'dbo.AspNetUserRoles', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.AspNetUserRoles
    (
        UserId NVARCHAR(128) NOT NULL,
        RoleId NVARCHAR(128) NOT NULL,
        CONSTRAINT PK_AspNetUserRoles PRIMARY KEY CLUSTERED (UserId, RoleId),
        CONSTRAINT FK_AspNetUserRoles_Users FOREIGN KEY (UserId) REFERENCES dbo.AspNetUsers(Id) ON DELETE CASCADE,
        CONSTRAINT FK_AspNetUserRoles_Roles FOREIGN KEY (RoleId) REFERENCES dbo.AspNetRoles(Id) ON DELETE CASCADE
    );
    PRINT '>> Created dbo.AspNetUserRoles.';
END
ELSE PRINT '>> dbo.AspNetUserRoles already exists — skipped.';
GO

-- ================================ Customers ========================================
IF OBJECT_ID(N'dbo.Customers', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Customers
    (
        Id          INT             IDENTITY(1,1) NOT NULL,
        UserId      NVARCHAR(128)   NULL,               -- link to AspNetUsers (nullable: guest data possible)
        FullName    NVARCHAR(150)   NOT NULL,
        Email       NVARCHAR(256)   NOT NULL,
        Phone       NVARCHAR(30)    NULL,
        CreatedUtc  DATETIME2(0)    NOT NULL CONSTRAINT DF_Customers_CreatedUtc DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT PK_Customers PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT FK_Customers_AspNetUsers FOREIGN KEY (UserId) REFERENCES dbo.AspNetUsers(Id)
    );
    PRINT '>> Created dbo.Customers.';
END
ELSE PRINT '>> dbo.Customers already exists — skipped.';
GO

-- ================================ Addresses ========================================
IF OBJECT_ID(N'dbo.Addresses', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Addresses
    (
        Id          INT             IDENTITY(1,1) NOT NULL,
        CustomerId  INT             NOT NULL,
        Label       NVARCHAR(50)    NULL,               -- Home / Office
        FullName    NVARCHAR(150)   NOT NULL,
        Line1       NVARCHAR(200)   NOT NULL,
        Line2       NVARCHAR(200)   NULL,
        City        NVARCHAR(100)   NOT NULL,
        State       NVARCHAR(100)   NULL,
        PostalCode  NVARCHAR(20)    NOT NULL,
        Country     NVARCHAR(100)   NOT NULL,
        Phone       NVARCHAR(30)    NULL,
        IsDefault   BIT             NOT NULL CONSTRAINT DF_Addresses_IsDefault DEFAULT (0),
        CONSTRAINT PK_Addresses PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT FK_Addresses_Customers FOREIGN KEY (CustomerId) REFERENCES dbo.Customers(Id) ON DELETE CASCADE
    );
    PRINT '>> Created dbo.Addresses.';
END
ELSE PRINT '>> dbo.Addresses already exists — skipped.';
GO

-- ================================ CartItems ========================================
IF OBJECT_ID(N'dbo.CartItems', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.CartItems
    (
        Id           INT             IDENTITY(1,1) NOT NULL,
        CartId       NVARCHAR(64)    NOT NULL,           -- session id (anonymous) or user id (authenticated)
        UserId       NVARCHAR(128)   NULL,               -- set when the cart belongs to a signed-in user
        ProductId    INT             NOT NULL,
        VariantId    INT             NULL,
        Quantity     INT             NOT NULL CONSTRAINT CK_CartItems_Quantity CHECK (Quantity > 0),
        UnitPrice    DECIMAL(18,2)   NOT NULL CONSTRAINT CK_CartItems_UnitPrice CHECK (UnitPrice >= 0),
        CreatedUtc   DATETIME2(0)    NOT NULL CONSTRAINT DF_CartItems_CreatedUtc DEFAULT (SYSUTCDATETIME()),
        ModifiedUtc  DATETIME2(0)    NULL,
        CONSTRAINT PK_CartItems PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT FK_CartItems_Products FOREIGN KEY (ProductId) REFERENCES dbo.Products(Id),
        CONSTRAINT FK_CartItems_Variants FOREIGN KEY (VariantId) REFERENCES dbo.ProductVariants(Id),
        CONSTRAINT FK_CartItems_AspNetUsers FOREIGN KEY (UserId) REFERENCES dbo.AspNetUsers(Id)
    );
    PRINT '>> Created dbo.CartItems.';
END
ELSE PRINT '>> dbo.CartItems already exists — skipped.';
GO

-- ================================== Orders =========================================
IF OBJECT_ID(N'dbo.Orders', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Orders
    (
        Id                 INT             IDENTITY(1,1) NOT NULL,
        OrderNumber        NVARCHAR(30)    NOT NULL,           -- generated by ICheckoutService
        UserId             NVARCHAR(128)   NOT NULL,           -- [Authorize] required to place/view orders
        CustomerId         INT             NULL,
        OrderDateUtc       DATETIME2(0)    NOT NULL CONSTRAINT DF_Orders_OrderDateUtc DEFAULT (SYSUTCDATETIME()),
        Status             NVARCHAR(20)    NOT NULL CONSTRAINT DF_Orders_Status DEFAULT (N'Pending')
                                CONSTRAINT CK_Orders_Status CHECK (Status IN (N'Pending', N'Paid', N'Shipped', N'Delivered', N'Cancelled', N'Refunded')),
        SubTotal           DECIMAL(18,2)   NOT NULL CONSTRAINT CK_Orders_SubTotal CHECK (SubTotal >= 0),
        ShippingCost       DECIMAL(18,2)   NOT NULL CONSTRAINT DF_Orders_ShippingCost DEFAULT (0),
        Tax                DECIMAL(18,2)   NOT NULL CONSTRAINT DF_Orders_Tax DEFAULT (0),
        Total              DECIMAL(18,2)   NOT NULL CONSTRAINT CK_Orders_Total CHECK (Total >= 0),
        Currency           NCHAR(3)        NOT NULL CONSTRAINT DF_Orders_Currency DEFAULT (N'USD'),
        ShippingMethod     NVARCHAR(50)    NULL,               -- checkout step 2
        PaymentMethod      NVARCHAR(50)    NULL,               -- checkout step 3
        PaymentReference   NVARCHAR(100)   NULL,               -- fake reference only — no real gateway
        Notes              NVARCHAR(500)   NULL,
        -- Address snapshot (checkout step 1) — copied so later address edits never alter history
        ShipFullName       NVARCHAR(150)   NOT NULL,
        ShipLine1          NVARCHAR(200)   NOT NULL,
        ShipLine2          NVARCHAR(200)   NULL,
        ShipCity           NVARCHAR(100)   NOT NULL,
        ShipState          NVARCHAR(100)   NULL,
        ShipPostalCode     NVARCHAR(20)    NOT NULL,
        ShipCountry        NVARCHAR(100)   NOT NULL,
        ShipPhone          NVARCHAR(30)    NULL,
        CONSTRAINT PK_Orders PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT UQ_Orders_OrderNumber UNIQUE (OrderNumber),
        CONSTRAINT FK_Orders_AspNetUsers FOREIGN KEY (UserId) REFERENCES dbo.AspNetUsers(Id),
        CONSTRAINT FK_Orders_Customers FOREIGN KEY (CustomerId) REFERENCES dbo.Customers(Id)
    );
    PRINT '>> Created dbo.Orders.';
END
ELSE PRINT '>> dbo.Orders already exists — skipped.';
GO

-- ================================ OrderLines =======================================
IF OBJECT_ID(N'dbo.OrderLines', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.OrderLines
    (
        Id           INT             IDENTITY(1,1) NOT NULL,
        OrderId      INT             NOT NULL,
        ProductId    INT             NOT NULL,
        ProductName  NVARCHAR(200)   NOT NULL,           -- snapshot for order history
        Sku          NVARCHAR(50)    NULL,
        VariantName  NVARCHAR(100)   NULL,
        UnitPrice    DECIMAL(18,2)   NOT NULL CONSTRAINT CK_OrderLines_UnitPrice CHECK (UnitPrice >= 0),
        Quantity     INT             NOT NULL CONSTRAINT CK_OrderLines_Quantity CHECK (Quantity > 0),
        LineTotal    DECIMAL(18,2)   NOT NULL CONSTRAINT CK_OrderLines_LineTotal CHECK (LineTotal >= 0),
        CONSTRAINT PK_OrderLines PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT FK_OrderLines_Orders FOREIGN KEY (OrderId) REFERENCES dbo.Orders(Id) ON DELETE CASCADE,
        CONSTRAINT FK_OrderLines_Products FOREIGN KEY (ProductId) REFERENCES dbo.Products(Id)
    );
    PRINT '>> Created dbo.OrderLines.';
END
ELSE PRINT '>> dbo.OrderLines already exists — skipped.';
GO

/* -------------------------------------------------------------------------------------
   STEP 4 — Indexes (guarded)
   ------------------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_Categories_Slug' AND object_id = OBJECT_ID(N'dbo.Categories'))
    CREATE UNIQUE INDEX UX_Categories_Slug ON dbo.Categories(Slug);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Categories_ParentCategoryId' AND object_id = OBJECT_ID(N'dbo.Categories'))
    CREATE INDEX IX_Categories_ParentCategoryId ON dbo.Categories(ParentCategoryId);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_Products_Slug' AND object_id = OBJECT_ID(N'dbo.Products'))
    CREATE UNIQUE INDEX UX_Products_Slug ON dbo.Products(Slug);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_Products_Sku' AND object_id = OBJECT_ID(N'dbo.Products'))
    CREATE UNIQUE INDEX UX_Products_Sku ON dbo.Products(Sku);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Products_CategoryId' AND object_id = OBJECT_ID(N'dbo.Products'))
    CREATE INDEX IX_Products_CategoryId ON dbo.Products(CategoryId);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Products_IsFeatured' AND object_id = OBJECT_ID(N'dbo.Products'))
    CREATE INDEX IX_Products_IsFeatured ON dbo.Products(IsFeatured);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ProductImages_ProductId' AND object_id = OBJECT_ID(N'dbo.ProductImages'))
    CREATE INDEX IX_ProductImages_ProductId ON dbo.ProductImages(ProductId);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ProductVariants_ProductId' AND object_id = OBJECT_ID(N'dbo.ProductVariants'))
    CREATE INDEX IX_ProductVariants_ProductId ON dbo.ProductVariants(ProductId);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_AspNetUsers_UserName' AND object_id = OBJECT_ID(N'dbo.AspNetUsers'))
    CREATE UNIQUE INDEX UX_AspNetUsers_UserName ON dbo.AspNetUsers(UserName);

/* UX_AspNetRoles_Name — guarded, with a duplicate-name pre-check:
   if conflicting role names already exist the index is NOT created and the
   conflicting rows are reported instead (so this script never fails midway). */
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_AspNetRoles_Name' AND object_id = OBJECT_ID(N'dbo.AspNetRoles'))
BEGIN
    IF EXISTS (SELECT Name FROM dbo.AspNetRoles GROUP BY Name HAVING COUNT(*) > 1)
    BEGIN
        PRINT '!! DUPLICATE role names detected — UX_AspNetRoles_Name NOT created. Conflicts listed below:';
        SELECT 'RoleNameConflict' AS [Check], Name AS [RoleName], COUNT(*) AS [Occurrences]
        FROM dbo.AspNetRoles
        GROUP BY Name
        HAVING COUNT(*) > 1;
    END
    ELSE
    BEGIN
        CREATE UNIQUE INDEX UX_AspNetRoles_Name ON dbo.AspNetRoles(Name);
        PRINT '>> Created UX_AspNetRoles_Name.';
    END
END
ELSE PRINT '>> UX_AspNetRoles_Name already exists — skipped.';

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AspNetUsers_Email' AND object_id = OBJECT_ID(N'dbo.AspNetUsers'))
    CREATE INDEX IX_AspNetUsers_Email ON dbo.AspNetUsers(Email);

-- filtered unique: allows many anonymous carts (NULL), one link row per user
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_Customers_UserId' AND object_id = OBJECT_ID(N'dbo.Customers'))
    CREATE UNIQUE INDEX UX_Customers_UserId ON dbo.Customers(UserId) WHERE UserId IS NOT NULL;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Addresses_CustomerId' AND object_id = OBJECT_ID(N'dbo.Addresses'))
    CREATE INDEX IX_Addresses_CustomerId ON dbo.Addresses(CustomerId);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_CartItems_CartId' AND object_id = OBJECT_ID(N'dbo.CartItems'))
    CREATE INDEX IX_CartItems_CartId ON dbo.CartItems(CartId);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Orders_UserId' AND object_id = OBJECT_ID(N'dbo.Orders'))
    CREATE INDEX IX_Orders_UserId ON dbo.Orders(UserId);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Orders_OrderDateUtc' AND object_id = OBJECT_ID(N'dbo.Orders'))
    CREATE INDEX IX_Orders_OrderDateUtc ON dbo.Orders(OrderDateUtc);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_OrderLines_OrderId' AND object_id = OBJECT_ID(N'dbo.OrderLines'))
    CREATE INDEX IX_OrderLines_OrderId ON dbo.OrderLines(OrderId);

PRINT '>> Indexes verified.';
GO

/* -------------------------------------------------------------------------------------
   STEP 5 — Optional roles only (no users, NO passwords/credentials are seeded)
   ------------------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM dbo.AspNetRoles WHERE Id = N'admin-role')
    INSERT dbo.AspNetRoles (Id, Name) VALUES (N'admin-role', N'Admin');
IF NOT EXISTS (SELECT 1 FROM dbo.AspNetRoles WHERE Id = N'customer-role')
    INSERT dbo.AspNetRoles (Id, Name) VALUES (N'customer-role', N'Customer');
PRINT '>> Roles ensured (Admin, Customer). No user accounts created.';
GO

/* -------------------------------------------------------------------------------------
   STEP 6 — Seed catalog demo data (guarded: runs only when the catalog is empty)
   Images are local paths; placeholder files are added under Content/images/products/ later.
   ------------------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM dbo.Categories)
BEGIN
    INSERT dbo.Categories (ParentCategoryId, Name, Slug, Description, IsActive, DisplayOrder)
    VALUES (NULL, N'Laptops',     N'laptops',     N'Notebook computers',                 1, 1),
           (NULL, N'Phones',      N'phones',      N'Smartphones and accessories',        1, 2),
           (NULL, N'Audio',       N'audio',       N'Headphones and speakers',            1, 3),
           (NULL, N'Accessories', N'accessories', N'Cables, adapters and peripherals',   1, 4),
           (NULL, N'Deals',       N'deals',       N'Certified refurbished offers',       1, 5);
    PRINT '>> Seeded 5 categories.';
END
ELSE PRINT '>> Categories already present — seed skipped.';
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Products)
BEGIN
    DECLARE @CatLaptops INT = (SELECT Id FROM dbo.Categories WHERE Slug = N'laptops');
    DECLARE @CatPhones  INT = (SELECT Id FROM dbo.Categories WHERE Slug = N'phones');
    DECLARE @CatAudio   INT = (SELECT Id FROM dbo.Categories WHERE Slug = N'audio');
    DECLARE @CatAcc     INT = (SELECT Id FROM dbo.Categories WHERE Slug = N'accessories');
    DECLARE @CatDeals   INT = (SELECT Id FROM dbo.Categories WHERE Slug = N'deals');

    INSERT dbo.Products (CategoryId, Name, Slug, Sku, ShortDescription, Description, Price, Stock, IsFeatured, IsActive, ThumbnailUrl)
    VALUES
    (@CatLaptops, N'NovaBook Pro 14',        N'novabook-pro-14',    N'NB-P14',  N'14" powerhouse for work and play.',        N'Intel-class 8-core laptop with 16 GB RAM and 512 GB SSD.',           1299.00, 25, 1, 1, N'/Content/images/products/novabook-pro-14.jpg'),
    (@CatLaptops, N'NovaBook Air 13',        N'novabook-air-13',    N'NB-A13',  N'Lightweight all-day laptop.',               N'13" ultrabook, 1.1 kg, 18-hour battery life.',                        999.00,  40, 0, 1, N'/Content/images/products/novabook-air-13.jpg'),
    (@CatLaptops, N'TerraBook 15 Classic',   N'terrabook-15',       N'TB-15',   N'Reliable everyday laptop.',                 N'15" business notebook with DVD-free slim chassis.',                   749.00,  30, 0, 1, N'/Content/images/products/terrabook-15.jpg'),
    (@CatPhones,  N'Stellar X2 Phone',       N'stellar-x2',         N'SX2-128', N'Flagship camera phone.',                    N'6.5" OLED, 128 GB storage, triple camera.',                           799.00,  50, 1, 1, N'/Content/images/products/stellar-x2.jpg'),
    (@CatPhones,  N'Stellar X2 Mini',        N'stellar-x2-mini',    N'SX2M-64', N'Compact version of the X2.',                N'5.8" OLED, 64 GB storage.',                                           599.00,  45, 0, 1, N'/Content/images/products/stellar-x2-mini.jpg'),
    (@CatAudio,   N'AeroPods Wireless',      N'aeropods-wireless',  N'AP-W1',   N'True wireless earbuds.',                    N'Active noise cancelling, 24 h total battery with case.',              149.00, 120, 1, 1, N'/Content/images/products/aeropods-wireless.jpg'),
    (@CatAudio,   N'StudioOver Pro',         N'studioover-pro',     N'SO-P1',   N'Over-ear studio headphones.',               N'Closed-back monitoring headphones with detachable cable.',           249.00,  35, 0, 1, N'/Content/images/products/studioover-pro.jpg'),
    (@CatAcc,     N'ThunderCharge 65W',      N'thundercharge-65w',  N'TC-65',   N'USB-C GaN charger.',                        N'65 W USB-C wall charger, foldable pins.',                              39.00, 200, 0, 1, N'/Content/images/products/thundercharge-65w.jpg'),
    (@CatAcc,     N'FlexCase Keyboard',      N'flexcase-kb',        N'FC-KB',   N'Low-profile wireless keyboard.',            N'Bluetooth keyboard with backlit keys.',                                89.00,  60, 0, 1, N'/Content/images/products/flexcase-kb.jpg'),
    (@CatDeals,   N'Stellar X2 (Refurbished)', N'stellar-x2-refurb', N'SX2-RF', N'Certified refurbished X2.',                N'Grade A refurbishment, 12-month warranty.',                           649.00,  12, 0, 1, N'/Content/images/products/stellar-x2-refurb.jpg');
    PRINT '>> Seeded 10 products.';
END
ELSE PRINT '>> Products already present — seed skipped.';
GO

IF NOT EXISTS (SELECT 1 FROM dbo.ProductImages)
BEGIN
    INSERT dbo.ProductImages (ProductId, Url, AltText, DisplayOrder, IsPrimary)
    SELECT p.Id, p.ThumbnailUrl, p.Name + N' main image', 0, 1
    FROM dbo.Products p;
    PRINT '>> Seeded one primary image per product.';
END
ELSE PRINT '>> ProductImages already present — seed skipped.';
GO

IF NOT EXISTS (SELECT 1 FROM dbo.ProductVariants)
BEGIN
    INSERT dbo.ProductVariants (ProductId, Name, Sku, PriceAdjustment, Stock, IsActive)
    SELECT Id, N'16 GB / 512 GB',  Sku + N'-16-512', 0.00,   15, 1 FROM dbo.Products WHERE Slug = N'novabook-pro-14';
    INSERT dbo.ProductVariants (ProductId, Name, Sku, PriceAdjustment, Stock, IsActive)
    SELECT Id, N'32 GB / 1 TB',    Sku + N'-32-1TB', 200.00, 10, 1 FROM dbo.Products WHERE Slug = N'novabook-pro-14';
    INSERT dbo.ProductVariants (ProductId, Name, Sku, PriceAdjustment, Stock, IsActive)
    SELECT Id, N'Midnight',        Sku + N'-MID',    0.00,   20, 1 FROM dbo.Products WHERE Slug = N'stellar-x2';
    INSERT dbo.ProductVariants (ProductId, Name, Sku, PriceAdjustment, Stock, IsActive)
    SELECT Id, N'Silver',          Sku + N'-SLV',    0.00,   20, 1 FROM dbo.Products WHERE Slug = N'stellar-x2';
    INSERT dbo.ProductVariants (ProductId, Name, Sku, PriceAdjustment, Stock, IsActive)
    SELECT Id, N'Green',           Sku + N'-GRN',    10.00,  15, 1 FROM dbo.Products WHERE Slug = N'stellar-x2';
    INSERT dbo.ProductVariants (ProductId, Name, Sku, PriceAdjustment, Stock, IsActive)
    SELECT Id, N'White',           Sku + N'-WHT',    0.00,   60, 1 FROM dbo.Products WHERE Slug = N'aeropods-wireless';
    INSERT dbo.ProductVariants (ProductId, Name, Sku, PriceAdjustment, Stock, IsActive)
    SELECT Id, N'Black',           Sku + N'-BLK',    0.00,   60, 1 FROM dbo.Products WHERE Slug = N'aeropods-wireless';
    PRINT '>> Seeded product variants.';
END
ELSE PRINT '>> ProductVariants already present — seed skipped.';
GO

/* -------------------------------------------------------------------------------------
   STEP 7 — Application SQL login (PLACEHOLDER password — local dev only)
   Replace the placeholder before real use; never commit real credentials.
   ------------------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.server_principals WHERE name = N'legacy_app_user')
BEGIN
    CREATE LOGIN [legacy_app_user]
        WITH PASSWORD = N'ChangeMe!Dev2026_Qz',   -- placeholder only
             CHECK_POLICY = ON,                   -- meets Windows complexity rules
             CHECK_EXPIRATION = OFF;              -- no expiry for local dev
    PRINT '>> Created LOGIN [legacy_app_user] (placeholder password).';
END
ELSE PRINT '>> LOGIN [legacy_app_user] already exists — skipped.';
GO

USE [LegacyEcommerceDb];
GO
IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = N'legacy_app_user')
BEGIN
    CREATE USER [legacy_app_user] FOR LOGIN [legacy_app_user] WITH DEFAULT_SCHEMA = [dbo];
    PRINT '>> Created DATABASE USER [legacy_app_user].';
END
ELSE PRINT '>> DATABASE USER [legacy_app_user] already exists — skipped.';
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.database_role_members rm
    JOIN sys.database_principals r ON r.principal_id = rm.role_principal_id
    JOIN sys.database_principals m ON m.principal_id = rm.member_principal_id
    WHERE r.name = N'db_datareader' AND m.name = N'legacy_app_user')
    ALTER ROLE db_datareader ADD MEMBER [legacy_app_user];

IF NOT EXISTS (
    SELECT 1
    FROM sys.database_role_members rm
    JOIN sys.database_principals r ON r.principal_id = rm.role_principal_id
    JOIN sys.database_principals m ON m.principal_id = rm.member_principal_id
    WHERE r.name = N'db_datawriter' AND m.name = N'legacy_app_user')
    ALTER ROLE db_datawriter ADD MEMBER [legacy_app_user];
PRINT '>> Login permissions ensured (db_datareader + db_datawriter).';
GO

/* -------------------------------------------------------------------------------------
   STEP 8 — Verification report (read-only)
   ------------------------------------------------------------------------------------- */
SELECT 'Databases' AS [Check], name AS [Value] FROM sys.databases WHERE name = N'LegacyEcommerceDb';

SELECT 'Tables' AS [Check],
       t.name  AS [TableName],
       SUM(p.rows) AS [RowCounts]
FROM sys.tables t
JOIN sys.partitions p ON p.object_id = t.object_id AND p.index_id IN (0, 1)
GROUP BY t.name
ORDER BY t.name;

SELECT 'Indexes' AS [Check], t.name AS [TableName], i.name AS [IndexName], i.is_unique AS [IsUnique]
FROM sys.indexes i
JOIN sys.tables t ON t.object_id = i.object_id
WHERE i.type > 0 AND i.is_primary_key = 0
ORDER BY t.name, i.name;

SELECT 'Roles' AS [Check], Id, Name FROM dbo.AspNetRoles ORDER BY Name;

-- duplicate role-name guard (must return 0 rows after UX_AspNetRoles_Name is in place)
SELECT 'RoleNameDuplicates' AS [Check], Name AS [RoleName], COUNT(*) AS [Occurrences]
FROM dbo.AspNetRoles
GROUP BY Name
HAVING COUNT(*) > 1;

SELECT 'LoginUser' AS [Check], sp.name AS [Login], dp.name AS [DatabaseUser]
FROM sys.server_principals sp
LEFT JOIN sys.database_principals dp ON dp.name = sp.name
WHERE sp.name = N'legacy_app_user';

SELECT 'LoginPermissions' AS [Check], r.name AS [DatabaseRole], m.name AS [Member]
FROM sys.database_role_members rm
JOIN sys.database_principals r ON r.principal_id = rm.role_principal_id
JOIN sys.database_principals m ON m.principal_id = rm.member_principal_id
WHERE m.name = N'legacy_app_user'
ORDER BY r.name;
