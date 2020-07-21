using JJ.DevOps.AddTime.Domain;
using JJ.DevOps.AddTime.DTOs.Options;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace JJ.DevOps.AddTime.Services {

    public class TimeManagementService {
        private readonly ILogger<TimeManagementService> _logger;
        private readonly int _updateDelay;
        private readonly float[] _forcedUpdateTime;
        private readonly DevOpsUser _user;

        public TimeManagementService(ILogger<TimeManagementService> logger, TimeManagementOptions options, DevOpsUser user) {
            _logger = logger;
            _updateDelay = options.UpdatesPerHour > 0 ? (int)(360_0000 / options.UpdatesPerHour) : -1;
            _forcedUpdateTime = options.ForcedUpdateHours;
            _user = user;
        }

        public async Task Run() {
            while (true) {
                _logger.LogInformation("Updating..");
                await TryUpdateTime();
                await Task.Delay(_updateDelay);
            }
        }

        private async Task TryUpdateTime() {
            try {
                await _user.UpdateTime()
                        .ConfigureAwait(false);
            }
            catch (Exception ex) {
                _logger.LogError(ex, "Failed to update user.");
            }
        }
    }
}