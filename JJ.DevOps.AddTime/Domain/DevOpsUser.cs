using JJ.DevOps.AddTime.Helpers.Options;
using JJ.DevOps.AddTime.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace JJ.DevOps.AddTime.Domain {

    public class DevOpsUser {
        private readonly ILogger<DevOpsUser> _logger;
        private readonly string _email;
        private readonly DevOpsClient _devOps;
        private readonly float _targetHours;
        private readonly TimeSpan _dayStart;
        private readonly string[] _priorityStates;

        public DevOpsUser(DevOpsUserOptions options, DevOpsClient devOps, ILogger<DevOpsUser> logger) {
            _email = options.Email;
            _targetHours = options.TargetHours;
            _dayStart = TimeSpan.FromHours(options.DayStartHour);
            _devOps = devOps;
            _priorityStates = options.OrderedPriorityStates;
            _logger = logger;
        }

        public async Task UpdateTime() {
            var hours = await GetHoursToday();
            var expected = CalcTodaysExpectedHours();

            _logger.LogInformation($"{_email}: {hours} total hours with {expected} hours expected.");
            if (expected > hours) {
                float missingTime = (float)(Math.Round((expected - hours) * 4, MidpointRounding.ToEven) / 4);
                await AddMissingTime(missingTime);
            }
            else {
                _logger.LogInformation($"{_email}: Did not need to add time.");
            }
        }

        private async Task AddMissingTime(float missingTime) {
            var activeItemId = await FindPriorityActiveWorkItem();

            if (activeItemId > 0) {
                string url = await CreateTimedTask(activeItemId, missingTime);
                _logger.LogInformation($"{_email}: Added {missingTime} hours to {url}");
            }
            else {
                // send email about no active tasks.
                _logger.LogWarning("No Active Tasks!");
            }
        }

        private async Task<int> FindPriorityActiveWorkItem() {
            var activeItems = _devOps.GetActiveWorkItems();

            // Search for priority states (in order).
            foreach (var state in _priorityStates) {
                await foreach (var item in activeItems) {
                    foreach (var field in item.fields) {
                        if (field.Key == "System.State" && string.Equals((string)field.Value, state, StringComparison.OrdinalIgnoreCase)) {
                            return (int)item.id;
                        }
                    }
                }
            }

            // Just return the first one..
            await foreach (var item in activeItems) {
                return item;
            }

            return 0;
        }

        private async Task<string> CreateTimedTask(int parentId, float missingTime) {
            var id = await _devOps.CreateTask(parentId, missingTime, _email);
            return await _devOps.SetItemToClosed(id);
        }

        private float CalcTodaysExpectedHours() {
            var currentTime = DateTime.Now.TimeOfDay;

            if (currentTime < _dayStart)
                return 0;

            var expected = (float)(currentTime - _dayStart).TotalHours;
            return expected > _targetHours
                ? _targetHours
                : expected;
        }

        private async Task<float> GetHoursToday() {
            var complete = _devOps.GetCompleteWorkItems();

            float total = 0;
            await foreach (var item in complete) {
                foreach (var field in item.fields) {
                    if (field.Key == "Microsoft.VSTS.Scheduling.CompletedWork") {
                        total += (float)field.Value;
                    }
                }
            }

            return total;
        }
    }
}