using DevopsSharp.Enums;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DevopsSharp {

    public partial class DevopsClient {

        public async Task SearchWorkItems(int top = 50, IEnumerable<WorkItemType> workItems = null, IEnumerable<WorkItemState> workItemStates = null, IEnumerable<string> assignedTo = null) {
            JObject body = new JObject();
            body.Add(new JProperty("$top", top));

            // filter
            var filter = new JObject();
            if (workItems?.Any() == true)
                filter.Add(new JProperty("System.WorkItemType", new JArray(workItems.Select(item => item.ToString()))));
            if (workItemStates?.Any() == true)
                filter.Add(new JProperty("System.State", new JArray(workItemStates.Select(item => item.ToString()))));
            if (assignedTo?.Any() == true)
                filter.Add(new JProperty("System.AssignedTo", new JArray(assignedTo.Select(item => item.ToString()))));

            body.Add(new JProperty("$filters", filter));

            var content = new StringContent(body.ToString(), Encoding.UTF8);
            var response = await _client.PostAsync($"{_orgUri}/_apis/search/workitemsearchresults", content);
            response.EnsureSuccessStatusCode();

            var data = await response.Content.ReadAsStringAsync();
            Console.WriteLine(data);
        }
    }
}