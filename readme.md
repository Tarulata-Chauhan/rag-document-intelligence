# Enterprise Document RAG API

An enterprise-grade Retrieval-Augmented Generation (RAG) Web API built with **.NET 8** and **Microsoft Semantic Kernel**. 

This project demonstrates how to securely integrate Large Language Models (LLMs) with proprietary unstructured data (PDFs) to create an intelligent, grounded document assistant. It solves the common enterprise problem of AI hallucination by strictly restricting the model to answer queries based *only* on the provided context.

## 🏗️ Architecture & Flow
1. **Ingestion:** Upload a PDF document via the API.
2. **Parsing:** The API extracts text synchronously while preserving layout semantics (via PdfPig).
3. **Retrieval & Grounding:** User queries are matched against the document context.
4. **Generation:** Microsoft Semantic Kernel orchestrates the prompt engineering, forcing the OpenAI model to cite the provided text and safely reject out-of-scope questions.

## 🚀 Tech Stack
* **Framework:** .NET 8 Web API
* **AI Orchestration:** Microsoft Semantic Kernel
* **LLM Provider:** OpenAI (`gpt-4o-mini`)
* **Document Parsing:** UglyToad.PdfPig

## ⚙️ Getting Started

### Prerequisites
* [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
* An active [OpenAI API Key](https://platform.openai.com/api-keys)

### Installation
1. Clone the repository:
   ```bash
   git clone https://github.com/yourusername/RAG_Document_Intelligence_API.git
   cd RAG_Document_Intelligence_API/RAG_API
   ```
2. Restore dependencies:
   ```bash
   dotnet restore
   ```
3. Configure your API Key:
   Create an `appsettings.Development.json` file in the root of the `RAG_API` project and add your OpenAI key:
   ```json
   {
     "OpenAI": {
       "ApiKey": "sk-your-actual-api-key"
     }
   }
   ```
   *(Note: This file is ignored by Git to prevent secret leakage).*

4. Run the application:
   ```bash
   dotnet run --environment Development
   ```
5. Navigate to `http://localhost:5000/swagger` to access the interactive API documentation.

## 🔌 API Endpoints

### `POST /api/Document/upload`
Uploads a PDF file, extracts the text, and stores the context in memory for querying.
* **Payload:** `multipart/form-data` (File)
* **Response:** Metadata including character count and a text preview.

### `POST /api/Document/chat`
Submits a query against the uploaded document. The AI evaluates the query against the context and provides a grounded answer.
* **Payload:** `application/json` (String query)
* **Response:** The AI-generated answer or a safe rejection message if the answer is not in the document.

## 🗺️ Roadmap / Future Enhancements
* [ ] **Vector Database Integration:** Replace in-memory string matching with PostgreSQL (`pgvector`), Qdrant, or Pinecone for semantic vector search.
* [ ] **Chunking Strategy:** Implement token-aware text chunking with overlap for handling massive enterprise documents.
* [ ] **Dockerization:** Add a `Dockerfile` for streamlined containerized deployment.
* [ ] **Authentication:** Secure endpoints using JWT bearer tokens.