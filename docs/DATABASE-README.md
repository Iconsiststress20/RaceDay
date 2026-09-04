# RaceDay Database

The RaceDay database supports the management of race events, organisers, participants, event environments, event categories, enrolments, and results.

## Tables

The database contains eight tables:

1. Users
2. Organisers
3. Participants
4. EventEnvironments
5. Events
6. EventCategories
7. EventEnrollments
8. Results

## Relationships

- Users can have an Organiser profile or Participant profile.
- Organisers create and manage Events.
- Events are associated with an Event Environment.
- Event Enrollments connect Participants to Events and Categories.
- Results are linked to Event Enrollments.

## Database Features

The database uses:

- Primary keys
- Foreign keys
- Unique constraints
- Default values
- Identity columns
- Sample data

The SQL database script is located in `docs/RACEDAY.sql`.