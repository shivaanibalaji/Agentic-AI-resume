# Kascade360

## Project - Kascade360

**Shivaani Balaji** developed and worked on the **Kascade360** project as part of her role as a **Full Stack .NET Developer and L2 Software Engineer at Propel Technology Group Inc.**

**Kascade360** is an enterprise taxation and accounting application developed for a US client.

The application involves taxation, accounting, financial data, reporting, Excel processing, data mapping, and data-intensive enterprise workflows.

I work on Kascade360 as part of my role as an **L2 Software Engineer at Propel Technology Group Inc.**

---

## My Role

### L2 Software Engineer

I contribute to the development and maintenance of enterprise tax and accounting features.

My responsibilities include:

* Understanding business requirements
* Designing technical solutions
* Backend development
* API development
* Data processing
* Database operations
* Frontend integration
* Unit testing
* Debugging
* Performance optimization
* Production issue analysis
* Code reviews
* Refactoring

---

## Technology Stack

### Backend

* C#
* .NET 8
* ASP.NET Core
* Entity Framework Core
* LINQ
* MediatR
* CQRS
* REST APIs

### Frontend

* React
* TypeScript
* JavaScript
* HTML5
* CSS

### Database

* PostgreSQL
* SQL Server
* Redis
* DuckDB

### Data Processing

* Apache Parquet
* SpreadsheetGear
* GrapeCity Documents for Excel
* ExpandoObject
* Dynamic Data Processing

### Distributed Systems

* RabbitMQ
* SignalR
* gRPC

### Cloud and DevOps

* Azure
* Azure Blob Storage
* Azure Key Vault
* Azure DevOps
* Docker
* Kubernetes
* CI/CD

---

## Major Functional Areas

My work involves enterprise workflows related to areas such as:

* Trial Balance
* Federal Tax Grouping
* Project Statement
* Tax Form Mapping
* Account Mapping
* Financial Reporting
* Excel Import
* Excel Export
* Data Processing

---

## Excel-to-Parquet Architecture

One of the significant technical areas I worked on was developing an **Excel-to-Parquet data-processing architecture**.

The objective was to reduce the dependency of downstream processing logic on constantly changing Excel schemas.

Excel files can evolve over time through changes to:

* Columns
* Formulas
* Data structure
* Field mappings

A strongly typed DTO-based approach can become difficult to maintain when these schemas change frequently.

To address this, I worked with dynamically structured data.

---

## Dynamic Data Processing

I used **C# ExpandoObject** to represent dynamically changing Excel data.

This allowed the processing layer to work with dynamic columns without requiring every Excel schema change to result in changes to strongly typed DTO models.

This approach helped separate downstream data-processing logic from the fixed structure of an Excel file.

---

## DuckDB and Parquet

I used **DuckDB** to generate and process Parquet files from dynamically structured Excel data.

The workflow allows data to be transformed from Excel into Parquet while keeping downstream querying and processing independent of frequent Excel schema changes.

Parquet provides a suitable format for data-intensive processing workflows, while DuckDB provides querying and processing capabilities over structured data.

---

## Excel Import and Export

I implemented and maintained Excel import and export workflows using:

* SpreadsheetGear
* GrapeCity Documents for Excel
* SJS conversion
* Parquet-backed processing

These workflows support enterprise reporting and financial data-processing requirements.

---

## Asynchronous Excel Processing

Excel processing can involve long-running operations.

To avoid blocking API requests during lengthy processing, I worked with a **publish/subscribe background-job architecture**.

The high-level workflow is:

```text
Excel Upload
     ↓
API Request
     ↓
Publish Processing Message
     ↓
Background Consumer
     ↓
Excel Processing
     ↓
Dynamic Data Generation
     ↓
Parquet Generation
     ↓
Downstream Processing
```

This architecture allows long-running operations to execute asynchronously.

---

## CQRS and MediatR

The application follows a CQRS and MediatR-based architecture.

Commands and queries are separated so that:

* Commands handle state-changing operations.
* Queries handle data retrieval.
* Business logic remains within appropriate application-layer handlers.
* Controllers remain relatively lightweight.

---

## Design Patterns

I have applied design patterns such as:

### Strategy Pattern

Used for Excel and reporting workflows where different processing or display strategies are required.

### Mediator Pattern

Implemented using MediatR for request handling and separation between controllers and application logic.

### Clean Architecture

Used to maintain separation between application, domain, infrastructure, and presentation responsibilities.

---

## AdminService Microservice

I designed and developed the **AdminService microservice from scratch**.

The implementation included:

* API development
* Business logic
* Data access
* Validation
* Service functionality
* Integration with the existing architecture

The service follows the application's established architecture and coding standards.

---

## Distributed Communication

The application uses distributed communication and messaging technologies including:

* RabbitMQ
* gRPC
* SignalR
* Redis

These technologies support asynchronous workflows, service communication, caching, and real-time communication.

---

## Testing and Debugging

I perform unit testing using **xUnit and Moq**.

I also work on:

* Debugging
* Production issue analysis
* Code reviews
* Refactoring
* Performance optimization
* Regression analysis

---

## Key Engineering Challenges

### Changing Excel Schemas

A major challenge is maintaining processing logic when Excel schemas evolve.

The dynamic Excel-to-Parquet approach helps reduce tight coupling between Excel structure and downstream processing.

### Large Data Processing

Data-intensive workflows require efficient processing and querying.

Using Parquet and DuckDB provides a data-processing approach that is less dependent on traditional fixed DTO structures.

### Long-Running Operations

Excel processing can take significant time.

Asynchronous background processing prevents API requests from being blocked by long-running operations.

---

## Skills Demonstrated

This project demonstrates experience in:

* Enterprise .NET development
* Backend API development
* Microservices
* CQRS
* MediatR
* Clean Architecture
* Dynamic data processing
* Excel processing
* Apache Parquet
* DuckDB
* RabbitMQ
* Redis
* SignalR
* gRPC
* PostgreSQL
* Azure
* Docker
* Kubernetes
* CI/CD
* Unit testing
* Debugging
* Performance optimization
