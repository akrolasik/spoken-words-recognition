using System.Collections.Generic;

namespace API.Helpers
{
    public class JwtToken
    {
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public string StsDiscoveryEndpoint { get; set; }
        public List<RequiredClaim> Claims { get; set; }
    }
}