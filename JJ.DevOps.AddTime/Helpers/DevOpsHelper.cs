namespace JJ.DevOps.AddTime.Helpers {

    public static class DevOpsHelper {

        public static string WorkItemData(string op, string path, string value, bool valueIsString = true) {
            if (valueIsString)
                return "{\"op\": \"" + op + "\", \"path\": \"" + path + "\", \"from\": null, \"value\": \"" + value + "\"}";
            else
                return "{\"op\": \"" + op + "\", \"path\": \"" + path + "\", \"from\": null, \"value\": " + value + "}";
        }
    }
}