# ThesisBenchmark
This project is a benchmarking program for measuring the CRUD performance of various combinations of DBMS's, data access patterns and scaling techniques,
made for my bachelor's thesis.

This program measures the CRUD performance of the following databases/data access patterns:

1. MySQL - Relational DB
2. PostgreSQL - Object-Relational DB
3. Cassandra - Column-store
4. MongoDB - Document-store
5. Redis - KV-store
6. Perst - Object-Oriented DB
7. Dapper ORM - Minimalistic ORM
8. Entity Framework ORM - Larger ORM
9. CQRS (Using PostgreSQL for writing, Redis for reading) - Command-Query Request Separation, separating reads and writes

The benchmark includes horizontal (for Cassandra, MongoDB and Redis) and vertical (for all the others) scaling using docker.



Pre-requisites
-----------------------------------------
1. .NET Framework v. 4.5
2. Docker v. 18.06.1-ce (Other versions are most likely fine as well, but only this version was tested)

First-time setup steps
-----------------------------------------
0. `cd Benchmarking\ Console\ App`
1. `cd Docker/Container\ Initializations/`
2. Create all necessary Docker machines using `./create_all_machines.sh`
3. Start either the unscaled or scaled versions of the DB's listed above by running either `unscaled_startall.sh` or `scaled_startall.sh`
4. Adjust the string in configFileToUse.config to the location of the config file that you wish to use: point it to unscaled.config to 
   use the unscaled versions of the databases, and scaled.config for the scaled variants.
5. If necessary, adjust the connection strings in either the unscaled.config or the scaled.config files.
6. If you want to, you can adjust the amount of models to be tested by editing the 'modelAmounts' variable in Program.cs.
7. Run the project in an IDE of your choice.
8. The resulting output files can be found in the \Output directory, along with results that were generated during the course of the thesis project, visualisations included.

