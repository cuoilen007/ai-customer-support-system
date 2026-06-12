# AI Customer Support System

An AI-powered customer support platform built with ASP.NET Core, React, FastAPI, OpenAI Embeddings, ChromaDB, and Retrieval-Augmented Generation (RAG).


### 🏗️ System Architecture

```mermaid
graph TD
    React[React Frontend] -->|HTTP Requests / JSON| ASPNET[ASP.NET Core API Backend]
    ASPNET -->|Internal API Calls| FastAPI[FastAPI AI Service]
    FastAPI -->|Generate Embeddings & Chat| OpenAI[OpenAI API]
    FastAPI -->|Vector Search / Retrieval| ChromaDB[(ChromaDB Vector Store)]
    
    style React fill:#61DAFB,stroke:#333,stroke-width:2px,color:#000
    style ASPNET fill:#512BD4,stroke:#333,stroke-width:2px,color:#fff
    style FastAPI fill:#009688,stroke:#333,stroke-width:2px,color:#fff
    style OpenAI fill:#10a37f,stroke:#333,stroke-width:2px,color:#fff
    style ChromaDB fill:#e91e63,stroke:#333,stroke-width:2px,color:#fff
```

## Features

* JWT Authentication
* Conversation Management
* Chat History
* AI-powered Customer Support
* Vector Database (ChromaDB)
* Semantic Search
* Retrieval-Augmented Generation (RAG)
* React CRM Dashboard
* ASP.NET Core REST API
* FastAPI AI Service

## Tech Stack

### Backend

* ASP.NET Core 8
* Entity Framework Core
* SQL Server
* JWT Authentication

### Frontend

* React
* TypeScript
* TailwindCSS
* Axios

### AI

* FastAPI
* OpenAI API
* Embeddings
* ChromaDB
* RAG

## Architecture

React Frontend
↓
ASP.NET Core API
↓
FastAPI AI Service
↓
OpenAI Embeddings
↓
ChromaDB Vector Database

## Future Improvements

* Knowledge Base Management
* Ticket Classification using Machine Learning
* Analytics Dashboard
* Docker Deployment
* CI/CD Pipeline

