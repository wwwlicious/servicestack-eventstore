namespace ServiceStack.EventStore.ConnectionManagement
{
    using System.Collections.Generic;
    using System.Text;
    using FluentValidation;

    public class ConnectionBuilder
    {
        public MonitorSettings MonitorSettings { get; set; }
        private readonly Dictionary<string, object> settings = new Dictionary<string, object>();
        private readonly Validator validator = new Validator();

        private string hostName = "";
        private string userName = "";
        private string password = "";

        public ConnectionBuilder()
        {
            MonitorSettings = new MonitorSettings();
        }

        private class Validator : AbstractValidator<ConnectionBuilder>
        {
            public Validator()
            {
                RuleFor(cb => cb.userName).NotEmpty();
                RuleFor(cb => cb.password).NotEmpty();
                RuleFor(cb => cb.hostName).NotEmpty();
            }
        }

        public string GetConnectionString()
        {
            validator.ValidateAndThrow(this);

            var connectionString = new StringBuilder();
            connectionString.Append($"ConnectTo=tcp://{userName}:{password}@{hostName}; ");
            settings.Each(s => connectionString.Append($"{s.Key}={s.Value}; "));
            return connectionString.ToString();
        }

        public ConnectionBuilder Host(string host)
        {
            hostName = host;
            return this;
        }

        public ConnectionBuilder UserName(string name)
        {
            userName = name;
            return this;
        }

        public ConnectionBuilder Password(string pwd)
        {
            password = pwd;
            return this;
        }

        public ConnectionBuilder ReconnectionDelay(int delay)
        {
            settings["ReconnectionDelay"] = delay;
            return this;
        }

        public ConnectionBuilder HeartbeatTimeout(int timeout)
        {
            settings["HeartbeatTimeout"] = timeout;
            return this;
        }

        public ConnectionBuilder MaxReconnections(int reconnections)
        {
            settings["MaxReconnections"] = reconnections;
            return this;
        }
    }
}
