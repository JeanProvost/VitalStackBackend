# VitalStack Architecture Constraints
1. The System of Record (.NET 9 Web API): Stateful monolith. Sole owner of the PostgreSQL database, user auth, and caching logic.
2. The Calculator (Python FastAPI): Stateless service dedicated purely to AI/ML inference.
3. ABSOLUTE CONSTRAINT: The Python AI Service NEVER connects to the database. It accepts JSON from .NET, processes it, and returns JSON to .NET.
4. Caching Flow (Cache-Aside): .NET checks PostgreSQL for an interaction cache. On a miss, it calls Python via HTTP, receives the result, saves it to the DB, and returns it.
