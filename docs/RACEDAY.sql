CREATE DATABASE RaceDay;
GO

USE RaceDay;
GO


-- =============================================
-- 1. Users
-- =============================================
CREATE TABLE Users
(
    UserID INT IDENTITY(1,1) PRIMARY KEY,
    FirstName NVARCHAR(100) NOT NULL,
    LastName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(150) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(255) NOT NULL,
    PhoneNumber NVARCHAR(20) NULL,
    Role NVARCHAR(20) NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE()
);
GO


-- =============================================
-- 2. Organisers
-- =============================================
CREATE TABLE Organisers
(
    OrganiserID INT IDENTITY(1,1) PRIMARY KEY,
    UserID INT NOT NULL UNIQUE,
    OrganisationName NVARCHAR(150) NOT NULL,
    ContactEmail NVARCHAR(150) NOT NULL,
    ContactPhone NVARCHAR(20) NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),

    CONSTRAINT FK_Organisers_Users
        FOREIGN KEY (UserID) REFERENCES Users(UserID)
);
GO


-- =============================================
-- 3. Participants
-- =============================================
CREATE TABLE Participants
(
    ParticipantID INT IDENTITY(1,1) PRIMARY KEY,
    UserID INT NOT NULL UNIQUE,
    DateOfBirth DATE NOT NULL,
    Gender NVARCHAR(10) NOT NULL,
    ContactNumber NVARCHAR(20) NULL,
    Address NVARCHAR(255) NULL,
    EmergencyContactName NVARCHAR(150) NULL,
    EmergencyContactNumber NVARCHAR(20) NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),

    CONSTRAINT FK_Participants_Users
        FOREIGN KEY (UserID) REFERENCES Users(UserID)
);
GO


-- =============================================
-- 4. Event Environments
-- =============================================
CREATE TABLE EventEnvironments
(
    EnvironmentID INT IDENTITY(1,1) PRIMARY KEY,
    EnvironmentName NVARCHAR(150) NOT NULL,
    EnvironmentType NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500) NULL,
    Address NVARCHAR(255) NULL,
    City NVARCHAR(100) NULL,
    Province NVARCHAR(100) NULL,
    RouteInformation NVARCHAR(500) NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE()
);
GO


-- =============================================
-- 5. Events
-- =============================================
CREATE TABLE Events
(
    EventID INT IDENTITY(1,1) PRIMARY KEY,
    OrganiserID INT NOT NULL,
    EnvironmentID INT NOT NULL,
    EventName NVARCHAR(150) NOT NULL,
    EventDescription NVARCHAR(500) NULL,
    EventDate DATE NOT NULL,
    EventTime TIME NOT NULL,
    Location NVARCHAR(255) NOT NULL,
    MaxParticipants INT NOT NULL,
    EntryFee DECIMAL(10,2) NOT NULL DEFAULT 0.00,
    Status NVARCHAR(20) NOT NULL DEFAULT 'Active',
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),

    CONSTRAINT FK_Events_Organisers
        FOREIGN KEY (OrganiserID) REFERENCES Organisers(OrganiserID),

    CONSTRAINT FK_Events_Environments
        FOREIGN KEY (EnvironmentID) REFERENCES EventEnvironments(EnvironmentID)
);
GO


-- =============================================
-- 6. Event Categories
-- =============================================
CREATE TABLE EventCategories
(
    CategoryID INT IDENTITY(1,1) PRIMARY KEY,
    CategoryName NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500) NULL,
    DistanceKM DECIMAL(5,2) NOT NULL,
    MinAge INT NULL,
    MaxAge INT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE()
);
GO


-- =============================================
-- 7. Event Enrollments
-- =============================================
CREATE TABLE EventEnrollments
(
    EnrollmentID INT IDENTITY(1,1) PRIMARY KEY,
    ParticipantID INT NOT NULL,
    EventID INT NOT NULL,
    CategoryID INT NOT NULL,
    EnrollmentDate DATETIME NOT NULL DEFAULT GETDATE(),
    Status NVARCHAR(20) NOT NULL DEFAULT 'Registered',
    PaymentStatus NVARCHAR(20) NOT NULL DEFAULT 'Pending',
    RaceNumber INT NULL,

    CONSTRAINT FK_EventEnrollments_Participants
        FOREIGN KEY (ParticipantID) REFERENCES Participants(ParticipantID),

    CONSTRAINT FK_EventEnrollments_Events
        FOREIGN KEY (EventID) REFERENCES Events(EventID),

    CONSTRAINT FK_EventEnrollments_Categories
        FOREIGN KEY (CategoryID) REFERENCES EventCategories(CategoryID)
);
GO


-- =============================================
-- 8. Results
-- =============================================
CREATE TABLE Results
(
    ResultID INT IDENTITY(1,1) PRIMARY KEY,
    EnrollmentID INT NOT NULL UNIQUE,
    FinishTime TIME NULL,
    OverallPosition INT NULL,
    CategoryPosition INT NULL,
    Points INT NULL,
    Remarks NVARCHAR(500) NULL,
    RecordedAt DATETIME NOT NULL DEFAULT GETDATE(),

    CONSTRAINT FK_Results_Enrollments
        FOREIGN KEY (EnrollmentID) REFERENCES EventEnrollments(EnrollmentID)
);
GO


-- =============================================
-- SAMPLE DATA
-- =============================================

-- Users
INSERT INTO Users
(
    FirstName,
    LastName,
    Email,
    PasswordHash,
    PhoneNumber,
    Role
)
VALUES
(
    'John',
    'Smith',
    'john@example.com',
    'hashed_password_1',
    '0821111111',
    'Organiser'
),
(
    'Sarah',
    'Jones',
    'sarah@example.com',
    'hashed_password_2',
    '0822222222',
    'Participant'
),
(
    'David',
    'Williams',
    'david@example.com',
    'hashed_password_3',
    '0823333333',
    'Participant'
);
GO


-- Organiser
INSERT INTO Organisers
(
    UserID,
    OrganisationName,
    ContactEmail,
    ContactPhone
)
VALUES
(
    1,
    'RaceDay Events',
    'events@raceday.com',
    '0115551234'
);
GO


-- Participants
INSERT INTO Participants
(
    UserID,
    DateOfBirth,
    Gender,
    ContactNumber,
    Address,
    EmergencyContactName,
    EmergencyContactNumber
)
VALUES
(
    2,
    '2000-05-15',
    'Female',
    '0822222222',
    'Cape Town',
    'Jane Jones',
    '0824444444'
),
(
    3,
    '1998-08-20',
    'Male',
    '0823333333',
    'Johannesburg',
    'Mary Williams',
    '0825555555'
);
GO


-- Event Environments
INSERT INTO EventEnvironments
(
    EnvironmentName,
    EnvironmentType,
    Description,
    Address,
    City,
    Province,
    RouteInformation
)
VALUES
(
    'Green Point Stadium',
    'Stadium',
    'Main race starting and finishing area',
    '1 Fritz Sonnenberg Road',
    'Cape Town',
    'Western Cape',
    'Stadium loop and surrounding roads'
),
(
    'Johannesburg Park',
    'Outdoor',
    'Outdoor running environment',
    '123 Park Road',
    'Johannesburg',
    'Gauteng',
    'Park route with marked running sections'
);
GO


-- Events
INSERT INTO Events
(
    OrganiserID,
    EnvironmentID,
    EventName,
    EventDescription,
    EventDate,
    EventTime,
    Location,
    MaxParticipants,
    EntryFee,
    Status
)
VALUES
(
    1,
    1,
    'Cape Town Fun Run',
    'A community running event',
    '2026-10-10',
    '08:00:00',
    'Cape Town',
    500,
    150.00,
    'Active'
),
(
    1,
    2,
    'Johannesburg City Run',
    'A city running event',
    '2026-11-15',
    '07:30:00',
    'Johannesburg',
    300,
    120.00,
    'Active'
);
GO


-- Event Categories
INSERT INTO EventCategories
(
    CategoryName,
    Description,
    DistanceKM,
    MinAge,
    MaxAge
)
VALUES
(
    '5KM Fun Run',
    'Short distance community race',
    5.00,
    12,
    100
),
(
    '10KM Race',
    'Standard 10 kilometre race',
    10.00,
    16,
    100
);
GO


-- Event Enrollments
INSERT INTO EventEnrollments
(
    ParticipantID,
    EventID,
    CategoryID,
    Status,
    PaymentStatus,
    RaceNumber
)
VALUES
(
    1,
    1,
    1,
    'Registered',
    'Paid',
    101
),
(
    2,
    2,
    2,
    'Registered',
    'Paid',
    202
);
GO


-- Results
INSERT INTO Results
(
    EnrollmentID,
    FinishTime,
    OverallPosition,
    CategoryPosition,
    Points,
    Remarks
)
VALUES
(
    1,
    '00:32:15',
    25,
    10,
    75,
    'Good performance'
),
(
    2,
    '00:51:40',
    18,
    5,
    90,
    'Excellent performance'
);
GO


-- =============================================
-- VERIFICATION
-- =============================================

SELECT * FROM Users;
SELECT * FROM Organisers;
SELECT * FROM Participants;
SELECT * FROM EventEnvironments;
SELECT * FROM Events;
SELECT * FROM EventCategories;
SELECT * FROM EventEnrollments;
SELECT * FROM Results;
GO