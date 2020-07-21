namespace JJ.DevOps.AddTime.Helpers.Options {

    public class DevOpsClientOptions {
        public string ActiveItemsQueryId { get; set; }
        public string ClosedTasksQueryId { get; set; }
        public dynamic[] TaskToCompletePackage { get; set; }
        public string PersonalAccessToken { get; set; }
        public string TeamName { get; set; }
        public string ProjectName { get; set; }
        public string Uri { get; set; }
    }
}