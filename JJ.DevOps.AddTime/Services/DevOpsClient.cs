using JJ.DevOps.AddTime.Helpers;
using JJ.DevOps.AddTime.Helpers.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace JJ.DevOps.AddTime.Services {

    public class DevOpsClient {
        private readonly DevOpsClientOptions _options;
        private HttpClient _client;
        private string _projectId;
        private string _teamId;

        public DevOpsClient(DevOpsClientOptions options) {
            _options = options;
        }

        public async Task ConnectAsync() {
            _client = new HttpClient();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                Convert.ToBase64String(Encoding.ASCII.GetBytes(string.Format("{0}:{1}", "", _options.PersonalAccessToken))));

            await Task.WhenAll(LoadProjectId()
                                , LoadTeamId())
                .ConfigureAwait(false);
        }

        public async IAsyncEnumerable<dynamic> GetActiveWorkItems() {
            var result = await RunQuery(_options.ActiveItemsQueryId);
            var workItemResults = result.workItems;

            foreach (var item in workItemResults) {
                yield return await GetWorkItem((int)item.id);
            }
        }

        public async IAsyncEnumerable<dynamic> GetCompleteWorkItems() {
            var result = await RunQuery(_options.ClosedTasksQueryId);

            foreach (var item in result.workItems) {
                yield return await GetWorkItem((int)item.id);
            }
        }

        public Task<dynamic> GetWorkItem(int id) {
            return GetDynamicResult($"{_options.Uri}/_apis/wit/workitems/{id}");
        }

        public async Task<int> CreateTask(int parent, float completed, string email) {
            string json = JsonConvert.SerializeObject(_options.TaskToCompletePackage)
                            .Replace("@@parent", parent.ToString(), StringComparison.OrdinalIgnoreCase)
                            .Replace("@@completed", completed.ToString(), StringComparison.OrdinalIgnoreCase)
                            .Replace("@@email", email.ToString(), StringComparison.OrdinalIgnoreCase);

            var content = new StringContent(json, Encoding.UTF8, "application/json-patch+json");
            var result = await PatchDynamicResult($"{_options.Uri}/{_projectId}/_apis/wit/workitems/$task?api-version=5.1", content);

            return (int)result.id;
        }

        public async Task<string> SetItemToClosed(int workItemId) {
            string json = "[" + DevOpsHelper.WorkItemData("add", "/fields/System.State", "Closed") + "]";

            var content = new StringContent(json, Encoding.UTF8, "application/json-patch+json");
            var response = await _client.PatchAsync($"{_options.Uri}/{_projectId}/_apis/wit/workitems/{workItemId}?api-version=5.1", content);
            response.EnsureSuccessStatusCode();

            return $"{_options.Uri}/{_projectId}/_workitems/edit/{workItemId}";
        }

        private async Task<JProperty> GetResponseValue(HttpResponseMessage response) {
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            JObject result = JObject.Parse(responseBody);
            return result.Property("value");
        }

        private async Task LoadProjectId() {
            HttpResponseMessage response = await _client.GetAsync($"{_options.Uri}/_apis/projects");
            var result = await GetResponseValue(response);
            var project = result.Value.Children().FirstOrDefault(token => string.Equals(token["name"].ToString(), _options.ProjectName, StringComparison.OrdinalIgnoreCase));

            if (project == null)
                throw new ApplicationException($"Project '{_options.ProjectName}' cannot be found.");
            else
                _projectId = project["id"].ToString();
        }

        private async Task LoadTeamId() {
            var response = await _client.GetAsync($"{_options.Uri}/_apis/teams");
            var result = await GetResponseValue(response);
            var team = result.Value.Children().FirstOrDefault(token => string.Equals(token["name"].ToString(), _options.TeamName, StringComparison.OrdinalIgnoreCase));

            if (team == null)
                throw new ApplicationException($"Team '{_options.TeamName}' cannot be found.");
            else
                _teamId = team["id"].ToString();
        }

        private async Task<dynamic> RunQuery(string id) {
            var response = await _client.GetAsync($"{_options.Uri}/_apis/wit/wiql/{id}");
            response.EnsureSuccessStatusCode();
            string content = await response.Content.ReadAsStringAsync();
            var converter = new ExpandoObjectConverter();
            return JsonConvert.DeserializeObject<ExpandoObject>(content, converter);
        }

        private async Task<dynamic> GetDynamicResult(string url) {
            var response = await _client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            string content = await response.Content.ReadAsStringAsync();
            var converter = new ExpandoObjectConverter();
            return JsonConvert.DeserializeObject<ExpandoObject>(content, converter);
        }

        private async Task<dynamic> PatchDynamicResult(string url, StringContent body) {
            var response = await _client.PatchAsync(url, body);
            response.EnsureSuccessStatusCode();
            string content = await response.Content.ReadAsStringAsync();
            var converter = new ExpandoObjectConverter();
            return JsonConvert.DeserializeObject<ExpandoObject>(content, converter);
        }
    }
}