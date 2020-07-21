using JJ.DevOps.AddTime.Domain;
using JJ.DevOps.AddTime.DTOs.Options;
using JJ.DevOps.AddTime.Helpers.Options;
using JJ.DevOps.AddTime.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;

namespace JJ.DevOps.AddTime {

    internal class Program {
        private static IConfiguration _config;
        private static ServiceProvider _provider;

        private static async Task Main(string[] args) {
            _config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            IServiceCollection services = new ServiceCollection();
            _provider = services
                // Service
                .AddSingleton<TimeManagementService>()
                .AddSingleton<DevOpsClient>()
                .AddSingleton<DevOpsUser>()
                // Options
                .AddSingleton(GetDevOpsUserConfiguration())
                .AddSingleton(GetDevOpsClientConfiguration())
                .AddSingleton(GetTimeManagementOptions())
                // Utility
                .AddLogging(configure => configure.AddConsole())
                .BuildServiceProvider();

            await _provider.GetRequiredService<DevOpsClient>().ConnectAsync();
            await _provider.GetRequiredService<TimeManagementService>()
                            .Run();
        }

        private static DevOpsClientOptions GetDevOpsClientConfiguration() {
            var options = _config.GetSection("DevOpsClientOptions").Get<DevOpsClientOptions>();

            var requests = _config.GetSection("DevOpsClientOptions")
                            .GetChildren()
                            .FirstOrDefault(child => child.Key == "TaskToCompletePackage")
                             ?.GetChildren();

            options.TaskToCompletePackage = requests.Select(GetDynamicObject).ToArray();

            // ENV Overrides
            options.ActiveItemsQueryId = FindEnviromentSetting("DEVOPSCLIENTOPTIONS_ACTIVEITEMQUERYID", options.ActiveItemsQueryId);
            options.ClosedTasksQueryId = FindEnviromentSetting("DEVOPSCLIENTOPTIONS_CLOSEDTASKSQUERYID", options.ClosedTasksQueryId);
            options.PersonalAccessToken = FindEnviromentSetting("DEVOPSCLIENTOPTIONS_PERSONALACCESSTOKEN", options.PersonalAccessToken);
            options.ProjectName = FindEnviromentSetting("DEVOPSCLIENTOPTIONS_PROJECTNAME", options.ProjectName);
            options.TeamName = FindEnviromentSetting("DEVOPSCLIENTOPTIONS_TEAMNAME", options.TeamName);

            if (string.IsNullOrWhiteSpace(options.ActiveItemsQueryId))
                throw new ApplicationException("DEVOPSCLIENTOPTIONS_ACTIVEITEMQUERYID is not set.");
            if (string.IsNullOrWhiteSpace(options.ClosedTasksQueryId))
                throw new ApplicationException("DEVOPSCLIENTOPTIONS_CLOSEDTASKSQUERYID is not set.");
            if (string.IsNullOrWhiteSpace(options.PersonalAccessToken))
                throw new ApplicationException("DEVOPSCLIENTOPTIONS_PERSONALACCESSTOKEN is not set.");
            if (string.IsNullOrWhiteSpace(options.ProjectName))
                throw new ApplicationException("DEVOPSCLIENTOPTIONS_PROJECTNAME is not set.");
            if (string.IsNullOrWhiteSpace(options.TeamName))
                throw new ApplicationException("DEVOPSCLIENTOPTIONS_TEAMNAME is not set.");
            if (options.TaskToCompletePackage?.Any() != true)
                throw new ApplicationException("DEVOPSCLIENTOPTIONS_TASKTOCOMPLETEPACKAGE is empty or null. There should be an entry for each mandatory field.");

            return options;
        }

        private static DevOpsUserOptions GetDevOpsUserConfiguration() {
            var options = _config.GetSection("DevOpsUserOptions").Get<DevOpsUserOptions>();

            // ENV Overrides
            options.Email = FindEnviromentSetting("DEVOPSUSEROPTIONS_EMAIL", options.Email);
            if (FindEnviromentSetting("DEVOPSUSEROPTIONS_TARGETHOURS") != null && float.TryParse(FindEnviromentSetting("DEVOPSUSEROPTIONS_TARGETHOURS"), out float target))
                options.TargetHours = target;
            if (FindEnviromentSetting("DEVOPSUSEROPTIONS_DAYSTARTHOUR") != null && float.TryParse(FindEnviromentSetting("DEVOPSUSEROPTIONS_DAYSTARTHOUR"), out float start))
                options.DayStartHour = start;

            if (string.IsNullOrWhiteSpace(options.Email))
                throw new ApplicationException("DEVOPSUSEROPTIONS_EMAIL is not set.");
            if (options.TargetHours <= 0)
                throw new ApplicationException("DEVOPSUSEROPTIONS_TARGETHOURS is not set (or is an invalid value).");
            if (options.DayStartHour <= 0)
                throw new ApplicationException("DEVOPSUSEROPTIONS_DAYSTARTHOUR is not set (or is an invalid value).");

            return options;
        }

        private static string FindEnviromentSetting(string key, string backup = null) {
            return Environment.GetEnvironmentVariable(key, EnvironmentVariableTarget.User)
                ?? Environment.GetEnvironmentVariable(key, EnvironmentVariableTarget.Process)
                ?? Environment.GetEnvironmentVariable(key, EnvironmentVariableTarget.Machine)
                ?? backup;
        }

        private static ExpandoObject GetDynamicObject(IConfigurationSection section) {
            if (section.GetChildren().All(x => x.Value != null))
                return section.Get<ExpandoObject>();

            var children = section.GetChildren();
            var output = new ExpandoObject() as IDictionary<string, object>;

            // Process NULL values.
            var nulls = children.Where(x => x.Value == null);
            var subSections = nulls.Select(x => new { x.Key, Value = GetDynamicObject(x) });
            foreach (var subSection in subSections) {
                output.Add(subSection.Key, subSection.Value);
            }
            // Process populated values.
            foreach (var child in children) {
                if (child.Value != null) // 'Except(null)' doesn't work here. Who knows why.
                    output.Add(child.Key, child.Value);
            }

            return output as ExpandoObject;
        }

        private static TimeManagementOptions GetTimeManagementOptions() {
            var options = _config.GetSection("TimeManagementOptions").Get<TimeManagementOptions>();

            if (FindEnviromentSetting("TIMEMANAGEMENTOPTIONS_UPDATESPERHOUR") != null && float.TryParse(FindEnviromentSetting("TIMEMANAGEMENTOPTIONS_TARGETHOURS"), out float updatesPerHour))
                options.UpdatesPerHour = updatesPerHour;

            if (options.UpdatesPerHour <= 0 && !options.ForcedUpdateHours.Any())
                throw new ApplicationException("TIMEMANAGEMENTOPTIONS_UPDATESPERHOUR must be greater than zero, or a value must be present for FORCEDUPDATEHOURS");

            return options;
        }
    }
}