<body>

<h1>🎓 Training Center Management System (TCMS)</h1>

<p>
A robust, enterprise-grade Web API system designed to manage students, instructors, and courses with a comprehensive role-based access control system.
</p>

<h2>📌 Overview</h2>

<p>
The <b>Training Center Management System</b> is a professional backend solution that facilitates administrative operations for educational centers. It features a sophisticated workflow where:
</p>

<ul>
  <li><b>Students</b> can browse, search, and enroll in courses.</li>
  <li><b>Instructors</b> can manage and create course content.</li>
  <li><b>Admins</b> have full control over system users, roles, and administrative configurations.</li>
</ul>

<hr/>

<h2>⚙️ Technology Stack</h2>

<ul>
  <li><b>Language & Framework:</b> C#, ASP.NET Core</li>
  <li><b>Database & ORM:</b> SQL Server, Entity Framework Core (EF Core)</li>
  <li><b>Architecture:</b> Repository Pattern, Unit of Work, SOLID Principles</li>
  <li><b>Security:</b> JWT (Access & Refresh Tokens), Policy-based Authorization, Rate Limiting</li>
  <li><b>Performance & Quality:</b> Redis Caching, FluentValidation, Global Exception Handling, Async Programming</li>
  <li><b>Logging:</b> Structured Logging for system auditing</li>
</ul>

<hr/>

<h2>🚀 Core Features</h2>

<ul>
  <li><b>Role-Based Access Control (RBAC):</b> Granular authorization for Admins, Instructors, and Students using Policies.</li>
  <li><b>Database Integrity:</b> Properly designed relational schema with strict constraints and normalized data.</li>
  <li><b>Resilience & Security:</b> Anti-brute force mechanisms, endpoint rate limiting, and secure token management.</li>
  <li><b>Clean Code:</b> Adherence to SOLID principles and modular design using Repository Pattern & Unit of Work.</li>
  <li><b>Caching:</b> Redis integration to optimize high-traffic data retrieval.</li>
</ul>

<hr/>

<h2>🛠️ How to Run</h2>

<h3>Prerequisites</h3>

<ul>
  <li>Visual Studio 2026 (or latest version)</li>
  <li>SQL Server & SSMS installed</li>
  <li>Redis server running locally (localhost:6379)</li>
</ul>

<h3>Setup Steps</h3>

<ol>
  <li>
    Clone the repository:
    <pre><code>git clone [Your-Repository-Link]</code></pre>
  </li>

  <li>
    Configure AppSettings:
    <p>Update <b>appsettings.json</b> with your database credentials and SMTP settings:</p>

    <pre><code>
"ConnectionStrings": {
  "DefaultConnection": "Server=YOUR_SERVER;Database=TrainingCenterDB;Trusted_Connection=True;TrustServerCertificate=True;"
}
    </code></pre>
  </li>

  <li>
    Database Migration:
    <p>Run the following command in Package Manager Console:</p>

    <pre><code>Update-Database</code></pre>
  </li>

  <li>
    Run the Project:
    <p>Start the application and access Swagger UI to test API endpoints.</p>
  </li>
</ol>

<hr/>

<h2>🔐 Security Configuration Highlights</h2>

<ul>
  <li><b>Auth Endpoints:</b> Limited to 5 requests per 15 minutes to prevent brute-force attacks.</li>
  <li><b>Admin Endpoints:</b> High-performance rate limiting to ensure system stability.</li>
  <li><b>Public Read Operations:</b> Optimized with caching and balanced limits.</li>
</ul>

<hr/>

<p>
<b>Built with passion for clean, secure, and maintainable enterprise software.</b>
</p>

</body>
</html>
