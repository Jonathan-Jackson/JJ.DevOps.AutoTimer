using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Dynamic;
using System.Threading.Tasks;

namespace DevopsSharp {

    public partial class DevopsClient {

        public async Task<dynamic> RunQuery(string id) {
            var response = await _client.GetAsync($"{_orgUri}/_apis/wit/wiql/{id}");
            response.EnsureSuccessStatusCode();
            string content = await response.Content.ReadAsStringAsync();
            var converter = new ExpandoObjectConverter();
            return JsonConvert.DeserializeObject<ExpandoObject>(content, converter);
        }
    }
}