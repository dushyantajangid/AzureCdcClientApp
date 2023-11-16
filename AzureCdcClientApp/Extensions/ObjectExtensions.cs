
namespace AzureCdcClientApp.Extensions
{
    public static class ObjectExtensions
    {
        public static T ToObject<T>(this IDictionary<string, string> source)
            where T : class, new()
        {
            var someObject = new T();
            var someObjectType = someObject.GetType();

            foreach (var item in source)
            {
                someObjectType.GetProperty(item.Key).SetValue(someObject, item.Value, null);
            }

            return someObject;
        }
        public static string GetValue(this IDictionary<string, string> source, string columnName)
        {
            string value;
            source.TryGetValue(columnName, out value);
            return value;
        }
    }
}
