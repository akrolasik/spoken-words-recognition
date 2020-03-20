using System;

namespace Utils.Extensions
{
    public static class ObjectExtension
    {
        public static void AssertAllPropertiesNotNull(this object obj)
        {
            foreach (var propertyInfo in obj.GetType().GetProperties())
            {
                if (string.IsNullOrEmpty(propertyInfo.GetValue(obj)?.ToString()))
                {
                    throw new ArgumentNullException(nameof(obj), $"Property {propertyInfo.Name} cannot be null or empty");
                }
            }
        }
    }
}
