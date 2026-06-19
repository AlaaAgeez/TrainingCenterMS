<!DOCTYPE html>
<html lang="en">
<head>
<meta charset="UTF-8" />
<meta name="viewport" content="width=device-width, initial-scale=1.0"/>
<title>Training Center Management System</title>

<style>
    body {
        font-family: 'Segoe UI', Tahoma;
        line-height: 1.7;
        margin: 0;
        background: #0f172a;
        color: #e5e7eb;
    }

    .container {
        max-width: 900px;
        margin: auto;
        padding: 30px;
    }

    h1, h2, h3 {
        color: #38bdf8;
    }

    section {
        background: #111827;
        padding: 20px;
        margin-bottom: 20px;
        border-radius: 12px;
        border: 1px solid #334155;
    }

    .badge {
        display: inline-block;
        background: #1d4ed8;
        padding: 3px 8px;
        border-radius: 6px;
        font-size: 12px;
        margin-left: 5px;
    }

    ul {
        padding-left: 20px;
    }

    li {
        margin-bottom: 5px;
    }
</style>
</head>

<body>

<div class="container">

<!-- TITLE -->
<section>
    <h1>🚀 Training Center Management System</h1>
    <p><b>Backend Developer Project</b> | ASP.NET Core Web API | Clean Architecture</p>
</section>

<!-- OVERVIEW -->
<section>
    <h2>📌 Project Overview</h2>
    <p>
        Designed and developed a production-grade Training Center Management System using ASP.NET Core Web API following Clean Architecture principles.
        The system simulates a real-world educational platform where students, instructors, and administrators interact through a secure and scalable backend infrastructure.
    </p>
    <p>
        The project focuses on modularity, scalability, security, and maintainability, with strict separation of concerns across all layers.
    </p>
</section>

<!-- ARCHITECTURE -->
<section>
    <h2>🏗️ Architecture & Design</h2>

    <p><b>Implemented Clean Architecture with 4 main layers:</b></p>
    <ul>
        <li>API Layer (Controllers + Middleware)</li>
        <li>Business Layer (Application Logic + Services)</li>
        <li>Core Layer (Entities, DTOs, Interfaces, Constants, Exceptions)</li>
        <li>Data Access Layer (Repositories + Unit of Work + EF Core)</li>
    </ul>

    <p><b>Separation of concerns:</b></p>
    <ul>
        <li>Domain logic</li>
        <li>Business logic</li>
        <li>Infrastructure concerns</li>
    </ul>

    <p>Designed a fully decoupled system using Dependency Injection and interface-driven development.</p>
</section>

<!-- FEATURES -->
<section>
    <h2>⚙️ Key Features & Implementation</h2>

    <h3>🔐 Authentication & Authorization</h3>
    <ul>
        <li>JWT Authentication with Refresh Token mechanism</li>
        <li>Role-Based Access Control (RBAC)</li>
        <li>Policy-Based Authorization for fine-grained access control</li>
    </ul>

    <h3>⚡ Performance & Optimization</h3>
    <ul>
        <li>Redis Caching for frequently accessed data</li>
        <li>Server-side pagination with optimized queries</li>
        <li>EF Core AsNoTracking for read-heavy operations</li>
        <li>Asynchronous programming throughout the system</li>
    </ul>

    <h3>🛡️ Security & Reliability</h3>
    <ul>
        <li>Global Exception Handling Middleware</li>
        <li>Custom AppException handling strategy</li>
        <li>FluentValidation per DTO</li>
        <li>Rate Limiting to prevent abuse and brute-force attacks</li>
    </ul>

    <h3>📊 Data Layer</h3>
    <ul>
        <li>Repository Pattern + Unit of Work</li>
        <li>Clean EF Core data access layer</li>
        <li>Separation between domain and persistence logic</li>
    </ul>

    <h3>📧 External Integrations</h3>
    <ul>
        <li>SMTP Email Service (Gmail integration)</li>
        <li>Automated email notifications</li>
    </ul>

    <h3>📚 API Design</h3>
    <ul>
        <li>RESTful API with DTO abstraction</li>
        <li>Swagger documentation</li>
        <li>Consistent response structure</li>
    </ul>

    <h3>🚀 Deployment</h3>
    <ul>
        <li>Production deployment via SmarterASP.NET</li>
        <li>HTTPS enabled</li>
        <li>Production-ready configuration</li>
    </ul>
</section>

<!-- TECH STACK -->
<section>
    <h2>🧰 Technology Stack</h2>
    <ul>
        <li>ASP.NET Core Web API</li>
        <li>Entity Framework Core</li>
        <li>SQL Server</li>
        <li>Redis</li>
        <li>JWT Authentication</li>
        <li>FluentValidation</li>
        <li>SMTP Email Service</li>
        <li>Swagger</li>
        <li>Dependency Injection</li>
        <li>C# .NET</li>
    </ul>
</section>

<!-- ACHIEVEMENTS -->
<section>
    <h2>🎯 Key Achievements</h2>
    <ul>
        <li>Built a scalable enterprise-grade backend system</li>
        <li>Optimized performance using caching and query optimization</li>
        <li>Implemented secure authentication and authorization system</li>
        <li>Maintained clean and maintainable architecture</li>
        <li>Delivered production-ready deployed API</li>
    </ul>
</section>

<!-- NOTE -->
<section>
    <h2>💡 Note</h2>
    <p>
        This project demonstrates strong backend engineering skills including system design, clean architecture,
        security implementation, and production deployment readiness.
    </p>
</section>

</div>

</body>
</html>
