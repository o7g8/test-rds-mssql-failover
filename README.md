# Tool to evaluate RDS MS SQL downtime during RDS modification

The tool connects to the specified RDS MS SQL instance and executes the same query (reading available collations) every second and prints results.

If the DB is unavailable, the tool keeps reconnecting to the DB every second during one minute.

The tool prints messages on stdout wit timestamps showing when the DB is available and when the DB is not available, so the user can tell for what period the DB was unavailable.

Usage:

```bash
dotnet tool -- -e <db-endpoint> -u <username> -p <password> -l <label>
```

To assess the DB availability via different endpoints run the tool against RDS endpoint, Always-ON listener endpoint, and RDS Proxy endpoint in separate terminal windows:

* via RDS endpoint:

```bash
dotnet tool -- -e <db-endpoint> -u <username> -p <password> -l RDS
```

* via Always-ON listener endpoint:

```bash
dotnet tool -- -e <db-endpoint> -u <username> -p <password> -l AlwaysON
```

NOTE: first time it took 5-7 connection retires till the tool was able to connect to the Always-ON listener in my case. Thereafter the tool connected to the endpoint instantaneously.

* via RDS Proxy endpoint:

```bash
dotnet tool -- -e <db-endpoint> -u <username> -p <password> -l RDSProxy
```

Then modify the RDS database, e.g. resize the instance:

```bash
./rds-resize.sh <instance-identifier> <instance-size>
```

E.g. run:

```bash
./rds-resize.sh database-1 db.m6i.xlarge
```

Keep watching the tool output to detect the downtime.

I resized my instance from `db.m6i.large` to `db.m6i.xlarge` (the operation took ~25 min) and got following results regarding the DB downtime:

* RDS endpoint: downtime 3 sec;

* Always-ON Listener endpoint: no downtime;

* RDS Proxy endpoint: no downtime.

## Build

Install .NET Core 6 (or better) following a guide for your OS, e.g. <https://learn.microsoft.com/en-us/dotnet/core/install/linux-ubuntu>

Build the tool:

```bash
dotnet build
```

## Create RDS MS SQL database

* RDS > Create Database

* Standard Create

* Engine type: Microsoft SQL Server

* Database Management Type: Amazon RDS

* Edition: SQL Server Standard Edition

* Engine Version: SQL Server 2019 (pick the latest one)

* Templates: Production

* DB instance identifier: `<choose your own>`

* Master username: admin

* Master Password: `<choose your own>`

* DB instance class: Standard classes, `db.m6i.large` (or `<choose your own>`)

* Storage type: `gp3`

* Allocated storage: `20Gb`

* Enable storage autoscaling: Yes

* Multi-AZ deployment: Yes (Mirroring / Always On)

* Don't connect to an EC2 compute resource.

* VPC: `<choose your own>`

* Public access: No

* VPC security group: Create new. NB! You will need to edit the security group associated with the RDS instance to allow traffic from your EC2 instance running the "downtime detection" tool.

* Create an RDS Proxy: Yes
