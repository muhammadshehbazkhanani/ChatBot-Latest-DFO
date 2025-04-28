
# Chatbot Application with Dialogflow, WebSocket, and MongoDB

This project is a chatbot application built using Dialogflow, WebSocket, MongoDB, .NET, and Angular. It integrates the NICE CX chat widget for live chat functionality and stores user and configuration data in MongoDB.

## Features
- Dialogflow integration for natural language processing.
- WebSocket communication for real-time interaction.
- NICE CX chatbot widget integration for live customer service.
- MongoDB to store user and configuration data (message data is not stored).
- Dockerized deployment.

## Prerequisites
1. .NET 8.0
2. MongoDB
3. Docker
4. Dialogflow account and JSON file for intent configuration

## Setup

### 1. Clone the repository:
```bash
git clone <repository_url>
cd <project_directory>
```

### 2. Add Dialogflow JSON file:
- Download your **Dialogflow service account JSON file** from the Google Cloud console.
- Add this JSON file to the project directory where your .NET backend can access it.

### 3. Configure MongoDB:
- Set up MongoDB and create a database.
- In `appsettings.json`, update the MongoDB connection string:

```json
{
  "ConnectionStrings": {
    "MongoDb": "mongodb://your_mongo_connection_string_here"
  }
}
```

### 4. Docker Setup:
- Build the Docker image:
  ```bash
  docker build --no-cache -t chatapp .
  ```

- Run the Docker container:
  ```bash
  docker run -d -p 8080:8080 --name chatapp-container chatapp
  ```


### 5. Access the Application:
- The frontend Angular application & backend will be available at `http://localhost:8080`.

## Notes:
- Message data is not stored in MongoDB.
- You can customize the chatbot's behavior by adjusting the configuration in Dialogflow.
- Ensure that MongoDB is running and accessible via the connection string provided in `appsettings.json`.
