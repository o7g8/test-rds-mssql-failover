using CommandLine;
using Microsoft.Data.SqlClient;

var endpoint = "";
var user = "admin";
var password = "";
var label = "";

Parser.Default.ParseArguments<CommandLineOptions>(args)
      .WithParsed<CommandLineOptions>(o =>
      {
          endpoint = o.Endpoint;
          label = o.Label;
          user = o.User;
          password = o.Password;
      })
      .WithNotParsed<CommandLineOptions>(o =>
      {
          System.Environment.Exit(0);
      });

var builder = new SqlConnectionStringBuilder()
{
    DataSource = endpoint,
    UserID = user,
    Password = password,
    TrustServerCertificate = true
};

int RetryMaxAttempts = 60;
int RetryIntervalPeriodInSeconds = 1;

var sql = "SELECT name, collation_name FROM sys.databases";
int iRetryCount = 0;
while (iRetryCount < RetryMaxAttempts)
{
    try
    {
        Log($"Opening a DB connection to {builder.ConnectionString}");
        using var connection = new SqlConnection(builder.ConnectionString);
        connection.Open();

        using var command = new SqlCommand(sql, connection);
        while (true)
        {
            Log("Starting a query...");
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                Console.WriteLine("{0} {1}", reader.GetString(0), reader.GetString(1));
            }
            Thread.Sleep(1000);
        }
    }
    catch (Exception ex)
    {
        iRetryCount++;
        Log($"ERROR: {ex.Message}, retry #{iRetryCount}");
    }
    Thread.Sleep(RetryIntervalPeriodInSeconds * 1000);
}

void Log(string msg)
{
    Console.WriteLine($"{DateTime.Now} : {label} {msg}");
}

public class CommandLineOptions
{
    [Option('e', "endpoint", Required = true, HelpText = "MS SQL endpoint")]
    public string? Endpoint { get; set; }

    [Option('u', "user", Required = true, Default = "admin", HelpText = "DB username")]
    public string? User { get; set; }

    [Option('p', "password", Required = true, HelpText = "DB password")]
    public string? Password { get; set; }

    [Option('l', "label", Required = false, Default = "", HelpText = "Free-text label which will be used as prefix in stdout messages")]
    public string? Label { get; set; }
}