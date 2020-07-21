using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DevopsSharp {

    public partial class DevopsClient {

        public async Task GetTeam(string id) {
            var response = await _client.GetAsync($"{_orgUri}/_apis/teams");
            response.EnsureSuccessStatusCode();

            var data = await response.Content.ReadAsStringAsync();
            Console.WriteLine(data);
        }

        private async Task LoadTeamId() {
            var response = await _client.GetAsync($"{_orgUri}/_apis/teams");
            var result = await GetResponseValue(response);
            var team = result.Value.Children().First(token => string.Equals(token["name"].ToString(), _teamName, StringComparison.OrdinalIgnoreCase));
            _teamId = team["id"].ToString();
        }
    }
}