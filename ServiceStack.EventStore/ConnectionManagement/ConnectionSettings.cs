namespace ServiceStack.EventStore.ConnectionManagement
{
    using System.Collections.Generic;
    using System.Text;
    using FluentValidation;

    /// <summary>
    /// Enables the developer to specify the connection settings to the running EventStore instance.
    /// </summary>
    public class ConnectionSettings
    {
        public MonitorSettings MonitorSettings { get; set; }
        private readonly Dictionary<string, object> settings = new Dictionary<string, object>();
        private readonly Validator validator = new Validator();

        private string httpAddress;
        private string tcpAddress;
        private string userName = "";
        private string password = "";

        public ConnectionSettings()
        {
            MonitorSettings = new MonitorSettings();
        }

        private class Validator : AbstractValidator<ConnectionSettings>
        {
            public Validator()
            {
                RuleFor(cb => cb.userName).NotEmpty();
                RuleFor(cb => cb.password).NotEmpty();
                RuleFor(cb => cb.tcpAddress).NotEmpty();
            }
        }

        public string GetConnectionString()
        {
            validator.ValidateAndThrow(this);

            var connectionString = new StringBuilder();
            connectionString.Append($"ConnectTo=tcp://{userName}:{password}@{tcpAddress}; ");
            settings.Each(s => connectionString.Append($"{s.Key}={s.Value}; "));
            return connectionString.ToString();
        }

        public string GetHttpEndpoint()
        {
            return httpAddress;
        }

        public ConnectionSettings HttpAddress(string address)
        {
            httpAddress = address;
            return this;
        }

        public string GetTcpEndpoint()
        {
            return tcpAddress;
        }

        public ConnectionSettings TCPEndpoint(string address)
        {
            tcpAddress = address;
            return this;
        }

        public ConnectionSettings UserName(string name)
        {
            userName = name;
            return this;
        }

        public ConnectionSettings Password(string pwd)
        {
            password = pwd;
            return this;
        }

        public ConnectionSettings ReconnectionDelay(int delay)
        {
            settings["ReconnectionDelay"] = delay;
            return this;
        }

        public ConnectionSettings HeartbeatTimeout(int timeout)
        {
            settings["HeartbeatTimeout"] = timeout;
            return this;
        }

        public ConnectionSettings MaxReconnections(int reconnections)
        {
            settings["MaxReconnections"] = reconnections;
            return this;
        }
    }
}
