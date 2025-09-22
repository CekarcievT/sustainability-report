using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SustainabilityReportingUI.Services
{
    public class UserHeaderHandler : DelegatingHandler
    {
        private readonly MockAuthService _authService;

        public UserHeaderHandler(MockAuthService authService)
        {
            _authService = authService;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (_authService.IsAuthenticated && !string.IsNullOrEmpty(_authService.UserName))
            {
                request.Headers.Remove("X-User");
                request.Headers.Add("X-User", _authService.UserName);
            }
            return base.SendAsync(request, cancellationToken);
        }
    }
}