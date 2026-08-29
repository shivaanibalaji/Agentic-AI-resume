# Agentic AI Resume / Portfolio Chat

## Project Overview

The **Agentic AI Resume / Portfolio Chat** is a personal Generative AI project designed to transform a traditional static resume into an interactive AI-powered portfolio.

Instead of requiring users to manually read through a resume, the application allows users to ask questions about my professional background through a conversational interface.

The system is designed to answer questions about:

* About me
* Technical skills
* Work experience
* Education
* Projects
* Technologies
* Professional experience
* AI knowledge
* Career interests

---

## Project Objective

The primary objective is to build an AI-powered resume that can understand a user's question and retrieve relevant information from my professional knowledge base before generating a response.

The system is designed around **Retrieval-Augmented Generation (RAG)** so that responses are grounded in structured resume information rather than relying only on the language model's general knowledge.

---

## Knowledge Base

The resume information is maintained as structured Markdown documents.

The planned knowledge base contains:

```text
KnowledgeBase/
│
├── about.md
├── experience.md
├── skills.md
├── education.md
│
└── projects/
    ├── Kascade360.md
    └── Agentic portfolio chat.md
```

Each document contains detailed information about a specific area of my professional profile.

---

## RAG Architecture

The application follows a Retrieval-Augmented Generation approach.

The general workflow is:

```text
User Question
      ↓
Chat API
      ↓
Question Processing
      ↓
Embedding Generation
      ↓
Vector Similarity Search
      ↓
Retrieve Relevant Resume Context
      ↓
Prompt Construction
      ↓
LLM
      ↓
Grounded Response
      ↓
User
```

---

## Document Processing

The knowledge-base Markdown files are processed before being used by the application.

The processing pipeline involves concepts such as:

* Document loading
* Text extraction
* Chunking
* Embedding generation
* Vector storage
* Semantic retrieval

The goal is to make individual pieces of resume information searchable based on semantic meaning rather than only exact keyword matching.

---

## Embeddings

The application uses embeddings to convert text into numerical vector representations.

For example, a question such as:

> "What experience does Shivaani have with asynchronous processing?"

should be able to retrieve relevant information from project and experience documents even if the exact words in the question do not appear together in the source document.

---

## Vector Search

The generated embeddings are stored in a vector database.

When a user asks a question, the question is converted into an embedding and compared with stored document embeddings.

The most relevant chunks are retrieved and supplied to the LLM as context.

---

## LLM Response Generation

The language model receives:

* User question
* Retrieved resume context
* System instructions

The model then generates a response based on the retrieved information.

The purpose of this architecture is to keep responses grounded in the actual resume knowledge base.

---

## Conversation History

The application is designed to support conversational interactions rather than treating every question as an isolated request.

Conversation history allows follow-up questions such as:

```text
User:
What is Kascade360?

User:
What was my role in that project?

User:
What technologies did I use there?

User:
What problem did I solve using Parquet?
```

The system can use previous conversation context to understand what the user is referring to.

---

## Anonymous Sessions

The portfolio is designed without requiring traditional user login.

Instead, anonymous users can have a session associated with their conversation.

This allows the application to provide a ChatGPT-like conversational experience without requiring users to create an account.

---

## Redis

Redis is planned/used as part of the session and conversation-management architecture.

Potential responsibilities include:

* Anonymous session storage
* Conversation history
* Fast retrieval of recent messages
* Temporary session information
* Caching

---

## Streaming Responses

The application explores streaming LLM responses instead of waiting for the entire model response before displaying anything.

The objective is to improve perceived responsiveness.

Instead of:

```text
User Question
      ↓
Wait for complete LLM response
      ↓
Display response
```

the application can progressively return generated content:

```text
User Question
      ↓
LLM generates tokens
      ↓
Stream partial response
      ↓
UI displays response progressively
```

---

## Agentic AI Direction

The project is being extended toward an Agentic AI architecture.

The goal is to move beyond a simple:

```text
Question → RAG → Answer
```

system.

An agentic architecture can determine what action is required based on the user's request.

For example, a future workflow could involve:

```text
User Question
      ↓
Agent
      ↓
Determine Intent
      ↓
Select Tool / Knowledge Source
      ↓
Retrieve Information
      ↓
Reason Over Context
      ↓
Generate Response
```

This allows the resume assistant to evolve into a more capable AI portfolio assistant.

---

## Job Description Analyzer

Another planned capability is a **Job Description Analyzer**.

The user can provide a job description by:

* Pasting the job description
* Uploading a PDF

The system can analyze the job description against the resume knowledge base and identify:

* Strong matches
* Partial matches
* Weak matches
* Missing skills
* Relevant experience
* Overall match percentage

This turns the portfolio from a static resume into an interactive career-assistance application.

---

## Source Grounding

The application is designed to provide source-aware responses.

When the system retrieves information from the knowledge base, the response can reference the relevant source document or section.

This helps users understand where the answer originated and reduces unsupported claims.

---

## Technologies

### Backend

* .NET
* ASP.NET Core
* C#
* REST APIs

### AI

* Large Language Models
* Generative AI
* RAG
* Embeddings
* Semantic Search
* Prompt Engineering
* Agentic AI

### Data

* PostgreSQL
* pgvector
* Redis

### LLM Infrastructure

* Ollama
* Qwen
* Embedding models

### Frontend

* React
* TypeScript

### Infrastructure

* Docker
* Cloud deployment
* CI/CD

---

## Key Learning Areas

This project helps me explore the combination of traditional software engineering and modern AI application development.

The major areas of learning include:

* RAG architecture
* Vector search
* Embeddings
* LLM integration
* Prompt construction
* Context management
* Conversation history
* Redis-based sessions
* Streaming responses
* Agentic workflows
* AI application architecture
* LLM performance optimization

---

## Project Goal

The long-term goal of the project is to create an intelligent AI portfolio that can act as a conversational representation of my professional profile.

Instead of simply displaying my resume, the application should allow recruiters, interviewers, and other users to interact with my professional information naturally through an AI-powered conversational interface.
