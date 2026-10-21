USE master;
GO

IF EXISTS (SELECT * FROM sys.databases WHERE name = 'MasterNetDb')
BEGIN
    ALTER DATABASE MasterNetDb SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE MasterNetDb;
END
GO

CREATE DATABASE MasterNetDb;
GO

USE MasterNetDb;
GO

-- =============================================
-- Domain Tables
-- =============================================

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='devices' AND xtype='U')
BEGIN
    CREATE TABLE devices (
        Id UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
        name NVARCHAR(200) NOT NULL
    );
END
GO

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='prices' AND xtype='U')
BEGIN
    CREATE TABLE prices (
        Id UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
        Name VARCHAR(250) NULL,
        CurrentPrice DECIMAL(10,2) NOT NULL,
        PromotionPrice DECIMAL(10,2) NOT NULL
    );
END
GO

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='instructors' AND xtype='U')
BEGIN
    CREATE TABLE instructors (
        Id UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
        FirstName NVARCHAR(MAX) NULL,
        LastName NVARCHAR(MAX) NULL,
        Degree NVARCHAR(MAX) NULL
    );
END
GO

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='courses' AND xtype='U')
BEGIN
    CREATE TABLE courses (
        Id UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
        Title NVARCHAR(MAX) NULL,
        Description NVARCHAR(MAX) NULL,
        PublishedAt DATETIME2 NULL
    );
END
GO

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='ratings' AND xtype='U')
BEGIN
    CREATE TABLE ratings (
        Id UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
        Student NVARCHAR(MAX) NULL,
        Score INT NOT NULL,
        Comment NVARCHAR(MAX) NULL,
        CourseId UNIQUEIDENTIFIER NULL,
        CONSTRAINT FK_ratings_courses_CourseId FOREIGN KEY (CourseId) 
            REFERENCES courses (Id) ON DELETE NO ACTION
    );
END
GO

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='images' AND xtype='U')
BEGIN
    CREATE TABLE images (
        Id UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
        Url NVARCHAR(MAX) NULL,
        PublicId NVARCHAR(MAX) NULL,
        CourseId UNIQUEIDENTIFIER NULL,
        CONSTRAINT FK_images_courses_CourseId FOREIGN KEY (CourseId) 
            REFERENCES courses (Id) ON DELETE CASCADE
    );
END
GO

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='courses_instructors' AND xtype='U')
BEGIN
    CREATE TABLE courses_instructors (
        InstructorId UNIQUEIDENTIFIER NOT NULL,
        CourseId UNIQUEIDENTIFIER NOT NULL,
        CONSTRAINT PK_courses_instructors PRIMARY KEY (InstructorId, CourseId),
        CONSTRAINT FK_courses_instructors_instructors FOREIGN KEY (InstructorId) 
            REFERENCES instructors (Id) ON DELETE CASCADE,
        CONSTRAINT FK_courses_instructors_courses FOREIGN KEY (CourseId) 
            REFERENCES courses (Id) ON DELETE CASCADE
    );
END
GO

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='courses_prices' AND xtype='U')
BEGIN
    CREATE TABLE courses_prices (
        PriceId UNIQUEIDENTIFIER NOT NULL,
        CourseId UNIQUEIDENTIFIER NOT NULL,
        CONSTRAINT PK_courses_prices PRIMARY KEY (PriceId, CourseId),
        CONSTRAINT FK_courses_prices_prices FOREIGN KEY (PriceId) 
            REFERENCES prices (Id) ON DELETE CASCADE,
        CONSTRAINT FK_courses_prices_courses FOREIGN KEY (CourseId) 
            REFERENCES courses (Id) ON DELETE CASCADE
    );
END
GO

-- =============================================
-- ASP.NET Identity Tables
-- =============================================

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='AspNetRoles' AND xtype='U')
BEGIN
    CREATE TABLE AspNetRoles (
        Id NVARCHAR(450) PRIMARY KEY,
        Name NVARCHAR(256) NULL,
        NormalizedName NVARCHAR(256) NULL,
        ConcurrencyStamp NVARCHAR(MAX) NULL
    );
    CREATE UNIQUE INDEX RoleNameIndex ON AspNetRoles(NormalizedName) 
        WHERE NormalizedName IS NOT NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='AspNetUsers' AND xtype='U')
BEGIN
    CREATE TABLE AspNetUsers (
        Id NVARCHAR(450) PRIMARY KEY,
        UserName NVARCHAR(256) NULL,
        NormalizedUserName NVARCHAR(256) NULL,
        Email NVARCHAR(256) NULL,
        NormalizedEmail NVARCHAR(256) NULL,
        EmailConfirmed BIT NOT NULL,
        PasswordHash NVARCHAR(MAX) NULL,
        SecurityStamp NVARCHAR(MAX) NULL,
        ConcurrencyStamp NVARCHAR(MAX) NULL,
        PhoneNumber NVARCHAR(MAX) NULL,
        PhoneNumberConfirmed BIT NOT NULL,
        TwoFactorEnabled BIT NOT NULL,
        LockoutEnd DATETIMEOFFSET NULL,
        LockoutEnabled BIT NOT NULL,
        AccessFailedCount INT NOT NULL,
        -- Custom fields from AppUser
        FullName NVARCHAR(MAX) NULL,
        Major NVARCHAR(MAX) NULL
    );
    CREATE INDEX EmailIndex ON AspNetUsers(NormalizedEmail);
    CREATE UNIQUE INDEX UserNameIndex ON AspNetUsers(NormalizedUserName) 
        WHERE NormalizedUserName IS NOT NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='AspNetUserRoles' AND xtype='U')
BEGIN
    CREATE TABLE AspNetUserRoles (
        UserId NVARCHAR(450) NOT NULL,
        RoleId NVARCHAR(450) NOT NULL,
        CONSTRAINT PK_AspNetUserRoles PRIMARY KEY (UserId, RoleId),
        CONSTRAINT FK_AspNetUserRoles_AspNetRoles FOREIGN KEY (RoleId) 
            REFERENCES AspNetRoles (Id) ON DELETE CASCADE,
        CONSTRAINT FK_AspNetUserRoles_AspNetUsers FOREIGN KEY (UserId) 
            REFERENCES AspNetUsers (Id) ON DELETE CASCADE
    );
    CREATE INDEX IX_AspNetUserRoles_RoleId ON AspNetUserRoles(RoleId);
END
GO

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='AspNetRoleClaims' AND xtype='U')
BEGIN
    CREATE TABLE AspNetRoleClaims (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        RoleId NVARCHAR(450) NOT NULL,
        ClaimType NVARCHAR(MAX) NULL,
        ClaimValue NVARCHAR(MAX) NULL,
        CONSTRAINT FK_AspNetRoleClaims_AspNetRoles FOREIGN KEY (RoleId) 
            REFERENCES AspNetRoles (Id) ON DELETE CASCADE
    );
    CREATE INDEX IX_AspNetRoleClaims_RoleId ON AspNetRoleClaims(RoleId);
END
GO

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='AspNetUserClaims' AND xtype='U')
BEGIN
    CREATE TABLE AspNetUserClaims (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        UserId NVARCHAR(450) NOT NULL,
        ClaimType NVARCHAR(MAX) NULL,
        ClaimValue NVARCHAR(MAX) NULL,
        CONSTRAINT FK_AspNetUserClaims_AspNetUsers FOREIGN KEY (UserId) 
            REFERENCES AspNetUsers (Id) ON DELETE CASCADE
    );
    CREATE INDEX IX_AspNetUserClaims_UserId ON AspNetUserClaims(UserId);
END
GO

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='AspNetUserLogins' AND xtype='U')
BEGIN
    CREATE TABLE AspNetUserLogins (
        LoginProvider NVARCHAR(450) NOT NULL,
        ProviderKey NVARCHAR(450) NOT NULL,
        ProviderDisplayName NVARCHAR(MAX) NULL,
        UserId NVARCHAR(450) NOT NULL,
        CONSTRAINT PK_AspNetUserLogins PRIMARY KEY (LoginProvider, ProviderKey),
        CONSTRAINT FK_AspNetUserLogins_AspNetUsers FOREIGN KEY (UserId) 
            REFERENCES AspNetUsers (Id) ON DELETE CASCADE
    );
    CREATE INDEX IX_AspNetUserLogins_UserId ON AspNetUserLogins(UserId);
END
GO

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='AspNetUserTokens' AND xtype='U')
BEGIN
    CREATE TABLE AspNetUserTokens (
        UserId NVARCHAR(450) NOT NULL,
        LoginProvider NVARCHAR(450) NOT NULL,
        Name NVARCHAR(450) NOT NULL,
        Value NVARCHAR(MAX) NULL,
        CONSTRAINT PK_AspNetUserTokens PRIMARY KEY (UserId, LoginProvider, Name),
        CONSTRAINT FK_AspNetUserTokens_AspNetUsers FOREIGN KEY (UserId) 
            REFERENCES AspNetUsers (Id) ON DELETE CASCADE
    );
END
GO

-- =============================================
-- Seed Data
-- =============================================

-- Seed Roles
IF NOT EXISTS (SELECT * FROM AspNetRoles WHERE Id = 'd3b07384-d9a0-4c9b-8a0d-1b2e2b3c4d5e')
BEGIN
    INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
    VALUES 
        ('d3b07384-d9a0-4c9b-8a0d-1b2e2b3c4d5e', 'ADMIN', 'ADMIN', NEWID()),
        ('e4b07384-d9a0-4c9b-8a0d-1b2e2b3c4d5f', 'CLIENT', 'CLIENT', NEWID());
END
GO

-- Seed Role Claims (Policies)
IF NOT EXISTS (SELECT * FROM AspNetRoleClaims)
BEGIN
    INSERT INTO AspNetRoleClaims (RoleId, ClaimType, ClaimValue)
    VALUES 
        -- ADMIN Role Claims
        ('d3b07384-d9a0-4c9b-8a0d-1b2e2b3c4d5e', 'POLICIES', 'COURSE_READ'),
        ('d3b07384-d9a0-4c9b-8a0d-1b2e2b3c4d5e', 'POLICIES', 'COURSE_UPDATE'),
        ('d3b07384-d9a0-4c9b-8a0d-1b2e2b3c4d5e', 'POLICIES', 'COURSE_WRITE'),
        ('d3b07384-d9a0-4c9b-8a0d-1b2e2b3c4d5e', 'POLICIES', 'COURSE_DELETE'),
        ('d3b07384-d9a0-4c9b-8a0d-1b2e2b3c4d5e', 'POLICIES', 'INSTRUCTOR_READ'),
        ('d3b07384-d9a0-4c9b-8a0d-1b2e2b3c4d5e', 'POLICIES', 'INSTRUCTOR_UPDATE'),
        ('d3b07384-d9a0-4c9b-8a0d-1b2e2b3c4d5e', 'POLICIES', 'INSTRUCTOR_CREATE'),
        ('d3b07384-d9a0-4c9b-8a0d-1b2e2b3c4d5e', 'POLICIES', 'RATING_READ'),
        ('d3b07384-d9a0-4c9b-8a0d-1b2e2b3c4d5e', 'POLICIES', 'RATING_DELETE'),
        ('d3b07384-d9a0-4c9b-8a0d-1b2e2b3c4d5e', 'POLICIES', 'RATING_CREATE'),
        
        -- CLIENT Role Claims
        ('e4b07384-d9a0-4c9b-8a0d-1b2e2b3c4d5f', 'POLICIES', 'COURSE_READ'),
        ('e4b07384-d9a0-4c9b-8a0d-1b2e2b3c4d5f', 'POLICIES', 'INSTRUCTOR_READ'),
        ('e4b07384-d9a0-4c9b-8a0d-1b2e2b3c4d5f', 'POLICIES', 'RATING_READ'),
        ('e4b07384-d9a0-4c9b-8a0d-1b2e2b3c4d5f', 'POLICIES', 'RATING_CREATE');
END
GO

-- Seed Devices
IF NOT EXISTS (SELECT * FROM devices)
BEGIN
    INSERT INTO devices (Id, name)
    VALUES 
        (NEWID(), 'iPhone 15 Pro'),
        (NEWID(), 'Samsung Galaxy S24'),
        (NEWID(), 'Google Pixel 8'),
        (NEWID(), 'iPad Air'),
        (NEWID(), 'MacBook Pro M3'),
        (NEWID(), 'Dell XPS 15'),
        (NEWID(), 'Surface Pro 9'),
        (NEWID(), 'Apple Watch Series 9'),
        (NEWID(), 'Samsung Galaxy Watch 6'),
        (NEWID(), 'Sony WH-1000XM5');
END
GO

-- Seed Prices
IF NOT EXISTS (SELECT * FROM prices)
BEGIN
    INSERT INTO prices (Id, Name, CurrentPrice, PromotionPrice)
    VALUES 
        ('11111111-1111-1111-1111-111111111111', 'Free', 0.00, 0.00),
        ('22222222-2222-2222-2222-222222222222', 'Basic', 29.99, 19.99),
        ('33333333-3333-3333-3333-333333333333', 'Standard', 49.99, 39.99),
        ('44444444-4444-4444-4444-444444444444', 'Premium', 99.99, 79.99),
        ('55555555-5555-5555-5555-555555555555', 'Enterprise', 199.99, 149.99);
END
GO

-- Seed Instructors
IF NOT EXISTS (SELECT * FROM instructors)
BEGIN
    INSERT INTO instructors (Id, FirstName, LastName, Degree)
    VALUES 
        ('a1111111-1111-1111-1111-111111111111', 'John', 'Smith', 'PhD in Computer Science'),
        ('a2222222-2222-2222-2222-222222222222', 'Maria', 'Garcia', 'Master in Software Engineering'),
        ('a3333333-3333-3333-3333-333333333333', 'David', 'Chen', 'PhD in Artificial Intelligence'),
        ('a4444444-4444-4444-4444-444444444444', 'Sarah', 'Johnson', 'Master in Data Science'),
        ('a5555555-5555-5555-5555-555555555555', 'Michael', 'Brown', 'PhD in Cybersecurity');
END
GO

-- Seed Courses
IF NOT EXISTS (SELECT * FROM courses)
BEGIN
    INSERT INTO courses (Id, Title, Description, PublishedAt)
    VALUES 
        ('c1111111-1111-1111-1111-111111111111', 'ASP.NET Core Fundamentals', 'Learn the basics of ASP.NET Core web development', '2024-01-15'),
        ('c2222222-2222-2222-2222-222222222222', 'Advanced C# Programming', 'Master advanced C# concepts and patterns', '2024-02-01'),
        ('c3333333-3333-3333-3333-333333333333', 'Blazor WebAssembly Complete Guide', 'Build modern web applications with Blazor', '2024-03-10'),
        ('c4444444-4444-4444-4444-444444444444', 'Entity Framework Core Mastery', 'Deep dive into EF Core and database design', '2024-04-05'),
        ('c5555555-5555-5555-5555-555555555555', 'Microservices with .NET', 'Build scalable microservices architecture', '2024-05-20'),
        ('c6666666-6666-6666-6666-666666666666', 'Azure DevOps and CI/CD', 'Implement continuous integration and deployment', '2024-06-15'),
        ('c7777777-7777-7777-7777-777777777777', 'Clean Architecture in .NET', 'Learn clean code principles and architecture', '2024-07-01'),
        ('c8888888-8888-8888-8888-888888888888', 'Docker and Kubernetes for .NET', 'Containerize and orchestrate .NET applications', '2024-08-10');
END
GO

-- Seed Courses-Instructors
IF NOT EXISTS (SELECT * FROM courses_instructors)
BEGIN
    INSERT INTO courses_instructors (CourseId, InstructorId)
    VALUES 
        ('c1111111-1111-1111-1111-111111111111', 'a1111111-1111-1111-1111-111111111111'),
        ('c1111111-1111-1111-1111-111111111111', 'a2222222-2222-2222-2222-222222222222'),
        ('c2222222-2222-2222-2222-222222222222', 'a1111111-1111-1111-1111-111111111111'),
        ('c3333333-3333-3333-3333-333333333333', 'a2222222-2222-2222-2222-222222222222'),
        ('c4444444-4444-4444-4444-444444444444', 'a3333333-3333-3333-3333-333333333333'),
        ('c5555555-5555-5555-5555-555555555555', 'a4444444-4444-4444-4444-444444444444'),
        ('c6666666-6666-6666-6666-666666666666', 'a5555555-5555-5555-5555-555555555555'),
        ('c7777777-7777-7777-7777-777777777777', 'a1111111-1111-1111-1111-111111111111'),
        ('c8888888-8888-8888-8888-888888888888', 'a5555555-5555-5555-5555-555555555555');
END
GO

-- Seed Courses-Prices
IF NOT EXISTS (SELECT * FROM courses_prices)
BEGIN
    INSERT INTO courses_prices (CourseId, PriceId)
    VALUES 
        ('c1111111-1111-1111-1111-111111111111', '22222222-2222-2222-2222-222222222222'),
        ('c2222222-2222-2222-2222-222222222222', '33333333-3333-3333-3333-333333333333'),
        ('c3333333-3333-3333-3333-333333333333', '33333333-3333-3333-3333-333333333333'),
        ('c4444444-4444-4444-4444-444444444444', '44444444-4444-4444-4444-444444444444'),
        ('c5555555-5555-5555-5555-555555555555', '44444444-4444-4444-4444-444444444444'),
        ('c6666666-6666-6666-6666-666666666666', '55555555-5555-5555-5555-555555555555'),
        ('c7777777-7777-7777-7777-777777777777', '33333333-3333-3333-3333-333333333333'),
        ('c8888888-8888-8888-8888-888888888888', '55555555-5555-5555-5555-555555555555');
END
GO

-- Seed Ratings
IF NOT EXISTS (SELECT * FROM ratings)
BEGIN
    INSERT INTO ratings (Id, Student, Score, Comment, CourseId)
    VALUES 
        (NEWID(), 'Alice Thompson', 5, 'Excellent course! Very comprehensive and well explained.', 'c1111111-1111-1111-1111-111111111111'),
        (NEWID(), 'Bob Martinez', 4, 'Good content, but could use more practical examples.', 'c1111111-1111-1111-1111-111111111111'),
        (NEWID(), 'Carol Wilson', 5, 'Outstanding! The instructor is very knowledgeable.', 'c2222222-2222-2222-2222-222222222222'),
        (NEWID(), 'David Lee', 5, 'Best Blazor course I have taken. Highly recommend!', 'c3333333-3333-3333-3333-333333333333'),
        (NEWID(), 'Emma Davis', 4, 'Very detailed explanation of EF Core concepts.', 'c4444444-4444-4444-4444-444444444444'),
        (NEWID(), 'Frank Anderson', 5, 'Great introduction to microservices architecture.', 'c5555555-5555-5555-5555-555555555555'),
        (NEWID(), 'Grace Taylor', 4, 'Solid CI/CD practices and real-world examples.', 'c6666666-6666-6666-6666-666666666666'),
        (NEWID(), 'Henry Moore', 5, 'Clean Architecture principles explained perfectly.', 'c7777777-7777-7777-7777-777777777777'),
        (NEWID(), 'Isabel Clark', 5, 'Docker and K8s made easy! Loved this course.', 'c8888888-8888-8888-8888-888888888888'),
        (NEWID(), 'Jack White', 4, 'Very practical and hands-on approach.', 'c1111111-1111-1111-1111-111111111111');
END
GO

-- Seed Images (Photos)
IF NOT EXISTS (SELECT * FROM images)
BEGIN
    INSERT INTO images (Id, Url, PublicId, CourseId)
    VALUES 
        (NEWID(), 'https://res.cloudinary.com/demo/image/upload/aspnet-core.jpg', 'aspnet-core', 'c1111111-1111-1111-1111-111111111111'),
        (NEWID(), 'https://res.cloudinary.com/demo/image/upload/csharp-advanced.jpg', 'csharp-advanced', 'c2222222-2222-2222-2222-222222222222'),
        (NEWID(), 'https://res.cloudinary.com/demo/image/upload/blazor-wasm.jpg', 'blazor-wasm', 'c3333333-3333-3333-3333-333333333333'),
        (NEWID(), 'https://res.cloudinary.com/demo/image/upload/ef-core.jpg', 'ef-core', 'c4444444-4444-4444-4444-444444444444'),
        (NEWID(), 'https://res.cloudinary.com/demo/image/upload/microservices.jpg', 'microservices', 'c5555555-5555-5555-5555-555555555555'),
        (NEWID(), 'https://res.cloudinary.com/demo/image/upload/azure-devops.jpg', 'azure-devops', 'c6666666-6666-6666-6666-666666666666'),
        (NEWID(), 'https://res.cloudinary.com/demo/image/upload/clean-arch.jpg', 'clean-arch', 'c7777777-7777-7777-7777-777777777777'),
        (NEWID(), 'https://res.cloudinary.com/demo/image/upload/docker-k8s.jpg', 'docker-k8s', 'c8888888-8888-8888-8888-888888888888');
END
GO

PRINT 'Database MasterNetDb created and seeded successfully!';
GO