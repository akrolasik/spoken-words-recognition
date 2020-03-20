using Newtonsoft.Json;
using NUnit.Framework;

namespace API.UnitTests.Helpers
{
    public class AssertExtension
    {
        public static void AreEqualJson(object expected, object actual)
        {
            Assert.NotNull(expected);
            Assert.NotNull(actual);

            var expectedJson = JsonConvert.SerializeObject(expected);
            var actualJson = JsonConvert.SerializeObject(actual);

            Assert.AreEqual(expectedJson, actualJson);
        }
    }
}