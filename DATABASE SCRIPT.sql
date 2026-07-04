USE Wheelsell;
GO

IF OBJECT_ID('AdvertFeatures',         'U') IS NOT NULL DROP TABLE AdvertFeatures;
IF OBJECT_ID('Favorites',              'U') IS NOT NULL DROP TABLE Favorites;
IF OBJECT_ID('Notifications',          'U') IS NOT NULL DROP TABLE Notifications;
IF OBJECT_ID('Reviews',                'U') IS NOT NULL DROP TABLE Reviews;
IF OBJECT_ID('Messages',               'U') IS NOT NULL DROP TABLE Messages;
IF OBJECT_ID('Conversations',          'U') IS NOT NULL DROP TABLE Conversations;
IF OBJECT_ID('AdvertImages',           'U') IS NOT NULL DROP TABLE AdvertImages;
IF OBJECT_ID('AdvertVideos',           'U') IS NOT NULL DROP TABLE AdvertVideos;
IF OBJECT_ID('Adverts',                'U') IS NOT NULL DROP TABLE Adverts;
IF OBJECT_ID('Features',               'U') IS NOT NULL DROP TABLE Features;
IF OBJECT_ID('FeatureCategories',      'U') IS NOT NULL DROP TABLE FeatureCategories;
IF OBJECT_ID('CarModels',              'U') IS NOT NULL DROP TABLE CarModels;
IF OBJECT_ID('Brands',                 'U') IS NOT NULL DROP TABLE Brands;
IF OBJECT_ID('Currencies',             'U') IS NOT NULL DROP TABLE Currencies;
IF OBJECT_ID('PasswordResetTokens',    'U') IS NOT NULL DROP TABLE PasswordResetTokens;
IF OBJECT_ID('EmailConfirmationTokens','U') IS NOT NULL DROP TABLE EmailConfirmationTokens;
IF OBJECT_ID('RefreshTokens',          'U') IS NOT NULL DROP TABLE RefreshTokens;
IF OBJECT_ID('Users',                  'U') IS NOT NULL DROP TABLE Users;
GO

CREATE TABLE Users (
    Id               INT           IDENTITY(1,1) PRIMARY KEY,
    Username         NVARCHAR(50)  NOT NULL,
    Email            NVARCHAR(255) NOT NULL,
    PasswordHash     NVARCHAR(MAX) NOT NULL,
    Name             NVARCHAR(100) NOT NULL,
    Surname          NVARCHAR(100) NOT NULL,
    Phone            NVARCHAR(MAX) NULL,
    City             NVARCHAR(100) NOT NULL,
    County           NVARCHAR(100) NOT NULL,
    ProfilePhotoPath NVARCHAR(MAX) NULL,
    Role             INT           NOT NULL DEFAULT 0,
    IsEmailConfirmed BIT           NOT NULL DEFAULT 0,
    IsBanned         BIT           NOT NULL DEFAULT 0,
    BannedAt         DATETIME2     NULL,
    BanReason        NVARCHAR(MAX) NULL,
    CreatedAt        DATETIME2     NOT NULL DEFAULT GETUTCDATE(),
    IsDeleted        BIT           NOT NULL DEFAULT 0,
    DeletedAt        DATETIME2     NULL,
    CONSTRAINT UQ_Users_Username UNIQUE (Username),
    CONSTRAINT UQ_Users_Email    UNIQUE (Email)
);
GO

CREATE TABLE RefreshTokens (
    Id        INT           IDENTITY(1,1) PRIMARY KEY,
    UserId    INT           NOT NULL,
    Token     NVARCHAR(MAX) NOT NULL,
    ExpiresAt DATETIME2     NOT NULL,
    IsRevoked BIT           NOT NULL DEFAULT 0,
    CreatedAt DATETIME2     NOT NULL DEFAULT GETUTCDATE(),
    IsDeleted BIT           NOT NULL DEFAULT 0,
    DeletedAt DATETIME2     NULL,
    CONSTRAINT FK_RefreshTokens_Users FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
);

CREATE TABLE EmailConfirmationTokens (
    Id        INT           IDENTITY(1,1) PRIMARY KEY,
    UserId    INT           NOT NULL,
    Token     NVARCHAR(MAX) NOT NULL,
    ExpiresAt DATETIME2     NOT NULL,
    IsUsed    BIT           NOT NULL DEFAULT 0,
    CreatedAt DATETIME2     NOT NULL DEFAULT GETUTCDATE(),
    IsDeleted BIT           NOT NULL DEFAULT 0,
    DeletedAt DATETIME2     NULL,
    CONSTRAINT FK_EmailConfirmationTokens_Users FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
);

CREATE TABLE PasswordResetTokens (
    Id        INT           IDENTITY(1,1) PRIMARY KEY,
    UserId    INT           NOT NULL,
    Token     NVARCHAR(MAX) NOT NULL,
    ExpiresAt DATETIME2     NOT NULL,
    IsUsed    BIT           NOT NULL DEFAULT 0,
    CreatedAt DATETIME2     NOT NULL DEFAULT GETUTCDATE(),
    IsDeleted BIT           NOT NULL DEFAULT 0,
    DeletedAt DATETIME2     NULL,
    CONSTRAINT FK_PasswordResetTokens_Users FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
);
GO

CREATE TABLE Currencies (
    Id        INT           IDENTITY(1,1) PRIMARY KEY,
    Code      NVARCHAR(MAX) NOT NULL,
    Symbol    NVARCHAR(MAX) NOT NULL,
    Name      NVARCHAR(MAX) NOT NULL,
    CreatedAt DATETIME2     NOT NULL DEFAULT GETUTCDATE(),
    IsDeleted BIT           NOT NULL DEFAULT 0,
    DeletedAt DATETIME2     NULL,
    CONSTRAINT UQ_Currencies_Code UNIQUE (Code)
);

CREATE TABLE Brands (
    Id        INT           IDENTITY(1,1) PRIMARY KEY,
    Name      NVARCHAR(MAX) NOT NULL,
    LogoPath  NVARCHAR(MAX) NULL,
    CreatedAt DATETIME2     NOT NULL DEFAULT GETUTCDATE(),
    IsDeleted BIT           NOT NULL DEFAULT 0,
    DeletedAt DATETIME2     NULL
);

CREATE TABLE CarModels (
    Id        INT           IDENTITY(1,1) PRIMARY KEY,
    BrandId   INT           NOT NULL,
    Name      NVARCHAR(MAX) NOT NULL,
    CreatedAt DATETIME2     NOT NULL DEFAULT GETUTCDATE(),
    IsDeleted BIT           NOT NULL DEFAULT 0,
    DeletedAt DATETIME2     NULL,
    CONSTRAINT FK_CarModels_Brands FOREIGN KEY (BrandId) REFERENCES Brands(Id)
);

CREATE TABLE FeatureCategories (
    Id        INT           IDENTITY(1,1) PRIMARY KEY,
    Name      NVARCHAR(MAX) NOT NULL,
    [Order]   INT           NOT NULL DEFAULT 0,
    CreatedAt DATETIME2     NOT NULL DEFAULT GETUTCDATE(),
    IsDeleted BIT           NOT NULL DEFAULT 0,
    DeletedAt DATETIME2     NULL
);

CREATE TABLE Features (
    Id                INT           IDENTITY(1,1) PRIMARY KEY,
    FeatureCategoryId INT           NOT NULL,
    Name              NVARCHAR(MAX) NOT NULL,
    CreatedAt         DATETIME2     NOT NULL DEFAULT GETUTCDATE(),
    IsDeleted         BIT           NOT NULL DEFAULT 0,
    DeletedAt         DATETIME2     NULL,
    CONSTRAINT FK_Features_FeatureCategories FOREIGN KEY (FeatureCategoryId) REFERENCES FeatureCategories(Id)
);
GO

CREATE TABLE Adverts (
    Id                INT           IDENTITY(1,1) PRIMARY KEY,
    SellerId          INT           NOT NULL,
    BuyerId           INT           NULL,
    BrandId           INT           NOT NULL,
    CarModelId        INT           NOT NULL,
    CurrencyId        INT           NOT NULL,
    PreviousAdvertId  INT           NULL,
    Title             NVARCHAR(MAX) NOT NULL,
    Description       NVARCHAR(MAX) NOT NULL,
    Trim              NVARCHAR(MAX) NULL,
    Year              INT           NOT NULL,
    Mileage           INT           NOT NULL,
    FuelType          INT           NOT NULL,
    Transmission      INT           NOT NULL,
    BodyType          INT           NOT NULL,
    Drivetrain        INT           NOT NULL,
    EnginePowerHp     INT           NOT NULL,
    EngineSizeLiters  DECIMAL(4,1)  NOT NULL,
    Color             NVARCHAR(MAX) NOT NULL,
    OwnersCount       INT           NOT NULL,
    Condition         INT           NOT NULL,
    DamageSeverity    INT           NULL,
    RepairDescription NVARCHAR(MAX) NULL,
    Price             DECIMAL(18,2) NOT NULL,
    SellerFullName    NVARCHAR(MAX) NOT NULL,
    SellerCity        NVARCHAR(MAX) NOT NULL,
    SellerEmail       NVARCHAR(MAX) NOT NULL,
    SellerPhone       NVARCHAR(MAX) NOT NULL,
    Status            INT           NOT NULL DEFAULT 0,
    SoldAt            DATETIME2     NULL,
    IsBanned          BIT           NOT NULL DEFAULT 0,
    BannedAt          DATETIME2     NULL,
    BanReason         NVARCHAR(MAX) NULL,
    CreatedAt         DATETIME2     NOT NULL DEFAULT GETUTCDATE(),
    IsDeleted         BIT           NOT NULL DEFAULT 0,
    DeletedAt         DATETIME2     NULL,
    CONSTRAINT FK_Adverts_Seller   FOREIGN KEY (SellerId)         REFERENCES Users(Id),
    CONSTRAINT FK_Adverts_Buyer    FOREIGN KEY (BuyerId)          REFERENCES Users(Id),
    CONSTRAINT FK_Adverts_Brand    FOREIGN KEY (BrandId)          REFERENCES Brands(Id),
    CONSTRAINT FK_Adverts_Model    FOREIGN KEY (CarModelId)       REFERENCES CarModels(Id),
    CONSTRAINT FK_Adverts_Currency FOREIGN KEY (CurrencyId)       REFERENCES Currencies(Id),
    CONSTRAINT FK_Adverts_Previous FOREIGN KEY (PreviousAdvertId) REFERENCES Adverts(Id)
);

CREATE TABLE AdvertImages (
    Id        INT           IDENTITY(1,1) PRIMARY KEY,
    AdvertId  INT           NOT NULL,
    Path      NVARCHAR(MAX) NOT NULL,
    [Order]   INT           NOT NULL DEFAULT 0,
    CreatedAt DATETIME2     NOT NULL DEFAULT GETUTCDATE(),
    IsDeleted BIT           NOT NULL DEFAULT 0,
    DeletedAt DATETIME2     NULL,
    CONSTRAINT FK_AdvertImages_Adverts FOREIGN KEY (AdvertId) REFERENCES Adverts(Id) ON DELETE CASCADE
);

CREATE TABLE AdvertVideos (
    Id        INT           IDENTITY(1,1) PRIMARY KEY,
    AdvertId  INT           NOT NULL,
    Path      NVARCHAR(MAX) NOT NULL,
    [Order]   INT           NOT NULL DEFAULT 0,
    CreatedAt DATETIME2     NOT NULL DEFAULT GETUTCDATE(),
    IsDeleted BIT           NOT NULL DEFAULT 0,
    DeletedAt DATETIME2     NULL,
    CONSTRAINT FK_AdvertVideos_Adverts FOREIGN KEY (AdvertId) REFERENCES Adverts(Id) ON DELETE CASCADE
);

CREATE TABLE AdvertFeatures (
    AdvertId  INT NOT NULL,
    FeatureId INT NOT NULL,
    CONSTRAINT PK_AdvertFeatures         PRIMARY KEY (AdvertId, FeatureId),
    CONSTRAINT FK_AdvertFeatures_Advert  FOREIGN KEY (AdvertId)  REFERENCES Adverts(Id)  ON DELETE CASCADE,
    CONSTRAINT FK_AdvertFeatures_Feature FOREIGN KEY (FeatureId) REFERENCES Features(Id)
);
GO

CREATE TABLE Conversations (
    Id        INT       IDENTITY(1,1) PRIMARY KEY,
    AdvertId  INT       NOT NULL,
    BuyerId   INT       NOT NULL,
    SellerId  INT       NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    IsDeleted BIT       NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,
    CONSTRAINT FK_Conversations_Advert FOREIGN KEY (AdvertId)  REFERENCES Adverts(Id) ON DELETE CASCADE,
    CONSTRAINT FK_Conversations_Buyer  FOREIGN KEY (BuyerId)   REFERENCES Users(Id),
    CONSTRAINT FK_Conversations_Seller FOREIGN KEY (SellerId)  REFERENCES Users(Id)
);

CREATE TABLE Messages (
    Id             INT           IDENTITY(1,1) PRIMARY KEY,
    ConversationId INT           NOT NULL,
    SenderId       INT           NOT NULL,
    Content        NVARCHAR(MAX) NOT NULL,
    IsRead         BIT           NOT NULL DEFAULT 0,
    CreatedAt      DATETIME2     NOT NULL DEFAULT GETUTCDATE(),
    IsDeleted      BIT           NOT NULL DEFAULT 0,
    DeletedAt      DATETIME2     NULL,
    CONSTRAINT FK_Messages_Conversation FOREIGN KEY (ConversationId) REFERENCES Conversations(Id) ON DELETE CASCADE,
    CONSTRAINT FK_Messages_Sender       FOREIGN KEY (SenderId)       REFERENCES Users(Id)
);

CREATE TABLE Reviews (
    Id         INT           IDENTITY(1,1) PRIMARY KEY,
    AdvertId   INT           NOT NULL,
    ReviewerId INT           NOT NULL,
    RevieweeId INT           NOT NULL,
    Rating     INT           NOT NULL,
    Comment    NVARCHAR(MAX) NOT NULL,
    CreatedAt  DATETIME2     NOT NULL DEFAULT GETUTCDATE(),
    IsDeleted  BIT           NOT NULL DEFAULT 0,
    DeletedAt  DATETIME2     NULL,
    CONSTRAINT FK_Reviews_Advert   FOREIGN KEY (AdvertId)   REFERENCES Adverts(Id),
    CONSTRAINT FK_Reviews_Reviewer FOREIGN KEY (ReviewerId) REFERENCES Users(Id),
    CONSTRAINT FK_Reviews_Reviewee FOREIGN KEY (RevieweeId) REFERENCES Users(Id)
);

CREATE TABLE Favorites (
    Id        INT       IDENTITY(1,1) PRIMARY KEY,
    UserId    INT       NOT NULL,
    AdvertId  INT       NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    IsDeleted BIT       NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,
    CONSTRAINT UQ_Favorites_User_Advert UNIQUE (UserId, AdvertId),
    CONSTRAINT FK_Favorites_User   FOREIGN KEY (UserId)   REFERENCES Users(Id)   ON DELETE CASCADE,
    CONSTRAINT FK_Favorites_Advert FOREIGN KEY (AdvertId) REFERENCES Adverts(Id) ON DELETE CASCADE
);

CREATE TABLE Notifications (
    Id                    INT           IDENTITY(1,1) PRIMARY KEY,
    UserId                INT           NOT NULL,
    Type                  INT           NOT NULL,
    Message               NVARCHAR(MAX) NOT NULL,
    IsRead                BIT           NOT NULL DEFAULT 0,
    RelatedAdvertId       INT           NULL,
    RelatedConversationId INT           NULL,
    CreatedAt             DATETIME2     NOT NULL DEFAULT GETUTCDATE(),
    IsDeleted             BIT           NOT NULL DEFAULT 0,
    DeletedAt             DATETIME2     NULL,
    CONSTRAINT FK_Notifications_User FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
);
GO

SET IDENTITY_INSERT Currencies ON;
INSERT INTO Currencies (Id, Code, Symbol, Name) VALUES
(1, 'EUR', N'€', 'Euro'),
(2, 'USD', N'$', 'US Dollar'),
(3, 'GBP', N'£', 'British Pound'),
(4, 'RUB', N'₽', 'Russian Ruble'),
(5, 'UAH', N'₴', 'Ukrainian Hryvnia'),
(6, 'CNY', N'¥', 'Chinese Yuan');
SET IDENTITY_INSERT Currencies OFF;

SET IDENTITY_INSERT Brands ON;
INSERT INTO Brands (Id, Name) VALUES
(1,  'Volkswagen'),
(2,  'BMW'),
(3,  'Mercedes-Benz'),
(4,  'Audi'),
(5,  'Toyota'),
(6,  'Ford'),
(7,  'Opel'),
(8,  'Renault'),
(9,  'Peugeot'),
(10, 'Skoda');
SET IDENTITY_INSERT Brands OFF;

SET IDENTITY_INSERT CarModels ON;
INSERT INTO CarModels (Id, BrandId, Name) VALUES
(1,  1,  'Golf'),
(2,  1,  'Passat'),
(3,  1,  'Tiguan'),
(4,  2,  '3 Series'),
(5,  2,  '5 Series'),
(6,  2,  'X5'),
(7,  3,  'C-Class'),
(8,  3,  'E-Class'),
(9,  3,  'GLE'),
(10, 4,  'A4'),
(11, 4,  'A6'),
(12, 4,  'Q5'),
(13, 5,  'Corolla'),
(14, 5,  'RAV4'),
(15, 6,  'Focus'),
(16, 6,  'Kuga'),
(17, 7,  'Astra'),
(18, 7,  'Insignia'),
(19, 8,  'Megane'),
(20, 8,  'Clio'),
(21, 9,  '308'),
(22, 9,  '3008'),
(23, 10, 'Octavia'),
(24, 10, 'Superb');
SET IDENTITY_INSERT CarModels OFF;

SET IDENTITY_INSERT FeatureCategories ON;
INSERT INTO FeatureCategories (Id, Name, [Order]) VALUES
(1, 'Safety',      1),
(2, 'Comfort',     2),
(3, 'Other',       3),
(4, 'Protection',  4),
(5, 'Interior',    5),
(6, 'Exterior',    6),
(7, 'Specialized', 7);
SET IDENTITY_INSERT FeatureCategories OFF;

SET IDENTITY_INSERT Features ON;
INSERT INTO Features (Id, FeatureCategoryId, Name) VALUES
(1,  1, 'GPS Tracking System'),
(2,  1, 'Adaptive Headlights'),
(3,  1, 'Anti-lock Braking System'),
(4,  1, 'Rear Airbags'),
(5,  1, 'Front Airbags'),
(6,  1, 'Side Airbags'),
(7,  1, 'Electronic Brake-force Distribution'),
(8,  1, 'Electronic Stability Program'),
(9,  1, 'Tire Pressure Monitoring'),
(10, 1, 'Parking Sensors'),
(11, 1, 'ISOFIX System'),
(12, 1, 'Traction Control System'),
(13, 1, 'Anti-slip Regulation'),
(14, 1, 'Distance Control System'),
(15, 1, 'Descent Control System'),
(16, 2, '360 Camera / Rear Camera'),
(17, 2, 'Apple CarPlay / Android Auto'),
(18, 2, 'Auto Start-Stop Function'),
(19, 2, 'Bluetooth / Handsfree System'),
(20, 2, 'DVD / TV'),
(21, 2, 'Head-up Display'),
(22, 2, 'Steptronic / Tiptronic Gearbox'),
(23, 2, 'USB / Audio / Video / AUX Inputs'),
(24, 2, 'Automatic Tailgate'),
(25, 2, 'Adaptive Suspension'),
(26, 2, 'Keyless Start'),
(27, 2, 'Differential Lock'),
(28, 2, 'On-board Computer'),
(29, 2, 'Fast / Slow Gear Modes'),
(30, 2, 'Seat Ventilation'),
(31, 2, 'Light Sensor'),
(32, 2, 'Electric Mirrors'),
(33, 2, 'Electric Windows'),
(34, 2, 'Electric Seat Adjustment'),
(35, 2, 'Electric Power Steering'),
(36, 2, 'Air Conditioning'),
(37, 2, 'Climate Control'),
(38, 2, 'Multifunction Steering Wheel'),
(39, 2, 'Navigation System'),
(40, 2, 'Heated Steering Wheel'),
(41, 2, 'Heating'),
(42, 2, 'Heated Windshield'),
(43, 2, 'Heated Seats'),
(44, 2, 'Steering Wheel Adjustment'),
(45, 2, 'Rain Sensor'),
(46, 2, 'Power Steering'),
(47, 2, 'Headlight Washer System'),
(48, 3, '4x4'),
(49, 3, '7 Seats'),
(50, 3, 'Buy Back Option'),
(51, 3, 'Trade-in Possible'),
(52, 3, 'LPG System'),
(53, 3, 'Long Wheelbase'),
(54, 3, 'Reserved / Sold'),
(55, 3, 'Has Been in an Accident'),
(56, 3, 'Short Wheelbase'),
(57, 3, 'Leasing Available'),
(58, 3, 'CNG System'),
(59, 3, 'For Parts'),
(60, 3, 'Fully Serviced'),
(61, 3, 'Recently Imported'),
(62, 3, 'Registered'),
(63, 3, 'Service Book Available'),
(64, 3, 'Tuned'),
(65, 4, 'Offroad Package'),
(66, 4, 'Alarm System'),
(67, 4, 'Armored'),
(68, 4, 'Comprehensive Insurance Included'),
(69, 4, 'Winch'),
(70, 4, 'Central Locking'),
(71, 5, 'Velour Interior'),
(72, 5, 'Right-hand Drive'),
(73, 5, 'Leather Interior'),
(74, 5, 'Light Interior'),
(75, 6, '2/3 Doors'),
(76, 6, '4/5 Doors'),
(77, 6, 'LED Headlights'),
(78, 6, 'Xenon Headlights'),
(79, 6, 'Alloy Wheels'),
(80, 6, 'Metallic Paint'),
(81, 6, 'Panoramic Sunroof'),
(82, 6, 'Roof Rails'),
(83, 6, 'Spoilers'),
(84, 6, 'Tow Bar'),
(85, 6, 'Halogen Headlights'),
(86, 6, 'Running Boards'),
(87, 7, 'Taxi'),
(88, 7, 'Accessible for Disabled'),
(89, 7, 'Hearse'),
(90, 7, 'Ambulance'),
(91, 7, 'Driving School Vehicle'),
(92, 7, 'Refrigerated Vehicle'),
(93, 7, 'N1 Homologation');
SET IDENTITY_INSERT Features OFF;
GO
