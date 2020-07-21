using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace DevopsSharp {

    public partial class DevopsClient {
        private HttpClient _client;
        private string _token;
        private Uri _orgUri;
        private string _projectName;
        private string _teamName;
        private string _projectId;
        private string _teamId;

        public DevopsClient(string token, Uri orgUri, string projectName, string teamName) {
            _token = token;
            _orgUri = orgUri;
            _projectName = projectName;
            _teamName = teamName;
        }

        private async Task LoadProjectId() {
            HttpResponseMessage response = await _client.GetAsync($"{_orgUri}/_apis/projects");
            var result = await GetResponseValue(response);
            var project = result.Value.Children().First(token => string.Equals(token["name"].ToString(), _projectName, StringComparison.OrdinalIgnoreCase));
            _projectId = project["id"].ToString();
        }

        public async Task ConnectAsync() {
            _client = new HttpClient();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                Convert.ToBase64String(Encoding.ASCII.GetBytes(string.Format("{0}:{1}", "", _token))));

            await LoadProjectId().ConfigureAwait(false);
            await LoadTeamId().ConfigureAwait(false);
        }

        private async Task<JProperty> GetResponseValue(HttpResponseMessage response) {
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            JObject result = JObject.Parse(responseBody);
            return result.Property("value");
        }
    }
}