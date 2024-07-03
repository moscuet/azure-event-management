# Event Management System API

![Build Status](https://img.shields.io/badge/build-passing-brightgreen)
![License](https://img.shields.io/badge/license-MIT-blue)
![Azure](https://img.shields.io/badge/Azure-Enabled-blue)
![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-8.0-blue)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-Enabled-blue)

## Project Description

The Event Management System is a web application built with ASP.NET Core, Entity Framework Core, and PostgreSQL. It uses ASP.NET Core Identity for user authentication and authorization, and integrates with Azure services for storage and monitoring.

### Azure fuction app repo  [Link](https://github.com/moscuet/function-app)

## Features

1. **User registration and authentication** with ASP.NET Core Identity.
2. **Role-based authorization** (Admin, EventProvider, User).
3. **CRUD operations** for events.
4. **Event registration with FIFO processing** using Azure Service Bus and Azure Functions.
5. **Storage of images and documents** in Azure Blob Storage.
6. **Monitoring and diagnostics** with Azure Application Insights.

#### Future Implementation Plans
7. **Caching of event data** with Redis. (_Optional_)
8. **Storage of event metadata and user interactions** in Cosmos DB for NoSQL. (_Optional_)


## Database Structure

- **Cosmos DB for PostgreSQL**
  - **Users:** Stores user information including authentication details.
  - **Events:** Stores details of each event.
  - **EventRegistrations:** Tracks which users have registered for which events.

- **Azure Blob Storage**
  - **EventImages:** Stores images related to events.
  - **UserProfiles:** Stores user profile pictures.
  - **EventDocuments:** Stores documents related to events.

## Workflow

1. **User accesses the Event Management System web app and signs in.**
2. **Browser pulls static resources from Azure CDN.**
6. **Pulls event-related images and documents from Azure Blob Storage.**
7. **User registers/unregisters for an event.** Registration information is placed in an Azure Service Bus queue with sessions enabled.
8. **Azure Functions processes the registration/unregistration** from the Service Bus queue, ensuring FIFO order, use transaction to modify event's properties.
9. **Azure Functions updates the registration status in Cosmos DB** currently remove from que once handled and log status . ( future plan: may trigger other necessary actions such as sending confirmation emails.)

10. **Application Insights monitors and diagnoses issues** in the application.


## API End Points

Accounts Controller
1. Register User
Endpoint: POST /api/accounts/register  
Request Body:
```
{
"Email": "user@example.com",
"FullName": "John Doe",
"Password": "securepassword123",
"Role": "User"
}
```
Response:
```
{
"Message": "User registered successfully"
}
```

2. User Login
Endpoint: POST /api/accounts/login  
Request Body:
```
{
"Email": "user@example.com",
"Password": "securepassword123"
}
```
Response:
```
{
"Token": "eyJhbG..."
}
```

3. Update User Profile
Endpoint: PUT /api/accounts/update  
Request Body:
```
{
"Email": "newemail@example.com",
"FullName": "John Doe Updated"
}
```
Response:
```
{
"Message": "User profile updated successfully"
}
```

4. Get User Profile
Endpoint: GET /api/accounts/profile
Response:
```
{
"UserName": "user@example.com",
"Email": "user@example.com",
"FullName": "John Doe",
"Id": "12345",
"Roles": ["User"]
}
```

5. Get All Users (Admins Only)
Endpoint: GET /api/accounts/users
Response:
```
[
{
"Id": "12345",
"UserName": "user@example.com",
"Email": "user@example.com",
"FullName": "John Doe"
}
]
```

6. Delete User (Admins Only)
Endpoint: DELETE /api/accounts/{id}
Response:
```
{
"Message": "User with ID 12345 deleted successfully"
}
```

Events Controller
7. Create Event
Endpoint: POST /api/v1/events  
Request Body:
```
{
"Name": "Sample Event",
"Description": "Detailed event description.",
"Location": "Event Location",
"Date": "2023-12-31T23:59:59Z",
"OrganizerId": "62f018fc-c73d-4d99-9cbf-e6933ed3383d",
"TotalSpots": 100
}
```
Response:
```
{
"Message": "Event created successfully"
}
```

8. Get All Events
Endpoint: GET /api/v1/events
Response:
```
[
{
"Id": "123",
"Name": "Sample Event",
"Description": "Detailed event description.",
"Location": "Event Location",
"Date": "2023-12-31T23:59:59Z",
"TotalSpots": 100,
"RegisteredCount": 0,
"ImageUrls": [],
"DocumentUrls": []
}
]
```

9. Get Event By ID
Endpoint: GET /api/v1/events/{id}
Response:
```
{
"Id": "123",
"Name": "Sample Event",
"Description": "Detailed event description.",
"Location": "Event Location",
"Date": "2023-12-31T23:59:59Z",
"TotalSpots": 100,
"RegisteredCount": 0,
"ImageUrls": [],
"DocumentUrls": []
}
```

10. Update Event
Endpoint: PUT /api/v1/events/{id}  
Request Body:
```
{
"Name": "Updated Event Name",
"Description": "Updated description.",
"Location": "New Location",
"Date": "2024-01-01T00:00:00Z",
"OrganizerId": "62f018fc-c73d-4d99-9cbf-e6933ed3383d",
"TotalSpots": 150
}
```
Response:
```
{
"Message": "Event updated successfully"
}
```

11. Delete Event
Endpoint: DELETE /api/v1/events/{id}
Response:
```
{
"Message": "Event deleted successfully"
}
```

12. Register for Event
Endpoint: POST /api/v1/events/{id}/register
Response:
```
{
"Message": "Registration request submitted"
}
```

13. Unregister from Event
Endpoint: DELETE /api/v1/events/{id}/unregister
Response:
```
{
"Message": "Unregistration request submitted"
}
```

14. Upload Event Images
Endpoint: POST /api/v1/events/{id}/upload-images
Response:
```
{
"ImageUrls": ["http://example.com/image1.jpg"]
}
```

15. Upload Event Documents
Endpoint: POST /api/v1/events/{id}/upload-documents
Response:
```
{
"DocumentUrls": ["http://example.com/document1.pdf"]
}
```

### Screen Shot of  Resource List in DashBoard

-- All Resources  
<img width="1384" alt="Screenshot 2024-07-03 at 9 50 36" src="https://github.com/moscuet/az-event-management/assets/51766137/921d097b-649a-4908-be5c-2b6776445cbe">  

-- Storage Browser  

<img width="1413" alt="Screenshot 2024-07-03 at 9 32 01" src="https://github.com/moscuet/az-event-management/assets/51766137/df1580af-5fb2-4226-b30d-1f6424ed735b">  

### Screen Shot of Cosmos DB ( psql)

- Insights  
<img width="1379" alt="Screenshot 2024-07-03 at 9 38 31" src="https://github.com/moscuet/az-event-management/assets/51766137/9b55f1e6-9d70-41ae-a179-99bb931b7a8b">

- Metrics  
<img width="1391" alt="Screenshot 2024-07-03 at 9 39 07" src="https://github.com/moscuet/az-event-management/assets/51766137/34e6f503-7f68-4ff9-a94b-22ad139b6696">



### Screen Shot of Blob storage

- Event Images  

<img width="1151" alt="Screenshot 2024-07-03 at 9 33 36" src="https://github.com/moscuet/az-event-management/assets/51766137/48a8c5fe-c8ac-48f6-9b90-cc2c858cb222">

-  Event Documents  
  
<img width="1153" alt="Screenshot 2024-07-03 at 9 33 25" src="https://github.com/moscuet/az-event-management/assets/51766137/b98739e8-3497-426d-8e7a-48dfd0045377">

### Screen Shot of Service bus   

<img width="1419" alt="Screenshot 2024-07-03 at 10 42 14" src="https://github.com/moscuet/az-event-management/assets/51766137/36e9cb50-ebf8-42a3-ab9b-4c4db2a0f3e3">

### Azure Function App
- Function log stream:  
  - successful registering for event  
  - registering error:  duplicate key value violates unique constraint "PK_EventRegistrations"
  - 
<img width="1424" alt="Screenshot 2024-07-03 at 10 57 22" src="https://github.com/moscuet/az-event-management/assets/51766137/f9299583-0458-4431-b9ab-507e59be8cfe">  




