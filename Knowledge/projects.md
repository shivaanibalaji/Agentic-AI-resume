# Projects

## AI Resume and Portfolio RAG Chatbot

### Overview

The AI Resume and Portfolio RAG Chatbot is an AI-powered application that allows users to ask questions about a person's professional background, experience, skills, and projects.

Instead of fine-tuning an LLM with resume information, the application uses Retrieval-Augmented Generation (RAG).

The resume and portfolio information is stored as Markdown knowledge files and converted into embeddings.

### Knowledge Base

The knowledge base contains files such as:

- about.md
- experience.md
- projects.md

These files contain structured information about the candidate.

### Architecture

The application consists of:

- React + TypeScript frontend
- ASP.NET Core .NET 10 backend
- PostgreSQL
- pgvector
- Ollama
- Qwen3 8B
- Qwen3-Embedding 0.6B

### RAG Pipeline

The knowledge-processing pipeline is:

1. Read Markdown knowledge files.
2. Extract the text.
3. Split the text into chunks.
4. Generate embeddings for each chunk.
5. Store embeddings in PostgreSQL using pgvector.

The query pipeline is:

1. Receive the user's question.
2. Convert the question into an embedding.
3. Search pgvector for semantically similar chunks.
4. Retrieve the most relevant chunks.
5. Combine the retrieved context with the user's question.
6. Send the prompt to Qwen3 8B.
7. Return the generated response to the frontend.

### Embedding Model

The project uses:

Qwen3-Embedding 0.6B

The embedding model converts text into numerical vectors.

For example:

```text
"Experience developing ASP.NET Core APIs"
                    ↓
       Qwen3-Embedding 0.6B
                    ↓
             Vector embedding