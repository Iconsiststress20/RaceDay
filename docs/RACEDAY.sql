CREATE DATABASE RaceDay;
GO

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

CREATE TABLE Organisers
(
    OrganiserID INT IDENTITY(1,1) PRIMARY KEY,
    UserID INT NOT NULL UNIQUE,
    OrganisationName NVARCHAR(150) NOT NULL,
    ContactEmail NVARCHAR(150) NOT NULL,
    ContactPhone NVARCHAR(20) NULL,

    CONSTRAINT FK_Organisers_Users
        FOREIGN KEY (UserID) REFERENCES Users(UserID)
);
GO

CREATE TABLE Participants
(
    ParticipantID INT IDENTITY(1,1) PRIMARY KEY,
    UserID INT NOT NULL UNIQUE,

    CONSTRAINT FK_Participants_Users
        FOREIGN KEY (UserID) REFERENCES Users(UserID)
);
GO

CREATE TABLE EventEnvironments
(
    EnvironmentID INT IDENTITY(1,1) PRIMARY KEY,
    EnvironmentName NVARCHAR(150) NOT NULL,
    EnvironmentType NVARCHAR(50) NOT NULL,
    Description NVARCHAR(500) NULL,
    Address NVARCHAR(200) NULL,
    City NVARCHAR(100) NULL,
    Province NVARCHAR(100) NULL,
    RouteInformation NVARCHAR(500) NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE()
);
GO

CREATE TABLE Events
(
    EventID INT IDENTITY(1,1) PRIMARY KEY,
    OrganiserID INT NOT NULL,
    EnvironmentID INT NOT NULL,
    EventName NVARCHAR(150) NOT NULL,
    EventDescription NVARCHAR(500) NULL,
    EventDate DATE NOT NULL,
    EventTime TIME NULL,
    Location NVARCHAR(150) NOT NULL,
    MaxParticipants INT NULL,
    EntryFee DECIMAL(10,2) NOT NULL DEFAULT 0.00,
    Status NVARCHAR(20) NOT NULL DEFAULT 'Active',
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),

    CONSTRAINT FK_Events_Organisers
        FOREIGN KEY (OrganiserID) REFERENCES Organisers(OrganiserID),

    CONSTRAINT FK_Events_Environments
        FOREIGN KEY (EnvironmentID) REFERENCES EventEnvironments(EnvironmentID)
);
GO

CREATE TABLE EventCategories
(
    CategoryID INT IDENTITY(1,1) PRIMARY KEY,
    CategoryName NVARCHAR(100) NOT NULL,
    Description NVARCHAR(255) NULL,
    DistanceKM DECIMAL(5,2) NOT NULL,
    MinAge INT NULL,
    MaxAge INT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE()
);
GO

CREATE TABLE EventEnrollments
(
    EnrollmentID INT IDENTITY(1,1) PRIMARY KEY,
    ParticipantID INT NOT NULL,
    EventID INT NOT NULL,
    CategoryID INT NOT NULL,
    EnrollmentDate DATETIME NOT NULL DEFAULT GETDATE(),
    Status NVARCHAR(20) NOT NULL DEFAULT 'Registered',
    PaymentStatus NVARCHAR(20) NOT NULL DEFAULT 'Pending',
    RaceNumber NVARCHAR(20) NULL,

    CONSTRAINT FK_EventEnrollments_Participants
        FOREIGN KEY (ParticipantID) REFERENCES Participants(ParticipantID),

    CONSTRAINT FK_EventEnrollments_Events
        FOREIGN KEY (EventID) REFERENCES Events(EventID),

    CONSTRAINT FK_EventEnrollments_Categories
        FOREIGN KEY (CategoryID) REFERENCES EventCategories(CategoryID)
);
GO

CREATE TABLE Results
(
    ResultID INT IDENTITY(1,1) PRIMARY KEY,
    EnrollmentID INT NOT NULL UNIQUE,
    FinishTime TIME NULL,
    OverallPosition INT NULL,
    CategoryPosition INT NULL,
    Points DECIMAL(10,2) NULL,
    Remarks NVARCHAR(255) NULL,
    RecordedAt DATETIME NOT NULL DEFAULT GETDATE(),

    CONSTRAINT FK_Results_EventEnrollments
        FOREIGN KEY (EnrollmentID) REFERENCES EventEnrollments(EnrollmentID)
);
GO

SELECT TABLE_NAME
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_TYPE = 'BASE TABLE'
ORDER BY TABLE_NAME;

INSERT INTO Users
    (FirstName, LastName, Email, PasswordHash, PhoneNumber, Role)
VALUES
    ('John', 'Smith', 'john.smith@example.com', 'SampleHash001', '0712345678', 'Participant'),
    ('Sarah', 'Jones', 'sarah.jones@example.com', 'SampleHash002', '0723456789', 'Organiser'),
    ('David', 'Williams', 'david.williams@example.com', 'SampleHash003', '0734567890', 'Participant');
GO

SELECT * FROM Users;

INSERT INTO Organisers
    (UserID, OrganisationName, ContactEmail, ContactPhone)
VALUES
    (2, 'RaceDay Events', 'sarah.jones@example.com', '0723456789');
GO

SELECT * FROM Organisers;

INSERT INTO Participants
    (UserID, DateOfBirth, Gender, ContactNumber, Address,
     EmergencyContactName, EmergencyContactNumber)
VALUES
    (1, '2002-05-15', 'Male', '0712345678',
     '10 Main Street', 'Mary Smith', '0798765432'),

    (3, '2001-09-20', 'Male', '0734567890',
     '25 Park Avenue', 'Lisa Williams', '0787654321');
GO

SELECT * FROM Participants;

ALTER TABLE Participants
ADD
    DateOfBirth DATE NOT NULL,
    Gender NVARCHAR(10) NOT NULL,
    ContactNumber NVARCHAR(20) NULL,
    Address NVARCHAR(255) NULL,
    EmergencyContactName NVARCHAR(150) NULL,
    EmergencyContactNumber NVARCHAR(20) NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE();
GO

SELECT * FROM Participants;


INSERT INTO Participants
    (UserID, DateOfBirth, Gender, ContactNumber, Address,
     EmergencyContactName, EmergencyContactNumber)
VALUES
    (1, '2002-05-15', 'Male', '0712345678',
     '10 Main Street', 'Mary Smith', '0798765432'),

    (3, '2001-09-20', 'Male', '0734567890',
     '25 Park Avenue', 'Lisa Williams', '0787654321');
GO

SELECT * FROM Participants;

INSERT INTO EventEnvironments
    (EnvironmentName, EnvironmentType, Description, Address, City, Province, RouteInformation)
VALUES
    ('City Park Route', 'Road',
     'Road race through the city park.',
     '10 Park Road', 'Johannesburg', 'Gauteng',
     '5 km loop around City Park'),

    ('Mountain Trail', 'Trail',
     'Off-road trail running environment.',
     'Mountain View Road', 'Cape Town', 'Western Cape',
     '10 km mountain trail');
GO

SELECT * FROM EventEnvironments;

ALTER TABLE EventEnvironments
ADD
    City NVARCHAR(100) NULL,
    Province NVARCHAR(100) NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE();
GO

SELECT * FROM EventEnvironments;

INSERT INTO Events
    (OrganiserID, EnvironmentID, EventName, EventDescription,
     EventDate, EventTime, Location, MaxParticipants, EntryFee)
VALUES
    (1, 1,
     'Johannesburg City Run',
     'A 5 km road race through the city park.',
     '2026-10-10', '08:00',
     'Johannesburg City Park', 200, 150.00),

    (1, 2,
     'Cape Mountain Challenge',
     'A 10 km trail running event through the mountain route.',
     '2026-11-15', '07:30',
     'Cape Mountain Trail', 150, 250.00);
GO

