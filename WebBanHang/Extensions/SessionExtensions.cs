using System.Text.Json;
using System.Text.Json.Serialization; // Thêm thư viện này
using Microsoft.AspNetCore.Http;

namespace WebBanHang.Extensions
{
    public static class SessionExtensions
    {
        public static void Set<T>(this ISession session, string key, T value)
        {
            // Cấu hình bỏ qua lỗi vòng lặp (IgnoreCycles)
            var options = new JsonSerializerOptions
            {
                ReferenceHandler = ReferenceHandler.IgnoreCycles
            };
            session.SetString(key, JsonSerializer.Serialize(value, options));
        }

        public static T Get<T>(this ISession session, string key)
        {
            var value = session.GetString(key);
            var options = new JsonSerializerOptions
            {
                ReferenceHandler = ReferenceHandler.IgnoreCycles
            };
            return value == null ? default : JsonSerializer.Deserialize<T>(value, options);
        }
    }
}