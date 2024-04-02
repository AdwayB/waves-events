# waves-events

## Description
`waves-events` is a comprehensive microservice for managing events, user feedback, payments, and event-driven email notifications for `waves-ems`. 
It is designed to be performant, scalable and provides multiple endpoints to fetch data for statistical analyses of event listings, user interactions and more.

## Features

### Event Management
- **CRUD Operations**: Secure endpoints with RBAC to manage event listings.
- **Artist Collaboration**: Manage events featuring multiple artists.
- **Genre Classification**: Categorize events by genres.
- **Location & Date Filtering**: Retrieve events based on location radius and date ranges.

### Registration and Attendance
- **Event Registration**: Facilitates user registrations for events.
- **Cancellation Handling**: Process cancellations with comprehensive validations.

### User Feedback
- **Feedback Collection**: Gather user ratings and comments for events.
- **Feedback Moderation**: Secure endpoints to manage user feedback efficiently.

### Mail Notifications
- **Event-Driven Notifications**: Action-triggered email notifications for event registrations, updates, deletions and cancellations.
- **Customizable Templates**: Tailored email content for various event-related notifications.